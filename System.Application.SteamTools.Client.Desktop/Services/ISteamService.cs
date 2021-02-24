namespace System.Application.Services
{
    public interface ISteamService
    {
        public static ISteamService Instance => DI.Get<ISteamService>();

        /// <summary>
        /// Steam 文件夹目录
        /// </summary>
        string SteamDirPath { get; }

        /// <summary>
        /// Steam 主程序文件目录
        /// </summary>
        string SteamProgramPath { get; }

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
    }

#if DEBUG

    [Obsolete("use ISteamService", true)]
    public class SteamToolService
    {

        [Obsolete("use SteamDirPath", true)]
        public string? SteamPath { get; }
    }

#endif
}