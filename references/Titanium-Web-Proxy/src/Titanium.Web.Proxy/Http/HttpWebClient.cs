using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Extensions;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network.Tcp;

namespace Titanium.Web.Proxy.Http
{
    /// <summary>
    ///     Used to communicate with the server over HTTP(S)
    /// </summary>
    public class HttpWebClient
    {
        private TcpServerConnection? connection;

        internal HttpWebClient(ConnectRequest? connectRequest, Request request, Lazy<int> processIdFunc)
        {
            ConnectRequest = connectRequest;
            Request = request;
            Response = new Response();
            ProcessId = processIdFunc;
        }

        /// <summary>
        ///     Connection to server
        /// </summary>
        internal TcpServerConnection Connection
        {
            get
            {
                if (connection == null)
                {
                    throw new Exception("Connection is null");
                }

                return connection;
            }
        }

        internal bool HasConnection => connection != null;

        /// <summary>
        ///     Should we close the server connection at the end of this HTTP request/response session.
        /// </summary>
        internal bool CloseServerConnection { get; set; }

        /// <summary>
        ///     Stores internal data for the session.
        /// </summary>
        internal InternalDataStore Data { get; } = new InternalDataStore();

        /// <summary>
        ///     Gets or sets the user data.
        /// </summary>
        public object? UserData { get; set; }

        /// <summary>
        ///     Override UpStreamEndPoint for this request; Local NIC via request is made
        /// </summary>
        public IPEndPoint? UpStreamEndPoint { get; set; }

        /// <summary>
        ///     Headers passed with Connect.
        /// </summary>
        public ConnectRequest? ConnectRequest { get; }

        /// <summary>
        ///     Web Request.
        /// </summary>
        public Request Request { get; }

        /// <summary>
        ///     Web Response.
        /// </summary>
        public Response Response { get; internal set; }

        /// <summary>
        ///     PID of the process that is created the current session when client is running in this machine
        ///     If client is remote then this will return
        /// </summary>
        public Lazy<int> ProcessId { get; internal set; }

        /// <summary>
        ///     Is Https?
        /// </summary>
        public bool IsHttps => Request.IsHttps;

        /// <summary>
        ///     Set the tcp connection to server used by this webclient
        /// </summary>
        /// <param name="serverConnection">Instance of <see cref="TcpServerConnection" /></param>
        internal void SetConnection(TcpServerConnection serverConnection)
        {
            serverConnection.LastAccess = DateTime.UtcNow;
            connection = serverConnection;
        }

        /// <summary>
        ///     Prepare and send the http(s) request
        /// </summary>
        /// <returns></returns> 
        internal async Task SendRequest(bool enable100ContinueBehaviour, bool isTransparent, CancellationToken cancellationToken)
        {
            var upstreamProxy = Connection.UpStreamProxy;

            bool useUpstreamProxy = upstreamProxy != null && upstreamProxy.ProxyType == ExternalProxyType.Http &&
                                    !Connection.IsHttps;

            var serverStream = Connection.Stream;

            string? upstreamProxyUserName = null;
            string? upstreamProxyPassword = null;

            string url;
            if (isTransparent)
            {
                url = Request.RequestUriString;
            }
            else if (!useUpstreamProxy)
            {
                if (UriExtensions.GetScheme(Request.RequestUriString8).Length == 0)
                {
                    url = Request.RequestUriString;
                }
                else
                {
                    url = Request.RequestUri.GetOriginalPathAndQuery();
                }
            }
            else
            {
                url = Request.RequestUri.ToString();

                // Send Authentication to Upstream proxy if needed
                if (!string.IsNullOrEmpty(upstreamProxy!.UserName) && upstreamProxy.Password != null)
                {
                    upstreamProxyUserName = upstreamProxy.UserName;
                    upstreamProxyPassword = upstreamProxy.Password;
                }
            }

            if (url == string.Empty)
            {
                url = "/";
            }

            // prepare the request & headers
            var headerBuilder = new HeaderBuilder();
            headerBuilder.WriteRequestLine(Request.Method, url, Request.HttpVersion);
            headerBuilder.WriteHeaders(Request.Headers, !isTransparent, upstreamProxyUserName, upstreamProxyPassword);

            // write request headers
            await serverStream.WriteHeadersAsync(headerBuilder, cancellationToken);

            if (enable100ContinueBehaviour && Request.ExpectContinue)
            {
                // wait for expectation response from server
                await ReceiveResponse(cancellationToken);

                if (Response.StatusCode == (int)HttpStatusCode.Continue)
                {
                    Request.ExpectationSucceeded = true;
                }
                else
                {
                    Request.ExpectationFailed = true;
                }
            }
        }

        /// <summary>
        ///     Receive and parse the http response from server
        /// </summary>
        /// <returns></returns>
        internal async Task ReceiveResponse(CancellationToken cancellationToken)
        {
            // return if this is already read
            if (Response.StatusCode != 0)
            {
                return;
            }

            Response.RequestMethod = Request.Method;

            var httpStatus = await Connection.Stream.ReadResponseStatus(cancellationToken);
            Response.HttpVersion = httpStatus.Version;
            Response.StatusCode = httpStatus.StatusCode;
            Response.StatusDescription = httpStatus.Description;

            // Read the response headers in to unique and non-unique header collections
            await HeaderParser.ReadHeaders(Connection.Stream, Response.Headers, cancellationToken);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        internal void FinishSession()
        {
            connection = null;

            ConnectRequest?.FinishSession();
            Request?.FinishSession();
            Response?.FinishSession();

            Data.Clear();
            UserData = null;
        }
    }
}
