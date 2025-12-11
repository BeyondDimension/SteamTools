namespace BD.WTTS.Models;

[MP2Obj(SerializeLayout.Explicit)]
public partial class PlatformSettings : BaseNotifyPropertyChanged
{
    [Reactive, MP2Key(0)]
    public string? PlatformPath { get; set; }
}
