namespace BD.WTTS;

public static class EPathFolderExtensions
{
    public static string GetFolderPath(this EPathFolder folderASFPath)
    {
        string folderASFPathValue = folderASFPath switch
        {
            EPathFolder.Config => SharedInfo.DirCreateByNotExists(Path.Combine(SharedInfo.HomeDirectory, SharedInfo.ConfigDirectory)),
            EPathFolder.Plugin => SharedInfo.DirCreateByNotExists(Path.Combine(SharedInfo.HomeDirectory, SharedInfo.PluginsDirectory)),
            EPathFolder.WWW => Path.Combine(SharedInfo.HomeDirectory, SharedInfo.WebsiteDirectory),
            EPathFolder.Logs => Path.Combine(SharedInfo.HomeDirectory, SharedInfo.ArchivalLogsDirectory),
            EPathFolder.ASF or _ => SharedInfo.HomeDirectory,
        };
        return folderASFPathValue;
    }
}
