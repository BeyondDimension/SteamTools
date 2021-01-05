using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Extensions;

namespace Titanium.Web.Proxy.Models
{
    /// <summary>
    ///     A proxy end point client is not aware of.
    ///     Useful when requests are redirected to this proxy end point through port forwarding via router.
    /// </summary>
    [DebuggerDisplay("SOCKS: {IpAddress}:{Port}")]
    public class SocksProxyEndPoint : TransparentBaseProxyEndPoint
    {
        /// <summary>
        ///     Initialize a new instance.
        /// </summary>
        /// <param name="ipAddress">Listening Ip address.</param>
        /// <param name="port">Listening port.</param>
        /// <param name="decryptSsl">Should we decrypt ssl?</param>
        public SocksProxyEndPoint(IPAddress ipAddress, int port, bool decryptSsl = true) : base(ipAddress, port,
            decryptSsl)
        {
            GenericCertificateName = "localhost";
        }

        /// <summary>
        ///     Name of the Certificate need to be sent (same as the hostname we want to proxy).
        ///     This is valid only when UseServerNameIndication is set to false.
        /// </summary>
        public override string GenericCertificateName { get; set; }

        /// <summary>
        ///     Before Ssl authentication this event is fired.
        /// </summary>
        public event AsyncEventHandler<BeforeSslAuthenticateEventArgs>? BeforeSslAuthenticate;

        internal override async Task InvokeBeforeSslAuthenticate(ProxyServer proxyServer,
            BeforeSslAuthenticateEventArgs connectArgs, ExceptionHandler exceptionFunc)
        {
            if (BeforeSslAuthenticate != null)
            {
                await BeforeSslAuthenticate.InvokeAsync(proxyServer, connectArgs, exceptionFunc);
            }
        }
    }
}
