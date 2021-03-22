using System.Application.Models;

namespace System.Application.Services
{
    /// <summary>
    /// Steam++ 应用 用户配置文件服务
    /// </summary>
    public interface IConfigFileService
    {
        const string TAG = "ConfigFileS";

        const string ConfigFileName = "Config.mpo";

        /// <summary>
        /// 获取或设置 Steam++ 应用 用户配置项
        /// </summary>
        AppUserSettings UserSettings { get; set; }

        /// <summary>
        /// 将 Steam++ 应用 用户配置项 保存到本地存储中
        /// </summary>
        void SaveChanges();

        public static IConfigFileService Instance => DI.Get<IConfigFileService>();
    }
}