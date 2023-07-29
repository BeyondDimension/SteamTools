namespace BD.WTTS.Models;

[JsonSerializable(typeof(SteamGuardModel))]
[JsonSerializable(typeof(SdaFileModel))]
[JsonSerializable(typeof(SdaFileConvertToSteamDataModel))]
public partial class ImportFileModelJsonContext : JsonSerializerContext
{

}
