using System;
using Titanium.Web.Proxy.Network.Tcp;

namespace Titanium.Web.Proxy.EventArguments
{
    /// <summary>
    ///     The base event arguments
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public abstract class ProxyEventArgsBase : EventArgs
    {
        private readonly TcpClientConnection clientConnection;

        public object ClientUserData
        {
            get => clientConnection.ClientUserData;
            set => clientConnection.ClientUserData = value;
        }

        internal ProxyEventArgsBase(TcpClientConnection clientConnection)
        {
            this.clientConnection = clientConnection;
        }
    }
}
