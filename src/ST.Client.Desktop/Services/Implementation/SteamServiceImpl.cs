using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Application.Services.ISteamService;

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
        readonly string? ConfigVdfPath;
        readonly string? AppInfoPath;
        readonly string? LibrarycacheDirPath;
        const string UserDataDirectory = "userdata";
        readonly IDesktopPlatformService platformService;
        readonly string? mSteamDirPath;
        readonly string? mSteamProgramPath;
        readonly string[] steamProcess = new[] { "steam", "steam_osx", "steamservice", "steamwebhelper" };
        readonly Lazy<IHttpService> _http = new(() => DI.Get<IHttpService>());
        IHttpService Http => _http.Value;

        public SteamServiceImpl(IDesktopPlatformService platformService)
        {
            this.platformService = platformService;
            mSteamDirPath = platformService.GetSteamDirPath();
            mSteamProgramPath = platformService.GetSteamProgramPath();
            UserVdfPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "config", "loginusers.vdf");
            AppInfoPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "appcache", "appinfo.vdf");
            LibrarycacheDirPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "appcache", "librarycache");
            ConfigVdfPath = SteamDirPath == null ? null : Path.Combine(SteamDirPath, "config", "config.vdf");

            if (!File.Exists(UserVdfPath)) UserVdfPath = null;
            if (!File.Exists(AppInfoPath)) AppInfoPath = null;
            if (!File.Exists(ConfigVdfPath)) ConfigVdfPath = null;
            if (!Directory.Exists(LibrarycacheDirPath)) LibrarycacheDirPath = null;
        }

        public string? SteamDirPath => mSteamDirPath;

        public string? SteamProgramPath => mSteamProgramPath;

        public bool IsRunningSteamProcess
        {
            get
            {
                return Process.GetProcessesByName(steamProcess[0]).Any_Nullable();
            }
        }

        public void KillSteamProcess()
        {
            foreach (var p in steamProcess)
            {
                var process = Process.GetProcessesByName(p);
                foreach (var item in process)
                {
                    if (item.HasExited == false)
                    {
                        item.Kill();
                        item.WaitForExit();
                    }
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
            finally
            {
                SteamConnectService.Current.IsConnectToSteam = false;
            }
        }

        public int? GetSteamProcessPid()
        {
            var processes = Process.GetProcessesByName(steamProcess[0]);
            if (processes.Any_Nullable())
                return processes.First().Id;
            return default;
        }

        public void StartSteam(string? arguments = null)
        {
            if (!string.IsNullOrEmpty(SteamProgramPath))
            {
                if (!string.IsNullOrEmpty(arguments))
                    DI.Get<IDesktopPlatformService>().OpenProcess(SteamProgramPath, arguments);
                else
                    DI.Get<IDesktopPlatformService>().OpenProcess(SteamProgramPath);
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
        public bool UpdateAuthorizedDeviceList(IEnumerable<AuthorizedDevice> model)
        {
            var authorizeds = new List<AuthorizedDevice>();
            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigVdfPath) && File.Exists(ConfigVdfPath))
                {
                    dynamic v = VdfHelper.Read(ConfigVdfPath);
                    var authorizedDevice = v.Value.AuthorizedDevice;
                    if (authorizedDevice != null)
                    { 
                        foreach (var item in authorizedDevice)
                        {
                            try
                            {
                                var i = item.Value;
                                authorizeds.Add(new AuthorizedDevice(item.ToString())
                                { 
                                    SteamId3_Int = Convert.ToInt64(item.Key.ToString()),
                                    Timeused = Convert.ToInt64(i.timeused.ToString()),
                                    Description = i.description.ToString(),
                                    Tokenid = i.tokenid.ToString(),
                                }); 
                            }
                            catch (Exception e)
                            {
                                Log.Error(TAG, e, "GetAuthorizedDeviceList Fail(0).");
                            }
                        }
                        var oldStr = $"\t{{\n{string.Join("\n", authorizeds.Select(x => x.CurrentVdfString))}\n\t}}".TrimEnd("\n");
                        //authorizedDevice.Select(x => x.ToString());
                        var newStr = $"\t{{\n{string.Join("\n", model.OrderBy(x => x.Index).Select(x => x.CurrentVdfString))}\n\t}}".TrimEnd("\n");
                        VdfHelper.UpdateValueByReplaceNoPattern(ConfigVdfPath, oldStr, newStr);
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "UpdateAuthorizedDeviceList Fail(0).");
                return false;
            }
            return false;
        }
        public bool RemoveAuthorizedDeviceList(AuthorizedDevice model)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigVdfPath) && File.Exists(ConfigVdfPath))
                {
                    VdfHelper.DeleteValueByKey(ConfigVdfPath, model.OriginVdfString.ThrowIsNull(nameof(model.OriginVdfString)));
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "RemoveAuthorizedDeviceList Fail(0).");
                return false;
            }
            return false;
        }
        public List<AuthorizedDevice> GetAuthorizedDeviceList()
        {
            var authorizeds = new List<AuthorizedDevice>();
            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigVdfPath) && File.Exists(ConfigVdfPath))
                {
                    // 注意：动态类型在移动端受限，且运行时可能抛出异常
                    dynamic v = VdfHelper.Read(ConfigVdfPath);
                    var authorizedDevice = v.Value.AuthorizedDevice;
                    if (authorizedDevice != null)
                    {
                        var index=0;
                        foreach (var item in authorizedDevice)
                        {
                            try
                            {
                                var i = item.Value;
                                authorizeds.Add(new AuthorizedDevice(item.ToString())
                                {
                                    Index= index,
                                    SteamId3_Int = Convert.ToInt64(item.Key.ToString()),
                                    Timeused = Convert.ToInt64(i.timeused.ToString()),
                                    Description = i.description.ToString(),
                                    Tokenid = i.tokenid.ToString(),
                                });
                                index++;
                            }
                            catch (Exception e)
                            {
                                Log.Error(TAG, e, "GetAuthorizedDeviceList Fail(0).");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "GetAuthorizedDeviceList Fail(1).");
            }
            return authorizeds;
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

        public void DeleteLocalUserData(SteamUser user, bool IsDeleteUserData = false)
        {
            if (string.IsNullOrWhiteSpace(UserVdfPath) || string.IsNullOrWhiteSpace(SteamDirPath))
            {
                return;
            }
            else
            {
                VdfHelper.DeleteValueByKey(UserVdfPath, user.SteamId64.ToString());
                if (IsDeleteUserData)
                {
                    var temp = Path.Combine(SteamDirPath, UserDataDirectory, user.SteamId3_Int.ToString());
                    if (Directory.Exists(temp))
                    {
                        Directory.Delete(temp, true);
                    }
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
                try
                {
                    if (string.IsNullOrEmpty(AppInfoPath) && !File.Exists(AppInfoPath))
                        return apps;
                    using (BinaryReader binaryReader = new(IOPath.OpenRead(AppInfoPath)))
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
                                if (GameLibrarySettings.DefaultIgnoreList.Value.Contains(app.AppId))
                                    continue;
                                if (GameLibrarySettings.HideGameList.Value!.ContainsKey(app.AppId))
                                    continue;
                                //if (app.ParentId > 0)
                                //{
                                //    var parentApp = apps.FirstOrDefault(f => f.AppId == app.ParentId);
                                //    if (parentApp != null)
                                //        parentApp.ChildApp.Add(app.AppId);
                                //    //continue;
                                //}
                                apps.Add(app);
                                //app.Modified += (s, e) =>
                                //{
                                //};
                            }
                        }
                    }
                    return apps;
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(SteamServiceImpl), ex, nameof(GetAppInfos));
                    GC.Collect();
                    return apps;
                }
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
            var value = await Http.GetImageAsync(url, ImageChannelType.SteamGames);
            return value ?? string.Empty;
        }

        public /*async*/ ValueTask LoadAppImageAsync(SteamApp app)
        {
            return ValueTask.CompletedTask;
            //if (app.LibraryLogoStream == null)
            //{
            //    app.LibraryLogoStream = await GetAppImageAsync(app, SteamApp.LibCacheType.Library_600x900);
            //}
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

        #region LoginUsingSteamClient

        public async Task<(LoginUsingSteamClientResultCode resultCode, CookieCollection? cookies)> GetLoginUsingSteamClientCookieCollectionAsync(bool runasInvoker = false)
        {
            (LoginUsingSteamClientResultCode resultCode, string[]? cookies) result;
            if (runasInvoker && DI.Platform == Platform.Windows && platformService.IsAdministrator)
            {
                result = await Task.Run(GetLoginUsingSteamClientCookies);
            }
            else
            {
                result = await GetLoginUsingSteamClientCookiesAsync();
            }
            CookieCollection? cookieCollection;
            if (result.resultCode == LoginUsingSteamClientResultCode.Success)
            {
                cookieCollection = GetCookieCollection(uri_store_steampowered_checkclientautologin, result.cookies);
            }
            else
            {
                cookieCollection = default;
            }
            return (result.resultCode, cookieCollection);
        }

        async Task<(LoginUsingSteamClientResultCode resultCode, string steamid, string encrypted_loginkey, string sessionkey, string digest)> GetLoginUsingSteamClientAuthAsync()
        {
            LoginUsingSteamClientResultCode resultCode;
            var canConn = await CanConnSteamCommunity();
            if (canConn)
            {
                try
                {
                    var client = Http.Factory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(.85);
                    var request = new HttpRequestMessage(HttpMethod.Get, url_localhost_auth_public);
                    request.Headers.Add("Origin", url_store_steampowered);
                    request.Headers.Add("Accept", MediaTypeNames.JSON);
                    request.Headers.UserAgent.ParseAdd(Http.PlatformHelper.UserAgent);
                    var response = await client.SendAsync(request);
#if DEBUG
                    Console.WriteLine("GetLoginUsingSteamClientAuthAsync");
                    Console.WriteLine($"StatusCode: {response.StatusCode}");
#endif
                    if (response.IsSuccessStatusCode)
                    {
                        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        using var json = new JsonTextReader(reader);
                        var jsonObj = JObject.Load(json);
                        var steamid = jsonObj["steamid"]!.ToString();
#if DEBUG
                        Console.WriteLine($"steamid: {steamid}");
#endif
                        var encrypted_loginkey = jsonObj["encrypted_loginkey"]!.ToString();
#if DEBUG
                        Console.WriteLine($"encrypted_loginkey: {encrypted_loginkey}");
#endif
                        var sessionkey = jsonObj["sessionkey"]!.ToString();
#if DEBUG
                        Console.WriteLine($"sessionkey: {sessionkey}");
#endif
                        var digest = jsonObj["digest"]!.ToString();
#if DEBUG
                        Console.WriteLine($"digest: {digest}");
#endif
                        resultCode = LoginUsingSteamClientResultCode.Success;
                        return (resultCode, steamid, encrypted_loginkey, sessionkey, digest);
                    }
                    else
                    {
                        resultCode = (LoginUsingSteamClientResultCode)response.StatusCode;
                    }
                }
                catch (OperationCanceledException)
                {
                    resultCode = LoginUsingSteamClientResultCode.Canceled;
                }
                catch (Exception)
                {
                    resultCode = LoginUsingSteamClientResultCode.Exception1;
                }
            }
            else
            {
                resultCode = LoginUsingSteamClientResultCode.CantConnSteamCommunity;
            }
            return (resultCode, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        const double url_steamcommunity_timeout_s = 5.95;

        async Task<bool> CanConnSteamCommunity()
        {
            try
            {
                var client = Http.Factory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(url_steamcommunity_timeout_s);
                var request = new HttpRequestMessage(HttpMethod.Get, url_store_steampowered);
                request.Headers.UserAgent.ParseAdd(Http.PlatformHelper.UserAgent);
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                return response.IsSuccessStatusCode;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
            }
            return default;
        }

        async Task<(LoginUsingSteamClientResultCode resultCode, string[]? cookies)> GetLoginUsingSteamClientCookiesAsync((string steamid, string encrypted_loginkey, string sessionkey, string digest) auth_data)
        {
            LoginUsingSteamClientResultCode resultCode;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, uri_store_steampowered_checkclientautologin)
                {
#pragma warning disable CS8620 // 由于引用类型的可为 null 性差异，实参不能用于形参。
                    Content = new FormUrlEncodedContent(new Dictionary<string, string?>
                {
                    { "steamid", auth_data.steamid },
                    { "sessionkey",  auth_data.sessionkey },
                    { "encrypted_loginkey", auth_data.encrypted_loginkey },
                    { "digest",  auth_data.digest },
                }),
#pragma warning restore CS8620 // 由于引用类型的可为 null 性差异，实参不能用于形参。
                };
                request.Headers.Add("Origin", url_store_steampowered);
                request.Headers.Add("Accept", MediaTypeNames.JSON);
                request.Headers.UserAgent.ParseAdd(Http.PlatformHelper.UserAgent);
                var client = Http.Factory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(url_steamcommunity_timeout_s);
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
#if DEBUG
                Console.WriteLine($"GetLoginUsingSteamClientCookiesAsync statusCode: {response.StatusCode}");
#endif
                if (response.IsSuccessStatusCode && response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    var r = cookies.ToArray();
#if DEBUG
                    foreach (var item in r)
                    {
                        Console.WriteLine($"Set-Cookie: {item}");
                    }
                    Console.WriteLine("OK");
#endif
                    resultCode = LoginUsingSteamClientResultCode.Success;
                    return (resultCode, r);
                }
                else
                {
#if DEBUG
                    Console.WriteLine("Missing response headers set-cookie");
#endif
                    resultCode = response.StatusCode == HttpStatusCode.OK ?
                        LoginUsingSteamClientResultCode.MissingCookieSteamLoginSecure :
                        (LoginUsingSteamClientResultCode)response.StatusCode;
                }
            }
            catch (Exception)
            {
                resultCode = LoginUsingSteamClientResultCode.Exception2;
            }
            return (resultCode, default);
        }

        public async Task<(LoginUsingSteamClientResultCode resultCode, string[]? cookies)> GetLoginUsingSteamClientCookiesAsync()
        {
            var (resultCode, steamid, encrypted_loginkey, sessionkey, digest) = await GetLoginUsingSteamClientAuthAsync();
#if DEBUG
            Console.WriteLine($"GetLoginUsingSteamClientAuthAsync resultCode: {resultCode}");
#endif
            if (resultCode != LoginUsingSteamClientResultCode.Success)
            {
                return (resultCode, default);
            }
            var cookies = await GetLoginUsingSteamClientCookiesAsync((steamid, encrypted_loginkey, sessionkey, digest));
            return cookies;
        }

        [SupportedOSPlatform("Windows")]
        (LoginUsingSteamClientResultCode resultCode, string[]? cookies) GetLoginUsingSteamClientCookies()
        {
            if (AppHelper.ProgramPath.EndsWith(FileEx.EXE, StringComparison.OrdinalIgnoreCase))
            {
                var consoleProgramPath = AppHelper.ProgramPath.Substring(0, AppHelper.ProgramPath.Length - FileEx.EXE.Length) + ".Console" + FileEx.EXE;
                if (File.Exists(consoleProgramPath))
                {
                    var tempFileDirectoryName = IOPath.CacheDirectory;
                    var tempFileName = Path.GetFileName(Path.GetTempFileName());
                    var tempFilePath = Path.Combine(tempFileDirectoryName, tempFileName);
                    var isDelTempFilePath = false;
                    IOPath.FileIfExistsItDelete(tempFilePath);

                    byte[]? value = null;
                    using var rsa = RSA.Create(2048);

                    using (var watcher = new FileSystemWatcher(tempFileDirectoryName, tempFileName)
                    {
                        NotifyFilter = NotifyFilters.Attributes
                            | NotifyFilters.CreationTime
                            | NotifyFilters.DirectoryName
                            | NotifyFilters.FileName
                            | NotifyFilters.LastAccess
                            | NotifyFilters.LastWrite
                            | NotifyFilters.Security
                            | NotifyFilters.Size,
                    })
                    {
                        var connStr = tempFilePath;
                        var rsaPK = rsa.ToJsonString(false);
                        var key = Serializable.SMPB64U((connStr, rsaPK));
                        try
                        {
                            var command = $"runas.exe /trustlevel:0x20000 \"\"{consoleProgramPath}\" getstmauth -key \"{key}\"\"";
                            platformService.UnelevatedProcessStart(command);

                            watcher.WaitForChanged(WatcherChangeTypes.Changed, IPC_Call_GetLoginUsingSteamClient_Timeout_MS);
                            if (File.Exists(tempFilePath))
                            {
                                value = File.ReadAllBytes(tempFilePath);
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (value != null)
                    {
                        try
                        {
                            File.Delete(tempFilePath);
                            isDelTempFilePath = true;
                        }
                        catch
                        {
                            isDelTempFilePath = false;
                        }
                        try
                        {
                            var fileBytes = Serializable.DMP<(byte[] cookiesBytes, byte[] aesKey)>(value);
                            var aesKey = rsa.Decrypt(fileBytes.aesKey);
                            using var aes = AESUtils.Create(aesKey);
                            var cookiesBytes = aes.Decrypt(fileBytes.cookiesBytes);
                            var cookies = Serializable.DMP<(LoginUsingSteamClientResultCode resultCode, string[]? result)>(cookiesBytes);
                            return cookies;
                        }
                        catch
                        {
                        }
                        finally
                        {
                            if (!isDelTempFilePath)
                            {
                                try
                                {
                                    IOPath.FileIfExistsItDelete(tempFilePath, true);
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
            return default;
        }

        static CookieCollection? GetCookieCollection(Uri url, IEnumerable<string>? cookies)
        {
            if (cookies == null) return null;
            CookieContainer container = new();
            foreach (var item in cookies)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                container.SetCookies(url, item);
            }
            var cookies2 = container.GetCookies(url);
            return cookies2;
        }

        #endregion
    }
}