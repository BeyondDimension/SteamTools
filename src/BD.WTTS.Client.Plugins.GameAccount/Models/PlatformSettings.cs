namespace BD.WTTS.Models;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public partial class PlatformSettings
{
    [MPKey(0), MP2Key(0)]
    public string? PlatformPath { get; set; }
}
