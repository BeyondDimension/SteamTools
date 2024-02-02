namespace Mobius.Models;

/// <summary>
/// 迅游游戏服务器信息
/// </summary>
[MP2Obj(MP2SerializeLayout.Explicit)]
public sealed partial record class XunYouGameServer
{
    /// <summary>
    /// 服务器 Id
    /// </summary>
    [JsonPropertyName("server_id")]
    [MP2Key(0)]
    public int Id { get; set; }

    /// <summary>
    /// 服务器名称
    /// </summary>
    [JsonPropertyName("server_name")]
    [MP2Key(1)]
    public string? Name { get; set; }
}
