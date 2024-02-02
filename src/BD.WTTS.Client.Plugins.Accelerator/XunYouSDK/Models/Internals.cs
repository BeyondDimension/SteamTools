namespace Mobius.Models;

// SDK 内部 Json 反序列化解构模型，不对外公开

#pragma warning disable IDE1006 // 命名样式

sealed class xunyou_get_gamelist_result<TGame> where TGame : class
{
    [JsonPropertyName("data")]
    public xunyou_get_gamelist_result_data<TGame>? Data { get; set; }
}

sealed class xunyou_get_gamelist_result_data<TGame> where TGame : class
{
    [JsonPropertyName("gamelist")]
    public TGame[]? GameList { get; set; }
}

sealed class xunyou_get_gameinfo_result<TGameInfo> where TGameInfo : class
{
    [JsonPropertyName("gameinfo")]
    public TGameInfo? GameInfo { get; set; }
}