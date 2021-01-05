using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Http.Responses;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.EventArguments
{
    /// <summary>
    /// Holds info related to a single proxy session (single request/response sequence).
    /// A proxy session is bounded to a single connection from client.
    /// A proxy session ends when client terminates connection to proxy
    /// or when server terminates connection from proxy.
    /// </summary>
    public class SessionEventArgs : SessionEventArgsBase
    {
        /// <summary>
        /// Backing field for corresponding public property
        /// </summary>
        private bool reRequest;

        private WebSocketDecoder? webSocketDecoderSend;
        
        private WebSocketDecoder? webSocketDecoderReceive;

        /// <summary>
        ///     Is this session a HTTP/2 promise?
        /// </summary>
        public bool IsPromise { get; internal set; }

        /// <summary>
        /// Constructor to initialize the proxy
        /// </summary>
        internal SessionEventArgs(ProxyServer server, ProxyEndPoint endPoint, HttpClientStream clientStream, ConnectRequest? connectRequest, CancellationTokenSource cancellationTokenSource)
            : base(server, endPoint, clientStream, connectRequest, new Request(), cancellationTokenSource)
        {
        }

        private bool hasMulipartEventSubscribers => MultipartRequestPartSent != null;

        /// <summary>
        /// Should we send the request again ?
        /// </summary>
        public bool ReRequest
        {
            get => reRequest;
            set
            {
                if (HttpClient.Response.StatusCode == 0)
                {
                    throw new Exception("Response status code is empty. Cannot request again a request " + "which was never send to server.");
                }

                reRequest = value;
            }
        }

        [Obsolete("Use [WebSocketDecoderReceive] instead")]
        public WebSocketDecoder WebSocketDecoder => WebSocketDecoderReceive;

        public WebSocketDecoder WebSocketDecoderSend => webSocketDecoderSend ??= new WebSocketDecoder(BufferPool);
        
        public WebSocketDecoder WebSocketDecoderReceive => webSocketDecoderReceive ??= new WebSocketDecoder(BufferPool);

        /// <summary>
        /// Occurs when multipart request part sent.
        /// </summary>
        public event EventHandler<MultipartRequestPartSentEventArgs>? MultipartRequestPartSent;

        /// <summary>
        /// Read request body content as bytes[] for current session
        /// </summary>
        private async Task readRequestBodyAsync(CancellationToken cancellationToken)
        {
            HttpClient.Request.EnsureBodyAvailable(false);

            var request = HttpClient.Request;

            // If not already read (not cached yet)
            if (!request.IsBodyRead)
            {
                if (request.HttpVersion == HttpHeader.Version20)
                {
                    // do not send to the remote endpoint
                    request.Http2IgnoreBodyFrames = true;

                    request.Http2BodyData = new MemoryStream();

                    var tcs = new TaskCompletionSource<bool>();
                    request.ReadHttp2BodyTaskCompletionSource = tcs;

                    // signal to HTTP/2 copy frame method to continue
                    request.ReadHttp2BeforeHandlerTaskCompletionSource!.SetResult(true);

                    await tcs.Task;

                    // Now set the flag to true
                    // So that next time we can deliver body from cache
                    request.IsBodyRead = true;
                }
                else
                {
                    var body = await readBodyAsync(true, cancellationToken);
                    if (!request.BodyAvailable)
                    {
                        request.Body = body;
                    }

                    // Now set the flag to true
                    // So that next time we can deliver body from cache
                    request.IsBodyRead = true;
                }
            }
        }

        /// <summary>
        /// reinit response object
        /// </summary>
        internal async Task ClearResponse(CancellationToken cancellationToken)
        {
            // syphon out the response body from server
            await SyphonOutBodyAsync(false, cancellationToken);
            HttpClient.Response = new Response();
        }

        internal void OnMultipartRequestPartSent(ReadOnlySpan<char> boundary, HeaderCollection headers)
        {
            try
            {
                MultipartRequestPartSent?.Invoke(this, new MultipartRequestPartSentEventArgs(this, boundary.ToString(), headers));
            }
            catch (Exception ex)
            {
                ExceptionFunc(new Exception("Exception thrown in user event", ex));
            }
        }

        /// <summary>
        /// Read response body as byte[] for current response
        /// </summary>
        private async Task readResponseBodyAsync(CancellationToken cancellationToken)
        {
            if (!HttpClient.Request.Locked)
            {
                throw new Exception("You cannot read the response body before request is made to server.");
            }

            var response = HttpClient.Response;
            if (!response.HasBody)
            {
                return;
            }

            // If not already read (not cached yet)
            if (!response.IsBodyRead)
            {
                if (response.HttpVersion == HttpHeader.Version20)
                {
                    // do not send to the remote endpoint
                    response.Http2IgnoreBodyFrames = true;

                    response.Http2BodyData = new MemoryStream();

                    var tcs = new TaskCompletionSource<bool>();
                    response.ReadHttp2BodyTaskCompletionSource = tcs;

                    // signal to HTTP/2 copy frame method to continue
                    response.ReadHttp2BeforeHandlerTaskCompletionSource!.SetResult(true);

                    await tcs.Task;

                    // Now set the flag to true
                    // So that next time we can deliver body from cache
                    response.IsBodyRead = true;
                }
                else
                {
                    var body = await readBodyAsync(false, cancellationToken);
                    if (!response.BodyAvailable)
                    {
                        response.Body = body;
                    }

                    // Now set the flag to true
                    // So that next time we can deliver body from cache
                    response.IsBodyRead = true;
                }
            }
        }

        private async Task<byte[]> readBodyAsync(bool isRequest, CancellationToken cancellationToken)
        {
            using var bodyStream = new MemoryStream();
            using var writer = new HttpStream(bodyStream, BufferPool, cancellationToken);

            if (isRequest)
            {
                await CopyRequestBodyAsync(writer, TransformationMode.Uncompress, cancellationToken);
            }
            else
            {
                await copyResponseBodyAsync(writer, TransformationMode.Uncompress, cancellationToken);
            }

            return bodyStream.ToArray();
        }

        /// <summary>
        ///     Syphon out any left over data in given request/response from backing tcp connection.
        ///     When user modifies the response/request we need to do this to reuse tcp connections.
        /// </summary>
        /// <param name="isRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task SyphonOutBodyAsync(bool isRequest, CancellationToken cancellationToken)
        {
            var requestResponse = isRequest ? (RequestResponseBase)HttpClient.Request : HttpClient.Response;
            if (requestResponse.IsBodyRead || !requestResponse.OriginalHasBody)
            {
                return;
            }

            var reader = isRequest ? (HttpStream)ClientStream : HttpClient.Connection.Stream;

            await reader.CopyBodyAsync(requestResponse, true, NullWriter.Instance, TransformationMode.None, null, cancellationToken);
        }

        /// <summary>
        ///  This is called when the request is PUT/POST/PATCH to read the body
        /// </summary>
        /// <returns></returns>
        internal async Task CopyRequestBodyAsync(IHttpStreamWriter writer, TransformationMode transformation, CancellationToken cancellationToken)
        {
            var request = HttpClient.Request;
            var reader = ClientStream;

            long contentLength = request.ContentLength;

            // send the request body bytes to server
            if (contentLength > 0 && hasMulipartEventSubscribers && request.IsMultipartFormData)
            {
                var boundary = HttpHelper.GetBoundaryFromContentType(request.ContentType);

                using (var copyStream = new CopyStream(reader, writer, BufferPool))
                {
                    while (contentLength > copyStream.ReadBytes)
                    {
                        long read = await readUntilBoundaryAsync(copyStream, contentLength, boundary, cancellationToken);
                        if (read == 0)
                        {
                            break;
                        }

                        if (contentLength > copyStream.ReadBytes)
                        {
                            var headers = new HeaderCollection();
                            await HeaderParser.ReadHeaders(copyStream, headers, cancellationToken);
                            OnMultipartRequestPartSent(boundary.Span, headers);
                        }
                    }

                    await copyStream.FlushAsync(cancellationToken);
                }
            }
            else
            {
                await reader.CopyBodyAsync(request, false, writer, transformation, OnDataSent, cancellationToken);
            }
        }

        private async Task copyResponseBodyAsync(IHttpStreamWriter writer, TransformationMode transformation, CancellationToken cancellationToken)
        {
            await HttpClient.Connection.Stream.CopyBodyAsync(HttpClient.Response, false, writer, transformation, OnDataReceived, cancellationToken);
        }

        /// <summary>
        /// Read a line from the byte stream
        /// </summary>
        /// <returns></returns>
        private async Task<long> readUntilBoundaryAsync(ILineStream reader, long totalBytesToRead, ReadOnlyMemory<char> boundary, CancellationToken cancellationToken)
        {
            int bufferDataLength = 0;

            var buffer = BufferPool.GetBuffer();
            try
            {
                int boundaryLength = boundary.Length + 4;
                long bytesRead = 0;

                while (bytesRead < totalBytesToRead && (reader.DataAvailable || await reader.FillBufferAsync(cancellationToken)))
                {
                    byte newChar = reader.ReadByteFromBuffer();
                    buffer[bufferDataLength] = newChar;

                    bufferDataLength++;
                    bytesRead++;

                    if (bufferDataLength >= boundaryLength)
                    {
                        int startIdx = bufferDataLength - boundaryLength;
                        if (buffer[startIdx] == '-' && buffer[startIdx + 1] == '-')
                        {
                            startIdx += 2;
                            bool ok = true;
                            for (int i = 0; i < boundary.Length; i++)
                            {
                                if (buffer[startIdx + i] != boundary.Span[i])
                                {
                                    ok = false;
                                    break;
                                }
                            }

                            if (ok)
                            {
                                break;
                            }
                        }
                    }

                    if (bufferDataLength == buffer.Length)
                    {
                        // boundary is not longer than 70 bytes according to the specification, so keeping the last 100 (minimum 74) bytes is enough
                        const int bytesToKeep = 100;
                        Buffer.BlockCopy(buffer, buffer.Length - bytesToKeep, buffer, 0, bytesToKeep);
                        bufferDataLength = bytesToKeep;
                    }
                }

                return bytesRead;
            }
            finally
            {
                BufferPool.ReturnBuffer(buffer);
            }
        }

        /// <summary>
        /// Gets the request body as bytes.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token for this async task.</param>
        /// <returns>The body as bytes.</returns>
        public async Task<byte[]> GetRequestBody(CancellationToken cancellationToken = default)
        {
            if (!HttpClient.Request.IsBodyRead)
            {
                await readRequestBodyAsync(cancellationToken);
            }

            return HttpClient.Request.Body;
        }

        /// <summary>
        /// Gets the request body as string.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token for this async task.</param>
        /// <returns>The body as string.</returns>
        public async Task<string> GetRequestBodyAsString(CancellationToken cancellationToken = default)
        {
            if (!HttpClient.Request.IsBodyRead)
            {
                await readRequestBodyAsync(cancellationToken);
            }

            return HttpClient.Request.BodyString;
        }

        /// <summary>
        /// Sets the request body.
        /// </summary>
        /// <param name="body">The request body bytes.</param>
        public void SetRequestBody(byte[] body)
        {
            var request = HttpClient.Request;
            if (request.Locked)
            {
                throw new Exception("You cannot call this function after request is made to server.");
            }

            request.Body = body;
        }

        /// <summary>
        /// Sets the body with the specified string.
        /// </summary>
        /// <param name="body">The request body string to set.</param>
        public void SetRequestBodyString(string body)
        {
            if (HttpClient.Request.Locked)
            {
                throw new Exception("You cannot call this function after request is made to server.");
            }

            SetRequestBody(HttpClient.Request.Encoding.GetBytes(body));
        }


        /// <summary>
        /// Gets the response body as bytes.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token for this async task.</param>
        /// <returns>The resulting bytes.</returns>
        public async Task<byte[]> GetResponseBody(CancellationToken cancellationToken = default)
        {
            if (!HttpClient.Response.IsBodyRead)
            {
                await readResponseBodyAsync(cancellationToken);
            }

            return HttpClient.Response.Body;
        }

        /// <summary>
        /// Gets the response body as string.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token for this async task.</param>
        /// <returns>The string body.</returns>
        public async Task<string> GetResponseBodyAsString(CancellationToken cancellationToken = default)
        {
            if (!HttpClient.Response.IsBodyRead)
            {
                await readResponseBodyAsync(cancellationToken);
            }

            return HttpClient.Response.BodyString;
        }

        /// <summary>
        /// Set the response body bytes.
        /// </summary>
        /// <param name="body">The body bytes to set.</param>
        public void SetResponseBody(byte[] body)
        {
            if (!HttpClient.Request.Locked)
            {
                throw new Exception("You cannot call this function before request is made to server.");
            }

            var response = HttpClient.Response;
            response.Body = body;
        }

        /// <summary>
        /// Replace the response body with the specified string.
        /// </summary>
        /// <param name="body">The body string to set.</param>
        public void SetResponseBodyString(string body)
        {
            if (!HttpClient.Request.Locked)
            {
                throw new Exception("You cannot call this function before request is made to server.");
            }

            var bodyBytes = HttpClient.Response.Encoding.GetBytes(body);

            SetResponseBody(bodyBytes);
        }

        /// <summary>
        /// Before request is made to server respond with the specified HTML string to client
        /// and ignore the request. 
        /// </summary>
        /// <param name="html">HTML content to sent.</param>
        /// <param name="headers">HTTP response headers.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void Ok(string html, IDictionary<string, HttpHeader>? headers,
            bool closeServerConnection = false)
        {
            Ok(html, headers?.Values, closeServerConnection);
        }

        /// <summary>
        /// Before request is made to server respond with the specified HTML string to client
        /// and ignore the request. 
        /// </summary>
        /// <param name="html">HTML content to sent.</param>
        /// <param name="headers">HTTP response headers.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void Ok(string html, IEnumerable<HttpHeader>? headers = null,
            bool closeServerConnection = false)
        {
            var response = new OkResponse();
            if (headers != null)
            {
                response.Headers.AddHeaders(headers);
            }

            response.HttpVersion = HttpClient.Request.HttpVersion;
            response.Body = response.Encoding.GetBytes(html ?? string.Empty);

            Respond(response, closeServerConnection);
        }

        /// <summary>
        /// Before request is made to server respond with the specified byte[] to client
        /// and ignore the request. 
        /// </summary>
        /// <param name="result">The html content bytes.</param>
        /// <param name="headers">The HTTP headers.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void Ok(byte[] result, IDictionary<string, HttpHeader>? headers,
            bool closeServerConnection = false)
        {
            Ok(result, headers?.Values, closeServerConnection);
        }

        /// <summary>
        /// Before request is made to server respond with the specified byte[] to client
        /// and ignore the request. 
        /// </summary>
        /// <param name="result">The html content bytes.</param>
        /// <param name="headers">The HTTP headers.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void Ok(byte[] result, IEnumerable<HttpHeader>? headers = null,
            bool closeServerConnection = false)
        {
            var response = new OkResponse();
            response.Headers.AddHeaders(headers);
            response.HttpVersion = HttpClient.Request.HttpVersion;
            response.Body = result;

            Respond(response, closeServerConnection);
        }

        /// <summary>
        /// Before request is made to server 
        /// respond with the specified HTML string and the specified status to client.
        /// And then ignore the request. 
        /// </summary>
        /// <param name="html">The html content.</param>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="headers">The HTTP headers.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void GenericResponse(string html, HttpStatusCode status,
            IDictionary<string, HttpHeader>? headers, bool closeServerConnection = false)
        {
            GenericResponse(html, status, headers?.Values, closeServerConnection);
        }

        /// <summary>
        /// Before request is made to server 
        /// respond with the specified HTML string and the specified status to client.
        /// And then ignore the request. 
        /// </summary>
        /// <param name="html">The html content.</param>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="headers">The HTTP headers.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void GenericResponse(string html, HttpStatusCode status,
            IEnumerable<HttpHeader>? headers = null, bool closeServerConnection = false)
        {
            var response = new GenericResponse(status);
            response.HttpVersion = HttpClient.Request.HttpVersion;
            response.Headers.AddHeaders(headers);
            response.Body = response.Encoding.GetBytes(html ?? string.Empty);

            Respond(response, closeServerConnection);
        }

        /// <summary>
        /// Before request is made to server respond with the specified byte[],
        /// the specified status  to client. And then ignore the request.
        /// </summary>
        /// <param name="result">The bytes to sent.</param>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="headers">The HTTP headers.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void GenericResponse(byte[] result, HttpStatusCode status,
            IDictionary<string, HttpHeader> headers, bool closeServerConnection = false)
        {
            GenericResponse(result, status, headers?.Values, closeServerConnection);
        }

        /// <summary>
        /// Before request is made to server respond with the specified byte[],
        /// the specified status  to client. And then ignore the request.
        /// </summary>
        /// <param name="result">The bytes to sent.</param>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="headers">The HTTP headers.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void GenericResponse(byte[] result, HttpStatusCode status,
            IEnumerable<HttpHeader>? headers, bool closeServerConnection = false)
        {
            var response = new GenericResponse(status);
            response.HttpVersion = HttpClient.Request.HttpVersion;
            response.Headers.AddHeaders(headers);
            response.Body = result;

            Respond(response, closeServerConnection);
        }

        /// <summary>
        /// Redirect to provided URL.
        /// </summary>
        /// <param name="url">The URL to redirect.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void Redirect(string url, bool closeServerConnection = false)
        {
            var response = new RedirectResponse();
            response.HttpVersion = HttpClient.Request.HttpVersion;
            response.Headers.AddHeader(KnownHeaders.Location, url);
#if NET45
            response.Body = Net45Compatibility.EmptyArray;
#else
            response.Body = Array.Empty<byte>();
#endif

            Respond(response, closeServerConnection);
        }

        /// <summary>
        /// Respond with given response object to client.
        /// </summary>
        /// <param name="response">The response object.</param>
        /// <param name="closeServerConnection">Close the server connection used by request if any?</param>
        public void Respond(Response response, bool closeServerConnection = false)
        {
            // request already send/ready to be sent.
            if (HttpClient.Request.Locked)
            {
                // response already received from server and ready to be sent to client.
                if (HttpClient.Response.Locked)
                {
                    throw new Exception("You cannot call this function after response is sent to the client.");
                }

                // cleanup original response.
                if (closeServerConnection)
                {
                    // no need to cleanup original connection.
                    // it will be closed any way.
                    TerminateServerConnection();
                }

                response.SetOriginalHeaders(HttpClient.Response);

                // response already received from server but not yet ready to sent to client.         
                HttpClient.Response = response;
                HttpClient.Response.Locked = true;
            }
            // request not yet sent/not yet ready to be sent.
            else
            {
                HttpClient.Request.Locked = true;
                HttpClient.Request.CancelRequest = true;

                // set new response.
                HttpClient.Response = response;
                HttpClient.Response.Locked = true;
            }
        }

        /// <summary>
        ///     Terminate the connection to server at the end of this HTTP request/response session.
        /// </summary>
        public void TerminateServerConnection()
        {
            HttpClient.CloseServerConnection = true;
        }

        /// <summary>
        /// Implement any cleanup here
        /// </summary>
        public override void Dispose()
        {
            MultipartRequestPartSent = null;
            base.Dispose();
        }
    }
}
