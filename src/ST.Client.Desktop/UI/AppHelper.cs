using System.Application.Services;
using System.Diagnostics;
using System.IO;
using System.Properties;

namespace System.Application.UI
{
    public static partial class AppHelper
    {
        public static bool EnableDevtools { get; set; } = ThisAssembly.Debuggable;

        /// <summary>
        /// 禁用 GPU 硬件加速(仅macOS)
        /// </summary>
        public static bool DisableGPU { get; set; }

        /// <summary>
        /// 使用 OpenGL(仅 Windows)
        /// </summary>
        public static bool UseOpenGL { get; set; }

        public static bool IsSystemWebViewAvailable { get; set; }

        public static Action? Initialized { get; set; }

        public static Action? Shutdown { get; set; }

        static bool isShutdown = false;
        public static void TryShutdown()
        {
            if (isShutdown) return;
            isShutdown = true;
            Shutdown?.Invoke();
        }

        static AppHelper()
        {
            var mainModule = Process.GetCurrentProcess().MainModule;
            if (mainModule == null)
                throw new ArgumentNullException(nameof(mainModule));
            ProgramPath = mainModule.FileName!;
            if (ProgramPath == null)
                throw new ArgumentNullException(nameof(ProgramPath));
            var programName = Path.GetFileName(ProgramPath);
            if (programName == null)
                throw new ArgumentNullException(nameof(programName));
            ProgramName = programName;
        }

        /// <summary>
        /// 当前主程序所在绝对路径
        /// </summary>
        public static string ProgramPath { get; }

        /// <summary>
        /// 获取当前主程序文件名，例如word.exe
        /// </summary>
        public static string ProgramName { get; }

        /// <inheritdoc cref="IDesktopPlatformService.SetBootAutoStart(bool, string)"/>
        public static void SetBootAutoStart(bool isAutoStart)
        {
            var s = DI.Get<IDesktopPlatformService>();
            s.SetBootAutoStart(isAutoStart, Constants.HARDCODED_APP_NAME);
        }

        public static IDesktopAppService Current => DI.Get<IDesktopAppService>();

#if DEBUG

        [Obsolete("use EnableLogger", true)]
        public static bool EnableTextLog
        {
            get => EnableLogger;
            set => EnableLogger = value;
        }

#endif
    }
}