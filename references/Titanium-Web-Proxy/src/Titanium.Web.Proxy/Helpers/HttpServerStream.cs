using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.StreamExtended.BufferPool;

namespace Titanium.Web.Proxy.Helpers
{
    internal sealed class HttpServerStream : HttpStream
    {
        internal HttpServerStream(Stream stream, IBufferPool bufferPool, CancellationToken cancellationToken)
            : base(stream, bufferPool, cancellationToken)
        {
        }

        /// <summary>
        ///     Writes the request.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="cancellationToken">Optional cancellation token for this async task.</param>
        /// <returns></returns>
        internal async ValueTask WriteRequestAsync(Request request, CancellationToken cancellationToken = default)
        {
            var headerBuilder = new HeaderBuilder();
            headerBuilder.WriteRequestLine(request.Method, request.RequestUriString, request.HttpVersion);
            await WriteAsync(request, headerBuilder, cancellationToken);
        }

        internal async ValueTask<ResponseStatusInfo> ReadResponseStatus(CancellationToken cancellationToken = default)
        {
            try
            {
                string httpStatus = await ReadLineAsync(cancellationToken) ??
                                    throw new ServerConnectionException("Server connection was closed.");

                if (httpStatus == string.Empty)
                {
                    // is this really possible?
                    httpStatus = await ReadLineAsync(cancellationToken) ??
                                 throw new ServerConnectionException("Server connection was closed. Response status is empty.");
                }

                Response.ParseResponseLine(httpStatus, out var version, out int statusCode, out string description);
                return new ResponseStatusInfo { Version = version, StatusCode = statusCode, Description = description };
            }
            catch (Exception e) when (!(e is ServerConnectionException))
            {
                throw new ServerConnectionException("Server connection was closed. Exception while reading the response status.", e);
            }
        }
    }
}
