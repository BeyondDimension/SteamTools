namespace BD.WTTS.Models;

public partial class PlatformSettings : ReactiveObject
{
    [Reactive]
    public string? PlatformPath { get; set; }
}
