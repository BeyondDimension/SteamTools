using Microsoft.International.Converters.PinYinConverter;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Application.Services.Implementation
{
    /// <summary>
    /// 使用 Microsoft Visual Studio International Pack 1.0 中的 Simplified Chinese Pin-Yin Conversion Library（简体中文拼音转换类库）实现的拼音功能
    /// </summary>
    internal sealed class PinyinImpl : IPinyin
    {
        bool IPinyin.IsChinese(char c)
        {
            return ChineseChar.IsValidChar(c);
        }

        static IEnumerable<string> GetPinyins(string s)
        {
            foreach (var c in s)
            {
                string? r = null;
                try
                {
                    var cc = new ChineseChar(c);
                    var py = cc.Pinyins.FirstOrDefault();
                    if (py != null)
                    {
                        if (py.Length > 1)
                        {
                            r = py[0..^1];
                        }
                        else
                        {
                            r = py;
                        }
                    }
                }
                catch
                {
                }
                yield return r ?? c.ToString();
            }
        }

        string IPinyin.GetPinyin(string s, PinyinFormat format)
        {
            var r = GetPinyins(s);
            return format switch
            {
                PinyinFormat.UpperVerticalBar => string.Join(Pinyin.SeparatorVerticalBar, r),
                PinyinFormat.AlphabetSort => string.Join(null, r),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            };
        }

        string[] IPinyin.GetPinyinArray(string s) => GetPinyins(s).ToArray();
    }
}