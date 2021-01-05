using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Network.Tcp
{
    /// <summary>
    ///     An object that holds TcpConnection to a particular server and port
    /// </summary>
    internal class TcpClientConnection : IDisposable
    {
        public object ClientUserData { get; set; }

        internal TcpClientConnection(ProxyServer proxyServer, Socket tcpClientSocket)
        {
            this.tcpClientSocket = tcpClientSocket;
            this.proxyServer = proxyServer;
            this.proxyServer.UpdateClientConnectionCount(true);
        }

        private ProxyServer proxyServer { get; }

        public Guid Id { get; } = Guid.NewGuid();

        public EndPoint LocalEndPoint => tcpClientSocket.LocalEndPoint;

        public EndPoint RemoteEndPoint => tcpClientSocket.RemoteEndPoint;

        internal SslProtocols SslProtocol { get; set; }

        internal SslApplicationProtocol NegotiatedApplicationProtocol { get; set; }

        private readonly Socket tcpClientSocket;

        private int? processId;

        public Stream GetStream()
        {
            return new NetworkStream(tcpClientSocket, true);
        }

        public int GetProcessId(ProxyEndPoint endPoint)
        {
            if (processId.HasValue)
            {
                return processId.Value;
            }

            if (RunTime.IsWindows)
            {
                var remoteEndPoint = (IPEndPoint)RemoteEndPoint;

                // If client is localhost get the process id
                if (NetworkHelper.IsLocalIpAddress(remoteEndPoint.Address))
                {
                    processId = TcpHelper.GetProcessIdByLocalPort(endPoint.IpAddress.AddressFamily, remoteEndPoint.Port);
                }
                else
                {
                    // can't access process Id of remote request from remote machine
                    processId = -1;
                }

                return processId.Value;
            }

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                // delay calling tcp connection close()
                // so that client have enough time to call close first.
                // This way we can push tcp Time_Wait to client side when possible.
                await Task.Delay(1000);
                proxyServer.UpdateClientConnectionCount(false);

                try
                {
                    tcpClientSocket.Close();
                }
                catch
                {
                    // ignore
                }
            });
        }
    }
}
