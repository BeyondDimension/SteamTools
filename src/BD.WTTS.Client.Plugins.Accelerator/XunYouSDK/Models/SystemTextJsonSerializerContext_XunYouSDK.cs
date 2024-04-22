namespace Mobius.Models;

/// <summary>
/// SDK 内部 Json 反序列化解构模型用的源生成器，不对外公开
/// </summary>
[JsonSerializable(typeof(xunyou_get_gamelist_result<XunYouSteamGame>))]
[JsonSerializable(typeof(xunyou_get_gamelist_result<XunYouGame>))]
[JsonSerializable(typeof(xunyou_get_gameinfo_result<XunYouGameInfo>))]
[JsonSerializable(typeof(XunYouPicInfo))]
[JsonSerializable(typeof(XunYouVipEndTimeRequest))]
[JsonSerializable(typeof(XunYouVipEndTimeResponse))]
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true)]
sealed partial class SystemTextJsonSerializerContext_XunYouSDK : SystemTextJsonSerializerContext
{
}
