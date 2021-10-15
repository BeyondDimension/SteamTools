using System.Properties;
using System.IO;
using System.Diagnostics;
using System.Application.Services;

namespace System.Application.UI
{
    partial interface IApplication
    {
        static readonly Lazy<AppType> mType = new(() => Instance.GetType());
        public static AppType Type => mType.Value;

        /// <summary>
        /// 是否使用移动端布局
        /// </summary>
        public static bool IsMobileLayout => Type switch
        {
            AppType.Maui or AppType.PlatformUI_Android => true,
            _ => false,
        };

        public static bool EnableDevtools { get; set; } = ThisAssembly.Debuggable;

        /// <summary>
        /// 禁用 GPU 硬件加速(仅macOS)
        /// </summary>
        public static bool DisableGPU { get; set; }

        /// <summary>
        /// 使用 OpenGL(仅 Windows)
        /// </summary>
        public static bool UseOpenGL { get; set; }

        static IApplication()
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
