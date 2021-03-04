using System.Application.Properties;

namespace System.Application.Services.CloudService
{
    public static class Constants
    {
        public const string Basic = "Bearer";

        public const string DefaultUserAgent = "Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; Microsoft; Lumia 950) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/14.14263";

        public static string NetworkConnectionInterruption => SR.NetworkConnectionInterruption;

        public static class Headers
        {
            public static class Request
            {
                public const string AppVersion = "App-Version";
                public const string SecurityKey = "App-SKey";
            }
        }

        public const string HeaderAppVersion = "App-Version";

        public const string HeaderSecurityKey = "App-S-Key";

        public const string HeaderRspCiphertext = "App-Rsp-Ciphertext";

        /// <summary>
        /// 短信间隔，60秒
        /// </summary>
        public const int SMSInterval = 60;

        /// <summary>
        /// 实际短信间隔
        /// </summary>
        public const double SMSIntervalActually = SMSInterval * .95;

        public const string Prefix_HTTPS = "https://";

        public const string Prefix_HTTP = "http://";
    }
}