namespace BD.WTTS.Client.Tools.Publish.Models;

partial class AppPublishFileInfo
{
    internal AppPublishFileInfo(string path, string relativeTo, string fileEx)
    {
        FilePath = path;
        RelativePath = Path.GetRelativePath(relativeTo, path);
        FileInfo = new(path);
        Length = FileInfo.Length;
        FileEx = fileEx;
    }

    [MP2Ignore]
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public FileInfo? FileInfo { get; set; }
}