//namespace System.Application.Services.Implementation
//{
//    partial class LinuxPlatformServiceImpl
//    {
//        static readonly Lazy<bool> mIsAdministrator = new(IsAdministrator_);

//        static bool IsAdministrator_() => string.Equals(Environment.UserName, "root", StringComparison.Ordinal);

//        /// <summary>
//        /// 是否是管理员权限
//        /// </summary>
//        /// <returns></returns>
//        public static bool IsAdministrator => mIsAdministrator.Value;

//        bool IPlatformService.IsAdministrator => IsAdministrator;
//    }
//}