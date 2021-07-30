using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.WebSocket.EventArgs;

namespace System.WebSocket
{
    public class WebSocketServer : IDisposable
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

        public WebSocketServer(string host, int port)
        {
            if (!IPAddress.TryParse(host, out ip))
                throw new ArgumentException("Invalid IP address");

            listener = new(ip, port);
        } 
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns>Task</returns>
        public async Task StartAsync()
        {
            listener.Start();
            RunState = true;
            while (RunState)
            {
                TcpClient tcp = await listener.AcceptTcpClientAsync();
                _ = Task.Run(async () =>
                {
                    ClientConnection client = new(tcp)
                    {
                        OnConnected = OnClientConnected,
                        OnReceived = OnClientReceived,
                        OnDisconnected = OnClientDisconnected
                    };

                    await client.StartAsync();
                });
            }
        }

        public void Dispose()
        {
            listener.Stop();
            RunState = false;
            RunState = false;
        }
    }
}
