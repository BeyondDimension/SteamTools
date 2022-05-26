using CoreFoundation;

// ReSharper disable once CheckNamespace
namespace System.Application.Services.Implementation;

/// <summary>
/// 使用 <see cref="CFStringTransform"/> 实现的拼音功能
/// </summary>
internal sealed class PinyinImpl : IPinyin
{
    public static bool TransformMandarinLatin(string s, out CFMutableString str)
    {
        str = new CFMutableString(s);
        return str.Transform(CFStringTransform.MandarinLatin, false);
    }

    public static bool GetPinyin(string s, out CFMutableString str)
        => TransformMandarinLatin(s, out str) &&
        str.Transform(CFStringTransform.StripDiacritics, false);

    string IPinyin.GetPinyin(string s, PinyinFormat format)
    {
        if (GetPinyin(s, out var str))
        {
            s = str!;
            s = format switch
            {
                PinyinFormat.UpperVerticalBar => s.Replace(' ', Pinyin.SeparatorVerticalBar).ToUpper(),
                PinyinFormat.AlphabetSort => s,
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            };
        }
        return s;
    }

    string[] IPinyin.GetPinyinArray(string s)
    {
        if (GetPinyin(s, out var str))
        {
            s = str!;
            return s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
        return Array.Empty<string>();
    }

    bool IPinyin.IsChinese(char c) => TransformMandarinLatin(c.ToString(), out var _);
}