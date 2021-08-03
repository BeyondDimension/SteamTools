namespace System.Net.WebSocket
{
    /// <summary>
    /// 离线消息
    /// </summary>
    public readonly struct DisconnectedEventArgs
    {
        /// <summary>
        /// 客户端连接
        /// </summary>
        public ClientConnection Client { get; }

        /// <summary>
        /// 断开原因
        /// </summary>
        public string Reason { get; }

        internal DisconnectedEventArgs(ClientConnection client, string reason)
        {
            if (string.IsNullOrEmpty(reason))
                reason = "No reason provided";

            Client = client;
            Reason = reason;
        }
    }
}