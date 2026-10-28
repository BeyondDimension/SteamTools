using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Helpers;
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
    readonly CancellationTokenSource lifetime = new();
    readonly CancellationToken lifetimeToken;

    public SteamTradePageViewModel(ref IAuthenticatorDTO authenticatorDto)
    {
        _authenticatorDto = authenticatorDto;
        lifetimeToken = lifetime.Token;
        Login = new SteamLoginViewModel(accountService: _steamAccountService, requireMobileSession: true);
        Login.LoginSucceededAsync = CompleteLoginAsync;
        Login.AddTo(this);

        ConfirmTradeCommand = ReactiveCommand.Create<SteamTradeConfirmationModel>(ConfirmTrade);
        CancelTradeCommand = ReactiveCommand.Create<SteamTradeConfirmationModel>(CancelTrade);

        try
        {
            if (authenticatorDto.Value is SteamAuthenticator steamAuthenticator)
            {
                _steamAuthenticator = steamAuthenticator;
                _steamClient = _steamAuthenticator.GetClient(ResourceService.GetCurrentCultureSteamLanguageName());
                Login.UserName = _steamAuthenticator.AccountName ?? string.Empty;
                authenticatorDto.Value = _steamAuthenticator;
                _ = Initialize();
                Login.AutomaticTwoFactorCode = state =>
                {
                    SteamAuthenticatorIdentity.ValidateCredentialAccount(_steamAuthenticator, state);

                    string? code;
                    try { code = _steamAuthenticator.CurrentCode; }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("无法获取此令牌的 Steam 验证码，请检查令牌后重试。", ex);
                    }
                    if (string.IsNullOrWhiteSpace(code))
                        throw new InvalidOperationException("此令牌的 Steam 验证码不可用，请检查令牌后重试。");
                    return code;
                };

                this.WhenAnyValue(v => v.Confirmations)
                    .Select(items => items == null
                        ? Observable.Empty<IChangeSet<SteamTradeConfirmationModel>>()
                        : items.ToObservableChangeSet().AutoRefresh(x => x.IsSelected))
                    .Switch()
                    .Subscribe(_ =>
                    {
                        var count = Confirmations.Count(x => x.IsSelected);
                        SelectedAll = count == 0 ? false : count == Confirmations.Count ? true : null;
                    })
                    .AddTo(this);
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
        if (IsLoading || Disposed)
            return;
        IsLoading = true;
        try
        {
            await base.Initialize();
            IsLogged = _steamClient?.IsLoggedIn() == true && await CheckAndRefreshToken();
            if (IsLogged && _steamClient != null)
                await GetConfirmations(_steamClient);
        }
        catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested) { }
        catch
        {
            IsLogged = false;
            LoginStatus = "检查交易确认登录状态失败，请检查网络或重新登录。";
        }
        finally { IsLoading = false; }
    }

    public async ValueTask<bool> CheckAndRefreshToken()
    {
        if (_steamClient?.Session == null || _steamAuthenticator == null)
            return false;
        var session = _steamClient.Session;
        if (_steamAuthenticator.SteamId64 is not > 0 || session.SteamID != (ulong)_steamAuthenticator.SteamId64.Value)
            return false;
        if (!session.IsAccessTokenExpired())
            return true;
        if (string.IsNullOrWhiteSpace(session.RefreshToken) || session.IsRefreshTokenExpired())
            return false;

        var wasRemembered = !string.IsNullOrWhiteSpace(_steamAuthenticator.SessionData);
        var token = await _steamAccountService.RefreshAccessToken(session.SteamID, session.RefreshToken).WaitAsync(lifetimeToken);
        lifetimeToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(token))
            return false;
        session.AccessToken = token;
        _steamClient.SessionSet(session.ToString(), ResourceService.GetCurrentCultureSteamLanguageName());
        if (wasRemembered)
        {
            _steamAuthenticator.SessionData = _steamClient.Session!.ToString();
            if (!await AuthenticatorHelper.SaveAuthenticator(_authenticatorDto))
                LoginStatus = "登录状态已续期，但保存失败；本次仍可使用。";
        }
        return true;
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

    async Task CompleteLoginAsync(SteamLoginState state)
    {
        lifetimeToken.ThrowIfCancellationRequested();
        if (_steamAuthenticator is null)
            throw new InvalidOperationException("此令牌不可用，未保存任何登录状态。");
        if (!Login.UsedQrCode)
            SteamAuthenticatorIdentity.ValidateCredentialAccount(_steamAuthenticator, state);
        SteamAuthenticatorIdentity.ConfirmAuthenticatedAccount(_steamAuthenticator, state, Login.UsedAutomaticTwoFactor);

        IsLoading = true;
        LoginStatus = null;
        try
        {
            var session = new WSteamClient.SteamSession
            {
                AccessToken = state.AccessToken,
                RefreshToken = state.RefreshToken,
                SessionID = state.SeesionId,
                SteamID = state.SteamId,
            };
            _steamClient = _steamAuthenticator.GetClient(ResourceService.GetCurrentCultureSteamLanguageName());
            // Install cookies from the live session even when the user does not persist it.
            _steamClient.SessionSet(session.ToString(), ResourceService.GetCurrentCultureSteamLanguageName());
            _steamAuthenticator.SessionData = RememberLogin ? _steamClient.Session!.ToString() : null;
            if (!await AuthenticatorHelper.SaveAuthenticator(_authenticatorDto))
                LoginStatus = "Steam 已登录，但登录状态保存失败。请重试保存后再使用扫码。";
            lifetimeToken.ThrowIfCancellationRequested();
            IsLogged = true;
            await GetConfirmations(_steamClient);
        }
        finally { IsLoading = false; }
    }

    public override void OnClosing(object? sender, CancelEventArgs e)
    {
        base.OnClosing(sender, e);
        if (!e.Cancel)
        {
            Login.Cancel();
            lifetime.Cancel();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            lifetime.Cancel();
            lifetime.Dispose();
            Login.LoginSucceededAsync = null;
            Login.AutomaticTwoFactorCode = null;
        }
        base.Dispose(disposing);
    }

    async Task<IEnumerable<SteamTradeConfirmationModel>?> GetConfirmations(WSteamClient steamClient)
    {
        IsLoading = true;

        try
        {
            var result = await steamClient.GetConfirmations().WaitAsync(lifetimeToken);
            lifetimeToken.ThrowIfCancellationRequested();
            Confirmations.Clear();
            if (result != null)
                Confirmations.Add(result.Select(item => new SteamTradeConfirmationModel(_steamAuthenticator!, item)));
        }
        catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested) { }
        catch (WinAuthUnauthorisedSteamRequestException)
        {
            _steamClient?.Logout();
            if (_steamAuthenticator != null)
                _steamAuthenticator.SessionData = null;
            Confirmations.Clear();
            IsLogged = false;
            LoginStatus = AppResources.LocalAuth_AuthTrade_GetError3;
        }
        catch
        {
            LoginStatus = "Steam 已登录，但读取交易确认失败。请检查网络并刷新，不要反复重新登录。";
        }
        finally { IsLoading = false; }

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
            Confirmations.Clear();
            Login.Cancel();
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