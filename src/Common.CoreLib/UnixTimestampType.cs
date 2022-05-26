namespace System;

/// <summary>
/// 时间戳类型
/// </summary>
public enum UnixTimestampType : byte
{
    /// <summary>
    /// 13位/milliseconds/毫秒
    /// <para>使用 <see cref="long"/></para>
    /// <para>13位最大值(北京时间) 2286-11-21 01:46:39.999</para>
    /// <para>最大值(北京时间) 9999-12-31 23:59:59.999</para>
    /// </summary>
    Milliseconds,

    /// <summary>
    /// 10位/seconds/秒
    /// <para>使用 <see cref="int"/></para>
    /// <para>最大值(北京时间) 2038-01-19 11:14:07.000</para>
    /// </summary>
    Seconds,
}