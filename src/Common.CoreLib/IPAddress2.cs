using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace System
{
    public static class IPAddress2
    {
        public static bool TryParse(
            [NotNullWhen(true)] string? ipString,
            [NotNullWhen(true)] out IPAddress? address)
        {
            if (ipString == "127.0.0.1" ||
                string.Equals(ipString, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                address = IPAddress.Loopback;
                return true;
            }
            else
            {
                return IPAddress.TryParse(ipString, out address);
            }
        }

        public static IPAddress Parse(string ipString)
        {
            if (ipString == null)
                throw new ArgumentNullException(nameof(ipString));
            if (!TryParse(ipString, out var ip))
                throw new FormatException("ipString is not a valid IP address.");
            return ip;
        }
    }
}