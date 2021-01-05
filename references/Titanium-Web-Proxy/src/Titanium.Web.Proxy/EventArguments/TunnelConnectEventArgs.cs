using System;
using System.Threading;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.EventArguments
{
    /// <summary>
    ///     A class that wraps the state when a tunnel connect event happen for Explicit endpoints.
    /// </summary>
    public class TunnelConnectSessionEventArgs : SessionEventArgsBase
    {
        private bool? isHttpsConnect;

        internal TunnelConnectSessionEventArgs(ProxyServer server, ProxyEndPoint endPoint, ConnectRequest connectRequest,
            HttpClientStream clientStream, CancellationTokenSource cancellationTokenSource)
            : base(server, endPoint, clientStream, connectRequest, connectRequest, cancellationTokenSource)
        {
        }

        /// <summary>
        ///     Should we decrypt the Ssl or relay it to server?
        ///     Default is true.
        /// </summary>
        public bool DecryptSsl { get; set; } = true;

        /// <summary>
        ///     When set to true it denies the connect request with a Forbidden status.
        /// </summary>
        public bool DenyConnect { get; set; }

        /// <summary>
        ///     Is this a connect request to secure HTTP server? Or is it to some other protocol.
        /// </summary>
        public bool IsHttpsConnect
        {
            get => isHttpsConnect ??
                   throw new Exception("The value of this property is known in the BeforeTunnelConnectResponse event");

            internal set => isHttpsConnect = value;
        }

        /// <summary>
        ///     Fired when decrypted data is sent within this session to server/client.
        /// </summary>
        public event EventHandler<DataEventArgs>? DecryptedDataSent;

        /// <summary>
        ///     Fired when decrypted data is received within this session from client/server.
        /// </summary>
        public event EventHandler<DataEventArgs>? DecryptedDataReceived;

        internal void OnDecryptedDataSent(byte[] buffer, int offset, int count)
        {
            try
            {
                DecryptedDataSent?.Invoke(this, new DataEventArgs(buffer, offset, count));
            }
            catch (Exception ex)
            {
                ExceptionFunc(new Exception("Exception thrown in user event", ex));
            }
        }

        internal void OnDecryptedDataReceived(byte[] buffer, int offset, int count)
        {
            try
            {
                DecryptedDataReceived?.Invoke(this, new DataEventArgs(buffer, offset, count));
            }
            catch (Exception ex)
            {
                ExceptionFunc(new Exception("Exception thrown in user event", ex));
            }
        }
    }
}
