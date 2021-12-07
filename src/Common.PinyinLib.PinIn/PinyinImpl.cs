using System.Collections.Generic;
using System.Text;
#if MONOANDROID
using ME.Towdium.Pinin;
using PininFormat = ME.Towdium.Pinin.Utils.PinyinFormat;
#endif

// ReSharper disable once CheckNamespace
namespace System.Application.Services.Implementation
{
    /// <summary>
    /// 使用 <see cref="PinIn"/>(https://github.com/Towdium/PinIn) or (https://github.com/LasmGratel/PininSharp) 实现的拼音功能
    /// </summary>
    internal sealed class PinyinImpl : IPinyin
    {
        readonly PinIn p;

        public PinyinImpl()
        {
            p = new();
        }

        public string GetPinyin(string s, PinyinFormat format)
        {
            var pinyin = p.GetPinyin(s);
            return PininFormat.Raw.Format(pinyin);
            return String.Empty;
        }

        public bool IsChinese(char c)
        {
            throw new NotImplementedException();
        }
    }
}
