using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SteamTool.Core;
using Newtonsoft.Json;
using System.Reflection;
using SteamTool.Model;

namespace SteamTool.Core
{
    public class ConfigService
    {
        private const string ConfigFileName = "Config.json";

        private readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Assembly.GetCallingAssembly().GetName().Name}.{ConfigFileName}");

        public SettingsModel SteamToolModel { get; set; }

        public void SaveConfig()
        {
            var jsonData = JsonConvert.SerializeObject(SteamToolModel);
            File.WriteAllText(ConfigPath, jsonData, Encoding.UTF8);
        }

        public SettingsModel ReadConfig()
        {
            SteamToolModel = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(ConfigPath, Encoding.UTF8));
            return SteamToolModel;
        }
    }
}
