namespace BD.WTTS.Diagnostics;

/// <summary>
/// 启动耗时跟踪
/// </summary>
[Obsolete]
public static class StartWatchTrace
{
    static Stopwatch? sw;
    static StringBuilder? sb;

    public static void Record(string? mark = null, bool dispose = false)
    {
        if (sw != null)
        {
            if (string.IsNullOrEmpty(mark))
            {
                if (dispose) sw.Stop();
                else sw.Restart();
                return;
            }
            sw.Stop();
            var isMobile = !IApplication.IsDesktop();
            if (isMobile)
            {
                sb ??= new();
                sb.AppendFormatLine("init {1} {0}ms", sw.ElapsedMilliseconds, mark);
            }
            else
            {
                var args = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).Take(1));
                var msg = $"{(string.IsNullOrWhiteSpace(args) ? "" : args + " ")}mark: {mark}, value: {sw.ElapsedMilliseconds}";
                Debug.WriteLine(msg);
                Console.WriteLine(msg);
            }
            if (!dispose) sw.Restart();
        }
        else
        {
            sw = Stopwatch.StartNew();
        }
    }

    public static new string ToString() => sb?.ToString() ?? string.Empty;

    public static long ElapsedMilliseconds => sw == null ? 0L : sw.ElapsedMilliseconds;
}
