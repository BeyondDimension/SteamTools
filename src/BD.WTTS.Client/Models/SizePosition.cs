namespace BD.WTTS.Models;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed partial class SizePosition : BaseNotifyPropertyChanged
{
    string DebuggerDisplay => $"Position: {X}x{Y}, Size: {Width}x{Height}";

    int _X;

    [MPKey(0), MP2Key(0)]
    public int X
    {
        get => _X;
        set => this.RaiseAndSetIfChanged(ref _X, value);
    }

    int _Y;

    [MPKey(1), MP2Key(1)]
    public int Y
    {
        get => _Y;
        set => this.RaiseAndSetIfChanged(ref _Y, value);
    }

    double _Height;

    [MPKey(2), MP2Key(2)]
    public double Height
    {
        get => _Height;
        set => this.RaiseAndSetIfChanged(ref _Height, value);
    }

    double _Width;

    [MPKey(3), MP2Key(3)]
    public double Width
    {
        get => _Width;
        set => this.RaiseAndSetIfChanged(ref _Width, value);
    }
}
