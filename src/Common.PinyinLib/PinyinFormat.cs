namespace System;

/// <summary>
/// 拼音格式
/// </summary>
public enum PinyinFormat : byte
{
    /// <summary>
    /// 拼音字母大写，分隔符使用竖线，无音标
    /// </summary>
    UpperVerticalBar,

    /// <summary>
    /// 用于字母表排序的格式，无音标，这将允许各平台有所差异(因对字母大小写、分隔符无要求)
    /// </summary>
    AlphabetSort,
}