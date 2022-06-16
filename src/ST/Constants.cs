using static System.Common.Constants;

namespace System.Application;

public static class Constants
{
    /// <summary>
    /// 当前应用程序的硬编码名称，此值不可更改！
    /// <para></para>
    /// 通常用于文件，文件夹名，等不可变值。
    /// <para></para>
    /// 可更变名称的值为 <see cref="ThisAssembly.AssemblyTrademark"/>
    /// </summary>
    public const string HARDCODED_APP_NAME = "Steam++";

    public const string CERTIFICATE_TAG = "#Steam++";

    public const string APP_LIST_FILE = "apps.json";

    public const string AUTHDATA_FILE = "authenticators.dat";

    public const string SCRIPT_DIR = "scripts";

    public const string LOGS_DIR = "logs";

    public static class Urls
    {
        public const string BaseUrl_OfficialWebsite = Prefix_HTTPS + "steampp.net";
        public const string BaseUrl_API = Prefix_HTTPS + "api.steampp.net";

        public const string OfficialWebsite_Notice = BaseUrl_OfficialWebsite + "/notice?id={0}";
        public const string API_Advertisement_Jump = BaseUrl_API + "/api/Advertisement/Jump/{0}";
        public const string API_Advertisement_Image = BaseUrl_API + "/api/Advertisement/Images/{0}";
    }
}