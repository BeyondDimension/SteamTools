using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Services.Implementation;

public sealed class BasicPlatformSwitcher : IPlatformSwitcher
{
    readonly IPlatformService platformService;

    public BasicPlatformSwitcher(IPlatformService platformService)
    {
        this.platformService = platformService;
    }

    async ValueTask<bool> BasicCopyInAccount(string accId, PlatformAccount platform)
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
#if WINDOWS
        if (platform.UniqueIdType is UniqueIdType.REGKEY && !string.IsNullOrEmpty(platform.UniqueIdPath))
        {
            var uniqueId = JTokenHelper.ReadDict(platform.FullName).FirstOrDefault(x => x.Value == accName).Key;

            if (!string.IsNullOrEmpty(uniqueId) && !IRegistryService.Instance.SetRegistryKey(platform.UniqueIdPath, uniqueId)) // Remove "REG:" and read data
            {
                Toast.Show(ToastIcon.Info, AppResources.Info_AccountAlreadyLogin);
                return false;
            }
        }
#endif

        var regJson = platform.UniqueIdPath.StartsWith("REG:") ? JTokenHelper.ReadRegJson(platform.RegJsonPath(accName)) : new Dictionary<string, string>();

        foreach (var (accFile, savedFile) in platform.LoginFiles)
        {
            // The "file" is a registry key
#if WINDOWS
            if (accFile.StartsWith("REG:"))
            {
                if (!regJson.ContainsKey(accFile))
                {
                    Toast.Show(ToastIcon.Error, AppResources.Error_ReadRegistryFailed);
                    continue;
                }

                var regValue = regJson[accFile] ?? "";

                if (!IRegistryService.Instance.SetRegistryKey(accFile[4..], regValue)) // Remove "REG:" and read data
                {
                    Toast.Show(ToastIcon.Error, AppResources.Error_WriteRegistryFailed);
                    return false;
                }
                continue;
            }
#endif

            // The "file" is a JSON value
            if (accFile.StartsWith("JSON"))
            {
                JToken? jToken = null;
                JTokenHelper.TryReadJsonFile(Path.Join(localCachePath, savedFile), ref jToken);

                var path = IOPath.ExpandEnvironmentVariables(accFile, platform.FolderPath).Split("::")[1];
                var selector = accFile.Split("::")[2];
                if (!JTokenHelper.ReplaceVarInJsonFile(path, selector, jToken))
                {
                    //Toast.Show(ToastIcon.Error, AppResources.Error_ModifyJsonFileFailed);
                    Log.Error(nameof(BasicPlatformSwitcher), $"Failed to modify JSON file: {path}");
                    //return false;
                }
                continue;
            }

            // FILE OR FOLDER
            PathHelper.HandleFileOrFolder(accFile, savedFile, localCachePath, true, platform.FolderPath);
        }

