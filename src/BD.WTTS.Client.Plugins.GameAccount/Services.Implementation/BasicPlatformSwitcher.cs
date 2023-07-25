using AppResources = BD.WTTS.Client.Resources.Strings;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BD.WTTS.Services.Implementation;

public sealed class BasicPlatformSwitcher : IPlatformSwitcher
{
    readonly IPlatformService platformService;

    public BasicPlatformSwitcher(IPlatformService platformService)
    {
        this.platformService = platformService;
    }

    private bool BasicCopyInAccount(string accId, PlatformAccount platform)
    {
        var allIds = JTokenHelper.ReadDict(platform.IdsJsonPath);
        var accName = allIds[accId];

        var localCachePath = Path.Combine(platform.PlatformLoginCache, accName);
        _ = Directory.CreateDirectory(localCachePath);

        if (platform.LoginFiles == null)
        {
            Toast.Show(ToastIcon.Error, "No data in platform: " + platform.FullName);
            return false;
        }

        if (string.IsNullOrEmpty(platform.UniqueIdPath))
        {
            Toast.Show(ToastIcon.Error, "No data in platform: " + platform.FullName);
            return false;
        }

        // Get unique ID from IDs file if unique ID is a registry key. Set if exists.
        if (OperatingSystem.IsWindows() && platform.UniqueIdType is UniqueIdType.REGKEY && !string.IsNullOrEmpty(platform.UniqueIdPath))
        {
            var uniqueId = JTokenHelper.ReadDict(platform.FullName).FirstOrDefault(x => x.Value == accName).Key;

            if (!string.IsNullOrEmpty(uniqueId) && !RegistryKeyHelper.SetRegistryKey(platform.UniqueIdPath, uniqueId)) // Remove "REG:" and read data
            {
                Toast.Show(ToastIcon.Info, AppResources.Info_AccountAlreadyLogin);
                return false;
            }
        }

        var regJson = platform.UniqueIdPath.StartsWith("REG:") ? JTokenHelper.ReadRegJson(platform.RegJsonPath(accName)) : new Dictionary<string, string>();

        foreach (var (accFile, savedFile) in platform.LoginFiles)
        {
            // The "file" is a registry key
            if (OperatingSystem.IsWindows() && accFile.StartsWith("REG:"))
            {
                if (!regJson.ContainsKey(accFile))
                {
                    Toast.Show(ToastIcon.Error, AppResources.Error_ReadRegistryFailed);
                    continue;
                }

                var regValue = regJson[accFile] ?? "";

                if (!RegistryKeyHelper.SetRegistryKey(accFile[4..], regValue)) // Remove "REG:" and read data
                {
                    Toast.Show(ToastIcon.Error, AppResources.Error_WriteRegistryFailed);
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
                    Toast.Show(ToastIcon.Error, AppResources.Error_ModifyJsonFileFailed);
                    return false;
                }
                continue;
            }

            // FILE OR FOLDER
            PathHelper.HandleFileOrFolder(accFile, savedFile, localCachePath, true, platform.FolderPath);
        }

        return true;
    }

