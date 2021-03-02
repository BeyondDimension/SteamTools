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

#if DEBUG

    [Obsolete("use IConfigFileService.Instance", true)]
    public class ConfigService
    {
        [Obsolete("use IConfigFileService.Instance.UserSettings", true)]
        public SettingsModel? SteamToolModel { get; set; }

        [Obsolete("use IConfigFileService.Instance.SaveChanges", true)]
        public void SaveConfig() => throw new NotImplementedException();

        [Obsolete("changedAutoRead", true)]
        public SettingsModel ReadConfig() => throw new NotImplementedException();
    }

#endif
}