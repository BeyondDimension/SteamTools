using System;

namespace Titanium.Web.Proxy.Exceptions
{
    /// <summary>
    ///     Base class exception associated with this proxy server.
    /// </summary>
    public abstract class ProxyException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyException" /> class.
        ///     - must be invoked by derived classes' constructors
        /// </summary>
        /// <param name="message">Exception message</param>
        protected ProxyException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyException" /> class.
        ///     - must be invoked by derived classes' constructors
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception associated</param>
        protected ProxyException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
