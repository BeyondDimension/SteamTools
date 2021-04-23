using System.Application.Models;
using System.Application.Models.Settings;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        readonly string? UserVdfPath;
        readonly string? AppInfoPath;
        readonly string? LibrarycacheDirPath;
        const string UserDataDirectory = "userdata";
        readonly IDesktopPlatformService platformService;
        readonly string? mSteamDirPath;
        readonly string? mSteamProgramPath;
        readonly string[] steamProcess = new[] { "steam", "steamservice", "steamwebhelper" };
        readonly IHttpService http;

        public SteamServiceImpl(IDesktopPlatformService platformService, IHttpService http)
        {
            this.platformService = platformService;
            this.http = http;
            mSteamDirPath = platformService.GetSteamDirPath();
            mSteamProgramPath = platformService.GetSteamProgramPath();
            UserVdfPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "config", "loginusers.vdf");
            AppInfoPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "appcache", "appinfo.vdf");
            LibrarycacheDirPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "appcache", "librarycache");

            if (!File.Exists(UserVdfPath)) UserVdfPath = null;
            if (!File.Exists(AppInfoPath)) AppInfoPath = null;
            if (!Directory.Exists(LibrarycacheDirPath)) LibrarycacheDirPath = null;
        }

        public string? SteamDirPath => mSteamDirPath;

        public string? SteamProgramPath => mSteamProgramPath;

        public bool IsRunningSteamProcess
        {
            get
            {
                return Process.GetProcesses().Any(p => steamProcess.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase));
            }
        }

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

                //if (IsRunningSteamProcess)
                //{
                //    Process closeProc = Process.Start(new ProcessStartInfo(SteamProgramPath, "-shutdown"));
                //    bool closeProcSuccess = closeProc != null && closeProc.WaitForExit(3000);
                //    return closeProcSuccess;
                //}
                //return false;
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "KillSteamProcess Fail.");
                return false;
            }
        }

        public int? GetSteamProcessPid()
        {
            var processes = Process.GetProcesses();
            foreach (var p in processes)
            {
                if (string.Equals(p.ProcessName, steamProcess[0], StringComparison.OrdinalIgnoreCase))
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
                if (!string.IsNullOrWhiteSpace(UserVdfPath) && File.Exists(UserVdfPath))
                {
                    // 注意：动态类型在移动端受限，且运行时可能抛出异常
                    dynamic v = VdfHelper.Read(UserVdfPath);
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
                            user.LastLoginTime = user.Timestamp.ToDateTimeS();

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

        public void SetCurrentUser(string userName) => platformService.SetCurrentUser(userName);

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
            if (string.IsNullOrWhiteSpace(UserVdfPath) || string.IsNullOrWhiteSpace(SteamDirPath))
            {
                return;
            }
            else
            {
                VdfHelper.DeleteValueByKey(UserVdfPath, user.SteamId64.ToString());
                var temp = Path.Combine(SteamDirPath, UserDataDirectory, user.SteamId3_Int.ToString());
                if (Directory.Exists(temp))
                {
                    Directory.Delete(temp, true);
                }
            }
        }

        public void UpdateLocalUserData(SteamUser user)
        {
            if (string.IsNullOrWhiteSpace(UserVdfPath))
            {
                return;
            }
            else
            {
                var originVdfStr = user.OriginVdfString;
                VdfHelper.UpdateValueByReplace(
                    UserVdfPath,
                    originVdfStr.ThrowIsNull(nameof(originVdfStr)),
                    user.CurrentVdfString);
            }
        }

        private uint unknownValueAtStart;
        private const uint MagicNumber = 123094055U;

        /// <summary>
        /// 从steam本地客户端缓存文件中读取游戏数据
        /// </summary>
        public /*async*/ Task<List<SteamApp>> GetAppInfos()
        {
            return Task.FromResult(GetAppInfos_());
            List<SteamApp> GetAppInfos_()
            {
                var apps = new List<SteamApp>();
                if (string.IsNullOrEmpty(AppInfoPath) && !File.Exists(AppInfoPath))
                    return apps;
                using (BinaryReader binaryReader = new(File.OpenRead(AppInfoPath)))
                {
                    uint num = binaryReader.ReadUInt32();
                    if (num != MagicNumber)
                    {
                        Log.Error(nameof(GetAppInfos), string.Format("\"{0}\" magic code is not supported: 0x{1:X8}", Path.GetFileName(AppInfoPath), num));
                        return apps;
                    }
                    SteamApp? app = new();
                    unknownValueAtStart = binaryReader.ReadUInt32();
                    while ((app = SteamApp.FromReader(binaryReader)) != null)
                    {
                        if (app.AppId > 0)
                        {
                            if (GameLibrarySettings.DefaultIgnoreList.Contains(app.AppId))
                                continue;
                            if (app.ParentId > 0)
                            {
                                var parentApp = apps.FirstOrDefault(f => f.AppId == app.ParentId);
                                if (parentApp != null)
                                    parentApp.ChildApp.Add(app.AppId);
                                //continue;
                            }
                            apps.Add(app);
                            //app.Modified += (s, e) =>
                            //{
                            //};
                        }
                    }
                }
                return apps;
            }
        }

        public string GetAppLibCacheFilePath(uint appId, SteamApp.LibCacheType type)
        {
            var fileName = type switch
            {
                SteamApp.LibCacheType.Header => $"{appId}_header.jpg",
                SteamApp.LibCacheType.Icon => $"{appId}_icon.jpg",
                SteamApp.LibCacheType.Library_600x900 => $"{appId}_library_600x900.jpg",
                SteamApp.LibCacheType.Library_Hero => $"{appId}_library_hero.jpg",
                SteamApp.LibCacheType.Library_Hero_Blur => $"{appId}_library_hero_blur.jpg",
                SteamApp.LibCacheType.Logo => $"{appId}_logo.png",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
            var filePath = Path.Combine(LibrarycacheDirPath, fileName);
            return filePath;
        }

        public async Task<string> GetAppImageAsync(SteamApp app, SteamApp.LibCacheType type)
        {
            var cacheFilePath = GetAppLibCacheFilePath(app.AppId, type);
            if (File.Exists(cacheFilePath)) return cacheFilePath;
            var url = type switch
            {
                SteamApp.LibCacheType.Header => app.HeaderLogoUrl,
                SteamApp.LibCacheType.Icon => app.IconUrl,
                SteamApp.LibCacheType.Library_600x900 => app.LibraryLogoUrl,
                SteamApp.LibCacheType.Library_Hero => app.LibraryHeaderUrl,
                SteamApp.LibCacheType.Library_Hero_Blur => app.LibraryHeaderBlurStream,
                SteamApp.LibCacheType.Logo => app.LibraryNameUrl,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
            if (url == null) return string.Empty;
            var value = await http.GetImageAsync(url, ImageChannelType.SteamGames);
            return value ?? string.Empty;
        }

        public async ValueTask LoadAppImageAsync(SteamApp app)
        {
            if (app.LibraryLogoStream == null)
            {
                app.LibraryLogoStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Library_600x900);
            }
            //if (app.LibraryHeaderStream == null)
            //{
            //    app.LibraryHeaderStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Library_Hero);
            //}
            //if (app.LibraryHeaderBlurStream == null)
            //{
            //    app.LibraryHeaderBlurStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Library_Hero_Blur);
            //}
            //if (app.LibraryNameStream == null)
            //{
            //    app.LibraryNameStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Logo);
            //}
            //if (app.HeaderLogoStream == null)
            //{
            //    app.HeaderLogoStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Header);
            //}
        }


    }
}