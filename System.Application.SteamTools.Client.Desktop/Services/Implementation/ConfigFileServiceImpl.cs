using System.Application.Models;
using System.IO;
using static System.Application.Services.IConfigFileService;

namespace System.Application.Services.Implementation
{
    internal sealed class ConfigFileServiceImpl : IConfigFileService
    {
        public string ConfigFilePath { get; }

        public ConfigFileServiceImpl()
        {
            ConfigFilePath = Path.Combine(IOPath.AppDataDirectory, ConfigFileName);
            UserSettings = TryReadConfig() ?? new AppUserSettings();
        }

        public AppUserSettings UserSettings { get; set; }

        public void SaveChanges()
        {
            try
            {
                var data = Serializable.SMP(UserSettings);
                File.WriteAllBytes(ConfigFilePath, data);
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "SaveChanges Fail.");
            }
        }

        AppUserSettings? TryReadConfig()
        {
            try
            {
                var data = File.ReadAllBytes(ConfigFilePath);
                return Serializable.DMP<AppUserSettings>(data);
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "TryReadConfig Fail.");
            }
            return null;
        }
    }
}