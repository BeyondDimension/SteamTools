using System;

namespace Titanium.Web.Proxy
{
    /// <summary>
    ///     A delegate to catch exceptions occuring in proxy.
    /// </summary>
    /// <param name="exception">The exception occurred in proxy.</param>
    public delegate void ExceptionHandler(Exception exception);
}
