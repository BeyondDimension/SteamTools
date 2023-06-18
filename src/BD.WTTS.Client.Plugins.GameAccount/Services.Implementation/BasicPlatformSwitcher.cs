using AvaloniaEdit;
using BD.WTTS.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.CommandLine;
using System.Threading.Tasks;
using System.Xml;

namespace BD.WTTS.Services.Implementation;

public sealed class BasicPlatformSwitcher : IPlatformSwitcher
{
    readonly IPlatformService platform;

    public BasicPlatformSwitcher(IPlatformService platform)
    {
        this.platform = platform;
    }

    private bool BasicCopyInAccount(string accId, PlatformAccount platform)
    {
        var allIds = JTokenHelper.ReadDict(platform.IdsJsonPath);
        var accName = allIds[accId];

        var localCachePath = Path.Combine(platform.PlatformLoginCache, accName);
        _ = Directory.CreateDirectory(localCachePath);

        if (platform.LoginFiles == null)
            throw new Exception("No data in basic platform: " + platform.FullName);

        if (string.IsNullOrEmpty(platform.UniqueIdPath))
            throw new Exception("No data in basic platform: " + platform.FullName);

        // Get unique ID from IDs file if unique ID is a registry key. Set if exists.
        if (OperatingSystem.IsWindows() && platform.UniqueIdType is UniqueIdType.REGKEY && !string.IsNullOrEmpty(platform.UniqueIdPath))
        {
            var uniqueId = JTokenHelper.ReadDict(platform.FullName).FirstOrDefault(x => x.Value == accName).Key;

            if (!string.IsNullOrEmpty(uniqueId) && !RegistryKeyHelper.SetRegistryKey(platform.UniqueIdPath, uniqueId)) // Remove "REG:" and read data
            {
                Toast.Show("已经使用此账号登录");
                return false;
            }
        }

        var regJson = platform.UniqueIdPath.StartsWith("REG:") ? ReadRegJson(accName) : new Dictionary<string, string>();

        foreach (var (accFile, savedFile) in platform.LoginFiles)
        {
            // The "file" is a registry key
            if (OperatingSystem.IsWindows() && accFile.StartsWith("REG:"))
            {
                if (!regJson.ContainsKey(accFile))
                {
                    Toast.Show("读取已保存的注册表项失败。如果不成功，请尝试删除并重新添加帐户。");
                    continue;
                }

                var regValue = regJson[accFile] ?? "";

                if (!RegistryKeyHelper.SetRegistryKey(accFile[4..], regValue)) // Remove "REG:" and read data
                {
                    Toast.Show("写入 Windows 注册表失败");
                    return false;
                }
                continue;
            }

            // The "file" is a JSON value
            if (accFile.StartsWith("JSON"))
            {
                JToken? jToken = null;
                JTokenHelper.TryReadJsonFile(Path.Join(localCachePath, savedFile), ref jToken);

                var path = accFile.Split("::")[1];
                var selector = accFile.Split("::")[2];
                if (!JTokenHelper.ReplaceVarInJsonFile(path, selector, jToken))
                {
                    Toast.Show("修改JSON文件失败");
                    return false;
                }
                continue;
            }

            // FILE OR FOLDER
            PathHelper.HandleFileOrFolder(accFile, savedFile, localCachePath, true);
        }

        return true;
    }

