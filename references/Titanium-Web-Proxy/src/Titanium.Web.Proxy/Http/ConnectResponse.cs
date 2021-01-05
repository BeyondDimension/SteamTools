using System;
using System.Net;
using Titanium.Web.Proxy.StreamExtended;

namespace Titanium.Web.Proxy.Http
{
    /// <summary>
    /// The tcp tunnel connect response object.
    /// </summary>
    public class ConnectResponse : Response
    {
        public ServerHelloInfo? ServerHelloInfo { get; set; }

        /// <summary>
        ///     Creates a successful CONNECT response
        /// </summary>
        /// <param name="httpVersion"></param>
        /// <returns></returns>
        internal static ConnectResponse CreateSuccessfulConnectResponse(Version httpVersion)
        {
            var response = new ConnectResponse
            {
                HttpVersion = httpVersion,
                StatusCode = (int)HttpStatusCode.OK,
                StatusDescription = "Connection Established"
            };

            return response;
        }
    }
}
