using Avalonia.Media;
using Avalonia.Threading;

namespace BD.WTTS.Models;

public class AuthenticatorItemModel : ItemViewModel
{
    public override string Name => nameof(AuthenticatorItemModel);

    IAuthenticatorDTO AuthenticatorDto { get; set; }

    string? text;

    public string? Text
    {
        get => text;
        set
        {
            this.RaiseAndSetIfChanged(ref text, value);
        }
    }

    string? sectext;

    public string? SecText
    {
        get => sectext;
        set
        {
            this.RaiseAndSetIfChanged(ref sectext, value);
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

    public AuthenticatorItemModel(IAuthenticatorDTO authenticatorDto)
    {
        SecText = "右击以复制";
        Color = Brush.Parse("#61a4f0");
        AuthenticatorDto = authenticatorDto;
        Test();
    }
    
    public void Test()
    {
        var progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
        if (AuthenticatorDto.Value == null) return;
        var servertime = AuthenticatorDto.Value.ServerTime;
        var currentTimeChunk = AuthenticatorDto.Value.CodeInterval;
        int seconds = (int)((servertime - (currentTimeChunk * 30000L)) / 1000L);
        var progress = AuthenticatorDto.Value.Period - seconds;
        Value = progress;
        Text = AuthenticatorDto.Value.CurrentCode;
        progressTimer.Tick += (_, _) =>
        {
            progress -= 1;
            if (progress < 1)
            { 
                servertime = AuthenticatorDto.Value.ServerTime;
                currentTimeChunk = AuthenticatorDto.Value.CodeInterval;
                seconds = (int)((servertime - (currentTimeChunk * 30000L)) / 1000L); 
                progress = AuthenticatorDto.Value.Period - seconds;
                Text = AuthenticatorDto.Value.CurrentCode;
            }
            if(progress%2==0) Color=Brush.Parse("#61a4f0");
            else Color=Brush.Parse("#6198ff");
            Value = progress;
            //SecText = AuthenticatorDto.Value.CurrentCode;
        };
        progressTimer.Start();
    }
}