using System.Collections.Generic;
using System.Linq;
using System.Text;
using Titanium.Web.Proxy.StreamExtended.Models;

namespace Titanium.Web.Proxy.StreamExtended
{
    internal class SslExtensions
    {
        internal static SslExtension GetExtension(int value, byte[] data, int position)
        {
            string name = GetExtensionName(value);
            string dataStr = GetExtensionData(value, data);
            return new SslExtension(value, name, dataStr, position);
        }

        private static string GetExtensionData(int value, byte[] data)
        {
            // https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml
            switch (value)
            {
                case 0:
                    var stringBuilder = new StringBuilder();
                    int index = 2;
                    while (index < data.Length)
                    {
                        int nameType = data[index];
                        int count = (data[index + 1] << 8) + data[index + 2];
                        string str = Encoding.ASCII.GetString(data, index + 3, count);
                        if (nameType == 0)
                        {
                            stringBuilder.AppendFormat("{0}{1}", stringBuilder.Length > 1 ? "; " : string.Empty, str);
                        }

                        index += 3 + count;
                    }
                    return stringBuilder.ToString();
                case 5:
                    if (data.Length == 5 && data[0] == 1 && data[1] == 0 && data[2] == 0 && data[3] == 0 && data[4] == 0)
                    {
                        return "OCSP - Implicit Responder";
                    }

                    return ByteArrayToString(data);
                case 10:
                    return GetSupportedGroup(data);
                case 11:
                    return GetEcPointFormats(data);
                case 13:
                    return GetSignatureAlgorithms(data);
                case 16:
                    return GetApplicationLayerProtocolNegotiation(data);
                case 35655:
                    return $"{data.Length} bytes";
                default:
                    return ByteArrayToString(data);
            }
        }

        private static string GetSupportedGroup(byte[] data)
        {
            // https://datatracker.ietf.org/doc/draft-ietf-tls-rfc4492bis/?include_text=1
            var list = new List<string>();
            if (data.Length < 2)
            {
                return string.Empty;
            }

            int i = 2;
            while (i < data.Length - 1)
            {
                int namedCurve = (data[i] << 8) + data[i + 1];
                switch (namedCurve)
                {
                    case 1:
                        list.Add("sect163k1 [0x1]"); // deprecated
                        break;
                    case 2:
                        list.Add("sect163r1 [0x2]"); // deprecated
                        break;
                    case 3:
                        list.Add("sect163r2 [0x3]"); // deprecated
                        break;
                    case 4:
                        list.Add("sect193r1 [0x4]"); // deprecated
                        break;
                    case 5:
                        list.Add("sect193r2 [0x5]"); // deprecated
                        break;
                    case 6:
                        list.Add("sect233k1 [0x6]"); // deprecated
                        break;
                    case 7:
                        list.Add("sect233r1 [0x7]"); // deprecated
                        break;
                    case 8:
                        list.Add("sect239k1 [0x8]"); // deprecated
                        break;
                    case 9:
                        list.Add("sect283k1 [0x9]"); // deprecated
                        break;
                    case 10:
                        list.Add("sect283r1 [0xA]"); // deprecated
                        break;
                    case 11:
                        list.Add("sect409k1 [0xB]"); // deprecated
                        break;
                    case 12:
                        list.Add("sect409r1 [0xC]"); // deprecated
                        break;
                    case 13:
                        list.Add("sect571k1 [0xD]"); // deprecated
                        break;
                    case 14:
                        list.Add("sect571r1 [0xE]"); // deprecated
                        break;
                    case 15:
                        list.Add("secp160k1 [0xF]"); // deprecated
                        break;
                    case 16:
                        list.Add("secp160r1 [0x10]"); // deprecated
                        break;
                    case 17:
                        list.Add("secp160r2 [0x11]"); // deprecated
                        break;
                    case 18:
                        list.Add("secp192k1 [0x12]"); // deprecated
                        break;
                    case 19:
                        list.Add("secp192r1 [0x13]"); // deprecated
                        break;
                    case 20:
                        list.Add("secp224k1 [0x14]"); // deprecated
                        break;
                    case 21:
                        list.Add("secp224r1 [0x15]"); // deprecated
                        break;
                    case 22:
                        list.Add("secp256k1 [0x16]"); // deprecated
                        break;
                    case 23:
                        list.Add("secp256r1 [0x17]");
                        break;
                    case 24:
                        list.Add("secp384r1 [0x18]");
                        break;
                    case 25:
                        list.Add("secp521r1 [0x19]");
                        break;
                    case 26:
                        list.Add("brainpoolP256r1 [0x1A]");
                        break;
                    case 27:
                        list.Add("brainpoolP384r1 [0x1B]");
                        break;
                    case 28:
                        list.Add("brainpoolP512r1 [0x1C]");
                        break;
                    case 29:
                        list.Add("x25519 [0x1D]");
                        break;
                    case 30:
                        list.Add("x448 [0x1E]");
                        break;
                    case 256:
                        list.Add("ffdhe2048	[0x0100]");
                        break;
                    case 257:
                        list.Add("ffdhe3072 [0x0101]");
                        break;
                    case 258:
                        list.Add("ffdhe4096 [0x0102]");
                        break;
                    case 259:
                        list.Add("ffdhe6144 [0x0103]");
                        break;
                    case 260:
                        list.Add("ffdhe8192 [0x0104]");
                        break;
                    case 65281:
                        list.Add("arbitrary_explicit_prime_curves [0xFF01]"); // deprecated
                        break;
                    case 65282:
                        list.Add("arbitrary_explicit_char2_curves [0xFF02]"); // deprecated
                        break;
                    default:
                        list.Add($"unknown [0x{namedCurve:X4}]");
                        break;
                }

                i += 2;
            }

            return string.Join(", ", list.ToArray());
        }