    public void SwapToAccount(IAccount account, PlatformAccount platform)
    {
        //LoadAccountIds();
        var accName = account.AccountName;

        if (!KillPlatformProcess())
            return;

        // Add currently logged in account if there is a way of checking unique ID.
        // If saved, and has unique key: Update
        if (platform.UniqueIdPath is not null)
        {
            var uniqueId = GetUniqueId(platform);

            // UniqueId Found >> Save!
            if (File.Exists(platform.IdsJsonPath))
            {
                if (!string.IsNullOrEmpty(uniqueId) && platform.Accounts.Any_Nullable(acc => acc.AccountId == uniqueId))
                {
                    if (account.AccountId == uniqueId)
                    {
                        //if (platform.AutoStart)
                        //{
                        RunPlatformProcess(platform, true);
                        //? Toast.Show("12123123");
                        //:Toast.Show("12123123");
                        //}
                        //Toast.Show("12123123");

                        return;
                    }
                    CurrnetUserAdd(platform.Accounts.First(acc => acc.AccountId == uniqueId).AccountName ?? "Unknown", platform);
                }
            }
        }

        // Clear current login
        ClearCurrentLoginUser(platform);

        // Copy saved files in
        if (accName != "")
        {
            if (!BasicCopyInAccount(account.AccountId!, platform)) return;
            //Globals.AddTrayUser(platform.SafeName, $"+{platform.PrimaryId}:" + accId, accName, BasicSettings.TrayAccNumber); // Add to Tray list, using first Identifier
        }

        //if (BasicSettings.AutoStart)
        RunPlatformProcess(platform, true);

        //if (accName != "" && BasicSettings.AutoStart && AppSettings.MinimizeOnSwitch) _ = AppData.InvokeVoidAsync("hideWindow");

        //NativeFuncs.RefreshTrayArea();
        //_ = AppData.InvokeVoidAsync("updateStatus", Lang["Done"]);
        //AppStats.IncrementSwitches(CurrentPlatform.SafeName);

        //try
        //{
        //    BasicSettings.LastAccName = accId;
        //    BasicSettings.LastAccTimestamp = Globals.GetUnixTimeInt();
        //    if (BasicSettings.LastAccName != "") _ = AppData.InvokeVoidAsync("highlightCurrentAccount", BasicSettings.LastAccName);
        //}
        //catch (Exception)
        //{
        //    //
        //}
    }

    public bool ClearCurrentLoginUser(PlatformAccount platform)
    {
        // Foreach file/folder/reg in Platform.PathListToClear
        if (platform.ClearPaths.Any_Nullable(accFile => !DeleteFileOrFolder(accFile, platform)))
            return false;

        var uniqueIdFile = PathHelper.ExpandEnvironmentVariables(platform.UniqueIdPath);

        if (platform.UniqueIdType is UniqueIdType.JSON_SELECT or UniqueIdType.JSON_SELECT_FIRST or UniqueIdType.JSON_SELECT_LAST)
        {
            var path = uniqueIdFile.Split("::")[0];
            var selector = uniqueIdFile.Split("::")[1];
            JTokenHelper.ReplaceVarInJsonFile(path, selector, "");
        }

        if (platform.UniqueIdType != UniqueIdType.CREATE_ID_FILE) return true;

        // Unique ID file --> This needs to be deleted for a new instance
        PathHelper.DeleteFile(uniqueIdFile);

        return true;
    }

    private bool DeleteFileOrFolder(string accFile, PlatformAccount platform)
    {
        // The "file" is a registry key
        if (OperatingSystem.IsWindows() && accFile.StartsWith("REG:"))
        {
            // If set to clear LoginCache for account before adding (Enabled by default):
            if (platform.IsRegDeleteOnClear)
            {
                if (RegistryKeyHelper.DeleteRegistryKey(accFile[4..])) return true;
            }
            else
            {
                if (RegistryKeyHelper.SetRegistryKey(accFile[4..])) return true;
            }
            Toast.Show("写入 Windows 注册表失败");
            return false;
        }

        // The "file" is a JSON value
        if (accFile.StartsWith("JSON"))
        {
            if (accFile.StartsWith("JSON_SELECT"))
            {
                var path = accFile.Split("::")[1];
                var selector = accFile.Split("::")[2];
                JTokenHelper.ReplaceVarInJsonFile(path, selector, "");
            }
        }

        // Handle wildcards
        if (accFile.Contains('*'))
        {
            var folder = PathHelper.ExpandEnvironmentVariables(Path.GetDirectoryName(accFile) ?? "");
            var file = Path.GetFileName(accFile);

            // Handle "...\\*" folder.
            if (file == "*")
            {
                if (!Directory.Exists(Path.GetDirectoryName(folder)))
                    return true;
                if (!PathHelper.RecursiveDelete(folder, false))
                    Toast.Show("无法删除某些文件（程序可能正在运行？）");
                return true;
            }

            // Handle "...\\*.log" or "...\\file_*", etc.
            // This is NOT recursive - Specify folders manually in JSON
            if (!Directory.Exists(folder)) return true;
            foreach (var f in Directory.GetFiles(folder, file))
                PathHelper.DeleteFile(f);

            return true;
        }

        var fullPath = PathHelper.ExpandEnvironmentVariables(accFile);
        // Is folder? Recursive copy folder
        if (Directory.Exists(fullPath))
        {
            if (!PathHelper.RecursiveDelete(fullPath, true))
                Toast.Show("无法删除某些文件（程序可能正在运行？）");
            return true;
        }

        try
        {
            // Is file? Delete file
            PathHelper.DeleteFile(fullPath, true);
        }
        catch (UnauthorizedAccessException e)
        {
            Log.Error(nameof(DeleteFileOrFolder), e, "无法删除某些文件（程序可能正在运行？）");
            Toast.Show("无法删除某些文件（程序可能正在运行？）");
        }
        return true;
    }

