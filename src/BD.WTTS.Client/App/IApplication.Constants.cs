// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    /// <summary>
    /// 当前应用程序的硬编码名称，此值不可更改！
    /// <para></para>
    /// 通常用于文件，文件夹名，等不可变值。
    /// <para></para>
    /// 可更变名称的值为 <see cref="AssemblyInfo.Trademark"/>
    /// </summary>
    const string HARDCODED_APP_NAME = "Steam++";

    /// <inheritdoc cref="HARDCODED_APP_NAME"/>
    const string HARDCODED_APP_NAME_NEW = "WattToolkit";

    const string CERTIFICATE_TAG = "#Steam++";

    const string APP_LIST_FILE = "apps.json";

    const string AUTHDATA_FILE = "authenticators.dat";

    const string SCRIPT_DIR = "scripts";

    const string LOGS_DIR = "logs";

    const string CUSTOM_URL_SCHEME_NAME = "spp";

    const string CUSTOM_URL_SCHEME = $"{CUSTOM_URL_SCHEME_NAME}://";
}