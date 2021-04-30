using Avalonia;
using System.Application.UI;
using System.Diagnostics;
using System.Threading;

namespace System.Application
{
    static class Program
    {
        internal static Mutex? mutex;
        static readonly Process currentProcess = Process.GetCurrentProcess();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            mutex = new Mutex(true, currentProcess.ProcessName, out var isNotRunning);
            if (isNotRunning)
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}