using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.WebSocket.EventArgs;
using System.Threading;

namespace System.Net.WebSocket
{
    public sealed class ClientConnection : IDisposable
    {
        private static int entities = 0;

        private readonly TcpClient tcp;
        private readonly NetworkStream stream;

        internal Func<ConnectedEventArgs, Task> OnConnected;
        internal Func<ReceivedEventArgs, Task> OnReceived;
        internal Func<DisconnectedEventArgs, Task> OnDisconnected;

        /// <summary>
        /// 连接ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///  状态
        /// </summary>
        public SocketState State { get; private set; } = SocketState.Connecting;

        public EndPoint Local { get; }
        public EndPoint Remote { get; }

        internal ClientConnection(TcpClient tcp)
        {
            this.tcp = tcp;
            stream = this.tcp.GetStream();

            Local = this.tcp.Client.LocalEndPoint;
            Remote = this.tcp.Client.RemoteEndPoint;

            Id = ++entities;
        }

        /// <summary>
        ///  启动连接且尝试握手
        /// </summary>
        /// <returns>Task</returns>
        internal async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!await Utility.TryHandshakeAsync(tcp))
                {
                    State = SocketState.Closing;
                    throw new SocketException(10053);
                }

                State = SocketState.Open;
                if (OnConnected != null)
                    await OnConnected!.Invoke(new ConnectedEventArgs(this));
                await Task.Run(() => ReadAsync(), cancellationToken); 
            }
            catch (SocketException e)
            {
                await CloseAsync(e);
            }
        }

        /// <summary>
        ///   关闭客户端连接
        /// </summary>
        /// <param name="reason">关闭连接的原因</param>
        /// <returns>Task</returns>
        public async Task CloseAsync(string reason = null)
        {
            State = SocketState.Closing;
            await SendPayload(Utility.BuildCloseFrame());
            State = SocketState.Closed;
            if (OnDisconnected != null)
                await OnDisconnected!.Invoke(new DisconnectedEventArgs(this, reason));
            Dispose();
        }

        /// <summary>
        ///  关闭带有异常的客户端连接
        /// </summary>
        /// <param name="e">异常</param>
        /// <returns>Task</returns>
        internal async Task CloseAsync(Exception e)
        {
            State = SocketState.Closing;
            await SendPayload(Utility.BuildCloseFrame());
            State = SocketState.Closed;
            if (OnDisconnected != null)
                await OnDisconnected!.Invoke(new DisconnectedEventArgs(this, e.ToString()));
        }

        private async Task ReadAsync()
        {
            while (State != (SocketState.Closed | SocketState.Closing))
            {
                if (!stream.DataAvailable)
                    continue;

                byte[] bytes = new byte[tcp.Available];
                await stream.ReadAsync(bytes.AsMemory(0, bytes.Length));
                if (bytes.Length <= 0)
                    continue;

                if (!Utility.TryDecodeMessage(bytes, out string message))
                {
                    if (message == "Connection closed")
                        await CloseAsync(message);
                    return;
                }

                if (OnReceived != null)
                    await OnReceived!.Invoke(new ReceivedEventArgs(this, message));
            }
        }

        public async Task WriteAsync(object data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                byte[] bytes = Utility.EncodeMessage(json);
                await stream.WriteAsync(bytes.AsMemory(0, bytes.Length));
                await stream.FlushAsync();
            }
            catch { throw; }
        }

        /// <summary>
        ///     Writes a string to the socket stream
        /// </summary>
        /// <param name="text">The string to write</param>
        /// <returns>Task</returns>
        public async Task WriteAsync(string text)
        {
            try
            {
                byte[] bytes = Utility.EncodeMessage(text);
                await stream.WriteAsync(bytes.AsMemory(0, bytes.Length));
                await stream.FlushAsync();
            }
            catch { throw; }
        }

        /// <summary>
        ///     Sends a payload to the client
        /// </summary>
        /// <param name="bytes">The payload</param>
        /// <returns>Task</returns>
        internal async Task SendPayload(byte[] bytes)
        {
            await stream.WriteAsync(bytes.AsMemory(0, bytes.Length));
            await stream.FlushAsync();
        }

        public void Dispose()
        {
            tcp.Close();
            tcp.Dispose();
        }
    }
}
