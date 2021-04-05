using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        bool IsRunningSteamProcess { get; }

        /// <summary>
        /// 结束 Steam 进程
        /// </summary>
        [Obsolete("可能引发，Win32异常，拒绝访问，改为 TryKillSteamProcess false 弹窗提示用户手动关闭 Steam，true 使用 IsRunningSteamProcess 再检测一遍，返回 int(pid) 提示用户去任务管理器中结束进程，null 再进行业务逻辑", true)]
        void KillSteamProcess();

        /// <summary>
        /// 尝试结束 Steam 进程
        /// </summary>
        /// <returns></returns>
        bool TryKillSteamProcess();

        /// <summary>
        /// Steam 进程是否正在运行，如果正在运行，返回进程PID提示用户去任务管理器中结束进程
        /// </summary>
        /// <returns></returns>
        int? GetSteamProcessPid();

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

        /// <summary>
        /// 设置下次登陆 Steam 用户
        /// </summary>
        /// <param name="userName"></param>
        void SetCurrentUser(string userName);

        List<SteamApp>? GetAppListJson(string filePath);

        bool UpdateAppListJson(List<SteamApp> apps, string filePath);

        bool UpdateAppListJson(string appsJsonStr, string filePath);

        void DeleteLocalUserData(SteamUser user);

        void UpdateLocalUserData(SteamUser user);

        /// <summary>
        /// 从steam本地客户端缓存文件中读取游戏数据
        /// </summary>
        Task<List<SteamApp>> GetAppInfos();
    }
}