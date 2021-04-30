#if DEBUG
using System.Diagnostics;
using System.Linq;

namespace System.Application.UI
{
    /// <summary>
    /// 启动耗时跟踪
    /// </summary>
    static class StartTrace
    {
        static Stopwatch? sw;

        public static void Restart(string? mark = null)
        {
            if (sw != null)
            {
                sw.Stop();
                var msg = $"{string.Join(" ", Environment.GetCommandLineArgs().Skip(1).Take(1))}mark: {mark}, value: {sw.ElapsedMilliseconds}";
                Debug.WriteLine(msg);
                Console.WriteLine(msg);
                sw.Restart();
            }
            else
            {
                sw = Stopwatch.StartNew();
            }
        }
    }
}
#endif