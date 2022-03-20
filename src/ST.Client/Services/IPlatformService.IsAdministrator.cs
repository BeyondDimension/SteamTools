using System.Collections.Generic;
using System.Text;

namespace System.Application.Services
{
    partial interface IPlatformService
    {
        /// <summary>
        /// 当前程序是否以 Administrator/System(Windows) 或 Root(FreeBSD/Linux/MacOS/Android/iOS) 权限运行
        /// </summary>
        bool IsAdministrator
        {
            get
            {
                return ArchiSteamFarm.Core.OS.IsRunningAsRoot();
            }
        }
    }
}
