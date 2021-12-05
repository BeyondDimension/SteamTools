using System.Linq;
using TinyPinyin;

// ReSharper disable once CheckNamespace
namespace System.Application.Services.Implementation
{
    /// <summary>
    /// 使用 <see cref="PinyinHelper"/>(https://github.com/promeG/TinyPinyin) or (https://github.com/hueifeng/TinyPinyin.Net) 实现的拼音功能
    /// </summary>
    internal sealed class PinyinImpl : IPinyin
    {
        static string GetPinyin(string str, string separator)
            => PinyinHelper.GetPinyin(str, separator) ?? string.Empty;

        string IPinyin.GetPinyin(string s, PinyinFormat format) => format switch
        {
            PinyinFormat.UpperVerticalBar => GetPinyin(s, Pinyin.SeparatorVerticalBar.ToString()) ?? string.Empty,
            PinyinFormat.AlphabetSort => GetPinyin(s, string.Empty) ?? string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

        bool IPinyin.IsChinese(char c) => PinyinHelper.IsChinese(c);

        string[] IPinyin.GetPinyinArray(string s)
        {
            if (string.IsNullOrEmpty(s)) return Array.Empty<string>();
#if MONOANDROID
            var trie = PinyinHelper.TrieDict;
            var selector = PinyinHelper.Selector;
            var pinyinDictList = PinyinHelper.PinyinDicts;
            if (trie != null && selector != null && pinyinDictList != null)
            {
                return Engine.GetPinyinArray(s, trie, pinyinDictList, selector).ToArray();
            }
#endif
            // 没有提供字典或选择器，按单字符转换输出
            return s.Select(x => PinyinHelper.GetPinyin(x) ?? string.Empty).ToArray();
        }
    }
}