using System.Text.Json.Serialization;
using ArchiSteamFarm.IPC;
using ArchiSteamFarm.IPC.Requests;
using ArchiSteamFarm.IPC.Responses;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Storage;

namespace BD.WTTS.Services.Implementation;

// This closed inventory is the request/response graph of ApiControllers, plus imported configs.
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(IPCConfig))]
[JsonSerializable(typeof(BotConfig))]
[JsonSerializable(typeof(GlobalConfig))]
[JsonSerializable(typeof(ASFEncryptRequest))]
[JsonSerializable(typeof(ASFHashRequest))]
[JsonSerializable(typeof(ASFRequest))]
[JsonSerializable(typeof(BotGamesToRedeemInBackgroundRequest))]
[JsonSerializable(typeof(BotInputRequest))]
[JsonSerializable(typeof(BotPauseRequest))]
[JsonSerializable(typeof(BotRedeemRequest))]
[JsonSerializable(typeof(BotRenameRequest))]
[JsonSerializable(typeof(BotRequest))]
[JsonSerializable(typeof(CommandRequest))]
[JsonSerializable(typeof(GenericResponse))]
[JsonSerializable(typeof(GenericResponse<string>))]
[JsonSerializable(typeof(GenericResponse<ASFResponse>))]
[JsonSerializable(typeof(GenericResponse<IReadOnlyDictionary<string, Bot>>))]
[JsonSerializable(typeof(GenericResponse<IReadOnlyDictionary<string, GamesToRedeemInBackgroundResponse>>))]
internal partial class ArchiSteamFarmJsonContext : JsonSerializerContext
{
}
