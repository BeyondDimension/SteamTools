// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    string IPlatformService.GetDefaultFontFamily(FontWeight fontWeight)
    {
        // https://support.apple.com/zh-cn/HT213266

        var culture = ResourceService.Culture;
        if (culture.IsMatch(AssemblyInfo.CultureName_SimplifiedChinese))
        {
            return "PingFang SC";
        }
        else if (culture.IsMatch("zh-HK"))
        {
            return "PingFang HK";
        }
        else if (culture.IsMatch(AssemblyInfo.CultureName_TraditionalChinese))
        {
            return "PingFang TC";
        }
        return IPlatformService.DefaultGetDefaultFontFamily();
    }
}