using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Titanium.Web.Proxy.Helpers
{
    internal class NetworkHelper
    {
        private static readonly string localhostName = Dns.GetHostName();
        private static readonly IPHostEntry localhostEntry = Dns.GetHostEntry(string.Empty);

        /// <summary>
        ///     Adapated from below link
        ///     http://stackoverflow.com/questions/11834091/how-to-check-if-localhost
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        internal static bool IsLocalIpAddress(IPAddress address)
        {
            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            // test if host IP equals any local IP
            return localhostEntry.AddressList.Contains(address);
        }

        internal static bool IsLocalIpAddress(string hostName)
        {
            if (IPAddress.TryParse(hostName, out var ipAddress)
                && IsLocalIpAddress(ipAddress))
            {
                return true;
            }

            if (hostName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // if hostname matches local host name
            if (hostName.Equals(localhostName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // if hostname matches fully qualified local DNS name
            if (hostName.Equals(localhostEntry.HostName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            try
            {
                // do reverse DNS lookup even if hostName is an IP address
                var hostEntry = Dns.GetHostEntry(hostName);
                // if DNS resolved hostname matches local DNS name,
                // or if host IP address list contains any local IP address
                if (hostEntry.HostName.Equals(localhostEntry.HostName, StringComparison.OrdinalIgnoreCase)
                    || hostEntry.AddressList.Any(hostIP => localhostEntry.AddressList.Contains(hostIP)))
                {
                    return true;
                }
            }
            catch (SocketException)
            {
            }

            return false;
        }
    }
}
