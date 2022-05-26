using System.Globalization;

// ReSharper disable once CheckNamespace
namespace System;

public static class CultureInfoExtensions
{
    public static bool IsMatch(this CultureInfo cultureInfo, string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureInfo.Name))
        {
            return false;
        }
        if (string.Equals(cultureInfo.Name, cultureName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else
        {
            return cultureInfo.Parent.IsMatch(cultureName);
        }
    }

    public static string GetAcceptLanguage(this CultureInfo culture)
    {
        if (culture.IsMatch("zh-Hans"))
        {
            return "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7";
        }
        else if (culture.IsMatch("zh-Hant"))
        {
            return "zh-HK,zh-TW,zh;q=0.9,en-US;q=0.8,en;q=0.7";
        }
        else if (culture.IsMatch("ko"))
        {
            return "ko;q=0.9,en-US;q=0.8,en;q=0.7";
        }
        else if (culture.IsMatch("ja"))
        {
            return "ja;q=0.9,en-US;q=0.8,en;q=0.7";
        }
        else if (culture.IsMatch("ru"))
        {
            return "ru;q=0.9,en-US;q=0.8,en;q=0.7";
        }
        else
        {
            return "en-US;q=0.9,en;q=0.8";
        }
    }

    // https://docs.microsoft.com/zh-cn/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
}