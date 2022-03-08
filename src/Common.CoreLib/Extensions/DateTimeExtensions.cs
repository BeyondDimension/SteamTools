// ReSharper disable once CheckNamespace
namespace System
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 根据时间日期获取当前月(第一天)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime GetCurrentMonth(this DateTime dt)
            => new(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind);

        /// <summary>
        /// 根据时间日期获取当前月(最后一天)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime GetCurrentMonthLastDay(this DateTime dt)
            => new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind).AddMonths(1).AddDays(-1);

        /// <summary>
        /// 根据时间日期获取当前月(第一天)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTimeOffset GetCurrentMonth(this DateTimeOffset dt)
            => new(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset);

        /// <summary>
        /// 根据时间日期获取当前月(最后一天)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTimeOffset GetCurrentMonthLastDay(this DateTimeOffset dt)
            => new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset).AddMonths(1).AddDays(-1);
    }
}
