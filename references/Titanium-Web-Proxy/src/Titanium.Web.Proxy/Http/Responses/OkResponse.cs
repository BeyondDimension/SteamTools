using System.Net;

namespace Titanium.Web.Proxy.Http.Responses
{
    /// <summary>
    ///     The http 200 Ok response.
    /// </summary>
    public sealed class OkResponse : Response
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public OkResponse()
        {
            StatusCode = (int)HttpStatusCode.OK;
            StatusDescription = "OK";
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        public OkResponse(byte[] body) : this()
        {
            Body = body;
        }
    }
}
