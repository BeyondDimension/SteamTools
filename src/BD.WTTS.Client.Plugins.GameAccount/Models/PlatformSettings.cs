namespace BD.WTTS.Models;

[MessagePackObjectAttribute]
[MP2Obj(SerializeLayout.Explicit)]
public partial class PlatformSettings : BaseNotifyPropertyChanged
{
    [MessagePack.Key(0)]
    [Reactive, MP2Key(0)]
    public partial string? PlatformPath { get; set; }
}
