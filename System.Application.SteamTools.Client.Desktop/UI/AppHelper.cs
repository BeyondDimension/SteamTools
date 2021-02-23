using System.Diagnostics;
using System.IO;

namespace System.Application.UI
{
    public static class AppHelper
    {
        static AppHelper()
        {
            var mainModule = Process.GetCurrentProcess().MainModule;
            if (mainModule == null)
                throw new ArgumentNullException(nameof(mainModule));
            var fullName = mainModule.FileName;
            if (fullName == null)
                throw new ArgumentNullException(nameof(fullName));
            var programName = Path.GetFileName(fullName);
            if (programName == null)
                throw new ArgumentNullException(nameof(programName));
            ProgramName = programName;
        }

        /// <summary>
        /// 获取当前主程序文件名，例如word.exe
        /// </summary>
        public static string ProgramName { get; }
    }
}