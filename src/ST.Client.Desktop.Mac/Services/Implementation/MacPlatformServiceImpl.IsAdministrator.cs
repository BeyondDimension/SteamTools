//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace System.Application.Services.Implementation
//{
//    partial class MacPlatformServiceImpl
//    {
//        static readonly Lazy<bool> mIsAdministrator = new(IsAdministrator_);

//        static bool IsAdministrator_() => string.Equals(Environment.UserName, "root", StringComparison.OrdinalIgnoreCase);

//        /// <summary>
//        /// 是否是管理员权限
//        /// </summary>
//        /// <returns></returns>
//        public static bool IsAdministrator => mIsAdministrator.Value;

//        bool IPlatformService.IsAdministrator => IsAdministrator;
//    }
//}
