using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using SteamTools.Win32;
using MetroRadiance.Interop.Win32;
using System.Diagnostics;
using SteamTool.Core.Common;
using System.Runtime.InteropServices;
using SteamTools.Services;
using SteamTool.Steam.Service.Web;
using SteamTool.Core;
using SteamTool.Steam.Service;
using SteamTool.Model;
using Newtonsoft.Json;
using SteamTools.Models.Settings;
using System.IO;
using SteamTools.Models;

namespace SteamTools.ViewModels
{
    public class SettingsAuthViewModel : Livet.ViewModel
    {
        public void ImportOldVersionAuthData()
        {
            var settingPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ProductInfo.Company,
        ProductInfo.Product, Const.SETTINGS_FILE);

            if (File.Exists(settingPath))
            {
                var text = File.ReadAllText(settingPath);
                var data = text.Substring("<x:String x:Key=\"GeneralSettings.Authenticators\">", "</x:String>", false);
                try
                {
                    AuthService.Current.ImportAuthenticatorsString(data.DecompressString());
                    AuthService.Current.SaveCurrentAuth();
                    if (WindowService.Current.MainWindow.Dialog($"导入旧版本令牌数据完成，{Environment.NewLine}是否删除Steam++旧版本数据文件？") == true)
                    {
                        File.Delete(settingPath);
                        StatusService.Current.Notify("旧版本数据文件删除完成");
                    }
                }
                catch (Exception ex)
                {
                    WindowService.Current.MainWindow.Dialog("导入旧版本令牌数据出错：" + ex.Message);
                }
            }
            else
            {
                StatusService.Current.Notify("旧版本令牌数据不存在");
            }
        }
    }
}
