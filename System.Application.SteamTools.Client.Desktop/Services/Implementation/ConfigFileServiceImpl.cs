using System.Application.Models;
using System.IO;
using System.Text;
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
                var jsonStr = Serializable.SJSON(UserSettings);
                File.WriteAllText(ConfigFilePath, jsonStr, Encoding.UTF8);
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
                var jsonStr = File.ReadAllText(ConfigFilePath);
                return Serializable.DJSON<AppUserSettings>(jsonStr);
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "TryReadConfig Fail.");
            }
            return null;
        }
    }
}