    public bool KillPlatformProcess()
    {
        return false;
    }

    public bool RunPlatformProcess(PlatformAccount platform, bool isAdmin)
    {
        if (string.IsNullOrEmpty(platform.DefaultExePath)) return false;
        Process2.Start(platform.DefaultExePath, platform.ExeExtraArgs, useShellExecute: true);
        return true;
    }

    public void NewUserLogin(PlatformAccount platform)
    {

    }

    public string GetCurrentAccountId(PlatformAccount platform)
    {
        try
        {
            var uniqueId = GetUniqueId(platform);
        }
        catch (Exception)
        {
            //
        }

        return "";
    }

    public bool CurrnetUserAdd(string name, PlatformAccount platform)
    {
        if (string.IsNullOrEmpty(platform.UniqueIdPath))
            return false;

        if (platform.IsExitBeforeInteract)
            if (!KillPlatformProcess())
                return false;

        var localCachePath = Path.Combine(platform.PlatformLoginCache, name);

        //Directory.Delete(localCachePath, true);

        //处理name中的特殊参数
        //var specialString = "";
        //if (name.Contains(":{"))
        //{
        //    var index = name.IndexOf(":{", StringComparison.Ordinal)! + 1;
        //    specialString = name[index..];
        //    name = name.Split(":{")[0];
        //}

        //_ = Directory.CreateDirectory(localCachePath);

        if (!platform.LoginFiles.Any_Nullable())
            throw new Exception("No data in basic platform: " + platform.FullName);

        var uniqueId = GetUniqueId(platform);

        if (string.IsNullOrEmpty(uniqueId))
            throw new Exception("No data in basic platform: " + platform.FullName);

        if (uniqueId == "" && platform.UniqueIdType is UniqueIdType.CREATE_ID_FILE)
        {
            // Unique ID file, and does not already exist: Therefore create!
            var uniqueIdFile = PathHelper.ExpandEnvironmentVariables(platform.UniqueIdPath);
            uniqueId = Random2.GenerateRandomString(16);
            if (!string.IsNullOrEmpty(uniqueIdFile))
                File.WriteAllText(uniqueIdFile, uniqueId);
        }

        // Handle special args in username
        //var hadSpecialProperties = ProcessSpecialAccName(specialString, accName, uniqueId);

        var regJson = platform.UniqueIdPath.StartsWith("REG:") ? ReadRegJson(platform.RegJsonPath(name)) : new Dictionary<string, string>();

        foreach (var (accFile, savedFile) in platform.LoginFiles)
        {
            if (accFile.StartsWith("REG:"))
            {
                // Remove "REG:    " and read data
                if (RegistryKeyHelper.TryReadRegistryKey(accFile[4..], out var response))
                {
                    // Write registry value to provided file
                    if (response is string s) regJson[accFile] = s;
                    else if (response is byte[] ba) regJson[accFile] = "(hex) " + RegistryKeyHelper.ByteArrayToString(ba);
                    else Log.Error(nameof(BasicPlatformSwitcher), $"Unexpected registry type encountered (2)! Report to TechNobo. {response.GetType()}");
                }
                continue;
            }

            if (accFile.StartsWith("JSON"))
            {
                var path = accFile.Split("::")[1];
                var selector = accFile.Split("::")[2];

                JToken? js = null;
                JTokenHelper.TryReadJsonFile(path, ref js);
                if (js == null)
                    continue;

                var originalValue = js.SelectToken(selector);
                if (originalValue == null)
                    continue;

                // Save if it's JUST getting the value
                if (!accFile.StartsWith("JSON_SELECT_"))
                    JTokenHelper.SaveJsonFile(Path.Combine(localCachePath, savedFile), originalValue);

                // Otherwise, if it's selecting a part of it
                string delimiter;
                var firstResult = true;
                if (accFile.StartsWith("JSON_SELECT_FIRST"))
                {
                    delimiter = accFile.Split("JSON_SELECT_FIRST")[1].Split("::")[0];
                }
                else
                {
                    delimiter = accFile.Split("JSON_SELECT_LAST")[1].Split("::")[0];
                    firstResult = false;
                }

                var originalValueString = (string)originalValue;
                originalValueString = PathHelper.CleanPathIlegalCharacter(firstResult ? originalValueString.Split(delimiter).First() : originalValueString.Split(delimiter).Last());

                JTokenHelper.SaveJsonFile(Path.Combine(localCachePath, savedFile), originalValueString);
                continue;
            }

            // FILE OR FOLDER
            if (PathHelper.HandleFileOrFolder(accFile, savedFile, localCachePath, false)) continue;

            // Could not find file/folder
            Toast.Show($"无法找到 {accFile} 的账号文件，添加失败");

            return false;

            // TODO: Run some action that can be specified in the Platforms.json file
            // Add for the start, and end of this function -- To allow 'plugins'?
            // Use reflection?
        }

        SaveRegJson(regJson, platform.RegJsonPath(name));

        var allIds = JTokenHelper.ReadDict(platform.IdsJsonPath);
        allIds[uniqueId] = name;
        File.WriteAllText(platform.IdsJsonPath, JsonConvert.SerializeObject(allIds));
        return true;
    }

