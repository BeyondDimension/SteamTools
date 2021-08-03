namespace System.Net.WebSocket
{
    /// <summary>
    /// 从客户端发送的数据
    /// </summary>
    public readonly struct ReceivedEventArgs
    {
        /// <summary>
        /// 客户端连接
        /// </summary>
        public ClientConnection Client { get; }

        /// <summary>
        /// 数据
        /// </summary>
        public string Message { get; }

        internal ReceivedEventArgs(ClientConnection client, string message)
        {
            Client = client;
            Message = message;
        }
    }
}