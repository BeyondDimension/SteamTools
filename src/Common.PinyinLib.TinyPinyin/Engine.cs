#if MONOANDROID || ANDROID
using Org.Ahocorasick.Trie;
using System;
using System.Collections;
using System.Collections.Generic;
using static TinyPinyin.ReflectUtils;

namespace TinyPinyin
{
    /// <summary>
    /// 字符串转拼音引擎，支持字典和 <see cref="ISegmentationSelector"/>
    /// </summary>
    partial class Engine
    {
        static readonly Lazy<Java.Util.IComparator> _EMIT_COMPARATOR = new(() => GetStaticField<Java.Util.IComparator>(nameof(EMIT_COMPARATOR))() ?? throw new ArgumentNullException(nameof(EMIT_COMPARATOR)));
        public static Java.Util.IComparator EMIT_COMPARATOR => _EMIT_COMPARATOR.Value;

        public static string[]? PinyinFromDict(string wordInDict, IList<IPinyinDict> pinyinDictSet)
        {
            if (pinyinDictSet != null)
            {
                foreach (var dict in pinyinDictSet)
                {
                    var words = dict?.Words();
                    if (words != null && words.Contains(wordInDict))
                    {
                        return dict!.ToPinyin(wordInDict);
                    }
                }
            }
            throw new Java.Lang.IllegalArgumentException($"No pinyin dict contains word: {wordInDict}");
        }

        public static IEnumerable<string> GetPinyinArray(string s, Trie trie, IList<IPinyinDict> pinyinDictList, ISegmentationSelector selector)
        {
            // https://github.com/promeG/TinyPinyin/blob/v2.0.3/lib/src/main/java/com/github/promeg/pinyinhelper/Engine.java#L44

            var selectedEmits = selector.Select(trie.ParseText(s))!;

            Java.Util.Collections.Sort((IList)selectedEmits!, EMIT_COMPARATOR);

            int nextHitIndex = 0;

            for (int i = 0; i < s.Length;)
            {
                // 首先确认是否有以第i个字符作为begin的hit
                if (nextHitIndex < selectedEmits.Count && i == selectedEmits[nextHitIndex].Start)
                {
                    // 有以第i个字符作为begin的hit
                    var fromDicts = PinyinFromDict(selectedEmits[nextHitIndex].Keyword!, pinyinDictList!)!;
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
                    yield return PinyinHelper.GetPinyin(s[i]) ?? string.Empty;
                    i++;
                }
            }
        }
    }
}
#endif
