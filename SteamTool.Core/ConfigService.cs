using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SteamTool.Core.Model;
using Newtonsoft.Json;
using System.Reflection;

namespace SteamTool.Core
{
    public class ConfigService
    {
        private const string ConfigFileName = "Config.json";

        private readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Assembly.GetCallingAssembly().GetName().Name}.{ConfigFileName}");

        public SteamToolConfig SteamToolConfig { get; set; }

        public void SaveConfig()
        {
            var jsonData = JsonConvert.SerializeObject(SteamToolConfig);
            File.WriteAllText(ConfigPath, jsonData, Encoding.UTF8);
        }

        public SteamToolConfig ReadConfig()
        {
            SteamToolConfig = JsonConvert.DeserializeObject<SteamToolConfig>(File.ReadAllText(ConfigPath, Encoding.UTF8));
            return SteamToolConfig;
        }
    }
}
