using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models;

[MPObject]
public class CreateVersionRequest
{
    [MPKey(0)]
    public string? Version { get; set; }

    /// <summary>
    /// 新版本的描述
    /// </summary>
    [MPKey(1)]
    public string? Desc { get; set; }

    /// <summary>
    /// 是否使用上一个版本的密钥
    /// </summary>
    [MPKey(2)]
    public bool UseLastSKey { get; set; }
}
