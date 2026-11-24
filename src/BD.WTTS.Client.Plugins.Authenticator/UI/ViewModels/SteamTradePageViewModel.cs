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
    CancellationTokenSource sessionOperations;
    bool logoutInProgress;

    public SteamTradePageViewModel(ref IAuthenticatorDTO authenticatorDto)
    {
        _authenticatorDto = authenticatorDto;
        lifetimeToken = lifetime.Token;
        sessionOperations = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken);
        Login = new SteamLoginViewModel(accountService: _steamAccountService,
            defaultCredentialPlatform: EAuthTokenPlatformType.KEauthTokenPlatformTypeWebBrowser);
        Login.TwoFactorCodeMismatchMessage = Strings.SteamTradeLoginTwoFactorCodeMismatch;
        Login.LoginThrottledMessage = Strings.SteamTradeLoginThrottled;
        Login.LoginSucceededAsync = CompleteLoginAsync;
        Login.AddTo(this);

        ConfirmTradeCommand = ReactiveCommand.CreateFromTask<SteamTradeConfirmationModel>(ConfirmTrade);
        CancelTradeCommand = ReactiveCommand.CreateFromTask<SteamTradeConfirmationModel>(CancelTrade);
        PropertyChanged += (_, change) =>
        {
            if (change.PropertyName is nameof(IsLogged) or nameof(HasLoadedConfirmations) or nameof(ConfirmationLoadError)
                or nameof(IsLoading) or nameof(IsOperatingConfirmations))
                this.RaisePropertyChanged(nameof(CanChangeConfirmations));
        };

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
                        throw new InvalidOperationException(Strings.L10nAuthIdle_Text097, ex);
                    }
                    if (string.IsNullOrWhiteSpace(code))
                        throw new InvalidOperationException(Strings.L10nAuthIdle_Text098);
                    return code;
                };

                var confirmationChanges = this.ObserveValue(nameof(Confirmations), static v => v.Confirmations)
                    .Select(items => items == null
                        ? Observable.Empty<IChangeSet<SteamTradeConfirmationModel>>()
                        : items.ToObservableChangeSet().AutoRefreshOnObservable(static item =>
                            item.ObserveValue(nameof(item.IsSelected), static x => x.IsSelected).Skip(1)));
                Observable.Switch(confirmationChanges)
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

    void SetLoginStatus(string? text) => LoginStatus = text;

    public override async Task Initialize()
    {
        if (IsLoading || Disposed)
            return;
        var token = sessionOperations.Token;
        IsLoading = true;
        try
        {
            await base.Initialize();
            token.ThrowIfCancellationRequested();
            if (_steamClient is { Session: { } session } steamClient
                && (!string.IsNullOrWhiteSpace(session.AccessToken) || !string.IsNullOrWhiteSpace(session.RefreshToken))
                && _steamAuthenticator?.SteamId64 is > 0 && session.SteamID == (ulong)_steamAuthenticator.SteamId64.Value)
            {
                IsLogged = true;
                await GetConfirmations(steamClient, token, refreshExpiredToken: true);
            }
        }
        catch
        {
            if (!token.IsCancellationRequested)
            {
                IsLogged = false;
                SetLoginStatus(Strings.L10nAuthIdle_Text099);
            }
        }
        finally { if (!token.IsCancellationRequested) IsLoading = false; }
    }

    async ValueTask<bool> CheckAndRefreshToken(WSteamClient steamClient, CancellationToken cancellationToken)
    {
        if (steamClient.Session is not { } session || _steamAuthenticator == null)
            return false;
        if (_steamAuthenticator.SteamId64 is not > 0 || session.SteamID != (ulong)_steamAuthenticator.SteamId64.Value)
            return false;
        if (!session.IsAccessTokenExpired())
            return true;
        if (string.IsNullOrWhiteSpace(session.RefreshToken) || session.IsRefreshTokenExpired())
            return false;

        var savedSession = _steamAuthenticator.SessionData;
        var oldAccessToken = session.AccessToken;
        var refreshToken = session.RefreshToken;
        var token = AuthenticatorSavedSteamAccounts.IsBrowserRefreshToken(refreshToken, session.SteamID)
            ? (await AuthenticatorSavedSteamAccounts.CreateWebSessionAsync(session.SteamID, null, refreshToken,
                _steamAccountService, cancellationToken)).AccessToken
            : await _steamAccountService.RefreshAccessToken(session.SteamID, refreshToken).WaitAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (!ReferenceEquals(steamClient.Session, session) || savedSession != _steamAuthenticator.SessionData
            || oldAccessToken != session.AccessToken || refreshToken != session.RefreshToken)
            throw new InvalidOperationException();
        // The app endpoint can return no token on a transient failure without explicitly rejecting the session.
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException();
        session.AccessToken = token;
        steamClient.SessionSet(session.ToString(), ResourceService.GetCurrentCultureSteamLanguageName());
        if (!string.IsNullOrWhiteSpace(savedSession))
        {
            _steamAuthenticator.SessionData = steamClient.Session!.ToString();
            var saved = await AuthenticatorHelper.SaveAuthenticator(_authenticatorDto);
            cancellationToken.ThrowIfCancellationRequested();
            if (!saved)
                SetLoginStatus(Strings.L10nAuthIdle_Text100);
        }
        return true;
    }

    public async Task Refresh()
    {
        if (!IsLogged || IsLoading || IsOperatingConfirmations || logoutInProgress || lifetimeToken.IsCancellationRequested
            || _steamClient is not { } steamClient)
            return;

        var token = sessionOperations.Token;
        IsLoading = true;
        try
        {
            await GetConfirmations(steamClient, token, refreshExpiredToken: true);
        }
        finally { if (!token.IsCancellationRequested) IsLoading = false; }
    }

    public void SelectAllCommand()
    {
        if (!CanChangeConfirmations)
            return;

        var all = SelectedAll != true;
        SelectedAll = all;

        foreach (var item in Confirmations)
        {
            item.IsSelected = all;
        }
    }

    async Task CompleteLoginAsync(SteamLoginState state)
    {
        var token = sessionOperations.Token;
        token.ThrowIfCancellationRequested();
        if (_steamAuthenticator is null)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text101);
        if (!Login.UsedQrCode)
            SteamAuthenticatorIdentity.ValidateCredentialAccount(_steamAuthenticator, state);
        SteamAuthenticatorIdentity.ConfirmAuthenticatedAccount(_steamAuthenticator, state, Login.UsedAutomaticTwoFactor);

        IsLoading = true;
        SetLoginStatus(null);
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
            var saved = await AuthenticatorHelper.SaveAuthenticator(_authenticatorDto);
            token.ThrowIfCancellationRequested();
            if (!saved)
                SetLoginStatus(Strings.L10nAuthIdle_Text102);
            IsLogged = true;
            await GetConfirmations(_steamClient, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        finally { if (!token.IsCancellationRequested) IsLoading = false; }
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
            sessionOperations.Dispose();
            lifetime.Dispose();
            Login.LoginSucceededAsync = null;
            Login.AutomaticTwoFactorCode = null;
        }
        base.Dispose(disposing);
    }

    async Task GetConfirmations(WSteamClient steamClient, CancellationToken token, bool refreshExpiredToken = false)
    {
        try
        {
            if (refreshExpiredToken && !await CheckAndRefreshToken(steamClient, token))
            {
                token.ThrowIfCancellationRequested();
                RequireSteamLogin(steamClient);
                return;
            }

            token.ThrowIfCancellationRequested();
            var result = await steamClient.GetConfirmations().WaitAsync(token);
            var items = result?.Select(static item => new SteamTradeConfirmationModel(item)).ToArray() ?? [];
            token.ThrowIfCancellationRequested();
            Confirmations.Clear();
            Confirmations.Add(items);
            HasLoadedConfirmations = true;
            ConfirmationLoadError = null;
            foreach (var item in items)
                _ = item.LoadDetailsAsync(steamClient, token);
        }
        catch (WinAuthUnauthorisedSteamRequestException) when (!token.IsCancellationRequested)
        {
            RequireSteamLogin(steamClient);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch
        {
            if (!token.IsCancellationRequested)
                ConfirmationLoadError = Strings.L10nAuthIdle_Text103;
        }
    }

    void RequireSteamLogin(WSteamClient steamClient)
    {
        steamClient.Logout();
        if (_steamAuthenticator != null)
            _steamAuthenticator.SessionData = null;
        IsLogged = false;
        ResetConfirmations();
        SetLoginStatus(AppResources.LocalAuth_AuthTrade_GetError3);
    }

    void ResetConfirmations()
    {
        Confirmations.Clear();
        HasLoadedConfirmations = false;
        ConfirmationLoadError = null;
    }

    public async Task Logout()
    {
        if (!IsLogged || IsLoading || IsOperatingConfirmations || logoutInProgress || lifetimeToken.IsCancellationRequested
            || _steamClient == null || _steamAuthenticator == null)
            return;

        var confirmed = false;
        logoutInProgress = true;
        try
        {
            if (!await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel() { Content = Strings.LocalAuth_LogoutTip, }, AppResources.Logout, isCancelButton: true,
                isDialog: false) || lifetimeToken.IsCancellationRequested || IsLoading || IsOperatingConfirmations)
                return;

            confirmed = true;
            IsLoading = true;
            using (var previous = sessionOperations)
            {
                sessionOperations = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken);
                previous.Cancel();
            }
            _steamClient.Logout();
            _steamAuthenticator.SessionData = null;
            IsLogged = false;
            ResetConfirmations();
            Login.Cancel();
            SetLoginStatus(null);
            if (!await AuthenticatorHelper.SaveAuthenticator(_authenticatorDto) && !lifetimeToken.IsCancellationRequested && !IsLogged)
                SetLoginStatus(Strings.L10nAuthIdle_Text102);
        }
        finally
        {
            logoutInProgress = false;
            if (confirmed && !lifetimeToken.IsCancellationRequested)
                IsLoading = false;
        }
    }

    public async Task ConfirmTrade(SteamTradeConfirmationModel trade)
    {
        if (!CanChangeConfirmations || !Confirmations.Contains(trade))
            return;

        if (trade.IsTrade && !await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel
                {
                    Content = AppResources.ModelContent_ConfirmTrade___.Format(trade.SendSummary,
                        trade.ReceiveSummary,
                        trade.ReceiveNoItems
                            ? AppResources.ModelContent_ConfirmTrade2_.Format(trade.Headline)
                            : string.Empty, trade.Warn ?? string.Empty)
                }, AppResources.ConfirmTransaction, isCancelButton: true, isDialog: false))
            return;

        await OperationTrade(true, trade);
    }

    public Task ConfirmAllTrade() => OperationAllTrade(true);

    public Task CancelAllTrade() => OperationAllTrade(false);

    public async Task CancelTrade(SteamTradeConfirmationModel trade)
    {
        if (!CanChangeConfirmations || !Confirmations.Contains(trade))
            return;

        if (await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel { Content = AppResources.ModelContent_CancelTrade },
                AppResources.Cancel, isCancelButton: true, isDialog: false))
            await OperationTrade(false, trade);
    }

    async Task OperationTrade(bool status, SteamTradeConfirmationModel trade)
    {
        if (await ChangeTradeStatus(status, [trade]))
            Toast.Show(ToastIcon.Success, $"{(status ? Strings.Agree : Strings.Cancel)}{trade.TypeName}");
    }

    async Task OperationAllTrade(bool status)
    {
        if (!CanChangeConfirmations)
            return;

        var selected = Confirmations.Where(confirmation => confirmation.IsSelected).ToArray();
        if (selected.Length == 0)
        {
            Toast.Show(ToastIcon.Warning, Strings.LocalAuth_AuthTrade_SelectNull);
            return;
        }

        var content = status ? AppResources.ModelContent_ConfirmAllTrade : AppResources.ModelContent_CancelAllTrade;
        var title = status ? AppResources.ConfirmTransaction : AppResources.Cancel;
        if (!await IWindowManager.Instance.ShowTaskDialogAsync(
                new MessageBoxWindowViewModel { Content = content }, title, isCancelButton: true, isDialog: false))
            return;

        var statusText = status ? Strings.BatchPass : Strings.BatchReject;
        if (await ChangeTradeStatus(status, selected))
            Toast.Show(ToastIcon.Success, AppResources.Success_ExecuteAllAuthEnd___.Format(statusText, selected.Length, 0));
    }

    async Task<bool> ChangeTradeStatus(bool status, SteamTradeConfirmationModel[] trades)
    {
        if (!CanChangeConfirmations || logoutInProgress || lifetimeToken.IsCancellationRequested
            || _steamClient is not { } steamClient || !trades.All(Confirmations.Contains))
            return false;

        var token = sessionOperations.Token;
        IsOperatingConfirmations = true;
        try
        {
            var ids = trades.ToDictionary(item => item.Id, item => item.Nonce);
            var result = await steamClient.ConfirmTrade(ids, status).WaitAsync(token);
            token.ThrowIfCancellationRequested();
            if (result)
                Confirmations.Remove(trades);
            else
                Toast.Show(ToastIcon.Error, Strings.LocalAuth_AuthTrade_ConfirmError);
            return result;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return false;
        }
        catch
        {
            if (!token.IsCancellationRequested)
                Toast.Show(ToastIcon.Error, Strings.LocalAuth_AuthTrade_ConfirmError);
            return false;
        }
        finally { if (!token.IsCancellationRequested) IsOperatingConfirmations = false; }
    }
}