        return true;
    }

    public async ValueTask<bool> SwapToAccount(IAccount? account, PlatformAccount platform)
    {
        //LoadAccountIds();

        var killResult = await KillPlatformProcess(platform);
        if (!killResult)
            return false;

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
                        return false;
                    }
                    await CurrnetUserAdd(platform.Accounts.First(acc => acc.AccountId == uniqueId).AccountName ?? "Unknown", platform);
                }
            }
        }

        // Clear current login
        await ClearCurrentLoginUser(platform);

        // Copy saved files in
        if (!string.IsNullOrEmpty(account?.AccountId))
        {
            if (!await BasicCopyInAccount(account.AccountId, platform)) return false;
            //Globals.AddTrayUser(platform.SafeName, $"+{platform.PrimaryId}:" + accId, accName, BasicSettings.TrayAccNumber); // Add to Tray list, using first Identifier
        }

        //if (BasicSettings.AutoStart)
        RunPlatformProcess(platform, false);

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
        return true;
    }

    public async ValueTask<bool> ClearCurrentLoginUser(PlatformAccount platform)
    {
        var result = await ClearCurrentLoginUserCore(platform);
        return result;
    }

    async ValueTask<bool> ClearCurrentLoginUserCore(PlatformAccount platform)
    {
        // Foreach file/folder/reg in Platform.PathListToClear

        foreach (var accFile in platform.ClearPaths!)
        {
            if (!await DeleteFileOrFolder(accFile, platform))
                return false;
        }

        var uniqueIdFile = IOPath.ExpandEnvironmentVariables(platform.UniqueIdPath, platform.FolderPath);

        if (platform.UniqueIdType is UniqueIdType.JSON_SELECT or UniqueIdType.JSON_SELECT_FIRST or UniqueIdType.JSON_SELECT_LAST)
        {
            var path = uniqueIdFile.Split("::")[0];
            var selector = uniqueIdFile.Split("::")[1];
            JTokenHelper.ReplaceVarInJsonFile(path, selector, string.Empty);
        }

        if (platform.UniqueIdType != UniqueIdType.CREATE_ID_FILE) return true;

        // Unique ID file --> This needs to be deleted for a new instance
        IPlatformService.Instance.FileTryDelete(uniqueIdFile);

        return true;
    }

    async ValueTask<bool> DeleteFileOrFolder(string accFile, PlatformAccount platform)
    {
        // The "file" is a registry key
#if WINDOWS
        if (accFile.StartsWith("REG:"))
        {
            // If set to clear LoginCache for account before adding (Enabled by default):
            if (platform.IsRegDeleteOnClear)
            {
                if (IRegistryService.Instance.DeleteRegistryKey(accFile[4..])) return true;
            }
            else
            {
                if (IRegistryService.Instance.SetRegistryKey(accFile[4..])) return true;
            }
            Toast.Show(ToastIcon.Error, AppResources.Error_WriteRegistryFailed);
            return false;
        }
#endif

        // The "file" is a JSON value
        if (accFile.StartsWith("JSON"))
        {
            if (accFile.StartsWith("JSON_SELECT"))
            {
                var path = IOPath.ExpandEnvironmentVariables(accFile.Split("::")[1]);
                var selector = accFile.Split("::")[2];
                JTokenHelper.ReplaceVarInJsonFile(path, selector, "");
                return true;
            }
        }

        // Handle wildcards
        if (accFile.Contains('*'))
        {
            var folder = IOPath.ExpandEnvironmentVariables(Path.GetDirectoryName(accFile) ?? "", platform.FolderPath);
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
                IPlatformService.Instance.FileTryDelete(f);

            return true;
        }

        var fullPath = IOPath.ExpandEnvironmentVariables(accFile, platform.FolderPath);
        // Is folder? Recursive copy folder
        if (Directory.Exists(fullPath))
        {
            if (!PathHelper.RecursiveDelete(fullPath, true))
                Toast.Show(ToastIcon.Warning, AppResources.Error_DelFileFailedRunning);
            return true;
        }

        // Is file? Delete file
        var isDelete = IPlatformService.Instance.FileTryDelete(fullPath);
        if (!isDelete)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Error_DelFileFailedRunning);
        }
        return true;
    }

    bool KillPlatformProcessCore(PlatformAccount platform)
    {
        var processNames = GetProcessNames(platform);
        if (processNames != null)
        {
            var processes = processNames.Select(static x =>
            {
                try
                {
                    var process = Process.GetProcessesByName(x);
                    return process;
                }
                catch
                {
                    return Array.Empty<Process>();
                }
            }).SelectMany(static x => x).ToArray();

            static ApplicationException? KillProcess(Process? process)
            {
                if (process == null)
                    return default;
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    return new ApplicationException(
                        $"KillProcesses fail, name: {process?.ProcessName}", ex);
                }
                return default;
            }

            try
            {
                if (processes.Any())
                {
                    var tasks = processes.Select(x =>
                    {
                        return Task.Run(() =>
                        {
                            return KillProcess(x);
                        });
                    }).ToArray();
                    Task.WaitAll(tasks);

                    var innerExceptions = tasks.Select(x => x.Result!)
                        .Where(x => x != null).ToArray();
                    if (innerExceptions.Any())
                    {
                        throw new AggregateException(
                            "KillProcess fail", innerExceptions);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(nameof(BasicPlatformSwitcher), ex, nameof(KillPlatformProcess));
                Toast.Show(ToastIcon.Error,
                    AppResources.Error_EndProcessFailed_.Format(platform.FullName));
                return false;
            }
        }

        return true;
    }

    static string[]? GetProcessNames(PlatformAccount platform)
    {
        IEnumerable<string>? processNames_ = platform.ExesToEnd;
        if (processNames_ != null)
        {
            processNames_ = processNames_.Select(x =>
            {
                try
                {
                    return x.Split(FileEx.EXE, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()!;
                }
                catch
                {

                }
                return default!;
            }).Where(x => !string.IsNullOrWhiteSpace(x));

            var processNames = processNames_.ToArray();
            return processNames;
        }

        return null;
    }

#if WINDOWS
    public async ValueTask<bool> KillPlatformProcess(PlatformAccount platform)
    {
        if (WindowsPlatformServiceImpl.IsPrivilegedProcess)
        {
            return KillPlatformProcessCore(platform);
        }
        else
        {
            try
            {
                var processNames = GetProcessNames(platform);
                if (processNames.Any_Nullable())
                {
                    var platformService = await IPlatformService.IPCRoot.Instance;
                    var r = platformService.KillProcesses(processNames);
                    return r ?? false;
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(BasicPlatformSwitcher), e, "KillPlatformProcess fail.");
                return false;
            }
        }
        return true;
    }
#else
    public ValueTask<bool> KillPlatformProcess(PlatformAccount platform)
    {
        var result = KillPlatformProcessCore(platform);
        return ValueTask.FromResult(result);
    }
#endif

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

    public async ValueTask NewUserLogin(PlatformAccount platform)
    {
        await SwapToAccount(null, platform);
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

    public async ValueTask<bool> CurrnetUserAdd(string name, PlatformAccount platform)
    {
        if (string.IsNullOrEmpty(platform.UniqueIdPath))
            return false;

        if (platform.IsExitBeforeInteract)
        {
            var killResult = await KillPlatformProcess(platform);
            if (!killResult)
                return false;
        }

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
            Toast.Show(ToastIcon.Warning, "No data in platform: " + platform.FullName);
            return false;
        }

        var uniqueId = GetUniqueId(platform);

        if (uniqueId == "" && platform.UniqueIdType is UniqueIdType.CREATE_ID_FILE)
        {
            // Unique ID file, and does not already exist: Therefore create!
            var uniqueIdFile = IOPath.ExpandEnvironmentVariables(platform.UniqueIdPath, platform.FolderPath);
            uniqueId = Random2.GenerateRandomString(16);
            if (!string.IsNullOrEmpty(uniqueIdFile))
                File.WriteAllText(uniqueIdFile, uniqueId);
        }

        if (string.IsNullOrEmpty(uniqueId))
        {
            Toast.Show(ToastIcon.Warning, "No data in platform: " + platform.FullName);
            return false;
        }

        // Handle special args in username
        //var hadSpecialProperties = ProcessSpecialAccName(specialString, accName, uniqueId);

        var regJson = platform.UniqueIdPath.StartsWith("REG:") ? JTokenHelper.ReadDict(platform.RegJsonPath(name)) : new Dictionary<string, string>();

        foreach (var (accFile, savedFile) in platform.LoginFiles)
        {
#if WINDOWS
            if (accFile.StartsWith("REG:"))
            {
                // Remove "REG:    " and read data
                if (Registry2.TryReadRegistryKey(accFile[4..], out var response))
                {
                    // Write registry value to provided file
                    if (response is string s) regJson[accFile] = s;
                    else if (response is byte[] ba) regJson[accFile] = $"(hex) {ba.ToHexString()}";
                    else Log.Error(nameof(BasicPlatformSwitcher), $"Unexpected registry type encountered (2)! {response?.GetType()}");
                }
                continue;
            }
#endif

            if (accFile.StartsWith("JSON"))
            {
                var path = IOPath.ExpandEnvironmentVariables(accFile, platform.FolderPath).Split("::")[1];
                var selector = accFile.Split("::")[2];

                JToken? js = null;
                JTokenHelper.TryReadJsonFile(path, ref js);
                if (js == null)
                    continue;

                JToken? originalValue = null;

                try
                {
                    originalValue = js.SelectToken(selector);
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(CurrnetUserAdd), ex, $"Failed to select token: {selector}");
                }

                if (originalValue == null)
                {
                    // 尝试以数组形式再次寻找
                    try
                    {
                        var tokens = js.SelectTokens(selector);

                        //暂时将就写一个临时的战网处理方法解决问题
                        if (platform.Platform == ThirdpartyPlatform.BattleNet)
                        {
                            originalValue = tokens.FirstOrDefault(x =>
                            {
                                var temp = x.Parent?.Parent?.Parent?.Parent?["Path"]?.Value<string>();
                                var temp2 = platform.FolderPath;
                                if (temp != null && temp2 != null)
                                    return string.Equals(
                                        temp.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                                        temp2.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                                        StringComparison.OrdinalIgnoreCase);
                                return false;
                            });
                        }
                        originalValue ??= tokens.FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(CurrnetUserAdd), ex, $"Failed to select tokens: {selector}");
                    }
                }

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
                originalValueString = IOPath.CleanPathIlegalCharacter(firstResult ? originalValueString.Split(delimiter).First() : originalValueString.Split(delimiter).Last());

                IOPath.DirCreateByNotExists(localCachePath);
                JTokenHelper.SaveJsonFile(Path.Combine(localCachePath, savedFile), originalValueString);
                continue;
            }

            // FILE OR FOLDER
            if (PathHelper.HandleFileOrFolder(accFile, savedFile, localCachePath, false, platform.FolderPath)) continue;

            if (platform.AllFilesRequired)
            {
                // Could not find file/folder
                Toast.Show(ToastIcon.Error, AppResources.Error_CannotFindAccountFile_.Format(accFile));
                return false;
            }
            else
            {
                Log.Warn(nameof(CurrnetUserAdd), $"{accFile} Could not find file/folder");
            }
        }

        JTokenHelper.SaveRegJson(regJson, platform.RegJsonPath(name));

        var allIds = JTokenHelper.ReadDict(platform.IdsJsonPath);
        allIds[uniqueId] = name;
        File.WriteAllText(platform.IdsJsonPath, JsonConvert.SerializeObject(allIds));
        return true;
    }

    public string? GetUniqueId(PlatformAccount platform)
    {
#if WINDOWS
        if (platform.UniqueIdType is UniqueIdType.REGKEY &&
            !string.IsNullOrEmpty(platform.UniqueIdPath))
        {
            var r = Registry2.ReadRegistryKey(platform.UniqueIdPath[4..]);
            if (r == null)
                return null;

            switch (r)
            {
                case string s:
                    return s;
                case byte[] b:
                    return Hashs.String.SHA256(b);
                default:
                    Log.Warn(nameof(BasicPlatformSwitcher), $"{platform.FullName} Unexpected registry type encountered (1)! {r.GetType()}");
                    return null;
            }
        }
#endif

        if (string.IsNullOrEmpty(platform.UniqueIdPath))
            return null;

        var uniqueIdPath = IOPath.ExpandEnvironmentVariables(platform.UniqueIdPath, platform.FolderPath);

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
                uniqueId = IOPath.CleanPathIlegalCharacter((string?)js?.SelectToken(searchFor));
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
            uniqueId = IOPath.CleanPathIlegalCharacter(firstResult ? res.Split(delimiter).First() : res.Split(delimiter).Last());
            return uniqueId;
        }

        if (!string.IsNullOrEmpty(uniqueIdPath) && (File.Exists(uniqueIdPath) || uniqueIdPath.Contains('*')))
        {
            if (!string.IsNullOrEmpty(platform.UniqueIdRegex))
            {
                uniqueId = IOPath.CleanPathIlegalCharacter(RegexHelper.RegexSearchFileOrFolder(uniqueIdPath, platform.UniqueIdRegex)); // Get unique ID from Regex, but replace any illegal characters.
            }
            else if (platform.UniqueIdType is UniqueIdType.FILE_MD5) // TODO: TEST THIS! -- This is used for static files that do not change throughout the lifetime of an account login.
            {
                var filePath = uniqueIdPath.Contains('*')
                    ? Directory.GetFiles(Path.GetDirectoryName(uniqueIdPath) ?? string.Empty, Path.GetFileName(uniqueIdPath)).First()
                    : uniqueIdPath;
                using var fileStream = IOPath.OpenRead(filePath);
                fileStream.ThrowIsNull();
                uniqueId = fileStream.Length != 0 ? Hashs.String.MD5(fileStream) : "0";
            }
        }
        else if (uniqueId != "")
            uniqueId = string.IsNullOrEmpty(uniqueId) ? string.Empty : Hashs.String.SHA512(uniqueId);

        return uniqueId;
    }

    public Task<IEnumerable<IAccount>?> GetUsers(PlatformAccount platform, Action? refreshUsers = null)
    {
        List<BasicAccount>? accounts = null;
        var localCachePath = platform.PlatformLoginCache;

        if (!Directory.Exists(localCachePath))
            return Task.FromResult<IEnumerable<IAccount>?>(accounts);

        var idsFile = Path.Combine(localCachePath, "ids.json");

        var accountRemarks = Ioc.Get_Nullable<IPartialGameAccountSettings>()?.AccountRemarks;
        var accList = File.Exists(idsFile) ? JTokenHelper.ReadDict(idsFile).ToList() : null;

        if (accList.Any_Nullable())
        {
            accounts = new();

            // Order
            //accList = OrderAccounts(accList, $"{localCachePath}\\order.json");

            foreach (var item in accList)
            {
                var account = new BasicAccount(item.Key)
                {
                    //AccountId = item.Key,
                    AccountName = item.Value,
                    PlatformName = platform.FullName,
                    Platform = platform.Platform,
                };
                if (accountRemarks?.TryGetValue($"{platform.FullName}-{account.AccountId}", out var remark) == true && !string.IsNullOrEmpty(remark))
                    account.AliasName = remark;
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

    public async Task<bool> DeleteAccountInfo(IAccount account, PlatformAccount platform)
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
                _ = allIds.Remove(allIds.Single(x => x.Key == account.AccountId).Key);
                File.WriteAllText(platform.IdsJsonPath, JsonConvert.SerializeObject(allIds));
            }

            // Remove cached files
            if (!string.IsNullOrEmpty(account.AccountName))
            {
                PathHelper.RecursiveDelete(Path.Combine(platform.PlatformLoginCache, account.AccountName), false);

                // Remove image
                IPlatformService.Instance.FileTryDelete(Path.Combine(platform.PlatformLoginCache, account.AccountName, "avatar.png"));
            }

            platform.Accounts?.Remove(account);
            return true;
        }
        return false;
    }
}
