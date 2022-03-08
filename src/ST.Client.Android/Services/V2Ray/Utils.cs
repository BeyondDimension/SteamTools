using Kotlin.Text;
using Android.Content;

namespace System.Application.Services.V2Ray
{
    static class V2RayUtils
    {
        public static string[] VpnDnsServers
        {
            get
            {
                return new[] { AppConfig.DNS_AGENT };
            }
        }

        public static bool IsPureIpAddress(string value) => IsIpv4Address(value) || IsIpv6Address(value);

        public static bool IsIpv4Address(string value)
        {
            var regV4 = new Regex("^([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\\.([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\\.([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\\.([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])$");
            return regV4.Matches(value);
        }

        public static bool IsIpv6Address(string value)
        {
            if (value.IndexOf('[') == 0 && value.LastIndexOf(']') > 0)
            {
                value = StringsKt.Drop(value, 1);
                value = StringsKt.DropLast(value, value.Length - value.LastIndexOf(']'));
            }
            var regV6 = new Regex("^((?:[0-9A-Fa-f]{1,4}))?((?::[0-9A-Fa-f]{1,4}))*::((?:[0-9A-Fa-f]{1,4}))?((?::[0-9A-Fa-f]{1,4}))*|((?:[0-9A-Fa-f]{1,4}))((?::[0-9A-Fa-f]{1,4})){7}$");
            return regV6.Matches(value);
        }

        public static string PackagePath(Context context)
        {
            var path = context.FilesDir!.ToString();
            path = path.Replace("files", "");
            //path += "tun2socks"
            return path;
        }
    }
}
