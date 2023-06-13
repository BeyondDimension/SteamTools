using BD.WTTS.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BD.WTTS.Services.Implementation;

public sealed class BasicPlatformSwitcher : IPlatformSwitcher
{
    public BasicPlatformSwitcher()
    {

    }

    public void SwapToUser(IAccount account)
    {

    }

    public void ClearCurrentLoginUser()
    {

    }

    public bool KillPlatformProcess()
    {
        return false;
    }

    public void RunPlatformProcess()
    {

    }

    public void NewUserLogin()
    {

    }

    public bool CurrnetUserAdd(string name, PlatformAccount platform)
    {
        if (string.IsNullOrEmpty(platform.UniqueIdPath))
            return false;

        if (platform.IsExitBeforeInteract)
            if (!KillPlatformProcess())
                return false;

        var localCachePath = Path.Combine($"LoginCache\\{platform.FullName}\\", $"{name}\\");

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
                    else if (response is byte[] ba) regJson[accFile] = "(hex) " + BitConverter.ToString(ba).Replace("-", "");
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
        if (OperatingSystem2.IsWindows() &&
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
        return Task.FromResult<IEnumerable<IAccount>?>(null);
    }
}