        private static string GetEcPointFormats(byte[] data)
        {
            List<string> list = new List<string>();
            if (data.Length < 1)
            {
                return string.Empty;
            }

            int i = 1;
            while (i < data.Length)
            {
                switch (data[i])
                {
                    case 0:
                        list.Add("uncompressed [0x0]");
                        break;
                    case 1:
                        list.Add("ansiX962_compressed_prime [0x1]");
                        break;
                    case 2:
                        list.Add("ansiX962_compressed_char2 [0x2]");
                        break;
                    default:
                        list.Add($"unknown [0x{data[i]:X2}]");
                        break;
                }

                i += 2;
            }

            return string.Join(", ", list.ToArray());
        }

        private static string GetSignatureAlgorithms(byte[] data)
        {
            // https://www.iana.org/assignments/tls-parameters/tls-parameters.xhtml
            int num = (data[0] << 8) + data[1];
            var sb = new StringBuilder();
            int index = 2;
            while (index < num + 2)
            {
                switch (data[index])
                {
                    case 0:
                        sb.Append("none");
                        break;
                    case 1:
                        sb.Append("md5");
                        break;
                    case 2:
                        sb.Append("sha1");
                        break;
                    case 3:
                        sb.Append("sha224");
                        break;
                    case 4:
                        sb.Append("sha256");
                        break;
                    case 5:
                        sb.Append("sha384");
                        break;
                    case 6:
                        sb.Append("sha512");
                        break;
                    case 8:
                        sb.Append("Intrinsic");
                        break;
                    default:
                        sb.AppendFormat("Unknown[0x{0:X2}]", data[index]);
                        break;
                }

                sb.AppendFormat("_");
                switch (data[index + 1])
                {
                    case 0:
                        sb.Append("anonymous");
                        break;
                    case 1:
                        sb.Append("rsa");
                        break;
                    case 2:
                        sb.Append("dsa");
                        break;
                    case 3:
                        sb.Append("ecdsa");
                        break;
                    case 7:
                        sb.Append("ed25519");
                        break;
                    case 8:
                        sb.Append("ed448");
                        break;
                    default:
                        sb.AppendFormat("Unknown[0x{0:X2}]", data[index + 1]);
                        break;
                }

                sb.AppendFormat(", ");
                index += 2;
            }

            if (sb.Length > 1)
                sb.Length -= 2;

            return sb.ToString();
        }

