using MetroTrilithon.Serialization;
using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.Models.Settings
{
    public class Providers
    {
        public static string RoamingFilePath { get; } = Path.Combine(
               Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
               ProductInfo.Company, ProductInfo.Title, Const.SETTINGS_FILE);

        public static string LocalFilePath => Path.Combine(App.Instance.LocalAppData.FullName, Const.SETTINGS_FILE);

        public static ISerializationProvider Roaming { get; } = new FileSettingsProvider(RoamingFilePath);

        public static ISerializationProvider Local { get; } = new FileSettingsProvider(LocalFilePath);
    }
}
