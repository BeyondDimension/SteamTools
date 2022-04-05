#if MONOANDROID || ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Org.Ahocorasick.Trie;
using static TinyPinyin.ReflectUtils;

namespace TinyPinyin
{
    partial class PinyinHelper
    {
        static readonly Lazy<Func<Trie?>> mTrieDict = new(() => GetStaticField<Trie>(nameof(mTrieDict)));
        public static Trie? TrieDict => mTrieDict.Value();

        static readonly Lazy<Func<ISegmentationSelector?>> mSelector = new(() => GetStaticField<ISegmentationSelector>(nameof(mSelector)));
        public static ISegmentationSelector? Selector => mSelector.Value();

        static readonly Lazy<Func<IList<IPinyinDict>?>> mPinyinDicts = new(() => GetStaticField<IList<IPinyinDict>>(nameof(mPinyinDicts)));
        public static IList<IPinyinDict>? PinyinDicts => mPinyinDicts.Value();

        /// <summary>
        /// 使用 <see cref="CnCityDict"/> 初始化拼音库
        /// </summary>
        /// <param name="context"></param>
        public static void InitWithCnCityDict(Context context)
        {
            var dict = CnCityDict.GetInstance(context);
            var config = NewConfig()!.With(dict);
            Init(config);
        }

        /// <summary>
        /// 初始化拼音库
        /// </summary>
        public static void Init()
        {
            var config = NewConfig();
            Init(config);
        }

        /// <summary>
        /// 初始化拼音库
        /// </summary>
        /// <param name="action"></param>
        public static void Init(Action<Config> action)
        {
            var config = NewConfig()!;
            action(config);
            Init(config);
        }

        /// <summary>
        /// 使用词典初始化拼音库
        /// </summary>
        /// <param name="dicts"></param>
        public static void InitWithDicts(params IPinyinDict[] dicts)
        {
            var config = NewConfig()!;
            if (dicts.Any())
            {
                foreach (var dict in dicts)
                {
                    config.With(dict);
                }
            }
            Init(config);
        }
    }
}
#endif