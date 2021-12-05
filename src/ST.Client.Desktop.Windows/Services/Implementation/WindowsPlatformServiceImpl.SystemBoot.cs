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
        public async void SystemLock(int waitSecond = 30)
        {
            await Task.Delay(waitSecond * 1000);
            Process2.Start("rundll32.exe", "user32.dll,LockWorkStation", true);
        }

        /// <summary>
        /// 关闭系统
        /// </summary>
        /// <param name="waitSecond">等待秒数</param>
        public async void SystemShutdown(int waitSecond = 30)
        {
            await Task.Delay(waitSecond * 1000);
            Process2.Start("shutdown", "/s /t 0", true);
        }

        /// <summary>
        /// 睡眠系统
        /// </summary>
        /// <param name="waitSecond">等待秒数</param>
        public async void SystemSleep(int waitSecond = 30)
        {
            await Task.Delay(waitSecond * 1000);
            NativeMethods.SetSuspendState(false, true, false);
        }

        /// <summary>
        /// 休眠系统
        /// </summary>
        /// <param name="waitSecond">等待秒数</param>
        public async void SystemHibernate(int waitSecond = 30)
        {
            await Task.Delay(waitSecond * 1000);
            NativeMethods.SetSuspendState(true, true, false);
        }
        #endregion
    }
}