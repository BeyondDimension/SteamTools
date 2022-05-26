using System.Properties;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Enum 扩展 <see cref="DayOfWeek"/>
/// </summary>
public static partial class DayOfWeekEnumExtensions
{
    /// <summary>
    /// 将 <see cref="DayOfWeek"/>(周) 转换为字符串
    /// </summary>
    /// <param name="dayOfWeek"></param>
    /// <param name="format_short_or_long">使用[(<see langword="true"/>)短]或[(<see langword="false"/>)长]的格式，默认使用长格式</param>
    /// <returns></returns>
    public static string ToString2(this DayOfWeek dayOfWeek, bool format_short_or_long = false)
        => dayOfWeek switch
        {
            DayOfWeek.Monday => format_short_or_long ? SR.DayOfWeek_S1 : SR.DayOfWeek_L1,
            DayOfWeek.Tuesday => format_short_or_long ? SR.DayOfWeek_S2 : SR.DayOfWeek_L2,
            DayOfWeek.Wednesday => format_short_or_long ? SR.DayOfWeek_S3 : SR.DayOfWeek_L3,
            DayOfWeek.Thursday => format_short_or_long ? SR.DayOfWeek_S4 : SR.DayOfWeek_L4,
            DayOfWeek.Friday => format_short_or_long ? SR.DayOfWeek_S5 : SR.DayOfWeek_L5,
            DayOfWeek.Saturday => format_short_or_long ? SR.DayOfWeek_S6 : SR.DayOfWeek_L6,
            DayOfWeek.Sunday => format_short_or_long ? SR.DayOfWeek_S0 : SR.DayOfWeek_L0,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null),
        };
}