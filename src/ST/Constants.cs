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

    /// <inheritdoc cref="HARDCODED_APP_NAME"/>
    public const string HARDCODED_APP_NAME_NEW = "WattToolkit";

    public const string CERTIFICATE_TAG = "#Steam++";

    public const string APP_LIST_FILE = "apps.json";

    public const string AUTHDATA_FILE = "authenticators.dat";

    public const string SCRIPT_DIR = "scripts";

    public const string LOGS_DIR = "logs";

    public static class Urls
    {
        #region OfficialWebsite

        public const string BaseUrl_OfficialWebsite = Prefix_HTTPS + "steampp.net";

        public const string OfficialWebsite_Notice
            = BaseUrl_OfficialWebsite + "/notice?id={0}";

        public const string OfficialWebsite_UploadsPublishFiles
            = BaseUrl_OfficialWebsite + $"/uploads/publish/files/{{0}}{FileEx.BIN}";

        public const string OfficialWebsite_UploadsPublish
            = BaseUrl_OfficialWebsite + "/uploads/publish/{0}";

        #endregion

        #region API

        public const string BaseUrl_API_Production = Prefix_HTTPS + "api.steampp.net";
        public const string BaseUrl_API_Development = Prefix_HTTPS + "pan.mossimo.net:8862";
        public const string BaseUrl_API_Debug = Prefix_HTTPS + "localhost:5001";

        static string BaseUrl_API = BaseUrl_API_Development;

        public static string ApiBaseUrl
        {
            get => BaseUrl_API;

            set => BaseUrl_API = value;
        }

        public static string API_Advertisement_JumpUrl(Guid id)
            => $"{BaseUrl_API}/api/Advertisement/Jump/{id}";

        public static string API_Advertisement_ImageUrl(Guid id)
            => $"{BaseUrl_API}/api/Advertisement/Images/{id}";

        #endregion
    }
}