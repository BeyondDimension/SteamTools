using System.Common;

namespace System
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public static class UnixTimestamp
    {
        static double ToTimestamp(this DateTime dt, UnixTimestampType unixTimestampType)
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

        /// <summary>
        /// 转换为Unix时间戳 <see cref="UnixTimestampType.Milliseconds"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime dt) => (long)dt.ToTimestamp(UnixTimestampType.Milliseconds);

        /// <summary>
        /// 转换为Unix时间戳 <see cref="UnixTimestampType.Seconds"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long ToTimestampS(this DateTime dt) => (long)dt.ToTimestamp(UnixTimestampType.Seconds);

        static long GetTicks(long timestamp)
        {
            if (timestamp > Constants.TimestampMillisecondsMaxValue) timestamp = Constants.TimestampMillisecondsMaxValue;
            var ticks = Constants.UnixEpochTicks + TimeSpan.FromMilliseconds(timestamp).Ticks;
            return ticks;
        }

        static long GetTicksS(long timestamp)
        {
            var ticks = Constants.UnixEpochTicks + TimeSpan.FromSeconds(timestamp).Ticks;
            return ticks;
        }

        static DateTime GetDateTime(long ticks, bool convertLocalTime = true)
        {
            var dt = new DateTime(ticks, DateTimeKind.Utc);
            return convertLocalTime ? dt.ToLocalTime() : dt;
        }

        static DateTimeOffset GetDateTimeOffset(long ticks, bool convertLocalTime = true)
        {
            var dt = new DateTimeOffset(ticks, TimeSpan.Zero);
            return convertLocalTime ? dt.ToLocalTime() : dt;
        }

        /// <summary>
        /// 将 <see cref="UnixTimestampType.Milliseconds"/> Unix时间戳 转换为 <see cref="DateTime"/>
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="convertLocalTime"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timestamp, bool convertLocalTime = true)
        {
            var ticks = GetTicks(timestamp);
            return GetDateTime(ticks, convertLocalTime);
        }

        /// <summary>
        /// 将 <see cref="UnixTimestampType.Seconds"/> Unix时间戳 转换为 <see cref="DateTime"/>
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="convertLocalTime"></param>
        /// <returns></returns>
        public static DateTime ToDateTimeS(this long timestamp, bool convertLocalTime = true)
        {
            var ticks = GetTicksS(timestamp);
            return GetDateTime(ticks, convertLocalTime);
        }

        /// <summary>
        /// 将 <see cref="UnixTimestampType.Milliseconds"/> Unix时间戳 转换为 <see cref="DateTimeOffset"/>
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="convertLocalTime"></param>
        /// <returns></returns>
        public static DateTimeOffset ToDateTimeOffset(this long timestamp, bool convertLocalTime = true)
        {
            var ticks = GetTicks(timestamp);
            return GetDateTimeOffset(ticks, convertLocalTime);
        }

        /// <summary>
        /// 将 <see cref="UnixTimestampType.Seconds"/> Unix时间戳 转换为 <see cref="DateTimeOffset"/>
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="convertLocalTime"></param>
        /// <returns></returns>
        public static DateTimeOffset ToDateTimeOffsetS(this long timestamp, bool convertLocalTime = true)
        {
            var ticks = GetTicksS(timestamp);
            return GetDateTimeOffset(ticks, convertLocalTime);
        }

#if DEBUG

        [Obsolete("use ToTimestamp", true)]
        public static long CurrentMillis(this DateTime dateTime) => ToTimestamp(dateTime);

#endif
    }
}