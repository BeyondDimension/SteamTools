// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup // 启动耗时跟踪
{
#if STARTUP_WATCH_TRACE || DEBUG
    /// <summary>
    /// 启动耗时跟踪
    /// </summary>
    public static class WatchTrace
    {
        static Stopwatch? sw;
        static long elapsedMilliseconds;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void WatchTraceWriteLine(
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format,
            params object?[] args)
        {
            Debug.WriteLine(format, args);
            Console.WriteLine(string.Format(format, args));
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Stop() => sw?.Stop();

        /// <summary>
        /// 启动计时器
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Start()
        {
            Stop();
            sw = Stopwatch.StartNew();
        }

        /// <summary>
        /// 根据标记记录耗时
        /// </summary>
        /// <param name="mark"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Record(string mark)
        {
            if (sw == null) return;

            sw.Stop();
            elapsedMilliseconds += sw.ElapsedMilliseconds;
            WatchTraceWriteLine("{1} {0}ms", sw.ElapsedMilliseconds, mark);
            sw.Restart();
        }

        /// <summary>
        /// 停止计时并显示总计耗时
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StopWriteTotal()
        {
            if (sw == null) return;

            sw.Stop();
            elapsedMilliseconds += sw.ElapsedMilliseconds;
            WatchTraceWriteLine("Total {0}ms", elapsedMilliseconds);
        }
    }
#endif
}