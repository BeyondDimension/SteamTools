// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 获取当前默认字体
    /// </summary>
    /// <param name="fontWeight"></param>
    /// <returns></returns>
    string GetDefaultFontFamily(FontWeight fontWeight = FontWeight.Normal)
        => DefaultGetDefaultFontFamily();

    protected static string DefaultGetDefaultFontFamily()
        => SkiaSharp.SKTypeface.Default.FamilyName;
}