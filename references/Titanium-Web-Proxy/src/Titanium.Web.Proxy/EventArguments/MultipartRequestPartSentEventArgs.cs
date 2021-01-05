using Titanium.Web.Proxy.Http;

namespace Titanium.Web.Proxy.EventArguments
{
    /// <summary>
    ///     Class that wraps the multipart sent request arguments.
    /// </summary>
    public class MultipartRequestPartSentEventArgs : ProxyEventArgsBase
    {
        internal MultipartRequestPartSentEventArgs(SessionEventArgs session, string boundary, HeaderCollection headers) : base(session.ClientConnection)
        {
            Session = session;
            Boundary = boundary;
            Headers = headers;
        }

        /// <value>
        ///     The session arguments.
        /// </value>
        public SessionEventArgs Session { get; }

        /// <summary>
        ///     Boundary.
        /// </summary>
        public string Boundary { get; }

        /// <summary>
        ///     The header collection.
        /// </summary>
        public HeaderCollection Headers { get; }
    }
}
