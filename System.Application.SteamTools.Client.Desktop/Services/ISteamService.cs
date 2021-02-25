using System.Application.Models;
using System.Collections.Generic;

namespace System.Application.Services
{
    /// <summary>
    /// Steam 相关助手、工具类服务
    /// </summary>
    public interface ISteamService
    {
        public static ISteamService Instance => DI.Get<ISteamService>();

        /// <summary>
        /// Steam 文件夹目录
        /// </summary>
        string? SteamDirPath { get; }

        /// <summary>
        /// Steam 主程序文件目录
        /// </summary>
        string? SteamProgramPath { get; }

        /// <summary>
        /// 结束 Steam 进程
        /// </summary>
        void KillSteamProcess();

        /// <summary>
        /// 启动 Steam
        /// </summary>
        /// <param name="arguments"></param>
        void StartSteam(string arguments = "");

        /// <summary>
        /// 获取最后一次自动登陆 Steam 用户名称
        /// </summary>
        /// <returns></returns>
        string GetLastLoginUserName();

        /// <summary>
        /// 获取所有记住登陆 Steam 用户信息
        /// </summary>
        /// <returns></returns>
        List<SteamUser> GetRememberUserList();

        List<SteamApp>? GetAppListJson(string filePath);

        bool UpdateAppListJson(List<SteamApp> apps, string filePath);

        bool UpdateAppListJson(string appsJsonStr, string filePath);

        void DeleteLocalUserData(SteamUser user);

        void UpdateLocalUserData(SteamUser user);

#if DEBUG

        [Obsolete("use AppHelper.SetBootAutoStart", true)]
        void SetWindowsStartupAutoRun(bool IsAutoRun, string Name = "Steam++");

#endif
    }

#if DEBUG

    [Obsolete("use ISteamService", true)]
    public class SteamToolService
    {
        [Obsolete("use SteamDirPath", true)]
        public string? SteamPath { get; }

        [Obsolete("use SteamProgramPath", true)]
        public string? SteamExePath { get; }

        [Obsolete("use ISteamService.DeleteLocalUserData", true)]
        public void DeleteSteamLocalUserData(SteamUser user)
        {
        }

        [Obsolete("use ISteamService.UpdateLocalUserData", true)]
        public void UpdateSteamLocalUserData(SteamUser user)
        {
        }
    }

    [Obsolete("use ISteamService", true)]
    public class SteamService
    {
    }

#endif
}