using System.Common;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// 时间戳
/// </summary>
public static partial class UnixTimestamp
{
    static double ToUnixTimestamp(this DateTime dt, UnixTimestampType unixTimestampType)
    {
        var timeDiff = new TimeSpan(dt.ToUniversalTime().Ticks - Constants.UnixEpochTicks);
        var total = unixTimestampType switch
        {
            UnixTimestampType.Milliseconds => timeDiff.TotalMilliseconds,
            UnixTimestampType.Seconds => timeDiff.TotalSeconds,
            _ => throw new ArgumentOutOfRangeException(nameof(unixTimestampType), unixTimestampType, null),
        };
        return (double)Math.Floor(total);
    }

    static long GetTicks(long timestamp, UnixTimestampType unixTimestampType)
    {
        long ticks;
        switch (unixTimestampType)
        {
            case UnixTimestampType.Milliseconds:
                if (timestamp > Constants.TimestampMillisecondsMaxValue) timestamp = Constants.TimestampMillisecondsMaxValue;
                ticks = Constants.UnixEpochTicks + TimeSpan.FromMilliseconds(timestamp).Ticks;
                break;
            case UnixTimestampType.Seconds:
                ticks = Constants.UnixEpochTicks + TimeSpan.FromSeconds(timestamp).Ticks;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(unixTimestampType), unixTimestampType, null);
        }
        return ticks;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static DateTime GetDateTime(long ticks, bool convertLocalTime = true)
    {
        var dt = new DateTime(ticks, DateTimeKind.Utc);
        return convertLocalTime ? dt.ToLocalTime() : dt;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static DateTimeOffset GetDateTimeOffset(long ticks, bool convertLocalTime = true)
    {
        var dt = new DateTimeOffset(ticks, TimeSpan.Zero);
        return convertLocalTime ? dt.ToLocalTime() : dt;
    }
}