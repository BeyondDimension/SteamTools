using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network.Tcp;
using Titanium.Web.Proxy.StreamExtended.BufferPool;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.EventArguments
{
    /// <summary>
    ///     Holds info related to a single proxy session (single request/response sequence).
    ///     A proxy session is bounded to a single connection from client.
    ///     A proxy session ends when client terminates connection to proxy
    ///     or when server terminates connection from proxy.
    /// </summary>
    public abstract class SessionEventArgsBase : ProxyEventArgsBase, IDisposable
    {
        private static bool isWindowsAuthenticationSupported => RunTime.IsWindows;

        internal readonly CancellationTokenSource CancellationTokenSource;

        internal TcpServerConnection ServerConnection => HttpClient.Connection;

        /// <summary>
        ///     Holds a reference to client
        /// </summary>
        internal TcpClientConnection ClientConnection => ClientStream.Connection;

        internal HttpClientStream ClientStream { get; }

        public Guid ClientConnectionId => ClientConnection.Id;

        public Guid ServerConnectionId => HttpClient.HasConnection ? ServerConnection.Id : Guid.Empty;

        protected readonly IBufferPool BufferPool;
        protected readonly ExceptionHandler ExceptionFunc;
        private bool enableWinAuth;

        /// <summary>
        /// Relative milliseconds for various events.
        /// </summary>
        public Dictionary<string, DateTime> TimeLine { get; } = new Dictionary<string, DateTime>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionEventArgsBase" /> class.
        /// </summary>
        private protected SessionEventArgsBase(ProxyServer server, ProxyEndPoint endPoint,
            HttpClientStream clientStream, ConnectRequest? connectRequest, Request request, CancellationTokenSource cancellationTokenSource) : base(clientStream.Connection)
        {
            BufferPool = server.BufferPool;
            ExceptionFunc = server.ExceptionFunc;
            TimeLine["Session Created"] = DateTime.UtcNow;

            CancellationTokenSource = cancellationTokenSource;

            ClientStream = clientStream;
            HttpClient = new HttpWebClient(connectRequest, request, new Lazy<int>(() => clientStream.Connection.GetProcessId(endPoint)));
            ProxyEndPoint = endPoint;
            EnableWinAuth = server.EnableWinAuth && isWindowsAuthenticationSupported;
        }

        /// <summary>
        ///     Returns a user data for this request/response session which is
        ///     same as the user data of HttpClient.
        /// </summary>
        public object? UserData
        {
            get => HttpClient.UserData;
            set => HttpClient.UserData = value;
        }

        /// <summary>
        ///     Enable/disable Windows Authentication (NTLM/Kerberos) for the current session.
        /// </summary>
        public bool EnableWinAuth
        {
            get => enableWinAuth;
            set
            {
                if (value && !isWindowsAuthenticationSupported)
                    throw new Exception("Windows Authentication is not supported");

                enableWinAuth = value;
            }
        }

        /// <summary>
        ///     Does this session uses SSL?
        /// </summary>
        public bool IsHttps => HttpClient.Request.IsHttps;

        /// <summary>
        ///     Client Local End Point.
        /// </summary>
        public IPEndPoint ClientLocalEndPoint => (IPEndPoint)ClientConnection.LocalEndPoint;

        /// <summary>
        ///     Client Remote End Point.
        /// </summary>
        public IPEndPoint ClientRemoteEndPoint => (IPEndPoint)ClientConnection.RemoteEndPoint;

        [Obsolete("Use ClientRemoteEndPoint instead.")]
        public IPEndPoint ClientEndPoint => ClientRemoteEndPoint;

        /// <summary>
        ///    The web client used to communicate with server for this session.
        /// </summary>
        public HttpWebClient HttpClient { get; }

        [Obsolete("Use HttpClient instead.")]
        public HttpWebClient WebSession => HttpClient;

        /// <summary>
        ///     Gets or sets the custom up stream proxy.
        /// </summary>
        /// <value>
        ///     The custom up stream proxy.
        /// </value>
        public IExternalProxy? CustomUpStreamProxy { get; set; }

        /// <summary>
        ///     Are we using a custom upstream HTTP(S) proxy?
        /// </summary>
        public IExternalProxy? CustomUpStreamProxyUsed { get; internal set; }

        /// <summary>
        ///     Local endpoint via which we make the request.
        /// </summary>
        public ProxyEndPoint ProxyEndPoint { get; }

        [Obsolete("Use ProxyEndPoint instead.")]
        public ProxyEndPoint LocalEndPoint => ProxyEndPoint;

        /// <summary>
        ///     Is this a transparent endpoint?
        /// </summary>
        public bool IsTransparent => ProxyEndPoint is TransparentProxyEndPoint;

        /// <summary>
        ///     Is this a SOCKS endpoint?
        /// </summary>
        public bool IsSocks => ProxyEndPoint is SocksProxyEndPoint;

        /// <summary>
        ///     The last exception that happened.
        /// </summary>
        public Exception? Exception { get; internal set; }

        /// <summary>
        ///     Implements cleanup here.
        /// </summary>
        public virtual void Dispose()
        {
            CustomUpStreamProxyUsed = null;

            DataSent = null;
            DataReceived = null;
            Exception = null;

            HttpClient.FinishSession();
        }

        /// <summary>
        ///     Fired when data is sent within this session to server/client.
        /// </summary>
        public event EventHandler<DataEventArgs>? DataSent;

        /// <summary>
        ///     Fired when data is received within this session from client/server.
        /// </summary>
        public event EventHandler<DataEventArgs>? DataReceived;

        internal void OnDataSent(byte[] buffer, int offset, int count)
        {
            try
            {
                DataSent?.Invoke(this, new DataEventArgs(buffer, offset, count));
            }
            catch (Exception ex)
            {
                ExceptionFunc(new Exception("Exception thrown in user event", ex));
            }
        }

        internal void OnDataReceived(byte[] buffer, int offset, int count)
        {
            try
            {
                DataReceived?.Invoke(this, new DataEventArgs(buffer, offset, count));
            }
            catch (Exception ex)
            {
                ExceptionFunc(new Exception("Exception thrown in user event", ex));
            }
        }

        /// <summary>
        ///     Terminates the session abruptly by terminating client/server connections.
        /// </summary>
        public void TerminateSession()
        {
            CancellationTokenSource.Cancel();
        }
    }
}
