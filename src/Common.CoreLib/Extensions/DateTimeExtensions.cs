// ReSharper disable once CheckNamespace
namespace System
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 根据时间日期获取当前月
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime GetCurrentMonth(this DateTime dt)
            => new(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind);
    }
}
