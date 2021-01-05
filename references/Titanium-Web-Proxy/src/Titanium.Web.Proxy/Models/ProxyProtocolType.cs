using System;

namespace Titanium.Web.Proxy.Models
{
    [Flags]
    public enum ProxyProtocolType
    {
        /// <summary>
        ///     The none
        /// </summary>
        None = 0,

        /// <summary>
        ///     HTTP
        /// </summary>
        Http = 1,

        /// <summary>
        ///     HTTPS
        /// </summary>
        Https = 2,

        /// <summary>
        ///     Both HTTP and HTTPS
        /// </summary>
        AllHttp = Http | Https
    }
}
