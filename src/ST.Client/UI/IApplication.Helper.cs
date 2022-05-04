using System.Application.Services;
using System.Diagnostics;
using System.IO;
using System.Properties;

namespace System.Application.UI
{
    partial interface IApplication
    {
        public static bool EnableDevtools { get; set; } = ThisAssembly.Debuggable;

        /// <summary>
        /// 禁用 GPU 硬件加速
        /// </summary>
        public static bool DisableGPU { get; set; }

        /// <summary>
        /// 使用 native OpenGL(仅 Windows)
        /// </summary>
        public static bool UseWgl { get; set; }

        static IApplication()
        {
#if NET6_0_OR_GREATER
            ProgramPath = Environment.ProcessPath!;
#else
            var mainModule = Process.GetCurrentProcess().MainModule;
            if (mainModule == null)
                throw new ArgumentNullException(nameof(mainModule));
            ProgramPath = mainModule.FileName!;
#endif
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
        /// 获取当前主程序文件名，例如 word.exe
        /// </summary>
        public static string ProgramName { get; }

        /// <inheritdoc cref="IPlatformService.SetBootAutoStart(bool, string)"/>
        public static void SetBootAutoStart(bool isAutoStart)
        {
            IPlatformService.Instance.SetBootAutoStart(isAutoStart, Constants.HARDCODED_APP_NAME);
        }
    }
}
