using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IPlatformService
    {
        protected const string TAG = "PlatformS";

        public static IPlatformService Instance => DI.Get<IPlatformService>();

        string[] GetMacNetworksetup() => throw new PlatformNotSupportedException();
        async void AdminShell(string str, bool admin = false) => await AdminShellAsync(str, admin);

        ValueTask AdminShellAsync(string str, bool admin = false) => throw new PlatformNotSupportedException();

        /// <summary>
        /// 使用文本阅读器打开文件
        /// </summary>
        /// <param name="filePath"></param>
        void OpenFileByTextReader(string filePath);

        /// <summary>
        /// .NET Framework 中 <see cref="Encoding.Default"/> 行为
        /// </summary>
        Encoding Default => Encoding.Default;

        /// <summary>
        /// 设置启用或关闭系统代理
        /// </summary>
        bool SetAsSystemProxy();
    }
}