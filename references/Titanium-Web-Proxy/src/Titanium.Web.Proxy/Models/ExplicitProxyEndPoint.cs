using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Extensions;

namespace Titanium.Web.Proxy.Models
{
    /// <summary>
    ///     A proxy endpoint that the client is aware of.
    ///     So client application know that it is communicating with a proxy server.
    /// </summary>
    [DebuggerDisplay("Explicit: {IpAddress}:{Port}")]
    public class ExplicitProxyEndPoint : ProxyEndPoint
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="ipAddress">Listening IP address.</param>
        /// <param name="port">Listening port.</param>
        /// <param name="decryptSsl">Should we decrypt ssl?</param>
        public ExplicitProxyEndPoint(IPAddress ipAddress, int port, bool decryptSsl = true) : base(ipAddress, port,
            decryptSsl)
        {
        }

        internal bool IsSystemHttpProxy { get; set; }

        internal bool IsSystemHttpsProxy { get; set; }

        /// <summary>
        ///     Intercept tunnel connect request.
        ///     Valid only for explicit endpoints.
        ///     Set the <see cref="TunnelConnectSessionEventArgs.DecryptSsl" /> property to false if this HTTP connect request
        ///     shouldn't be decrypted and instead be relayed.
        /// </summary>
        public event AsyncEventHandler<TunnelConnectSessionEventArgs>? BeforeTunnelConnectRequest;

        /// <summary>
        ///     Intercept tunnel connect response.
        ///     Valid only for explicit endpoints.
        /// </summary>
        public event AsyncEventHandler<TunnelConnectSessionEventArgs>? BeforeTunnelConnectResponse;

        internal async Task InvokeBeforeTunnelConnectRequest(ProxyServer proxyServer,
            TunnelConnectSessionEventArgs connectArgs, ExceptionHandler exceptionFunc)
        {
            if (BeforeTunnelConnectRequest != null)
            {
                await BeforeTunnelConnectRequest.InvokeAsync(proxyServer, connectArgs, exceptionFunc);
            }
        }

        internal async Task InvokeBeforeTunnelConnectResponse(ProxyServer proxyServer,
            TunnelConnectSessionEventArgs connectArgs, ExceptionHandler exceptionFunc, bool isClientHello = false)
        {
            if (BeforeTunnelConnectResponse != null)
            {
                connectArgs.IsHttpsConnect = isClientHello;
                await BeforeTunnelConnectResponse.InvokeAsync(proxyServer, connectArgs, exceptionFunc);
            }
        }
    }
}
