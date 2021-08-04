using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.WebSocket
{
    public class ServerWebSocket : IDisposable
    {
        private readonly TcpListener listener;

        public bool RunState { get; private set; }

        /// <summary>
        /// 连接
        /// </summary>
        public event Func<ConnectedEventArgs, Task>? OnClientConnected;

        /// <summary>
        /// 消息
        /// </summary>
        public event Func<ReceivedEventArgs, Task>? OnClientReceived;

        /// <summary>
        ///  断开
        /// </summary>
        public event Func<DisconnectedEventArgs, Task>? OnClientDisconnected;

        internal static bool IsPortOccupedFun(int port)
        {
            IPGlobalProperties iproperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = iproperties.GetActiveTcpListeners();
            return ipEndPoints.Any(x => x.Port == port);
        }

        public ServerWebSocket(IPAddress ip, out int port)
        {
            var random = new Random();
            port = random.Next(10000, 25564);
            while (IsPortOccupedFun(port))
            {
                port = random.Next(10000, 25564);
            }
            listener = new(ip, port);
        }

        public ServerWebSocket(IPAddress ip, int port)
        {
            listener = new(ip, port);
        }

        readonly CancellationTokenSource tokenSource = new();

        bool isStarted;
        public void Start()
        {
            if (isStarted) return;
            isStarted = true;
            Task.Factory.StartNew(StartCore,
                tokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        async void StartCore()
        {
            listener.Start();
            RunState = true;
            try
            {
                while (RunState)
                {
                    TcpClient tcp = await Task.Run(() => listener.AcceptTcpClientAsync(), tokenSource.Token);// await listener.AcceptTcpClientAsync();
                    _ = Task.Run(async () =>
                    {
                        ClientConnection client = new(tcp)
                        {
                            OnConnected = OnClientConnected,
                            OnReceived = OnClientReceived,
                            OnDisconnected = OnClientDisconnected
                        };
                        tokenSource.Token.Register(() => { client.Dispose(); });
                        await client.StartAsync();
                    }, tokenSource.Token);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    tokenSource.Cancel();
                    listener.Stop();
                    RunState = false;
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}