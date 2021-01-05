using Titanium.Web.Proxy.Network.Tcp;

namespace Titanium.Web.Proxy.EventArguments
{
    public class EmptyProxyEventArgs : ProxyEventArgsBase
    {
        internal EmptyProxyEventArgs(TcpClientConnection clientConnection) : base(clientConnection)
        {
        }
    }
}
