using Avalonia.Media;
using Avalonia.Threading;

namespace BD.WTTS.Models;

public class AuthenticatorItemModel : ItemViewModel
{
    public override string Name => nameof(AuthenticatorItemModel);

    string? text;

    public string? Text
    {
        get => text;
        set
        {
            this.RaiseAndSetIfChanged(ref text, value);
        }
    }

    IBrush? color;
    
    public IBrush? Color
    {
        get => color;
        set => this.RaiseAndSetIfChanged(ref color, value);
    }

    int _value;

    public int Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public AuthenticatorItemModel()
    {
        Value = 30;
        Color = Brush.Parse("#61a4f0");
        Text = "WD24S";
        Test();
    }
    
    public void Test()
    {
        var progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
        var progress = Value;
        progressTimer.Tick += (_, _) =>
        {
            progress -= 1;
            if (progress < 1)
                progress = 30;
            if(progress%2==0) Color=Brush.Parse("#61a4f0");
            else Color=Brush.Parse("#6198ff");
            Value = progress;
            Text = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
        };
        progressTimer.Start();
    }
}