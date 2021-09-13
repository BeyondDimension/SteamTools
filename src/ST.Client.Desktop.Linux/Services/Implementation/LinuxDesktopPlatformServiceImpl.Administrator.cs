namespace System.Application.Services.Implementation
{
    partial class LinuxDesktopPlatformServiceImpl
    {
        static readonly Lazy<bool> mIsAdministrator = new(IsAdministrator_);

        static bool IsAdministrator_() => string.Equals(Environment.UserName, "root", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 是否是管理员权限
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator => mIsAdministrator.Value;

        bool IDesktopPlatformService.IsAdministrator => IsAdministrator;
    }
}