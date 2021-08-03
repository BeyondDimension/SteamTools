namespace System.Net.WebSocket
{
    public class ConnectedEventArgs
    {
        public ClientConnection Client { get; }

        internal ConnectedEventArgs(ClientConnection client)
        {
            Client = client;
        }
    }
}