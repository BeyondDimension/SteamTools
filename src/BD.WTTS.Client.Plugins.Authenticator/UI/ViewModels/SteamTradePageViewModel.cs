using BD.WTTS.Client.Resources;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public partial class SteamTradePageViewModel
{
    SteamAuthenticator? _steamAuthenticator;

    WinAuth.SteamClient? _steamClient;

    string? _captchaId;
    
    readonly SourceList<WinAuth.SteamClient.Confirmation> _confirmationsSourceList = new();

    public SteamTradePageViewModel()
    {
        
    }

    public SteamTradePageViewModel(IAuthenticatorDTO authenticatorDto)
    {
        if (authenticatorDto.Value is SteamAuthenticator steamAuthenticator)
        {
            _steamAuthenticator = steamAuthenticator;
        }
    }

    public async Task Refresh()
    {
        if (_steamClient == null) return;
        _ = await GetConfirmations(_steamClient);
    }

    public async Task Login()
    {
        if (string.IsNullOrEmpty(UserNameText) || string.IsNullOrEmpty(PasswordText))
        {
            Toast.Show(Strings.User_LoginError_Null);
            return;
        }
        if (IsLogging || IsLoading || IsLogged || _steamAuthenticator == null) return;

        SetToastServiceStatus(Strings.Logining);

        IsLogging = true;
        
        var result = await RunTaskAndExceptionHandlingAsync(
            new Task<bool>(() =>
            {
                _steamClient = _steamAuthenticator.GetClient();
                var loginResult = _steamClient.Login(UserNameText, PasswordText, _captchaId, CaptchaCodeText,
                    ResourceService.GetCurrentCultureSteamLanguageName());
                return loginResult;
            }));

        if (_steamClient == null) return;
        
        if (!result)
        {
            if (_steamClient.Error == "Incorrect Login")
            {
                Toast.Show(Strings.User_LoginError);
                return;
            }

            if (_steamClient.RequiresCaptcha == true)
            {
                _captchaId = _steamClient.CaptchaId;
                CaptchaImageUrlText = _steamClient.CaptchaUrl;
                Toast.Show(Strings.User_LoginError_CodeImage);
                SelectIndex = 1;
                return;
            }

            Toast.Show($"未知登陆错误：{_steamClient.Error}");
            return;
        }

        Toast.Show(string.Format(Strings.Success_, Strings.User_Login));
            
        SetToastServiceStatus();
            
        IsLogged = true;
        
        IsLogging = false;
        
        //_steamAuthenticator.SessionData = RemenberLogin ? result.steamClient.Session.ToString() : null;

        var list = await GetConfirmations(_steamClient);

        if (list == null) return;
    }

    async Task<List<WinAuth.SteamClient.Confirmation>?> GetConfirmations(WinAuth.SteamClient steamClient)
    {
        IsLoading = true;
        SetToastServiceStatus(Strings.LocalAuth_AuthTrade_GetTip);

        var result = await RunTaskAndExceptionHandlingAsync(
            new Task<List<WinAuth.SteamClient.Confirmation>>(steamClient.GetConfirmations));

        _confirmationsSourceList.Clear();
        _confirmationsSourceList.AddRange(result);
        
        SetToastServiceStatus();
        IsLoading = false;

        return result;
    }

    void SetToastServiceStatus(string? statusMessage = null)
    {
        if (string.IsNullOrEmpty(statusMessage))
        {
            if (ToastService.IsSupported)
            {
                ToastService.Current.Set();
            }
            return;
        }
        if (ToastService.IsSupported)
        {
            ToastService.Current.Set(Strings.Logining);
        }
    }

    async Task<T> RunTaskAndExceptionHandlingAsync<T>(Task<T> task)
    {
        var result = await task;
        if (task.Exception == null) return result;

        ExceptionHandling(task.Exception);
        return result;
    }

    void ExceptionHandling(Exception exception)
    {
        //可能是启用了家庭监护功能
        if (exception is WinAuthUnauthorisedSteamRequestException unauthorisedSteamRequestException)
        {
            Toast.Show(Strings.LocalAuth_AuthTrade_GetError);
            return;
        }
        
        //可能是会话错误，重新尝试一次
        if (exception is WinAuthInvalidSteamRequestException invalidSteamRequestException)
        {
            try
            {
                if (_steamClient!.IsLoggedIn())
                {
                    _steamClient.Refresh();
                    var list = _steamClient.GetConfirmations();
                    _confirmationsSourceList.Clear();
                    _confirmationsSourceList.AddRange(list);
                }
                else throw invalidSteamRequestException;
            }
            catch (Exception e)
            {
                Log.Error(nameof(SteamTradePageViewModel), e, "可能是没有开加速器导致无法连接 Steam 社区登录地址");
                if (e.InnerException != null && e.Message.Contains("302"))
                {
                    Toast.Show(Strings.LocalAuth_AuthTrade_GetError3);
                    IsLogged = false;
                    _steamClient!.Clear();
                }
            }
        }
    }
    
    public async Task Logout()
    {
        if (_steamClient == null) return;
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = Strings.LocalAuth_LogoutTip, }, isCancelButton: true,
                isDialog: false))
        {
            _steamClient.Logout();
        }
    }
    
    void RefreshConfirmationsList()
    {
        var items = _confirmationsSourceList.Items.Where(s => s.IsOperate == 0);
        _confirmationsSourceList.Clear();
        var confirmations = items.ToList();
        if (confirmations.Any())
            _confirmationsSourceList.AddRange(confirmations);
    }

    public async Task ConfirmTrade(WinAuth.SteamClient.Confirmation trade)
    {
        await OperationTrade(true, trade);
    }

    public async Task CancelTrade(WinAuth.SteamClient.Confirmation trade)
    {
        await OperationTrade(false, trade);
    }

    public async Task OperationTrade(bool status, WinAuth.SteamClient.Confirmation trade)
    {
        var result = await ChangeTradeStatus(status, trade);
        if (result)
        {
            Toast.Show($"{(status ? Strings.Agree : Strings.Cancel)}{trade.Details}");
            trade.IsOperate = status ? 1 : 2;
            RefreshConfirmationsList();
        }
    }

    async Task<bool> ChangeTradeStatus(bool status, WinAuth.SteamClient.Confirmation trade)
    {
        var task = Task.Run(() =>
        {
            try
            {
                return _steamClient!.ConfirmTrade(trade.Id, trade.Key, status);
            }
            catch (Exception e)
            {
                return false;
            }
        });
        var result = await task;
        if (!result) Toast.Show(Strings.LocalAuth_AuthTrade_ConfirmError);
        if (task.Exception == null) return result;
        Log.Error(nameof(SteamTradePageViewModel), task.Exception, nameof(ChangeTradeStatus));
        Toast.Show(task.Exception.Message);
        return result;
    }

    public async Task ShowCaptchaUrl() => await AuthenticatorService.ShowCaptchaUrl(CaptchaImageUrlText);
}