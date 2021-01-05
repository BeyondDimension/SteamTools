using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Network.Tcp
{
    /// <summary>
    ///     An object that holds TcpConnection to a particular server and port
    /// </summary>
    internal class TcpServerConnection : IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();

        internal TcpServerConnection(ProxyServer proxyServer, Socket tcpSocket, HttpServerStream stream,
            string hostName, int port, bool isHttps, SslApplicationProtocol negotiatedApplicationProtocol,
            Version version, IExternalProxy? upStreamProxy, IPEndPoint? upStreamEndPoint, string cacheKey)
        {
            TcpSocket = tcpSocket;
            LastAccess = DateTime.UtcNow;
            this.proxyServer = proxyServer;
            this.proxyServer.UpdateServerConnectionCount(true);
            Stream = stream;
            HostName = hostName;
            Port = port;
            IsHttps = isHttps;
            NegotiatedApplicationProtocol = negotiatedApplicationProtocol;
            Version = version;
            UpStreamProxy = upStreamProxy;
            UpStreamEndPoint = upStreamEndPoint;

            CacheKey = cacheKey;
        }

        private ProxyServer proxyServer { get; }

        internal bool IsClosed => Stream.IsClosed;

        internal IExternalProxy? UpStreamProxy { get; set; }

        internal string HostName { get; set; }

        internal int Port { get; set; }

        internal bool IsHttps { get; set; }

        internal SslApplicationProtocol NegotiatedApplicationProtocol { get; set; }

        /// <summary>
        ///     Local NIC via connection is made
        /// </summary>
        internal IPEndPoint? UpStreamEndPoint { get; set; }

        /// <summary>
        ///     Http version
        /// </summary>
        internal Version Version { get; set; } = HttpHeader.VersionUnknown;

        /// <summary>
        /// The TcpClient.
        /// </summary>
        internal Socket TcpSocket { get; }

        /// <summary>
        ///     Used to write lines to server
        /// </summary>
        internal HttpServerStream Stream { get; }

        /// <summary>
        ///     Last time this connection was used
        /// </summary>
        internal DateTime LastAccess { get; set; }

        /// <summary>
        /// The cache key used to uniquely identify this connection properties
        /// </summary>
        internal string CacheKey { get; set; }

        /// <summary>
        /// Is this connection authenticated via WinAuth
        /// </summary>
        internal bool IsWinAuthenticated { get; set; }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                // delay calling tcp connection close()
                // so that server have enough time to call close first.
                // This way we can push tcp Time_Wait to server side when possible.
                await Task.Delay(1000);
                proxyServer.UpdateServerConnectionCount(false);
                Stream.Dispose();

                try
                {
                    TcpSocket.Close();
                }
                catch
                {
                    // ignore
                }
            });

        }
    }
}
