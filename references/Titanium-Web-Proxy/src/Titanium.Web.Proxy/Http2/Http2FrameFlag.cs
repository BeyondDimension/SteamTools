using System;

namespace Titanium.Web.Proxy.Http2
{
    [Flags]
    internal enum Http2FrameFlag : byte
    {
        Ack = 0x01,
        EndStream = 0x01,
        EndHeaders = 0x04,
        Padded = 0x08,
        Priority = 0x20,
    }
}