using AppResources = BD.WTTS.Client.Resources.Strings;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamTradePageViewModel
{
    readonly SteamAuthenticator? _steamAuthenticator;
    
    WinAuth.SteamClient? _steamClient;

    string? _captchaId;

    public ObservableCollection<SteamTradeConfirmationModel> Confirmations { get; set; } = new();

    CancellationTokenSource? _operationTradeAllCancelToken;

    public SteamTradePageViewModel()
    {
    }

    public SteamTradePageViewModel(ref IAuthenticatorDTO authenticatorDto)
    {
        if (authenticatorDto.Value is SteamAuthenticator steamAuthenticator)
        {
            _steamAuthenticator = steamAuthenticator;

            _steamClient = _steamAuthenticator.GetClient();
            
            Initialize();

            UserNameText = _steamAuthenticator.AccountName ?? "";

            authenticatorDto.Value = _steamAuthenticator;
        }
    }

    async void Initialize()
    {
        if (_steamClient!.IsLoggedIn() == false)
        {
            IsLoading = false;
            IsLogged = false;
        }
        else
        {
            IsLogged = true;
            await GetConfirmations(_steamClient);
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
            Toast.Show(ToastIcon.Warning, Strings.User_LoginError_Null);
            return;
        }
        if (IsLoading || IsLogged || _steamAuthenticator == null) return;
        
        _steamClient ??= _steamAuthenticator.GetClient();

        SetToastServiceStatus(Strings.Logining);

        IsLoading = true;
        IsLogged = true;
        var result = await RunTaskAndExceptionHandlingAsync(new Task<bool>(() =>
        {
            var loginResult = _steamClient.Login(UserNameText, PasswordText, _captchaId, CaptchaCodeText,
                ResourceService.GetCurrentCultureSteamLanguageName());
            return loginResult;
        }));

        if (_steamClient == null) return;
        
        if (!result)
        {
            IsLogged = result;
            if (_steamClient.Error == "Incorrect Login")
            {
                Toast.Show(ToastIcon.Warning, Strings.User_LoginError);
                return;
            }

            // if (_steamClient.RequiresCaptcha == true)
            // {
            //     _captchaId = _steamClient.CaptchaId;
            //     CaptchaImageUrlText = _steamClient.CaptchaUrl;
            //     Toast.Show(ToastIcon.None, Strings.User_LoginError_CodeImage);
            //     SelectIndex = 1;
            //     return;
            // }

            Toast.Show(ToastIcon.Error, AppResources.Error_UnknownLogin_.Format(_steamClient.Error));
            IsLoading = false;
            return;
        }

        Toast.Show(ToastIcon.Success, string.Format(Strings.Success_, Strings.User_Login));
            
        SetToastServiceStatus();

        //_steamAuthenticator.SessionData = RemenberLogin ? result.steamClient.Session.ToString() : null;
        _ = await GetConfirmations(_steamClient);
        
        IsLoading = false;
    }

    async Task<IEnumerable<SteamTradeConfirmationModel>?> GetConfirmations(WinAuth.SteamClient steamClient)
    {
        IsLoading = true;
        SetToastServiceStatus(Strings.LocalAuth_AuthTrade_GetTip);

        var result = await RunTaskAndExceptionHandlingAsync(
            new Task<IEnumerable<SteamMobileTradeConf>>(steamClient.GetConfirmations));

        if (result == null) return null;

        //var models = result.Select(item => new SteamTradeConfirmationModel(_steamAuthenticator!, item)).ToList();
        
        Confirmations.Clear();
        foreach (var item in result)
        {
            Confirmations.Add(new SteamTradeConfirmationModel(_steamAuthenticator!, item));
        }
        //Confirmations.AddRange(models);
        
        SetToastServiceStatus();
        IsLoading = false;

        return Confirmations;
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

    async Task<T?> RunTaskAndExceptionHandlingAsync<T>(Task<T> task)
    {
        try
        {
            task.Start();
            var result = await task;
            return result;
        }
        catch (Exception e)
        {
            ExceptionHandling(e);
            return default;
        }
    }

    void ExceptionHandling(Exception exception, bool allowRetry = true)
    {
        //可能是启用了家庭监护功能
        if (exception is WinAuthUnauthorisedSteamRequestException unauthorisedSteamRequestException)
        {
            Toast.Show(ToastIcon.Error, Strings.LocalAuth_AuthTrade_GetError);
            return;
        }

        //可能是会话错误，重新尝试一次
        if (exception is WinAuthInvalidSteamRequestException invalidSteamRequestException)
        {
            if (!allowRetry) return;
            try
            {
                if (_steamClient!.IsLoggedIn())
                {
                    //_steamClient.Refresh();
                    var list = _steamClient.GetConfirmations();

                    //var models = list.Select(item => new SteamTradeConfirmationModel(_steamAuthenticator!, item));

                    Confirmations.Clear();
                    foreach (var item in list)
                    {
                        Confirmations.Add(new SteamTradeConfirmationModel(_steamAuthenticator!, item));
                    }
                    //Confirmations.AddRange(models);
                }
                else throw invalidSteamRequestException;
            }
            catch (Exception e)
            {
                Log.Error(nameof(SteamTradePageViewModel), e, "可能是没有开加速器导致无法连接 Steam 社区登录地址");
                if (e.InnerException != null && e.Message.Contains("302"))
                {
                    Toast.Show(ToastIcon.Error, Strings.LocalAuth_AuthTrade_GetError3);
                    IsLogged = false;
                    _steamClient!.Clear();
                }
            }

            return;
        }

        Log.Error(nameof(SteamTradePageViewModel), exception, nameof(ExceptionHandling));
        Toast.Show(ToastIcon.Error, AppResources.Error_Exception_.Format(exception.Message));
    }

    // public async Task Logout()
    // {
    //     if (_steamClient == null) return;
    //     if (await IWindowManager.Instance.ShowTaskDialogAsync(
    //             new MessageBoxWindowViewModel() { Content = Strings.LocalAuth_LogoutTip, }, isCancelButton: true,
    //             isDialog: false))
    //     {
    //         _steamClient.Logout();
    //     }
    // }
    
    // void RefreshConfirmationsList()
    // {
    //     var items = _confirmationsSourceList.Items.Where(s => s.IsOperate == 0);
    //     _confirmationsSourceList.Clear();
    //     var confirmations = items.ToList();
    //     if (confirmations.Any())
    //         _confirmationsSourceList.AddRange(confirmations);
    // }

    public async Task ConfirmTrade(object sender)
    {
        if (sender is not SteamTradeConfirmationModel trade) return;
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel()
                {
                    Content =
                        $"{trade.SendSummary}\r\n" +
                        $"{trade.ReceiveSummary}\r\n" +
                        $"{(trade.ReceiveNoItems ? $"您尚未让 {trade.Headline} 选择任何物品以交换您的物品。如果 {trade.Headline}  接受此交易，您将失去您提供的物品，但不会收到任何物品。" : string.Empty)}\r\n" +
                        $"如果您没有创建此交易，请立即取消该交易。您的帐户或电脑可能已遭盗用。",
                }, "确认交易",
                isCancelButton: true, isDialog: false))
        {
            await OperationTrade(true, trade);
        }
    }

    public async Task ConfirmAllTrade()
    {
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = "您确认要通过所有交易报价吗\r\n" +
                                                            "您将同意列表内全部的报价请求。\r\n" +
                                                            "交易一经确认无法撤销，请谨慎选择。" },
                isCancelButton: true, isDialog: false))
        {
            await OperationTrade(true);
        }
    }

    public async Task CancelTrade(object sender)
    {
        if (sender is not SteamTradeConfirmationModel trade) return;
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = "您确认需要取消交易报价吗\r\n" +
                                                            "这将从以下列表中移除此条确认信息\r\n" +
                                                            "您可以在Steam库存查看交易报价记录。" },
                isCancelButton: true, isDialog: false))
        {
            await OperationTrade(false, trade);
        }
    }

    public async Task CancelAllTrade()
    {
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = "您确认要取消所有交易报价吗\r\n" +
                                                            "您将拒绝列表内全部的报价请求。\r\n" +
                                                            "这将从以下列表中移除所有交易信息\r\n" +
                                                            "您可以在Steam库存查看交易报价记录。" },
                isCancelButton: true, isDialog: false))
        {
            await OperationTrade(false);
        }
    }
    
    async Task<bool> OperationTrade(bool status, SteamTradeConfirmationModel trade)
    {
        var result = await ChangeTradeStatus(status, trade);
        if (result)
        {
            Toast.Show(ToastIcon.Success, $"{(status ? Strings.Agree : Strings.Cancel)}{trade.TypeName}");
            //trade.IsOperate = status ? 1 : 2;
            //RefreshConfirmationsList();
        }

        return result;
    }

    async Task OperationTrade(bool status)
    {
        if (!Confirmations.Any_Nullable())
        {
            Toast.Show(ToastIcon.Warning, Strings.LocalAuth_AuthTrade_Null);
            return;
        }

        if (_operationTradeAllCancelToken != null)
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseWaitExecuteFinish);
            return;
        }

        var statusText = status ? Strings.Agree : Strings.Cancel;

        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel()
                {
                    Content = Strings.LocalAuth_AuthTrade_MessageBoxTip.Format(statusText)
                }, isCancelButton: true, isDialog: false))
        {
            SetToastServiceStatus(
                Strings.LocalAuth_AuthTrade_ConfirmTip.Format(statusText));

            _operationTradeAllCancelToken = new CancellationTokenSource();
            ushort success = 0;
            ushort failed = 0;
            //var executableNum = _confirmationsSourceList.Items.Count();
            foreach (var item in Confirmations)
            {
                if (_operationTradeAllCancelToken.IsCancellationRequested)
                {
                    Toast.Show(ToastIcon.Warning, 
                        $"已终止{statusText}全部令牌,已操作成功令牌数量{success},操作失败令牌数量{failed}");
                    // Toast.Show(ToastIcon.None, 
                    //     $"已终止{statusText}全部令牌,已操作成功令牌数量{success},操作失败令牌数量{failed},剩余{executableNum - success - failed}令牌未操作");
                }

                // if (item.IsOperate != 0 || item.NotChecked)
                // {
                //     executableNum--;
                //     continue;
                // }

                var startTime = DateTime.Now;

                if (!await ChangeTradeStatus(status, item))
                {
                    failed++;
                    continue;
                }

                //item.IsOperate = 1;
                success++;
                
                var duration = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;
                var delay = WinAuth.SteamClient.CONFIRMATION_EVENT_DELAY +
                            Random2.Next(WinAuth.SteamClient.CONFIRMATION_EVENT_DELAY / 2);
                if (delay > duration) await Task.Delay(delay - duration);
            }

            _operationTradeAllCancelToken = null;
            //RefreshConfirmationsList();
            Toast.Show(ToastIcon.Success, 
                $"{statusText}全部令牌执行结束,成功数量{success},失败数量{failed}");
            // Toast.Show(ToastIcon.None, 
            //     $"{statusText}全部令牌执行结束,成功数量{success},失败数量{failed},剩余{executableNum - success - failed}令牌未操作");
            SetToastServiceStatus();
        }
    }

    async Task<bool> ChangeTradeStatus(bool status, SteamTradeConfirmationModel trade)
    {
        if (!await ChangeTradeStatus(status, new[] { trade })) return false;
        Confirmations.Remove(trade);
        return true;

    }
    
    async Task<bool> ChangeTradeStatus(bool status, IEnumerable<SteamTradeConfirmationModel> trades)
    {
        var ids = trades.ToDictionary(item => item.Id, item => item.Nonce);

        var task = Task.Run(() =>
            _steamClient!.ConfirmTrade(ids, status));
        var result = await task;
        if (!result) Toast.Show(ToastIcon.Error, Strings.LocalAuth_AuthTrade_ConfirmError);
        if (task.Exception == null) return result;
        Log.Error(nameof(SteamTradePageViewModel), task.Exception, nameof(ChangeTradeStatus));
        Toast.Show(ToastIcon.Error, task.Exception.Message);
        return result;
    }

    public async Task ShowCaptchaUrl() => await AuthenticatorService.ShowCaptchaUrl(CaptchaImageUrlText);
}