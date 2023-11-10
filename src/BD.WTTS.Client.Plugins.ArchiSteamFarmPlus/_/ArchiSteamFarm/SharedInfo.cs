// ReSharper disable once CheckNamespace

namespace ArchiSteamFarm;

static class SharedInfo
{
    internal static string DirCreateByNotExists(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            _ = Directory.CreateDirectory(dirPath);
        }
        return dirPath;
    }

    private static string _ASFExecuteFilePath = ASFSettings.ArchiSteamFarmExePath.Value ?? string.Empty;

    internal static string ASFExecuteFilePath
    {
        get
        {
            if (!string.IsNullOrEmpty(_ASFExecuteFilePath) && File.Exists(_ASFExecuteFilePath))
            {
                Version = new Version(FileVersionInfo.GetVersionInfo(_ASFExecuteFilePath)?.FileVersion ?? string.Empty);
                return _ASFExecuteFilePath;
            }
            return string.Empty;
        }
    }

    internal static string HomeDirectory
    {
        get
        {
            return Path.GetDirectoryName(_ASFExecuteFilePath);
        }
    }

    internal const string ASF = nameof(ASF);
    internal const string UlongCompatibilityStringPrefix = "s_";

    internal const string GlobalConfigFileName = $"{ASF}{JsonConfigExtension}";
    internal const string JsonConfigExtension = ".json";

    public static string ConfigDirectory = "config";

    internal const string IPCConfigExtension = ".config";
    internal const string IPCConfigFile = $"{nameof(IPC)}{IPCConfigExtension}";

    internal static string WebsiteDirectory = "www";

    internal static string PluginsDirectory = "plugins";

    internal const string ArchivalLogFile = "log.{#}.txt";
    internal static string ArchivalLogsDirectory = "logs";

    internal static Version Version = null;
}