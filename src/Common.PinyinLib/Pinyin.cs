using System.Application.Services;
using System.Common;

namespace System
{
    /// <inheritdoc cref="IPinyin"/>
    public static class Pinyin
    {
        const string TAG = "SysPinyin";
        /// <summary>
        /// 竖线分隔符
        /// </summary>
        public const char SeparatorVerticalBar = '|';
        /// <summary>
        /// 拼音分隔符(单引号)
        /// </summary>
        public const char Separator = '\'';
        /// <summary>
        /// 中文拼音分隔符(单引号)
        /// </summary>
        public const char SeparatorZH = '’';

        /// <inheritdoc cref="IPinyin.IsChinese(char)"/>
        public static bool IsChinese(char c) => IPinyin.Instance.IsChinese(c);

        /// <inheritdoc cref="IPinyin.GetPinyin(string, PinyinFormat)"/>
        public static string GetPinyin(string s, PinyinFormat format)
            => IPinyin.Instance.GetPinyin(s, format);

        /// <inheritdoc cref="IPinyin.GetPinyinArray(string)"/>
        public static string[] GetPinyinArray(string s)
            => IPinyin.Instance.GetPinyinArray(s);

        /// <summary>
        /// 获取字母表排序值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static string GetAlphabetSort(string s, string def = Constants.Sharp)
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                #region V1

                //var firstChar = s[0]; // 第一个字符
                //var index = Constants.UpperCaseLetters.IndexOf(firstChar); // 第一个字符是大写字母则返回大写字母。
                //if (index >= 0) return Constants.UpperCaseLetters[index].ToString();
                //index = Constants.LowerCaseLetters.IndexOf(firstChar); // 查找小写字母
                //if (index >= 0) return Constants.UpperCaseLetters[index].ToString(); // 找到了则返回大写字母。

                //var pinyin = IPinyin.Instance;
                //if (pinyin.IsChinese(firstChar))
                //{
                //    return pinyin.GetPinyin(s, PinyinFormat.AlphabetSort);
                //}
                //else
                //{
                //    return def + s;
                //}

                #endregion

                #region V2

                s = GetPinyin(s, PinyinFormat.AlphabetSort).ToUpper();
                var firstChar = s[0]; // 第一个字符
                var index = Constants.UpperCaseLetters.IndexOf(firstChar);
                return index >= 0 ? s : def + s;

                #endregion
            }
            return def;
        }

        /// <summary>
        /// 搜索比较，返回是否匹配
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="vm"></param>
        /// <param name="inputText">输入的搜索文本内容</param>
        /// <param name="getName">获取名称</param>
        /// <param name="getPinyinArray">获取名称对应的拼音数组，可使用 <see cref="GetPinyinArray(string)"/> 获取，最好在模型类中定义一个属性存放 <see cref="string[]"/> 拼音数组，且当 Name 发生改变时重新赋值</param>
        /// <param name="ignoreOtherChar">比较时是否忽略其他字符</param>
        /// <returns></returns>
        public static bool SearchCompare<TViewModel>(TViewModel vm, string inputText, Func<TViewModel, string> getName, Func<TViewModel, string[]> getPinyinArray, bool ignoreOtherChar = true)
        {
            if (string.IsNullOrEmpty(inputText)) return true; // 空值全部匹配。
            var name = getName(vm);
            if (name.Contains(inputText)) return true;
            var pinyinArray = getPinyinArray(vm);
            if (!pinyinArray.Any_Nullable()) return false;

            #region 汉字拼音混合比较算法

            try
            {
                var index = 0; // 输入分词数组的比较下标
                for (int i = 0; i < pinyinArray!.Length; i++) // 循环所有字的拼音
                {
                    var item_pinyin = pinyinArray[i]; // 当前字的拼音
                    if (index >= inputText.Length) return true; // 下标溢出即搜索完毕，返回比较成功。
                    int cache_index = 0;
                    var break_next = false;
                    var ignore_separator = true; // 本次循环忽略分隔符。
                    for (; index < inputText.Length; index++) // 循环输入内容
                    {
                        if (break_next) break;
                        var input_item = inputText[index]; // 当前输入内容字符
                        if (input_item == Separator || input_item == SeparatorZH) // 拼音分隔分号字符无视
                        {
                            if (i < pinyinArray.Length - 1) // 前面的可以输入分隔符跳过
                            {
                                if (ignore_separator)
                                {
                                    ignore_separator = !ignore_separator;
                                }
                                else
                                {
                                    break_next = true;
                                }
                            }
                            continue;
                        }
                        var isChinese = IsChinese(input_item); // 当前字符是否为中文字符。
                        if (isChinese)
                        {
                            if (name[i] != input_item) return false;
                            else continue;
                        }
                        // ↑ 当前字符为中文字符比较内容不正确返回比较失败。
                        char? letter = null;
                        var indexOfUpperCase = Constants.UpperCaseLetters.IndexOf(input_item); // 大写字母搜索
                        if (indexOfUpperCase >= 0) letter = Constants.UpperCaseLetters[indexOfUpperCase];
                        var indexOfLowerCase = Constants.LowerCaseLetters.IndexOf(input_item); // 小写字母搜索
                        if (indexOfLowerCase >= 0) letter = Constants.UpperCaseLetters[indexOfLowerCase];
                        if (!letter.HasValue && !ignoreOtherChar) return false; // 无效字符是否允许
                        if (letter.HasValue && item_pinyin[cache_index] != letter) return false;
                        else cache_index += 1;
                        if (cache_index >= item_pinyin.Length)
                        {
                            ignore_separator = true;
                            break_next = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "SearchCompare catch, name: {0}, inputText: {1}", name, inputText);
                return false;
            }

            return true;

            #endregion
        }
    }
}