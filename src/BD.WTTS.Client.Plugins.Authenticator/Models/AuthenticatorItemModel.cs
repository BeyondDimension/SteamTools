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

    async void ShowCode(object? sender, EventArgs e)
    {
        _progress -= 1;
        if (_progress < 1)
        {
            await RefreshCode().ContinueWith((t) =>
            {
                if (string.IsNullOrEmpty(Text))
                {
                    DisableShowCode();
                }
            });
        }
        StrokeColor = Brush.Parse(_progress % 2 == 0 ? "#61a4f0" : "#6198ff");
        Value = _progress;
    }

    async Task RefreshCode()
    {
        await Task.Run(() =>
        {
            try
            {
                var code = AuthData.Value!.CurrentCode;
                return code;
            }
            catch (Exception e)
            {
                Toast.Show(ToastIcon.Error, $"计算令牌时与服务器时间同步失败：{e.Message}");
                Log.Error(nameof(AuthenticatorItemModel), e, nameof(OnPointerLeftPressed));
                return null;
            }
        }).ContinueWith(async (code) =>
        {
            _progress = AuthData.Value!.Period -
                        (int)((AuthData.Value.ServerTime - (AuthData.Value.CodeInterval * 30000L)) / 1000L);
            Text = await code;
        });
    }

    async void EnableShowCode()
    {
        if (AuthData.Value == null) return;
        await RefreshCode().ContinueWith((t) =>
        {
            if (string.IsNullOrEmpty(Text)) return;
            Value = _progress;
            _progressTimer.Start();
        });
    }

    void DisableShowCode()
    {
        _progressTimer.Stop();
        Text = "-----";
        IsShowCode = false;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _progressTimer.Tick -= ShowCode;
    }
}