using BD.WTTS.Client.Resources;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamTradePageViewModel
{
    readonly SteamAuthenticator? _steamAuthenticator;

    WinAuth.SteamClient? _steamClient;

    string? _captchaId;
    
    readonly SourceList<WinAuth.SteamClient.Confirmation> _confirmationsSourceList = new();

    CancellationTokenSource? _operationTradeAllCancelToken;

    public SteamTradePageViewModel()
    {
    }

    public SteamTradePageViewModel(IAuthenticatorDTO authenticatorDto)
    {
        if (authenticatorDto.Value is SteamAuthenticator steamAuthenticator)
        {
            _steamAuthenticator = steamAuthenticator;
            
            Initialize();

            UserNameText = _steamAuthenticator.AccountName ?? "";
        }
    }

    void Initialize()
    {
        _confirmationsSourceList
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            //.Sort(SortExpressionComparer<WinAuthSteamClient.Confirmation>.Descending(x => x.))
            .Bind(out _confirmations)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(IsConfirmationsAny));
                this.RaisePropertyChanged(nameof(ConfirmationsCountMessage));
            });

        RegisterSelectAllObservable();
    }
    
    public override void Deactivation()
    { 
        _steamClient = _steamAuthenticator!.GetClient();
        if (!_steamClient.IsLoggedIn())
        {
            _steamClient.Clear();
        }
        base.Deactivation();
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

        var result = await RunTaskAndExceptionHandlingAsync(new Task<bool>(() =>
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

        SelectIndex = 2;
        
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
        task.Start();
        var result = await task;
        if (task.Exception == null) return result;
    
        ExceptionHandling(task.Exception);
        return result;
    }

    void ExceptionHandling(Exception exception, bool allowRetry = true)
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
            if (!allowRetry) return;
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

            return;
        }

        Log.Error(nameof(SteamTradePageViewModel), exception, nameof(ExceptionHandling));
        Toast.Show(exception.Message);
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

    public async Task ConfirmAllTrade()
    {
        await OperationTrade(true);
    }

    public async Task CancelTrade(WinAuth.SteamClient.Confirmation trade)
    {
        await OperationTrade(false, trade);
    }

    public async Task CancelAllTrade()
    {
        await OperationTrade(false);
    }
    
    async Task OperationTrade(bool status, WinAuth.SteamClient.Confirmation trade)
    {
        var result = await ChangeTradeStatus(status, trade);
        if (result)
        {
            Toast.Show($"{(status ? Strings.Agree : Strings.Cancel)}{trade.Details}");
            trade.IsOperate = status ? 1 : 2;
            RefreshConfirmationsList();
        }
    }

    async Task OperationTrade(bool status)
    {
        if (!_confirmationsSourceList.Items.Any_Nullable())
        {
            Toast.Show(Strings.LocalAuth_AuthTrade_Null);
            return;
        }

        if (_operationTradeAllCancelToken != null)
        {
            Toast.Show("已有一项操作正在进行中，请等待执行结束");
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
            var executableNum = _confirmationsSourceList.Items.Count();
            foreach (var item in _confirmationsSourceList.Items)
            {
                if (_operationTradeAllCancelToken.IsCancellationRequested)
                {
                    Toast.Show(
                        $"已终止{statusText}全部令牌,已操作成功令牌数量{success},操作失败令牌数量{failed},剩余{executableNum - success - failed}令牌未操作");
                }

                if (item.IsOperate != 0 || item.NotChecked)
                {
                    executableNum--;
                    continue;
                }

                var startTime = DateTime.Now;

                if (!await ChangeTradeStatus(status, item))
                {
                    failed++;
                    continue;
                }

                item.IsOperate = 1;
                success++;
                
                var duration = (int)DateTime.Now.Subtract(startTime).TotalMilliseconds;
                var delay = WinAuth.SteamClient.CONFIRMATION_EVENT_DELAY +
                            Random2.Next(WinAuth.SteamClient.CONFIRMATION_EVENT_DELAY / 2);
                if (delay > duration) await Task.Delay(delay - duration);
            }

            _operationTradeAllCancelToken = null;
            RefreshConfirmationsList();
            Toast.Show(
                $"{statusText}全部令牌执行结束,成功数量{success},失败数量{failed},剩余{executableNum - success - failed}令牌未操作");
            SetToastServiceStatus();
        }
    }

    async Task<bool> ChangeTradeStatus(bool status, WinAuth.SteamClient.Confirmation trade)
    {
        if (string.IsNullOrEmpty(trade.Id))
        {
            Toast.Show(Strings.LocalAuth_AuthTrade_TradeError);
            return false;
        }
        var task = Task.Run(() => _steamClient!.ConfirmTrade(trade.Id, trade.Key, status));
        var result = await task;
        if (!result) Toast.Show(Strings.LocalAuth_AuthTrade_ConfirmError);
        if (task.Exception == null) return result;
        Log.Error(nameof(SteamTradePageViewModel), task.Exception, nameof(ChangeTradeStatus));
        Toast.Show(task.Exception.Message);
        return result;
    }

    public async Task ShowCaptchaUrl() => await AuthenticatorService.ShowCaptchaUrl(CaptchaImageUrlText);
    
    bool _isUnselectAllChangeing;
    
    /// <summary>
    /// Confirmations SourceList Any
    /// </summary>
    public bool IsConfirmationsAny => _confirmationsSourceList.Items.Any_Nullable();

    public string ConfirmationsCountMessage
    {
        get
        {
            if (IsLoading)
            {
                return string.Empty;
            }
            if (!IsConfirmationsAny)
            {
                return Strings.LocalAuth_AuthTrade_ListNullTip;
            }
            return Strings.LocalAuth_AuthTrade_ListCountTip.Format(_confirmationsSourceList.Count);
        }
    }
    
    /// <summary>
    /// 注册全选监听
    /// </summary>
    void RegisterSelectAllObservable() => this.WhenAnyValue(x => x.Confirmations)
        .SubscribeInMainThread(x => x?
            .ToObservableChangeSet()
            .AutoRefresh(x => x.NotChecked)
            .ToCollection()
            .Select(x =>
            {
                int selectCount, count;
                if (_isUnselectAllChangeing)
                {
                    selectCount = 0;
                    count = 0;
                }
                else
                {
                    selectCount = x.Count(y => y.IsOperate == 0 && !y.NotChecked);
                    count = x.Count(y => y.IsOperate == 0);
                }
                return (select_count: selectCount, count);
            })
            .Subscribe(x =>
            {
                if (_isUnselectAllChangeing) return;
                if (x.count > 0)
                {
                    var unselectAll = x.select_count != x.count;
                    if (_unSelectAll != unselectAll)
                    {
                        _unSelectAll = unselectAll;
                        this.RaisePropertyChanged(nameof(UnSelectAll));
                    }
                    SelectAllText = Strings.SelectAllText_.Format(x.select_count, x.count);
                }
                else
                {
                    SelectAllText = null;
                }
            }).AddTo(this))
        .AddTo(this);
}