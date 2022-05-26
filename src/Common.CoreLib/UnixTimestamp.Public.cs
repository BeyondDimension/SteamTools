using System.Runtime.CompilerServices;

namespace System;

partial class UnixTimestamp
{
    /// <summary>
    /// 转换为 Unix 时间戳 <see cref="UnixTimestampType.Milliseconds"/>
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMilliseconds(this DateTime dt)
        => (long)dt.ToUnixTimestamp(UnixTimestampType.Milliseconds);

    /// <summary>
    /// 转换为 Unix 时间戳 <see cref="UnixTimestampType.Seconds"/>
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeSeconds(this DateTime dt)
        => (long)dt.ToUnixTimestamp(UnixTimestampType.Seconds);

    /// <summary>
    /// 将 Unix时间戳根据 <see cref="UnixTimestampType"/> 转换为 <see cref="DateTime"/>
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="type"></param>
    /// <param name="convertLocalTime"></param>
    /// <returns></returns>
    public static DateTime ToDateTime(this long timestamp, UnixTimestampType type = UnixTimestampType.Milliseconds, bool convertLocalTime = true)
    {
        var ticks = GetTicks(timestamp, type);
        return GetDateTime(ticks, convertLocalTime);
    }

    /// <summary>
    /// 将 Unix时间戳根据 <see cref="UnixTimestampType"/> 转换为 <see cref="DateTimeOffset"/>
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="type"></param>
    /// <param name="convertLocalTime"></param>
    /// <returns></returns>
    public static DateTimeOffset ToDateTimeOffset(this long timestamp, UnixTimestampType type = UnixTimestampType.Milliseconds, bool convertLocalTime = true)
    {
        var ticks = GetTicks(timestamp, type);
        return GetDateTimeOffset(ticks, convertLocalTime);
    }

    /// <summary>
    /// 将 Unix时间戳根据 <see cref="UnixTimestampType.Seconds"/> 转换为 <see cref="DateTime"/>
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="convertLocalTime"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime ToDateTimeS(this long timestamp, bool convertLocalTime = true)
        => ToDateTime(timestamp, UnixTimestampType.Seconds, convertLocalTime);

    /// <summary>
    /// 将 Unix时间戳根据 <see cref="UnixTimestampType.Seconds"/> 转换为 <see cref="DateTimeOffset"/>
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="convertLocalTime"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset ToDateTimeOffsetS(this long timestamp, bool convertLocalTime = true)
        => ToDateTime(timestamp, UnixTimestampType.Seconds, convertLocalTime);
}
