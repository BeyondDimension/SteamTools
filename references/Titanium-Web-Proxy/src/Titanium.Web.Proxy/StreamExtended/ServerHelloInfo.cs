using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Titanium.Web.Proxy.StreamExtended.Models;

namespace Titanium.Web.Proxy.StreamExtended
{
    /// <summary>
    /// Wraps up the server SSL hello information.
    /// </summary>
    public class ServerHelloInfo
    {
        private static readonly string[] compressions = {
            "null",
            "DEFLATE"
        };

        public ServerHelloInfo(int handshakeVersion, int majorVersion, int minorVersion, byte[] random,
            byte[] sessionId, int cipherSuite, int serverHelloLength)
        {
            HandshakeVersion = handshakeVersion;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            Random = random;
            SessionId = sessionId;
            CipherSuite = cipherSuite;
            ServerHelloLength = serverHelloLength;
        }

        public int HandshakeVersion { get; }

        public int MajorVersion { get; }

        public int MinorVersion { get; }

        public byte[] Random { get; }

        public DateTime Time
        {
            get
            {
                DateTime time = DateTime.MinValue;
                if (Random.Length > 3)
                {
                    time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                        .AddSeconds(((uint)Random[3] << 24) + ((uint)Random[2] << 16) + ((uint)Random[1] << 8) + (uint)Random[0]).ToLocalTime();
                }

                return time;
            }
        }

        public byte[] SessionId { get; }

        public int CipherSuite { get; }

        public byte CompressionMethod { get; set; }

        internal int ServerHelloLength { get; }

        internal int EntensionsStartPosition { get; set; }

        public Dictionary<string, SslExtension>? Extensions { get; set; }

        private static string SslVersionToString(int major, int minor)
        {
            string str = "Unknown";
            if (major == 3 && minor == 3)
                str = "TLS/1.2";
            else if (major == 3 && minor == 2)
                str = "TLS/1.1";
            else if (major == 3 && minor == 1)
                str = "TLS/1.0";
            else if (major == 3 && minor == 0)
                str = "SSL/3.0";
            else if (major == 2 && minor == 0)
                str = "SSL/2.0";

            return $"{major}.{minor} ({str})";
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"A SSLv{HandshakeVersion}-compatible ServerHello handshake was found. Titanium extracted the parameters below.");
            sb.AppendLine();
            sb.AppendLine($"Version: {SslVersionToString(MajorVersion, MinorVersion)}");
            sb.AppendLine($"Random: {string.Join(" ", Random.Select(x => x.ToString("X2")))}");
            sb.AppendLine($"\"Time\": {Time}");
            sb.AppendLine($"SessionID: {string.Join(" ", SessionId.Select(x => x.ToString("X2")))}");

            if (Extensions != null)
            {
                sb.AppendLine("Extensions:");
                foreach (var extension in Extensions.Values.OrderBy(x => x.Position))
                {
                    sb.AppendLine($"{extension.Name}: {extension.Data}");
                }
            }

            string compression = compressions.Length > CompressionMethod
                ? compressions[CompressionMethod]
                : $"unknown [0x{CompressionMethod:X2}]";
            sb.AppendLine($"Compression: {compression}");

            sb.Append("Cipher:");
            if (!SslCiphers.Ciphers.TryGetValue(CipherSuite, out string cipherStr))
            {
                cipherStr = "unknown";
            }

            sb.AppendLine($"[0x{CipherSuite:X4}] {cipherStr}");

            return sb.ToString();
        }
    }
}
