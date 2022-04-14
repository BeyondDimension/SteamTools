using System.Linq;
using System.Collections.Generic;
using TinyPinyin;
#if MONOANDROID || ANDROID
using Org.AhoCorasick.Trie;
#endif

// ReSharper disable once CheckNamespace
namespace System.Application.Services.Implementation
{
    /// <summary>
    /// 使用 <see cref="PinyinHelper"/>(https://github.com/promeG/TinyPinyin) or (https://github.com/hueifeng/TinyPinyin.Net) 实现的拼音功能
    /// </summary>
    internal sealed partial class PinyinImpl : IPinyin
    {
        static string GetPinyin(string str, string separator)
        {
            string? value = PinyinHelper.GetPinyin(str, separator);
            return value ?? string.Empty;
        }

        string IPinyin.GetPinyin(string s, PinyinFormat format) => format switch
        {
            PinyinFormat.UpperVerticalBar => GetPinyin(s, Pinyin.SeparatorVerticalBar.ToString()) ?? string.Empty,
            PinyinFormat.AlphabetSort => GetPinyin(s, string.Empty) ?? string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

        bool IPinyin.IsChinese(char c)
        {
            return PinyinHelper.IsChinese(c);
        }

        static string? GetPinyin(char c)
        {
            return PinyinHelper.GetPinyin(c);
        }

#if MONOANDROID || ANDROID
        static IEnumerable<string> GetPinyinArray(string s, Trie trie, IList<IPinyinDict> pinyinDictList, ISegmentationSelector selector)
        {
            // https://github.com/promeG/TinyPinyin/blob/v2.0.3/lib/src/main/java/com/github/promeg/pinyinhelper/Engine.java#L44

            var selectedEmits = Engine.Select(s, trie, selector)!;

            int nextHitIndex = 0;

            for (int i = 0; i < s.Length;)
            {
                // 首先确认是否有以第i个字符作为begin的hit
                if (nextHitIndex < selectedEmits.Count && i == selectedEmits[nextHitIndex].Start)
                {
                    // 有以第i个字符作为begin的hit
                    var fromDicts = Engine.PinyinFromDict(selectedEmits[nextHitIndex].Keyword!, pinyinDictList!)!;
                    for (int j = 0; j < fromDicts.Length; j++)
                    {
                        yield return fromDicts[j].ToUpper();
                    }

                    i += selectedEmits[nextHitIndex].Size();
                    nextHitIndex++;
                }
                else
                {
                    // 将第i个字符转为拼音
                    yield return GetPinyin(s[i]) ?? string.Empty;
                    i++;
                }
            }
        }

        static PinyinHelper.Config NewConfig() => PinyinHelper.NewConfig()!;

        static void Init(PinyinHelper.Config? config) => PinyinHelper.Init(config);

        /// <summary>
        /// 使用 CnCityDict 初始化拼音库
        /// </summary>
        /// <param name="context"></param>
        public static void InitWithCnCityDict(Android.Content.Context context)
        {
            var config = NewConfig();
            var dict = new CnCityDict(context, CnCityDict.DefaultAssetFileName);
            config = config.With(dict);
            Init(config);
        }

        ///// <summary>
        ///// 初始化拼音库
        ///// </summary>
        //public static void Init()
        //{
        //    var config = NewConfig();
        //    Init(config);
        //}

        ///// <summary>
        ///// 初始化拼音库
        ///// </summary>
        ///// <param name="action"></param>
        //public static void Init(Action<PinyinHelper.Config> action)
        //{
        //    var config = NewConfig()!;
        //    action(config);
        //    Init(config);
        //}

        ///// <summary>
        ///// 使用词典初始化拼音库
        ///// </summary>
        ///// <param name="dicts"></param>
        //public static void InitWithDicts(params IPinyinDict[] dicts)
        //{
        //    var config = NewConfig()!;
        //    if (dicts.Any())
        //    {
        //        foreach (var dict in dicts)
        //        {
        //            config.With(dict);
        //        }
        //    }
        //    Init(config);
        //}
#endif

        string[] IPinyin.GetPinyinArray(string s)
        {
            if (string.IsNullOrEmpty(s)) return Array.Empty<string>();
#if MONOANDROID || ANDROID
            var trie = PinyinHelper.TrieDict;
            var selector = PinyinHelper.Selector;
            var pinyinDictList = PinyinHelper.PinyinDicts;
            if (trie != null && selector != null && pinyinDictList != null)
            {
                return GetPinyinArray(s, trie, pinyinDictList, selector).ToArray();
            }
#endif
            // 没有提供字典或选择器，按单字符转换输出
            return s.Select(x => GetPinyin(x) ?? string.Empty).ToArray();
        }
    }
}