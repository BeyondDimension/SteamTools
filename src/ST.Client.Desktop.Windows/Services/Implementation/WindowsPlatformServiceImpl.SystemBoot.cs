using System.Runtime.Versioning;
using System.Security.Principal;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    partial class WindowsPlatformServiceImpl
    {
        #region System Boot

        /// <summary>
        /// 锁定
        /// </summary>
        /// <returns></returns>
        async void IPlatformService.SystemLock(int waitSecond)
        {
            await Task.Delay(waitSecond * 1000);
            Process2.Start("rundll32.exe", "user32.dll,LockWorkStation", true);
        }

        /// <summary>
        /// 关闭系统
        /// </summary>
        /// <param name="waitSecond">等待秒数</param>
        async void IPlatformService.SystemShutdown(int waitSecond)
        {
            await Task.Delay(waitSecond * 1000);
            Process2.Start("shutdown", "/s /t 0", true);
        }

        /// <summary>
        /// 睡眠系统
        /// </summary>
        /// <param name="waitSecond">等待秒数</param>
        async void IPlatformService.SystemSleep(int waitSecond)
        {
            await Task.Delay(waitSecond * 1000);
            NativeMethods.SetSuspendState(false, true, false);
        }

        /// <summary>
        /// 休眠系统
        /// </summary>
        /// <param name="waitSecond">等待秒数</param>
        async void IPlatformService.SystemHibernate(int waitSecond)
        {
            await Task.Delay(waitSecond * 1000);
            NativeMethods.SetSuspendState(true, true, false);
        }

        #endregion
    }
}