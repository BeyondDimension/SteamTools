using System;
using System.Collections.Generic;
using System.Text;

namespace System.Net.WebSocket
{
    public enum SocketState
    {
        Connecting = 0,
        Open = 1,
        Closing = 2,
        Closed = 3
    }
}
