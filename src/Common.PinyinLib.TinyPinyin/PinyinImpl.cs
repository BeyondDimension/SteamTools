using Android.Content;
using System;
using System.Linq;
using Xamarin.Android.Bindings.TinyPinyin;

// ReSharper disable once CheckNamespace
namespace System.Application.Services.Implementation
{
    /// <summary>
    /// 使用 <see cref="PinyinHelper"/>(https://github.com/promeG/TinyPinyin) 实现的拼音功能
    /// </summary>
    internal sealed class PinyinImpl : IPinyin
    {
        string IPinyin.GetPinyin(string s, PinyinFormat format) => format switch
        {
            PinyinFormat.UpperVerticalBar => PinyinHelper.ToPinyin(s, Pinyin.SeparatorVerticalBar.ToString()) ?? string.Empty,
            PinyinFormat.AlphabetSort => PinyinHelper.ToPinyin(s, string.Empty) ?? string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

        bool IPinyin.IsChinese(char c) => PinyinHelper.IsChinese(c);
    }
}

namespace Xamarin.Android.Bindings.TinyPinyin
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