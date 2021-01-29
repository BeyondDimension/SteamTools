using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using SteamTool.Model;
using Newtonsoft.Json;
using SteamTool.Core.Common;
using Microsoft.Win32.TaskScheduler;
using System.Reflection;
using System.Security.Principal;

namespace SteamTool.Core
{
    public class SteamToolService
    {
        private readonly RegistryKeyService registryKeyService = SteamToolCore.Instance.Get<RegistryKeyService>();
        private readonly VdfService vdfService = SteamToolCore.Instance.Get<VdfService>();

        public bool IsAdministrator
        {
            get
            {
                WindowsIdentity current = WindowsIdentity.GetCurrent();
                WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
                return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public string SteamPath { get; set; }

        public string SteamExePath { get; set; }

        public const string UserDataDirectory = @"\userdata";

        /* 
            Windows: ~\Steam\config\loginusers.vdf
            Linux: ~/.steam/steam/config/loginusers.vdf
            Mac: ~/Library/Application Support/Steam/config/loginusers.vdf
         */
        public const string UserVdfPath = @"\config\loginusers.vdf";

        private const string SteamRegistryPath = @"SOFTWARE\Valve\Steam";

        public SteamToolService()
        {
            var steamExePath = registryKeyService.ReadRegistryKey(Registry.CurrentUser, SteamRegistryPath, "SteamExe");
            var steamPath = registryKeyService.ReadRegistryKey(Registry.CurrentUser, SteamRegistryPath, "SteamPath");
            if (File.Exists(steamExePath))
            {
                SteamExePath = Path.GetFullPath(steamExePath);
            }
            if (Directory.Exists(steamPath))
            {
                SteamPath = Path.GetFullPath(steamPath);
            }
        }

        public void KillSteamProcess()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.Equals("steam", StringComparison.OrdinalIgnoreCase) || process.ProcessName.Equals("steamservice", StringComparison.OrdinalIgnoreCase) || process.ProcessName.Equals("steamwebhelper", StringComparison.OrdinalIgnoreCase))
                {
                    process.Kill();
                }
            }
        }

        public void StartSteam(string param = "")
        {
            if (!string.IsNullOrEmpty(SteamExePath))
            {
                Process.Start(SteamExePath, param);
            }
        }

        /// <summary>
        /// 获取最后一次自动登陆steam用户名称
        /// </summary>
        /// <returns></returns>
        public string GetLastLoginUserName()
        {
            return registryKeyService.ReadRegistryKey(Registry.CurrentUser, SteamRegistryPath, "AutoLoginUser");
        }

        /// <summary>
        /// 获取所有记住登陆steam用户信息
        /// </summary>
        /// <returns></returns>
        public List<SteamUser> GetRememberUserList()
        {
            var users = new List<SteamUser>();
            if (File.Exists(SteamPath + UserVdfPath))
            {
                var v = vdfService.GetVdfModelByPath(SteamPath + UserVdfPath);

                foreach (var item in v.Value)
                {
                    var i = item.Value;
                    var user = new SteamUser(item.ToString())
                    {
                        SteamId64 = Convert.ToInt64(item.Key.ToString()),
                        AccountName = i.AccountName?.ToString(),
                        SteamID = i.PersonaName?.ToString(),
                        PersonaName = i.PersonaName?.ToString(),
                        RememberPassword = Convert.ToBoolean(Convert.ToInt64(i.RememberPassword?.ToString())),
                        Timestamp = Convert.ToInt64(i.Timestamp?.ToString())
                    };
                    user.LastLoginTime = user.Timestamp.ToDateTime();

                    //老版本Steam数据 小写mostrecent 支持
                    user.MostRecent = i.mostrecent != null ?
                        Convert.ToBoolean(Convert.ToByte(i.mostrecent.ToString())) :
                        Convert.ToBoolean(Convert.ToByte(i.MostRecent.ToString()));

                    user.WantsOfflineMode = i.WantsOfflineMode != null ?
                        Convert.ToBoolean(Convert.ToByte(i.WantsOfflineMode.ToString())) : false;

                    // 因为警告这个东西应该都不需要所以直接默认跳过好了
                    user.SkipOfflineModeWarning = true;
                    //user.SkipOfflineModeWarning = i.SkipOfflineModeWarning != null ?
                    //    Convert.ToBoolean(Convert.ToByte(i.SkipOfflineModeWarning.ToString())) : false;

                    users.Add(user);
                }
            }

            return users;
        }

        /// <summary>
        /// 设置下次登陆steam用户
        /// </summary>
        public void SetCurrentUser(string username)
        {
            registryKeyService.AddOrUpdateRegistryKey(Registry.CurrentUser, SteamRegistryPath, "AutoLoginUser", username, RegistryValueKind.String);
        }

        public List<SteamApp> GetAppListJson(string filepath)
        {
            if (!File.Exists(filepath))
            {
                return null;
            }

            var lastChanged = File.GetLastWriteTime(filepath);
            int daysSinceChanged = (int)(DateTime.Now - lastChanged).TotalDays;
            if (daysSinceChanged > 10)
            {
                return null;
            }

            string json = File.ReadAllText(filepath, Encoding.UTF8);
            var apps = JsonConvert.DeserializeObject<SteamApps>(json);
            return apps.AppList.Apps;
        }

        public bool UpdateAppListJson(List<SteamApp> apps, string filepath)
        {
            var json = JsonConvert.SerializeObject(apps);
            try
            {
                File.WriteAllText(filepath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
                return false;
            }
            return true;
        }

        public bool UpdateAppListJson(string json, string filepath)
        {
            try
            {
                File.WriteAllText(filepath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
                return false;
            }
            return true;
        }

        public void SetWindowsStartupAutoRun(bool IsAutoRun, string Name = "Steam++")
        {
            if (IsAutoRun)
            {
                using TaskDefinition td = TaskService.Instance.NewTask();
                td.RegistrationInfo.Description = Name + "System Boot Run";
                td.Settings.Priority = System.Diagnostics.ProcessPriorityClass.Normal;
                td.Settings.ExecutionTimeLimit = new TimeSpan(0);
                td.Settings.AllowHardTerminate = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Triggers.Add(new LogonTrigger());
                td.Actions.Add(new ExecAction(Assembly.GetCallingAssembly().Location, "-minimized", Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)));
                if (IsAdministrator)
                    td.Principal.RunLevel = TaskRunLevel.Highest;
                TaskService.Instance.RootFolder.RegisterTaskDefinition(Name, td);
                //TaskService.Instance.RootFolder.AddTask(Name, QuickTriggerType.Boot, Assembly.GetCallingAssembly().Location, "-a");
            }
            else
            {
                TaskService.Instance.RootFolder.DeleteTask(Name);
            }
        }

        public void DeleteSteamLocalUserData(SteamUser user)
        {
            vdfService.DeleteVdfValueByKey(SteamPath + UserVdfPath, user.SteamId64.ToString());
            var temp = SteamPath + Path.Combine(UserDataDirectory, user.SteamId3_Int.ToString());
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        public void UpdateSteamLocalUserData(SteamUser user)
        {
            vdfService.UpdateVdfValueByReplace(SteamPath + UserVdfPath, user.OriginVdfString, user.CurrentVdfString);
        }
    }
}
