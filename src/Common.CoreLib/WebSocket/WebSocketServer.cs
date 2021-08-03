using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSocket.EventArgs;
using System.Threading;
using System.Net.NetworkInformation;
using System.Linq;
namespace System.Net.WebSocket
{
    public class ServerWebSocket : IDisposable
    {
        private readonly TcpListener listener;
        private readonly IPAddress ip;
        public bool RunState = false;
        /// <summary>
        ///  连接
        /// </summary>
        public event Func<ConnectedEventArgs, Task> OnClientConnected;

        /// <summary>
        ///  消息
        /// </summary>
        public event Func<ReceivedEventArgs, Task> OnClientReceived;

        /// <summary>
        ///  断开
        /// </summary>
        public event Func<DisconnectedEventArgs, Task> OnClientDisconnected;
        internal static bool IsPortOccupedFun(int port)
        {
            IPGlobalProperties iproperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = iproperties.GetActiveTcpListeners();
            return ipEndPoints.Any(x => x.Port == port);
        }
        public ServerWebSocket(string host, out int port)
        {
            if (!IPAddress.TryParse(host, out ip))
                throw new ArgumentException("Invalid IP address");
            var random = new Random();
            port = random.Next(10000, 25564);
            while (IsPortOccupedFun(port))
            {
                port = random.Next(10000, 25564);
            }
            listener = new(ip, port);
        }
        public ServerWebSocket(string host, int port)
        {
            if (!IPAddress.TryParse(host, out ip))
                throw new ArgumentException("Invalid IP address");

            listener = new(ip, port);
        }
        public CancellationTokenSource tokenSource;

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns>Task</returns>
        public void Start()
        {
            tokenSource = new CancellationTokenSource();
            _ = Task.Run(async () =>
              {
                  await StartAsync();
              }, tokenSource.Token);
        }
        public async Task StartAsync()
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

        public void Dispose()
        {
            RunState = false;
            tokenSource.Cancel();
        }
    }
}
