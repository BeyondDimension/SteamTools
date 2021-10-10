using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public partial interface IPlatformService
    {
        protected const string TAG = "PlatformS";

        public static IPlatformService Instance => DI.Get<IPlatformService>();

        string[] GetMacNetworkSetup() => throw new PlatformNotSupportedException();

        /// <summary>
        /// 运行 Shell 脚本
        /// </summary>
        /// <param name="script">要运行的脚本字符串</param>
        /// <param name="admin">是否以管理员或Root权限运行</param>
        async void RunShell(string script, bool admin = false) => await RunShellAsync(script, admin);

        /// <inheritdoc cref="RunShell(string, bool)"/>
        ValueTask RunShellAsync(string script, bool admin = false) => throw new PlatformNotSupportedException();

        /// <summary>
        /// 使用文本阅读器打开文件
        /// </summary>
        /// <param name="filePath"></param>
        void OpenFileByTextReader(string filePath);

        /// <summary>
        /// 在 Windows 上时使用 .NET Framework 中 <see cref="Encoding.Default"/> 行为。
        /// <para></para>
        /// 非 Windows 上等同于 <see cref="Encoding.Default"/>(UTF8)
        /// </summary>
        Encoding Default => Encoding.Default;

        /// <summary>
        /// 设置启用或关闭系统代理
        /// </summary>
        bool SetAsSystemProxy(bool state, IPAddress? ip = null, int port = -1)
        {
            return false;
        }

        /// <summary>
        /// 获取一个正在运行的进程的命令行参数
        /// 与 <see cref="Environment.GetCommandLineArgs"/> 一样，使用此方法获取的参数是包含应用程序路径的
        /// 关于 <see cref="Environment.GetCommandLineArgs"/> 可参见：
        /// .NET 命令行参数包含应用程序路径吗？https://blog.walterlv.com/post/when-will-the-command-line-args-contain-the-executable-path.html
        /// </summary>
        /// <param name="process">一个正在运行的进程</param>
        /// <returns>表示应用程序运行命令行参数的字符串</returns>
        string GetCommandLineArgs(Process process);
    }
}