    public void SwapToAccount(IAccount? account, PlatformAccount platform)
    {
        //LoadAccountIds();

        if (!KillPlatformProcess(platform))
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
                    if (account?.AccountId == uniqueId)
                    {
                        RunPlatformProcess(platform, true);
                        Toast.Show(ToastIcon.Info, AppResources.Info_AlreadyTheCurrentAccount);
                        return;
                    }
                    CurrnetUserAdd(platform.Accounts.First(acc => acc.AccountId == uniqueId).AccountName ?? "Unknown", platform);
                }
            }
        }

        // Clear current login
        ClearCurrentLoginUser(platform);

        // Copy saved files in
        if (!string.IsNullOrEmpty(account?.AccountId))
        {
            if (!BasicCopyInAccount(account.AccountId, platform)) return;
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

        var uniqueIdFile = PathHelper.ExpandEnvironmentVariables(platform.UniqueIdPath, platform.FolderPath);

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
            Toast.Show(ToastIcon.Error, AppResources.Error_WriteRegistryFailed);
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
            var folder = PathHelper.ExpandEnvironmentVariables(Path.GetDirectoryName(accFile) ?? "", platform.FolderPath);
            var file = Path.GetFileName(accFile);

            // Handle "...\\*" folder.
            if (file == "*")
            {
                if (!Directory.Exists(Path.GetDirectoryName(folder)))
                    return true;
                if (!PathHelper.RecursiveDelete(folder, false))
                    Toast.Show(ToastIcon.Warning, AppResources.Error_DelFileFailedRunning);
                return true;
            }

            // Handle "...\\*.log" or "...\\file_*", etc.
            // This is NOT recursive - Specify folders manually in JSON
            if (!Directory.Exists(folder)) return true;
            foreach (var f in Directory.GetFiles(folder, file))
                PathHelper.DeleteFile(f);

            return true;
        }

        var fullPath = PathHelper.ExpandEnvironmentVariables(accFile, platform.FolderPath);
        // Is folder? Recursive copy folder
        if (Directory.Exists(fullPath))
        {
            if (!PathHelper.RecursiveDelete(fullPath, true))
                Toast.Show(ToastIcon.Warning, AppResources.Error_DelFileFailedRunning);
            return true;
        }

        try
        {
            // Is file? Delete file
            PathHelper.DeleteFile(fullPath, true);
        }
        catch (UnauthorizedAccessException e)
        {
            Log.Error(nameof(DeleteFileOrFolder), e, AppResources.Error_DelFileFailedRunning);
            Toast.Show(ToastIcon.Warning, AppResources.Error_DelFileFailedRunning);
        }
        return true;
    }

    public bool KillPlatformProcess(PlatformAccount platform)
    {
        try
        {
            if (platform.ExesToEnd.Any_Nullable())
            {
                foreach (var procName in platform.ExesToEnd)
                {
                    var process = Process.GetProcessesByName(procName.Split(".exe")[0]);
                    foreach (var item in process)
                    {
                        if (!item.HasExited)
                        {
                            item.Kill();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(BasicPlatformSwitcher), ex, nameof(KillPlatformProcess));
            Toast.Show(ToastIcon.Error, AppResources.Error_EndProcessFailed_.Format(platform.FullName));
            return false;
        }

        return true;
    }

    public bool RunPlatformProcess(PlatformAccount platform, bool isAdmin)
    {
        if (string.IsNullOrEmpty(platform.ExePath)) return false;
        try
        {
            Process2.Start(platform.ExePath, platform.ExeExtraArgs, useShellExecute: true);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(BasicPlatformSwitcher), ex, nameof(RunPlatformProcess));
            Toast.Show(ToastIcon.Error, AppResources.Error_StartProcessFailed_.Format(platform.FullName));
            return false;
        }
        return true;
    }

    public void NewUserLogin(PlatformAccount platform)
    {
        SwapToAccount(null, platform);
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
            if (!KillPlatformProcess(platform))
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
        {
            Toast.Show("No data in platform: " + platform.FullName);
            return false;
        }

        var uniqueId = GetUniqueId(platform);

        if (string.IsNullOrEmpty(uniqueId))
        {
            Toast.Show("No data in platform: " + platform.FullName);
            return false;
        }

        if (uniqueId == "" && platform.UniqueIdType is UniqueIdType.CREATE_ID_FILE)
        {
            // Unique ID file, and does not already exist: Therefore create!
            var uniqueIdFile = PathHelper.ExpandEnvironmentVariables(platform.UniqueIdPath, platform.FolderPath);
            uniqueId = Random2.GenerateRandomString(16);
            if (!string.IsNullOrEmpty(uniqueIdFile))
                File.WriteAllText(uniqueIdFile, uniqueId);
        }

        // Handle special args in username
        //var hadSpecialProperties = ProcessSpecialAccName(specialString, accName, uniqueId);

        var regJson = platform.UniqueIdPath.StartsWith("REG:") ? JTokenHelper.ReadRegJson(platform.RegJsonPath(name)) : new Dictionary<string, string>();

        foreach (var (accFile, savedFile) in platform.LoginFiles)
        {
            if (accFile.StartsWith("REG:") && OperatingSystem.IsWindows())
            {
                // Remove "REG:    " and read data
                if (RegistryKeyHelper.TryReadRegistryKey(accFile[4..], out var response))
                {
                    // Write registry value to provided file
                    if (response is string s) regJson[accFile] = s;
                    else if (response is byte[] ba) regJson[accFile] = "(hex) " + RegistryKeyHelper.ByteArrayToString(ba);
                    else Log.Error(nameof(BasicPlatformSwitcher), $"Unexpected registry type encountered (2)! {response.GetType()}");
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
            if (PathHelper.HandleFileOrFolder(accFile, savedFile, localCachePath, false, platform.FolderPath)) continue;

            // Could not find file/folder
            Toast.Show(ToastIcon.Error, AppResources.Error_CannotFindAccountFile_.Format(accFile));

            return false;

            // TODO: Run some action that can be specified in the Platforms.json file
            // Add for the start, and end of this function -- To allow 'plugins'?
            // Use reflection?
        }

        JTokenHelper.SaveRegJson(regJson, platform.RegJsonPath(name));

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
            var r = RegistryKeyHelper.ReadRegistryKey(platform.UniqueIdPath[4..]);
            if (r == null)
                return null;

            switch (r)
            {
                case string s:
                    return s;
                case byte[] b:
                    return HashStringHelper.GetSha256HashString(b);
                default:
                    Log.Warn(nameof(BasicPlatformSwitcher), $"{platform.FullName} Unexpected registry type encountered (1)! {r.GetType()}");
                    return null;
            }
        }

        if (string.IsNullOrEmpty(platform.UniqueIdPath))
            return null;

        var uniqueIdPath = PathHelper.ExpandEnvironmentVariables(platform.UniqueIdPath, platform.FolderPath);

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
                    AccountName = item.Value
                };
                if (!string.IsNullOrEmpty(account.AccountName))
                {
                    // Handle account image
                    var imagePath = Path.Combine(platform.PlatformLoginCache, account.AccountName, "avatar.png");
                    if (File.Exists(imagePath))
                    {
                        account.ImagePath = imagePath;
                    }
                }

                accounts.Add(account);
            }
        }

        return Task.FromResult<IEnumerable<IAccount>?>(accounts);
    }

    public bool SetPlatformPath(PlatformAccount platform)
    {
        return false;
    }

    public async Task DeleteAccountInfo(IAccount account, PlatformAccount platform)
    {
        var result = await MessageBox.ShowAsync(Strings.UserChange_DeleteUserTip, button: MessageBox.Button.OKCancel);
        if (result == MessageBox.Result.OK)
        {
            // Remove ID from list of ids
            if (File.Exists(platform.IdsJsonPath))
            {
                var allIds = JTokenHelper.ReadDict(platform.IdsJsonPath);
                //if (accNameIsId)
                //{
                //    var accId = accName;
                //    accName = allIds[accName];
                //    _ = allIds.Remove(accId);
                //}
                //else
                _ = allIds.Remove(allIds.Single(x => x.Value == account.AccountId).Key);
                File.WriteAllText(platform.IdsJsonPath, JsonConvert.SerializeObject(allIds));
            }

            // Remove cached files
            if (!string.IsNullOrEmpty(account.AccountName))
            {
                PathHelper.RecursiveDelete(Path.Combine(platform.PlatformLoginCache, account.AccountName), false);

                // Remove image
                PathHelper.DeleteFile(Path.Combine(platform.PlatformLoginCache, account.AccountName, "avatar.png"));
            }

            platform.Accounts?.Remove(account);
        }
    }
}
