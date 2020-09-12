using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SteamTools.Model;
using Newtonsoft.Json;

namespace SteamTools.Helper
{
    public static class ConfigHelper
    {
        private const string ConfigFileName = "SteamToolData.dat";

        private static readonly string ConfigPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConfigFileName);

        public static SteamToolConfig SteamToolConfig { get; set; }

        public static void SaveConfig()
        {
            var jsonData = JsonConvert.SerializeObject(SteamToolConfig);
            File.WriteAllText(ConfigPath, jsonData, Encoding.UTF8);
        }

        public static SteamToolConfig ReadConfig()
        {
            SteamToolConfig = JsonConvert.DeserializeObject<SteamToolConfig>(File.ReadAllText(ConfigPath, Encoding.UTF8));
            return SteamToolConfig;
        }
    }
}
