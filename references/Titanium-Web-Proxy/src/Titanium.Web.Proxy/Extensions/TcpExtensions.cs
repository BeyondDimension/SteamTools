using System.Net.Sockets;

namespace Titanium.Web.Proxy.Extensions
{
    internal static class TcpExtensions
    {
        /// <summary>
        ///     Check if a TcpClient is good to be used.
        ///     This only checks if send is working so local socket is still connected.
        ///     Receive can only be verified by doing a valid read from server without exceptions.
        ///     So in our case we should retry with new connection from pool if first read after getting the connection fails.
        ///     https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.connected(v=vs.110).aspx
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        internal static bool IsGoodConnection(this Socket socket)
        {
            if (!socket.Connected)
            {
                return false;
            }

            // This is how you can determine whether a socket is still connected.
            bool blockingState = socket.Blocking;
            try
            {
                var tmp = new byte[1];

                socket.Blocking = false;
                socket.Send(tmp, 0, 0);
                // Connected.
            }
            catch
            {
                // Should we let 10035 == WSAEWOULDBLOCK as valid connection?
                return false;
            }
            finally
            {
                socket.Blocking = blockingState;
            }

            return true;
        }
    }
}
