using System;
using Titanium.Web.Proxy.EventArguments;

namespace Titanium.Web.Proxy.Exceptions
{
    /// <summary>
    ///     Proxy Connection exception.
    /// </summary>
    public class ProxyConnectException : ProxyException
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyConnectException" /> class.
        /// </summary>
        /// <param name="message">Message for this exception</param>
        /// <param name="innerException">Associated inner exception</param>
        /// <param name="session">Instance of <see cref="EventArguments.TunnelConnectSessionEventArgs" /> associated to the exception</param>
        internal ProxyConnectException(string message, Exception innerException, SessionEventArgsBase session) : base(
            message, innerException)
        {
            Session = session;
        }

        /// <summary>
        ///     Gets session info associated to the exception.
        /// </summary>
        /// <remarks>
        ///     This object properties should not be edited.
        /// </remarks>
        public SessionEventArgsBase Session { get; }
    }
}
