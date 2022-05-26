namespace System;

partial class UnixTimestamp
{
#if DEBUG

    [Obsolete("use ToUnixTimeMilliseconds", true)]
    public static long ToTimestamp(this DateTime dt) => dt.ToUnixTimeMilliseconds();

    [Obsolete("use ToUnixTimeSeconds", true)]
    public static long ToTimestampS(this DateTime dt) => dt.ToUnixTimeSeconds();

    [Obsolete("use ToTimestamp", true)]
    public static long CurrentMillis(this DateTime dateTime) => ToUnixTimeMilliseconds(dateTime);

#endif
}
