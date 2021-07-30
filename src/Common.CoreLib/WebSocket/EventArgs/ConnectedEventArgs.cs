using System;
using System.Collections.Generic;
using System.Text;

namespace System.Net.WebSocket.EventArgs
{
  public  class ConnectedEventArgs
    {
        public ClientConnection Client { get; }
        internal ConnectedEventArgs(ClientConnection client)
        {
            Client = client;
        }
    }
}
