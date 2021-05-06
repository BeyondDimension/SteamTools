using System.Security.Principal;

namespace System.Application.Services.Implementation
{
    partial class WindowsDesktopPlatformServiceImpl
    {
        #region Administrator

        static readonly Lazy<bool> mIsAdministrator = new(IsAdministrator_);

        static bool IsAdministrator_()
            => new WindowsPrincipal(WindowsIdentity.GetCurrent())
            .IsInRole(WindowsBuiltInRole.Administrator);

        /// <summary>
        /// 是否是管理员权限
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator => mIsAdministrator.Value;

        bool IDesktopPlatformService.IsAdministrator => IsAdministrator;

        #endregion
    }
}