        private static string GetApplicationLayerProtocolNegotiation(byte[] data)
        {
            List<string> stringList = new List<string>();
            int index = 2;
            while (index < data.Length)
            {
                int count = data[index];
                stringList.Add(Encoding.ASCII.GetString(data, index + 1, count));
                index += 1 + count;
            }

            return string.Join(", ", stringList.ToArray());
        }

        private static string GetExtensionName(int value)
        {
            // https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml
            switch (value)
            {
                case 0:
                    return "server_name";
                case 1:
                    return "max_fragment_length";
                case 2:
                    return "client_certificate_url";
                case 3:
                    return "trusted_ca_keys";
                case 4:
                    return "truncated_hmac";
                case 5:
                    return "status_request";
                case 6:
                    return "user_mapping";
                case 7:
                    return "client_authz";
                case 8:
                    return "server_authz";
                case 9:
                    return "cert_type";
                case 10:
                    return "supported_groups"; // renamed from "elliptic_curves" (RFC 7919 / TLS 1.3)
                case 11:
                    return "ec_point_formats";
                case 12:
                    return "srp";
                case 13:
                    return "signature_algorithms";
                case 14:
                    return "use_srtp";
                case 15:
                    return "heartbeat";
                case 16:
                    return "ALPN"; // application_layer_protocol_negotiation
                case 17:
                    return "status_request_v2";
                case 18:
                    return "signed_certificate_timestamp";
                case 19:
                    return "client_certificate_type";
                case 20:
                    return "server_certificate_type";
                case 21:
                    return "padding";
                case 22:
                    return "encrypt_then_mac";
                case 23:
                    return "extended_master_secret";
                case 24:
                    return "token_binding"; // TEMPORARY - registered 2016-02-04, extension registered 2017-01-12, expires 2018-02-04
                case 25:
                    return "cached_info";
                case 26:
                    return "quic_transports_parameters"; // Not yet assigned by IANA (QUIC-TLS Draft04)
                case 35:
                    return "SessionTicket TLS";
                // TLS 1.3 draft: https://tools.ietf.org/html/draft-ietf-tls-tls13
                case 40:
                    return "key_share";
                case 41:
                    return "pre_shared_key";
                case 42:
                    return "early_data";
                case 43:
                    return "supported_versions";
                case 44:
                    return "cookie";
                case 45:
                    return "psk_key_exchange_modes";
                case 46:
                    return "ticket_early_data_info";
                case 47:
                    return "certificate_authorities";
                case 48:
                    return "oid_filters";
                case 49:
                    return "post_handshake_auth";
                case 2570: // 0a0a
                case 6682: // 1a1a
                case 10794: // 2a2a
                case 14906: // 3a3a
                case 19018: // 4a4a
                case 23130: // 5a5a
                case 27242: // 6a6a
                case 31354: // 7a7a
                case 35466: // 8a8a
                case 39578: // 9a9a
                case 43690: // aaaa
                case 47802: // baba
                case 51914: // caca
                case 56026: // dada
                case 60138: // eaea
                case 64250: // fafa
                    return "Reserved (GREASE)";
                case 13172:
                    return "next_protocol_negotiation";
                case 30031:
                    return "channel_id_old"; // Google
                case 30032:
                    return "channel_id"; // Google
                case 35655:
                    return "draft-agl-tls-padding";
                case 65281:
                    return "renegotiation_info";
                case 65282:
                    return "Draft version of TLS 1.3"; // for experimentation only  https://www.ietf.org/mail-archive/web/tls/current/msg20853.html
                default:
                    return $"unknown_{value:x2}";
            }
        }

        private static string ByteArrayToString(byte[] data)
        {
            return string.Join(" ", data.Select(x => x.ToString("X2")));
        }
    }
}
