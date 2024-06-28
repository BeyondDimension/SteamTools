// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 获取当前默认字体
    /// </summary>
    /// <param name="fontWeight"></param>
    /// <returns></returns>
    [Mobius(
"""
App.GetDefaultFontFamily
""")]
    string GetDefaultFontFamily(FontWeight fontWeight = FontWeight.Normal)
        => DefaultGetDefaultFontFamily();

    [Mobius(
"""
App.GetDefaultFontFamily
""")]
    protected static string DefaultGetDefaultFontFamily()
        => SkiaSharp.SKTypeface.Default.FamilyName;
}