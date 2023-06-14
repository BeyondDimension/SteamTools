using Avalonia.Media;
using Avalonia.Threading;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.Models;

public class AuthenticatorItemModel : ItemViewModel
{
    public override string Name => nameof(AuthenticatorItemModel);

    public IAuthenticatorDTO AuthData { get; set; }

    bool isselected;

    public override bool IsSelected
    {
        get => isselected;
        set
        {
            this.RaiseAndSetIfChanged(ref isselected, value);
            OnAuthenticatorItemIsSelectedChanged?.Invoke(this);
        }
    }

    public void OnPointerPressed()
    {
        IsSelected = !IsSelected;
    }

    public delegate void AuthenticatorItemIsSelectedChangeEventHandler(AuthenticatorItemModel sender);

    public static event AuthenticatorItemIsSelectedChangeEventHandler? OnAuthenticatorItemIsSelectedChanged;

    string? authname;

    public string? AuthName
    {
        get => authname;
        set
        {
            this.RaiseAndSetIfChanged(ref authname, value);
        }
    }

    string? text;

    public string? Text
    {
        get => text;
        set
        {
            this.RaiseAndSetIfChanged(ref text, value);
        }
    }

    IBrush? strokecolor;

    public IBrush? StrokeColor
    {
        get => strokecolor;
        set => this.RaiseAndSetIfChanged(ref strokecolor, value);
    }

    double _value;

    public double Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, (double)(value * 12.00d));
    }

    public AuthenticatorItemModel(IAuthenticatorDTO authenticatorDto)
    {
        AuthData = authenticatorDto;
        AuthName = AuthData.Name;
        StrokeColor = Brush.Parse("#61a4f0");
        Test();
    }

    public void Test()
    {
        var progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
        if (AuthData.Value == null) return;
        var servertime = AuthData.Value.ServerTime;
        var currentTimeChunk = AuthData.Value.CodeInterval;
        int seconds = (int)((servertime - (currentTimeChunk * 30000L)) / 1000L);
        var progress = AuthData.Value.Period - seconds;
        Value = progress;
        Text = AuthData.Value.CurrentCode;
        progressTimer.Tick += (_, _) =>
        {
            progress -= 1;
            if (progress < 1)
            {
                servertime = AuthData.Value.ServerTime;
                currentTimeChunk = AuthData.Value.CodeInterval;
                seconds = (int)((servertime - (currentTimeChunk * 30000L)) / 1000L);
                progress = AuthData.Value.Period - seconds;
                Text = AuthData.Value.CurrentCode;
            }
            if (progress % 2 == 0) StrokeColor = Brush.Parse("#61a4f0");
            else StrokeColor = Brush.Parse("#6198ff");
            Value = progress;
            //SecText = AuthenticatorDto.Value.CurrentCode;
        };
        progressTimer.Start();
    }
}