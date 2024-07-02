using BD.SteamClient.Models;
using BD.SteamClient.Services;
using WinAuth;
using AppResources = BD.WTTS.Client.Resources.Strings;
using WSteamClient = WinAuth.SteamClient;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamTradePageViewModel : WindowViewModel
{
    readonly IAuthenticatorDTO _authenticatorDto;
    WSteamClient? _steamClient;
    readonly SteamAuthenticator? _steamAuthenticator;
    readonly ISteamAccountService _steamAccountService = Ioc.Get<ISteamAccountService>();

    public SteamTradePageViewModel(ref IAuthenticatorDTO authenticatorDto)
    {
        _authenticatorDto = authenticatorDto;

        ConfirmTradeCommand = ReactiveCommand.Create<SteamTradeConfirmationModel>(ConfirmTrade);
        CancelTradeCommand = ReactiveCommand.Create<SteamTradeConfirmationModel>(CancelTrade);

        try
        {
            if (authenticatorDto.Value is SteamAuthenticator steamAuthenticator)
            {
                _steamAuthenticator = steamAuthenticator;
                _steamClient = _steamAuthenticator.GetClient(ResourceService.GetCurrentCultureSteamLanguageName());
                UserNameText = _steamAuthenticator.AccountName ?? "";
                authenticatorDto.Value = _steamAuthenticator;
                _ = Initialize();

                this.WhenAnyValue(v => v.Confirmations)
                    .Subscribe(items => items?
                    .ToObservableChangeSet()
                    .AutoRefresh(x => x.IsSelected)
                    .WhenValueChanged(x => x.IsSelected)
                    .Subscribe(_ =>
                    {
                        bool? b = null;
                        var count = items.Count(s => s.IsSelected);
                        if (!items.Any_Nullable() || count == 0)
                            b = false;
                        else if (count == items.Count)
                            b = true;

                        if (SelectedAll != b)
                        {
                            SelectedAll = b;
                        }
                    }));
            }
            else
            {
                Close?.Invoke(false);
            }
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
    }

    public override async Task Initialize()
    {
        await base.Initialize();

        if (_steamClient!.IsLoggedIn() == false)
        {
            IsLoading = false;
            IsLogged = false;
        }
        else
        {
            IsLogged = await CheckAndRefreshToken();

            await GetConfirmations(_steamClient);
        }
    }

    public async ValueTask<bool> CheckAndRefreshToken()
    {
        IsLoading = true;

        // 不具备检查条件 没有保持的会话信息
        if (_steamClient == null)
            return false;

        if (_steamClient.Session == null || _steamAuthenticator == null)
            return false;

        try
        {
            // token 依旧有效 直接返回
            if (!_steamClient.Session.IsAccessTokenExpired())
                return true;

            // 是否有 refresh token
            string? refreshToken = _steamClient.Session?.RefreshToken;

            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var session = _steamClient.Session;

            // 刷新 access token (有效时间同样为一天)
            await session!.RefreshAccessToken(_steamClient);

            // 使用原先获取的具体派生类型 修改数据
            _steamAuthenticator.SessionData = session.ToString();

            // 重置当前 client
            if (!ResetCurrentClient())
            {
                return false;
            }

            // 通过原始令牌引用保存信息
            await AuthenticatorHelper.SaveAuthenticator(_authenticatorDto)
                .ConfigureAwait(false);

            return true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task Refresh()
    {
        if (_steamClient?.IsLoggedIn() == true)
            _ = await GetConfirmations(_steamClient);
    }

    public void SelectAllCommand()
    {
        var all = SelectedAll != true;
        SelectedAll = all;

        foreach (var item in Confirmations)
        {
            item.IsSelected = all;
        }
    }

    public async Task Login()
    {
        if (string.IsNullOrEmpty(UserNameText) || string.IsNullOrEmpty(PasswordText))
        {
            Toast.Show(ToastIcon.Warning, Strings.User_LoginError_Null);
            return;
        }
        if (IsLoading || IsLogged || _steamAuthenticator == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(_steamAuthenticator.AccountName) && UserNameText != _steamAuthenticator.AccountName)
        {
            Toast.Show(ToastIcon.Warning, "请确保登录的账号与令牌内的账号一致");
            return;
        }
        _steamClient ??= _steamAuthenticator.GetClient(ResourceService.GetCurrentCultureSteamLanguageName());
        if (_steamClient == null)
        {
            return;
        }

        IsLoading = true;

        var loginstate = new SteamLoginState()
        {
            Username = UserNameText,
            Password = PasswordText,
            Language = ResourceService.GetCurrentCultureSteamLanguageName(),
        };

        await _steamAccountService.DoLoginV2Async(loginstate);

        if (loginstate.Requires2FA)
        {
            loginstate.TwofactorCode = _steamAuthenticator.CurrentCode;
            await _steamAccountService.DoLoginV2Async(loginstate);
        }

        IsLogged = loginstate.Success;

        if (IsLogged)
        {
            _steamClient.Session = new WSteamClient.SteamSession
            {
                AccessToken = loginstate.AccessToken,
                RefreshToken = loginstate.RefreshToken,
                SessionID = loginstate.SeesionId,
                SteamID = loginstate.SteamId,
            };

            Toast.Show(ToastIcon.Success, string.Format(Strings.Success_, Strings.User_Login));
            _steamAuthenticator.SessionData = RemenberLogin ? _steamClient.Session.ToString() : null;
            await AuthenticatorHelper.SaveAuthenticator(_authenticatorDto!);
            _steamClient = _steamAuthenticator.GetClient(ResourceService.GetCurrentCultureSteamLanguageName());
            await GetConfirmations(_steamClient);
        }
        else
        {
            Toast.Show(ToastIcon.Error, AppResources.Error_UnknownLogin_.Format(loginstate.Message));
            _steamClient.Logout();
        }

        IsLoading = false;
    }

    bool ResetCurrentClient()
    {
        try
        {
            if (_steamAuthenticator == null)
                return false;

            _steamClient?.Dispose();

            // 重新生成 client
            _steamClient = _steamAuthenticator
                .GetClient(ResourceService.GetCurrentCultureSteamLanguageName());

            if (_steamClient == null)
                return false;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    async Task<IEnumerable<SteamTradeConfirmationModel>?> GetConfirmations(WSteamClient steamClient)
    {
        IsLoading = true;

        try
        {
            var result = await steamClient.GetConfirmations();

            if (result != null)
            {
                Confirmations.Clear();
                Confirmations.Add(result.Select(item => new SteamTradeConfirmationModel(_steamAuthenticator!, item)));
            }
        }
        catch (WinAuthUnauthorisedSteamRequestException)
        {
            Toast.Show(ToastIcon.Error, AppResources.LocalAuth_AuthTrade_GetError3);
            _steamClient?.Logout();
            IsLogged = false;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }

        IsLoading = false;

        return Confirmations;
    }

    public async Task Logout()
    {
        if (_steamClient == null || _steamAuthenticator == null) return;
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = Strings.LocalAuth_LogoutTip, }, AppResources.Logout, isCancelButton: true,
                isDialog: false))
        {
            _steamClient.Logout();
            _steamAuthenticator.SessionData = null;
            await AuthenticatorHelper.SaveAuthenticator(_authenticatorDto!);
            IsLogged = false;
        }
    }

    //void RefreshConfirmationsList()
    //{
    //    var items = _confirmationsSourceList.Items.Where(s => s.IsOperate == 0);
    //    _confirmationsSourceList.Clear();
    //    var confirmations = items.ToList();
    //    if (confirmations.Any())
    //        _confirmationsSourceList.AddRange(confirmations);
    //}

    public async void ConfirmTrade(SteamTradeConfirmationModel trade)
    {
        if (trade.IsTrade)
        {
            if (await IWindowManager.Instance.ShowTaskDialogAsync(
                        new MessageBoxWindowViewModel()
                        {
                            Content = AppResources.ModelContent_ConfirmTrade___.Format(trade.SendSummary,
                                trade.ReceiveSummary,
                                trade.ReceiveNoItems
                                    ? AppResources.ModelContent_ConfirmTrade2_.Format(trade.Headline)
                                    : string.Empty, trade.Warn ?? string.Empty)
                        }, AppResources.ConfirmTransaction, isCancelButton: true, isDialog: false))
            {
                await OperationTrade(true, trade);
            }
        }
        else
        {
            await OperationTrade(true, trade);
        }
    }

    public async Task ConfirmAllTrade()
    {
        if (!Confirmations.Any_Nullable(x => x.IsSelected))
        {
            Toast.Show(ToastIcon.Warning, Strings.LocalAuth_AuthTrade_SelectNull);
            return;
        }
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = AppResources.ModelContent_ConfirmAllTrade },
                AppResources.ConfirmTransaction, isCancelButton: true, isDialog: false))
        {
            await OperationAllTrade(true);
        }
    }

    public async Task CancelAllTrade()
    {
        if (!Confirmations.Any_Nullable(x => x.IsSelected))
        {
            Toast.Show(ToastIcon.Warning, Strings.LocalAuth_AuthTrade_SelectNull);
            return;
        }
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = AppResources.ModelContent_CancelAllTrade },
                AppResources.Cancel, isCancelButton: true, isDialog: false))
        {
            await OperationAllTrade(false);
        }
    }

    public async void CancelTrade(SteamTradeConfirmationModel trade)
    {
        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = AppResources.ModelContent_CancelTrade },
                AppResources.Cancel, isCancelButton: true, isDialog: false))
        {
            await OperationTrade(false, trade);
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

    async Task OperationAllTrade(bool status)
    {
        //if (!Confirmations.Any_Nullable())
        //{
        //    Toast.Show(ToastIcon.Warning, Strings.LocalAuth_AuthTrade_Null);
        //    return;
        //}

        var selectedList = Confirmations.Where(c => c.IsSelected).ToList();

        if (!selectedList.Any_Nullable())
        {
            //Toast.Show(ToastIcon.Warning, Strings.LocalAuth_AuthTrade_SelectNull);
            return;
        }

        var statusText = status ? Strings.BatchPass : Strings.BatchReject;

        //if (await IWindowManager.Instance.ShowTaskDialogAsync(
        //        new MessageBoxWindowViewModel()
        //        {
        //            Content = Strings.LocalAuth_AuthTrade_MessageBoxTip_.Format(statusText)
        //        }, statusText, isCancelButton: true, isDialog: false))
        //{
        Toast.Show(ToastIcon.Info, Strings.LocalAuth_AuthTrade_ConfirmTip_.Format(statusText));

        if (!await ChangeTradeStatus(status, selectedList)) return;
        Toast.Show(ToastIcon.Success,
            AppResources.Success_ExecuteAllAuthEnd___.Format(statusText, selectedList.Count, 0));
        //}
    }

    async Task<bool> ChangeTradeStatus(bool status, SteamTradeConfirmationModel trade)
    {
        if (!await ChangeTradeStatus(status, new[] { trade })) return false;
        Confirmations.Remove(trade);
        return true;

    }

    async Task<bool> ChangeTradeStatus(bool status, IEnumerable<SteamTradeConfirmationModel> trades)
    {
        var steamTradeConfirmationModels = trades.ToList();
        var ids = steamTradeConfirmationModels.ToDictionary(item => item.Id, item => item.Nonce);

        try
        {
            var result = await _steamClient!.ConfirmTrade(ids, status);
            if (result)
            {
                Confirmations.Remove(steamTradeConfirmationModels);
            }
            else
            {
                Toast.Show(ToastIcon.Error, Strings.LocalAuth_AuthTrade_ConfirmError);
            }
            return result;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            return false;
        }
    }
}