    public string? GetUniqueId(PlatformAccount platform)
    {
        if (OperatingSystem.IsWindows() &&
            platform.UniqueIdType is UniqueIdType.REGKEY &&
            !string.IsNullOrEmpty(platform.UniqueIdPath))
        {
            var r = RegistryKeyHelper.ReadRegistryKey(platform.UniqueIdPath);
            if (r == null)
                return null;

            switch (r)
            {
                case string s:
                    return s;
                case byte[] b:
                    return HashStringHelper.GetSha256HashString(b);
                default:
                    Log.Warn(nameof(BasicPlatformSwitcher), $"{platform.FullName} Unexpected registry type encountered (1)! Report to TechNobo. {r.GetType()}");
                    return null;
            }
        }

        if (string.IsNullOrEmpty(platform.UniqueIdPath))
            return null;

        var uniqueIdPath = PathHelper.ExpandEnvironmentVariables(platform.UniqueIdPath);

        if (string.IsNullOrEmpty(uniqueIdPath))
            return null;

        var uniqueId = "";

        if (platform.UniqueIdType is UniqueIdType.CREATE_ID_FILE)
        {
            return File.Exists(uniqueIdPath) ? File.ReadAllText(uniqueIdPath) : uniqueId;
        }

        if (uniqueId == "" && platform.UniqueIdType is UniqueIdType.JSON_SELECT or UniqueIdType.JSON_SELECT_FIRST or UniqueIdType.JSON_SELECT_LAST)
        {
            JToken? js = null;
            string searchFor;
            if (uniqueId == "" && platform.UniqueIdType is UniqueIdType.JSON_SELECT)
            {
                JTokenHelper.TryReadJsonFile(uniqueIdPath.Split("::")[0], ref js);
                searchFor = uniqueIdPath.Split("::")[1];
                uniqueId = PathHelper.CleanPathIlegalCharacter((string?)js?.SelectToken(searchFor));
                return uniqueId;
            }

            string? delimiter;
            var firstResult = true;
            if (platform.UniqueIdType is UniqueIdType.JSON_SELECT_FIRST)
            {
                delimiter = platform.UniqueIdRegex;
            }
            else
            {
                delimiter = platform.UniqueIdRegex;
                firstResult = false;
            }

            JTokenHelper.TryReadJsonFile(uniqueIdPath.Split("::")[0], ref js);
            searchFor = uniqueIdPath.Split("::")[1];
            var res = (string?)js?.SelectToken(searchFor);
            if (res is null)
                return "";
            uniqueId = PathHelper.CleanPathIlegalCharacter(firstResult ? res.Split(delimiter).First() : res.Split(delimiter).Last());
            return uniqueId;
        }

        if (!string.IsNullOrEmpty(uniqueIdPath) && (File.Exists(uniqueIdPath) || uniqueIdPath.Contains('*')))
        {
            if (!string.IsNullOrEmpty(platform.UniqueIdRegex))
            {
                uniqueId = PathHelper.CleanPathIlegalCharacter(RegexHelper.RegexSearchFileOrFolder(uniqueIdPath, platform.UniqueIdRegex)); // Get unique ID from Regex, but replace any illegal characters.
            }
            else if (platform.UniqueIdType is UniqueIdType.FILE_MD5) // TODO: TEST THIS! -- This is used for static files that do not change throughout the lifetime of an account login.
            {
                uniqueId = HashStringHelper.GetFileMd5(uniqueIdPath.Contains('*')
                    ? Directory.GetFiles(Path.GetDirectoryName(uniqueIdPath) ?? string.Empty, Path.GetFileName(uniqueIdPath)).First()
                    : uniqueIdPath);
            }
        }
        else if (uniqueId != "")
            uniqueId = HashStringHelper.GetSha256HashString(uniqueId);

        return uniqueId;
    }

