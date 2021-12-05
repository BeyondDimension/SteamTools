namespace System
{
    partial class UnixTimestamp
    {
        [Obsolete("use ToUnixTimeMilliseconds", true)]
        public static long ToTimestamp(this DateTime dt) => dt.ToUnixTimeMilliseconds();

        [Obsolete("use ToUnixTimeSeconds", true)]
        public static long ToTimestampS(this DateTime dt) => dt.ToUnixTimeSeconds();

#if DEBUG

        [Obsolete("use ToTimestamp", true)]
        public static long CurrentMillis(this DateTime dateTime) => ToUnixTimeMilliseconds(dateTime);

#endif
    }
}
