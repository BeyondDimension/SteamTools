using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.Client.Tools.Publish.Models;

partial class AppPublishInfo
{
    static readonly string filePath = Path.Combine(ProjectUtils.ProjPath, "build", "app_publish_info.json");

    static readonly Lazy<List<AppPublishInfo>> mInstance = new(() =>
    {
        using var fileStream = File.OpenRead(filePath);
        return JsonSerializer.Deserialize(fileStream, AppPublishInfoContext.Default.ListAppPublishInfo);
    });

    internal static List<AppPublishInfo> Instance => mInstance.Value;

    internal static void Save(ConcurrentBag<AppPublishInfo> infos)
    {
        using var fileStream = File.Open(filePath, FileMode.OpenOrCreate);
        JsonSerializer.Serialize(fileStream, infos, AppPublishInfoContext.Default.ConcurrentBagAppPublishInfo);
        fileStream.Flush();
        fileStream.SetLength(fileStream.Position);
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ConcurrentBag<AppPublishInfo>))]
[JsonSerializable(typeof(List<AppPublishInfo>))]
internal partial class AppPublishInfoContext : JsonSerializerContext
{

}