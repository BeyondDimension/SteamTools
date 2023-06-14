using Avalonia.Media;
using Avalonia.Threading;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.Models;

public class AuthenticatorItemModel : ItemViewModel
{
    public override string Name => nameof(AuthenticatorItemModel);

    IAuthenticatorDTO AuthData { get; set; }

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

    string? secondtext;

    public string? SecondText
    {
        get => secondtext;
        set
        {
            this.RaiseAndSetIfChanged(ref secondtext, value);
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
        AuthData = authenticatorDto;
        AuthName = AuthData.Name;
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
            if(progress%2==0) Color=Brush.Parse("#61a4f0");
            else Color=Brush.Parse("#6198ff");
            Value = progress;
            //SecText = AuthenticatorDto.Value.CurrentCode;
        };
        progressTimer.Start();
    }

    public async Task CopyCode()
    {
        
    }

    //未写完，临时存储用的
    public async Task DeleteAuthAsync()
    {
        var messageviewmodel =
            new MessageBoxWindowViewModel { Content = Strings.LocalAuth_DeleteAuthTip, IsCancelcBtn = true };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(messageviewmodel))
        {
            AuthenticatorService.DeleteAuth(AuthData);
        }
    }
        
    public async Task EditAuthNameAsync()
    {
        string? newname = null;

        var textviewmodel = new TextBoxWindowViewModel { InputType = TextBoxWindowViewModel.TextBoxInputType.TextBox };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textviewmodel, "请输入新令牌名或取消", isDialog: false,
                isCancelButton: true))
        {
            newname = textviewmodel.Value;
        }

        if (string.IsNullOrEmpty(newname)) return;

        AuthName = newname;
        await AuthenticatorService.SaveEditAuthNameAsync(AuthData, newname);
    }
}