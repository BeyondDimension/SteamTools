namespace BD.WTTS.Models;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public partial class DisableAuthorizedDevice
{
    [MPKey(0), MP2Key(0)]
    public long SteamId3_Int { get; set; }

    [MPKey(1), MP2Key(1)]
    public long Timeused { get; set; }

    [MPKey(2), MP2Key(2)]
    public string? Description { get; set; }

    [MPKey(3), MP2Key(3)]
    public string? Tokenid { get; set; }
}
