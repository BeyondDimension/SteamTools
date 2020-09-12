using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using SteamTool.Common.Helper;
using SteamTools.Model;

namespace SteamTools.Helper
{
    public class SteamHelper
    {
        public static readonly SteamHelper Instance = new SteamHelper();

        public string SteamPath { get; set; }

        public string SteamExePath { get; set; }

        private const string SteamRegistryPath = @"SOFTWARE\Valve\Steam";

        public SteamHelper()
        {
            var steamExePath = RegistryKeyHelper.ReadRegistryKey(Registry.CurrentUser, SteamRegistryPath, "SteamExe");
            var steamPath = RegistryKeyHelper.ReadRegistryKey(Registry.CurrentUser, SteamRegistryPath, "SteamPath");
            if (File.Exists(steamExePath))
            {
                SteamExePath = steamExePath;
            }
            if (Directory.Exists(steamPath))
            {
                SteamPath = steamPath;
            }
        }

        public void KillSteamProcess()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName == "Steam" || process.ProcessName == "SteamService" || process.ProcessName == "steamwebhelper")
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
            return RegistryKeyHelper.ReadRegistryKey(Registry.CurrentUser, SteamRegistryPath, "AutoLoginUser");
        }

        /// <summary>
        /// 获取所有记住登陆steam用户信息
        /// </summary>
        /// <returns></returns>
        public List<SteamUser> GetAllUser()
        {
            var users = new List<SteamUser>();
            if (SteamPath != null)
            {
                var v = VdfHelper.GetVdfModelByPath(SteamPath + "/config/loginusers.vdf");

                foreach (var item in v.Value)
                {
                    var i = item.Value;
                    var user = new SteamUser
                    {
                        AccountName = i.AccountName.ToString(),
                        PersonaName = i.PersonaName.ToString(),
                        RememberPassword = Convert.ToBoolean(Convert.ToInt64(i.RememberPassword.ToString())),
                        MostRecent = Convert.ToBoolean(Convert.ToInt64(i.MostRecent.ToString())),
                        Timestamp = Convert.ToInt64(i.Timestamp.ToString())
                    };
                    user.LastLoginTime= new DateTime(1970, 1, 1, 8, 0, 0).AddSeconds(user.Timestamp);
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
            RegistryKeyHelper.AddOrUpdateRegistryKey(Registry.CurrentUser, SteamRegistryPath, "AutoLoginUser", username, RegistryValueKind.String);
        }

    }
}
