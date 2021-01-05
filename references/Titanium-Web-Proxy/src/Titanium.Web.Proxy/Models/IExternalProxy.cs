namespace Titanium.Web.Proxy.Models
{
    public interface IExternalProxy
    {
        /// <summary>
        ///     Use default windows credentials?
        /// </summary>
        bool UseDefaultCredentials { get; set; }

        /// <summary>
        ///     Bypass this proxy for connections to localhost?
        /// </summary>
        bool BypassLocalhost { get; set; }

        ExternalProxyType ProxyType { get; set; }

        bool ProxyDnsRequests { get; set; }

        /// <summary>
        ///     Username.
        /// </summary>
        string? UserName { get; set; }

        /// <summary>
        ///     Password.
        /// </summary>
        string? Password { get; set; }

        /// <summary>
        ///     Host name.
        /// </summary>
        string HostName { get; set; }

        /// <summary>
        ///     Port.
        /// </summary>
        int Port { get; set; }

        string ToString();
    }
}
