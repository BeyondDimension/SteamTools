using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AppResources = BD.WTTS.Client.Resources.Strings;
using Formatting = Newtonsoft.Json.Formatting;
using SJsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS;

public static class JTokenHelper
{
    public static bool ReplaceVarInJsonFile(string path, string selector, JToken? replaceWith)
    {
        try
        {
            JToken? jToken = null;
            if (!TryReadJsonFile(path, ref jToken)) return false;

            // It is a good idea to check what kind of variable it is to replace it with it's default... Though most of the time a "" will suffice here.
            //var originalValue = js.SelectToken(selector);
            var newJs = jToken?.ReplacePath(selector, replaceWith);
            if (newJs == null)
                return false;
            SaveJsonFile(path, newJs);
        }
        catch (Exception e)
        {
            Log.Error(nameof(JTokenHelper), e, "Failed to ReplaceVarInJsonFile");
            return false;
        }

        return true;
    }

    public static bool TryReadJsonFile(string path, ref JToken? jToken)
    {
        if (!File.Exists(path)) return false;
        try
        {
            using var file = File.OpenText(path);
            using var reader = new JsonTextReader(file);
            jToken = JToken.ReadFrom(reader);
        }
        catch (Exception e)
        {
            Log.Error(nameof(JTokenHelper), e, "Could not JSON read file: " + path);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 将JToken保存到文件中，带缩进或不带缩进（默认情况下无缩进）
    /// </summary>
    public static void SaveJsonFile(string path, JToken jo, bool formatted = true)
    {
        File.WriteAllText(path, jo.ToString(formatted ? Formatting.Indented : Formatting.None));
    }

    /// <summary>
    /// Read all ids from requested platform file
    /// </summary>
    /// <param name="dictPath">Full *.json file path (file safe)</param>
    /// <param name="isBasic"></param>
    public static Dictionary<string, string> ReadDict(string dictPath, bool isBasic = false)
    {
        var s = "{}";
        if (!File.Exists(dictPath))
        {
            if (isBasic && !IOPath.PathIsDirectoryEmpty(Path.GetDirectoryName(dictPath)!))
            {
                Toast.Show(ToastIcon.Error, AppResources.Error_OperateRegistryFilesFailed);
            }
            return new Dictionary<string, string>();
        }
        try
        {
            s = IPlatformService.Instance.ReadAllText(dictPath);
        }
        catch (Exception)
        {
        }

        return SJsonSerializer.Deserialize(s, GameAccountJsonContext.Default.AccountDictionary) ?? new Dictionary<string, string>();
    }

    public static void SaveDict(Dictionary<string, string> dict, string path, bool deleteIfEmpty = false)
    {
        if (path == null) return;
        var outText = SJsonSerializer.Serialize(dict, GameAccountJsonContext.Default.AccountDictionary);
        if (outText.Length < 4 && File.Exists(path))
            IPlatformService.Instance.FileTryDelete(path);
        else
            File.WriteAllText(path, outText);
    }

    public static Dictionary<string, string> ReadRegJson(string path) => ReadDict(path, true);

    public static void SaveRegJson(Dictionary<string, string> regJson, string path)
    {
        if (regJson.Count > 0)
            JTokenHelper.SaveDict(regJson, path, true);
    }
}
