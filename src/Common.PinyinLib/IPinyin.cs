// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    /// <summary>
    /// 汉语拼音
    /// </summary>
    public interface IPinyin
    {
        /// <summary>
        /// 字符是否为中文
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        bool IsChinese(char c);

        /// <summary>
        /// 获取字符串的拼音
        /// </summary>
        /// <param name="s"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        string GetPinyin(string s, PinyinFormat format);

        /// <summary>
        /// 获取字符串的拼音数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        string[] GetPinyinArray(string s) => GetPinyin(s, PinyinFormat.UpperVerticalBar).Split(Pinyin.SeparatorVerticalBar, StringSplitOptions.RemoveEmptyEntries);

        public static IPinyin Instance => DI.Get<IPinyin>();
    }
}