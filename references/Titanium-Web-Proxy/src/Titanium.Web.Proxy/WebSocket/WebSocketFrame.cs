using System;
using System.Text;

namespace Titanium.Web.Proxy
{
    public class WebSocketFrame
    {
        public bool IsFinal { get; internal set; }

        public WebsocketOpCode OpCode { get; internal set; }

        public ReadOnlyMemory<byte> Data { get; internal set; }

        public string GetText()
        {
            return GetText(Encoding.UTF8);
        }

        public string GetText(Encoding encoding)
        {
#if NETSTANDARD2_1
            return encoding.GetString(Data.Span);
#else
            return encoding.GetString(Data.ToArray());
#endif
        }
    }
}
