using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace System.Net
{
    public static class SocketHelper
    {
        /// <summary>
        /// 获取一个随机的未使用的端口
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static int GetRandomUnusedPort(IPAddress address)
        {
            var listener = new TcpListener(address, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
