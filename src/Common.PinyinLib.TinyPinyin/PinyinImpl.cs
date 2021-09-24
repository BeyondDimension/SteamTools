using System;
using System.Linq;
using TinyPinyin;
#if MONOANDROID
using Android.Content;
#endif

// ReSharper disable once CheckNamespace
namespace System.Application.Services.Implementation
{
    /// <summary>
    /// 使用 <see cref="PinyinHelper"/>(https://github.com/promeG/TinyPinyin) or (https://github.com/hueifeng/TinyPinyin.Net) 实现的拼音功能
    /// </summary>
    internal sealed class PinyinImpl : IPinyin
    {
        static string GetPinyin(string str, string separator)
            => PinyinHelper.
#if MONOANDROID
            ToPinyin
#else
            GetPinyin
#endif
            (str, separator) ?? string.Empty;

        string IPinyin.GetPinyin(string s, PinyinFormat format) => format switch
        {
            PinyinFormat.UpperVerticalBar => GetPinyin(s, Pinyin.SeparatorVerticalBar.ToString()) ?? string.Empty,
            PinyinFormat.AlphabetSort => GetPinyin(s, string.Empty) ?? string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

        bool IPinyin.IsChinese(char c) => PinyinHelper.IsChinese(c);
    }
}

#if MONOANDROID
namespace TinyPinyin
{
    partial class PinyinHelper
    {
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