    static Dictionary<string, string> ReadRegJson(string path) => JTokenHelper.ReadDict(path, true);

    static void SaveRegJson(Dictionary<string, string> regJson, string path)
    {
        if (regJson.Count > 0)
            JTokenHelper.SaveDict(regJson, path, true);
    }

    public void ChangeUserRemark()
    {

    }

    public Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform)
    {
        List<BasicAccount>? accounts = null;
        var localCachePath = platform.PlatformLoginCache;

        if (!Directory.Exists(localCachePath))
            return Task.FromResult<IEnumerable<IAccount>?>(accounts);

        var idsFile = Path.Combine(localCachePath, "ids.json");

        var accList = File.Exists(idsFile) ? JTokenHelper.ReadDict(idsFile).ToList() : null;

        if (accList.Any_Nullable())
        {
            accounts = new();

            // Order
            //accList = OrderAccounts(accList, $"{localCachePath}\\order.json");

            foreach (var item in accList)
            {
                var account = new BasicAccount
                {
                    PlatformName = platform.FullName,
                    Platform = platform.Platform,
                    AccountId = item.Key,
                    DisplayName = item.Value
                };

                // Handle account image
                //account.ImagePath = GetImgPath(platform, str).Replace("%", "%25");
                //var actualImagePath = Path.Join("wwwroot\\", GetImgPath(platform, str));
                //if (!File.Exists(actualImagePath))
                //{
                //    // Make sure the directory exists
                //    Directory.CreateDirectory(Path.GetDirectoryName(actualImagePath)!);
                //    var defaultPng = $"wwwroot\\img\\platform\\{platform}Default.png";
                //    const string defaultFallback = "wwwroot\\img\\BasicDefault.png";
                //    if (File.Exists(defaultPng))
                //        File.Copy(defaultPng, actualImagePath, true);
                //    else if (File.Exists(defaultFallback))
                //        File.Copy(defaultFallback, actualImagePath, true);
                //}

                accounts.Add(account);
            }
        }

        return Task.FromResult<IEnumerable<IAccount>?>(accounts);
    }
}
