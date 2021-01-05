using System.Net;

namespace Titanium.Web.Proxy.Http.Responses
{
    /// <summary>
    ///     The http redirect response.
    /// </summary>
    public sealed class RedirectResponse : Response
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RedirectResponse" /> class.
        /// </summary>
        public RedirectResponse()
        {
            StatusCode = (int)HttpStatusCode.Found;
            StatusDescription = "Found";
        }
    }
}
