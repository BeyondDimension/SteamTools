using System.Application.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Application.Services.Implementation
{
    internal sealed class SteamServiceImpl : ISteamService
    {
        const string TAG = "SteamS";
        /// <summary>
        /// <list type="bullet">
        ///   <item>
        ///     Windows：~\Steam\config\loginusers.vdf
        ///   </item>
        ///   <item>
        ///     Linux：~/.steam/steam/config/loginusers.vdf
        ///   </item>
        ///   <item>
        ///     Mac：~/Library/Application Support/Steam/config/loginusers.vdf
        ///   </item>
        /// </list>
        /// </summary>
        const string UserVdfPath = @"\config\loginusers.vdf";
        const string UserDataDirectory = @"\userdata";
        readonly IDesktopPlatformService platformService;
        readonly Lazy<string?> mSteamDirPath;
        readonly Lazy<string?> mSteamProgramPath;
        readonly string[] steamProcess = new[] { "steam", "steamservice", "steamwebhelper" };

        public SteamServiceImpl(IDesktopPlatformService platformService)
        {
            this.platformService = platformService;
            mSteamDirPath = new Lazy<string?>(platformService.GetSteamDirPath);
            mSteamProgramPath = new Lazy<string?>(platformService.GetSteamProgramPath);
        }

        public string? SteamDirPath => mSteamDirPath.Value;

        public string? SteamProgramPath => mSteamProgramPath.Value;

        public void KillSteamProcess()
        {
            var processes = Process.GetProcesses();
            foreach (var p in processes)
            {
                if (steamProcess.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase))
                {
                    p.Kill();
                }
            }
        }

        public bool TryKillSteamProcess()
        {
            try
            {
                KillSteamProcess();
                return true;
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "KillSteamProcess Fail.");
                return false;
            }
        }

        public int? IsRunningSteamProcess()
        {
            var processes = Process.GetProcesses();
            foreach (var p in processes)
            {
                if (steamProcess.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase))
                {
                    return p.Id;
                }
            }
            return default;
        }

        public void StartSteam(string arguments)
        {
            if (!string.IsNullOrEmpty(SteamProgramPath))
            {
                Process.Start(SteamProgramPath, arguments);
            }
        }

        public string GetLastLoginUserName() => platformService.GetLastSteamLoginUserName();

        public List<SteamUser> GetRememberUserList()
        {
            var users = new List<SteamUser>();
            try
            {
                if (File.Exists(SteamDirPath + UserVdfPath))
                {
                    // 注意：动态类型在移动端受限，且运行时可能抛出异常
                    dynamic v = VdfHelper.Read(SteamDirPath + UserVdfPath);
                    foreach (var item in v.Value)
                    {
                        try
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

                            // 老版本 Steam 数据 小写 mostrecent 支持
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
                        catch (Exception e)
                        {
                            Log.Error(TAG, e, "GetRememberUserList Fail(0).");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "GetRememberUserList Fail(1).");
            }
            return users;
        }

        public List<SteamApp>? GetAppListJson(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var lastChanged = File.GetLastWriteTime(filePath);
            int daysSinceChanged = (int)(DateTime.Now - lastChanged).TotalDays;
            if (daysSinceChanged > 10)
            {
                return null;
            }

            string json = File.ReadAllText(filePath, Encoding.UTF8);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var apps = Serializable.DJSON<SteamApps?>(json);
            return apps?.AppList?.Apps;
        }

        public bool UpdateAppListJson(List<SteamApp> apps, string filePath)
        {
            try
            {
                var json_str = Serializable.SJSON(apps);
                File.WriteAllText(filePath, json_str, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "UpdateAppListJson(obj) Fail.");
                return false;
            }
        }

        public bool UpdateAppListJson(string appsJsonStr, string filePath)
        {
            try
            {
                File.WriteAllText(filePath, appsJsonStr, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "UpdateAppListJson(str) Fail.");
                return false;
            }
        }

        public void DeleteLocalUserData(SteamUser user)
        {
            VdfHelper.DeleteValueByKey(SteamDirPath + UserVdfPath, user.SteamId64.ToString());
            var temp = SteamDirPath + Path.Combine(UserDataDirectory, user.SteamId3_Int.ToString());
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }

        public void UpdateLocalUserData(SteamUser user)
        {
            var originVdfStr = user.OriginVdfString;
            VdfHelper.UpdateValueByReplace(
                SteamDirPath + UserVdfPath,
                originVdfStr.ThrowIsNull(nameof(originVdfStr)),
                user.CurrentVdfString);
        }

#if DEBUG

        [Obsolete("use AppHelper.SetBootAutoStart", true)]
        public void SetWindowsStartupAutoRun(bool IsAutoRun, string Name)
            => throw new NotImplementedException();

        [Obsolete("use SteamDirPath", true)]
        public string? SteamPath { get; }

        [Obsolete("use SteamProgramPath", true)]
        public string? SteamExePath { get; }

#endif
    }
}