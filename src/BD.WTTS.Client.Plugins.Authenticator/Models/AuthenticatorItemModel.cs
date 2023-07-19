using Avalonia.Media;
using Avalonia.Threading;
using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Client.Resources;
using WinAuth;
using KeyValuePair = BD.Common.Entities.KeyValuePair;

namespace BD.WTTS.Models;

public partial class AuthenticatorItemModel : ItemViewModel
{
    public override string Name => nameof(AuthenticatorItemModel);

    public IAuthenticatorDTO AuthData { get; set; }

    readonly DispatcherTimer _progressTimer;

    int _progress;
    
    public void OnPointerLeftPressed()
    {
        IsSelected = !IsSelected;
    }

    // public void OnPointerRightPressed()
    // {
    //     //CopyCode();
    // }

    public async Task OnTextPanelOnTapped()
    {
        await CopyCode();
    }
    
    async Task CopyCode()
    {
        if (!IsShowCode) return;
        await Clipboard2.SetTextAsync(AuthData.Value?.CurrentCode);
        Toast.Show(ToastIcon.Success, Strings.LocalAuth_CopyAuthTip + AuthName);
    }

    public AuthenticatorItemModel(IAuthenticatorDTO authenticatorDto)
    {
        AuthData = authenticatorDto;
        _authName = AuthData.Name;
        StrokeColor = Brush.Parse("#61a4f0");
        Text = "-----";
        _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
        _progressTimer.Tick += ShowCode;
    }
    
    void ShowCode(object? sender, EventArgs e)
    {
        _progress -= 1;
        if (_progress < 1)
        {
            Text = RefreshCode();
            if (string.IsNullOrEmpty(Text))
            {
                IsShowCode = false;
                _progressTimer.Stop();
                return;
            }
        }
        StrokeColor = Brush.Parse(_progress % 2 == 0 ? "#61a4f0" : "#6198ff");
        Value = _progress;
    }

    string? RefreshCode()
    {
        try
        {
            var code = AuthData.Value!.CurrentCode;
            _progress = AuthData.Value.Period -
                        (int)((AuthData.Value.ServerTime - (AuthData.Value.CodeInterval * 30000L)) / 1000L);
            return code;
        }
        catch (Exception e)
        {
            Toast.Show(ToastIcon.Error, $"计算令牌时与服务器时间同步失败：{e.Message}");
            Log.Error(nameof(AuthenticatorItemModel), e, nameof(OnPointerLeftPressed));
        }

        return null;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _progressTimer.Tick -= ShowCode;
    }
}