using BD.SteamClient.Models;
using BD.SteamClient.Services;

namespace BD.WTTS.UI.ViewModels;

/// <summary>Shared Steam sign-in state. Consumers own the work performed after authentication.</summary>
public sealed class SteamLoginViewModel : ViewModelBase
{
    static readonly SemaphoreSlim sessionSaveGate = new(1, 1);
    readonly ISteamAccountService account;
    readonly ISteamSessionService sessions;
    readonly bool requireMobileSession;
    SteamLoginState pendingState = new();
    CancellationTokenSource? operation;
    CancellationTokenSource? qrOperation;
    Func<SteamLoginState, string?>? automaticTwoFactorCode;
    bool qrVisible;
    bool loginCompleted;

    public SteamLoginViewModel(SteamLoginState? loginState = null, bool allowCookieLogin = false,
        bool rememberLogin = false, ISteamAccountService? accountService = null, ISteamSessionService? sessionService = null,
        bool requireMobileSession = false)
    {
        State = loginState ?? new();
        account = accountService ?? Ioc.Get<ISteamAccountService>();
        sessions = sessionService ?? Ioc.Get<ISteamSessionService>();
        AllowCookieLogin = allowCookieLogin;
        RememberLogin = rememberLogin;
        this.requireMobileSession = requireMobileSession;
        UserName = State.Username;
        LoginCommand = Register(LoginAsync);
        QrLoginCommand = Register(LoginWithQrCodeAsync);
        CookieLoginCommand = Register(CookieLoginAsync);
        SavedAccountLoginCommand = Register(SavedAccountLoginAsync);
        LoadSavedAccountsCommand = Register(LoadSavedAccountsAsync);
        var cancel = ReactiveCommand.Create(Cancel);
        CompositeDisposable.Add(cancel);
        CancelCommand = cancel;
        this.WhenAnyValue(x => x.UserName).Skip(1).Subscribe(_ =>
        {
            if (pendingState.ClientId.HasValue)
                Cancel();
        }).AddTo(this);
    }

    ICommand Register(Func<Task> action)
    {
        var command = ReactiveCommand.CreateFromTask(action);
        CompositeDisposable.Add(command);
        return command;
    }

    public SteamLoginState State { get; }
    public bool AllowCookieLogin { get; }
    public Func<CancellationToken, Task<IReadOnlyList<SteamSavedLoginAccount>>>? SavedAccountsProvider { get; set; }
    public bool AllowSavedAccountLogin => SavedAccountsProvider != null;
    [Reactive] public IReadOnlyList<SteamSavedLoginAccount> SavedAccounts { get; private set; } = Array.Empty<SteamSavedLoginAccount>();
    [Reactive] public SteamSavedLoginAccount? SelectedSavedAccount { get; set; }
    public ICommand LoadSavedAccountsCommand { get; }
    public ICommand SavedAccountLoginCommand { get; }
    public Func<SteamLoginState, Task>? LoginSucceededAsync { get; set; }
    public Func<SteamLoginState, string?>? AutomaticTwoFactorCode
    {
        get => automaticTwoFactorCode;
        set
        {
            automaticTwoFactorCode = value;
            this.RaisePropertyChanged(nameof(RequiresManualGuardCode));
        }
    }
    public bool HadTwoFactorChallenge { get; private set; }
    public bool UsedQrCode { get; private set; }
    public bool UsedAutomaticTwoFactor { get; private set; }
    public bool Requires2FA => pendingState.Requires2FA;
    public bool RequiresEmailAuth => pendingState.RequiresEmailAuth;
    public bool RequiresGuardCode => Requires2FA || RequiresEmailAuth;
    public bool RequiresManualGuardCode => RequiresEmailAuth || (Requires2FA && AutomaticTwoFactorCode is null);
    public ICommand LoginCommand { get; }
    public ICommand QrLoginCommand { get; }
    public ICommand CookieLoginCommand { get; }
    public ICommand CancelCommand { get; }
    [Reactive] public string? UserName { get; set; }
    [Reactive] public string? Password { get; set; }
    [Reactive] public string? GuardCode { get; set; }
    [Reactive] public bool RememberLogin { get; set; }
    [Reactive] public bool IsBusy { get; private set; }
    [Reactive] public bool IsQrBusy { get; private set; }
    [Reactive] public bool HasQrError { get; private set; }
    [Reactive] public string? QrStatusMessage { get; private set; }
    [Reactive] public string? StatusMessage { get; private set; }
    [Reactive] public Stream? QrCodeImage { get; private set; }
    [Reactive] public string? SessionId { get; set; }
    [Reactive] public string? SteamLoginSecure { get; set; }

    void NotifyChallenge()
    {
        this.RaisePropertyChanged(nameof(Requires2FA));
        this.RaisePropertyChanged(nameof(RequiresEmailAuth));
        this.RaisePropertyChanged(nameof(RequiresGuardCode));
        this.RaisePropertyChanged(nameof(RequiresManualGuardCode));
    }

    public async Task LoginAsync()
    {
        if (IsBusy || Disposed) return;
        if (!pendingState.ClientId.HasValue && !RequiresGuardCode && (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrEmpty(Password)))
        {
            StatusMessage = Strings.Error_PleaseEnterUsernamePassword;
            return;
        }
        if (RequiresManualGuardCode && string.IsNullOrWhiteSpace(GuardCode))
        {
            StatusMessage = Strings.Error_PleaseEnterCode;
            return;
        }
        UsedQrCode = false;
        var state = pendingState;
        if (!RequiresGuardCode && !state.ClientId.HasValue)
        {
            HadTwoFactorChallenge = false;
            UsedAutomaticTwoFactor = false;
            state = pendingState = new() { Username = UserName?.Trim(), Password = Password };
        }
        else if (Requires2FA && AutomaticTwoFactorCode is null) state.TwofactorCode = GuardCode?.Trim().ToUpperInvariant();
        else if (RequiresEmailAuth) state.EmailCode = GuardCode?.Trim();
        state.Language = ResourceService.GetCurrentCultureSteamLanguageName();
        await RunAsync(async token =>
        {
            var automaticAttempted = false;
            if (state.Requires2FA && AutomaticTwoFactorCode is { } currentProvider)
            {
                state.TwofactorCode = GetAutomaticCode(currentProvider, state);
                automaticAttempted = true;
            }
            var platform = requireMobileSession ? EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp : (EAuthTokenPlatformType?)null;
            await SubmitCredentialAsync(state, platform, automaticAttempted, token);
            HadTwoFactorChallenge |= state.Requires2FA || automaticAttempted;
            if (!automaticAttempted && state.Requires2FA && AutomaticTwoFactorCode is { } provider)
            {
                state.TwofactorCode = GetAutomaticCode(provider, state);
                state.Message = null;
                await SubmitCredentialAsync(state, platform, automatic: true, token);
            }
            state.TwofactorCode = state.EmailCode = null;
            GuardCode = null;
            NotifyChallenge();
            if (state.Success) await CompleteAsync(state, token);
            else StatusMessage = state.Message ?? (RequiresManualGuardCode ? "请输入 Steam 令牌或邮箱验证码后继续。" :
                RequiresGuardCode ? "自动令牌验证未完成，请重试或返回。" : "Steam 登录失败，请重试。");
        });
    }

    async Task SubmitCredentialAsync(SteamLoginState state, EAuthTokenPlatformType? platform, bool automatic, CancellationToken token)
    {
        try
        {
            await account.DoLoginV2Async(state, platform, token).WaitAsync(token);
        }
        finally
        {
            // The service clears the challenge immediately after Steam accepts the code,
            // even if subsequent polling or access-token generation is interrupted.
            if (automatic && !state.Requires2FA && !state.RequiresEmailAuth)
                UsedAutomaticTwoFactor = true;
        }
        token.ThrowIfCancellationRequested();
    }

    static string GetAutomaticCode(Func<SteamLoginState, string?> provider, SteamLoginState state)
    {
        var code = provider(state)?.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("无法获取当前令牌验证码，请检查令牌后重试。");
        return code.ToUpperInvariant();
    }

    public void SetQrLoginVisible(bool visible)
    {
        if (qrVisible == visible) return;
        qrVisible = visible;
        if (visible)
        {
            loginCompleted = false;
            StartQrIfVisible();
        }
        else StopQrLogin();
    }

    void StartQrIfVisible()
    {
        if (qrVisible && !IsBusy && !loginCompleted && !Disposed && qrOperation is null)
            _ = LoginWithQrCodeAsync();
    }

    void StopQrLogin()
    {
        HasQrError = false;
        QrStatusMessage = null;
        SetQrImage(null);
        qrOperation?.Cancel();
    }

    public async Task LoginWithQrCodeAsync()
    {
        if (IsBusy || IsQrBusy || Disposed) return;
        using var source = new CancellationTokenSource();
        qrOperation = source;
        var token = source.Token;
        IsQrBusy = true;
        HasQrError = false;
        QrStatusMessage = "正在获取 Steam 登录二维码…";
        SetQrImage(null);
        var uiScheduler = SynchronizationContext.Current == null ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
        try
        {
            while (true)
            {
                var state = new SteamLoginState();
                var rendered = false;
                Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>> render = response => Task.Factory.StartNew(() =>
                {
                    token.ThrowIfCancellationRequested();
                    if (!Uri.TryCreate(response.ChallengeUrl, UriKind.Absolute, out var url) ||
                        url.Scheme != Uri.UriSchemeHttps || url.Host != "s.team" || !url.AbsolutePath.StartsWith("/q/", StringComparison.Ordinal))
                        throw new InvalidOperationException("Steam 返回的登录二维码无效。");
                    var (result, stream, error) = QRCodeHelper.CreateLoginCode(response.ChallengeUrl);
                    if (result != QRCodeHelper.QRCodeCreateResult.Success || stream == null)
                        throw new InvalidOperationException("无法生成 Steam 登录二维码。", error);
                    SetQrImage(stream);
                    rendered = true;
                    QrStatusMessage = "请使用 Steam 手机客户端扫描，并在手机上确认登录。";
                    return true;
                }, token, TaskCreationOptions.None, uiScheduler);
                var request = requireMobileSession
                    ? account.DoLoginWithQRCodeAsync(state, render, EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, deviceFriendlyName: "Watt Toolkit", cancellationToken: token)
                    : account.DoLoginWithQRCodeAsync(state, render, deviceFriendlyName: "Watt Toolkit", cancellationToken: token);
                await request.WaitAsync(token);
                token.ThrowIfCancellationRequested();
                if (state.Success)
                {
                    IsBusy = true;
                    UsedQrCode = true;
                    HadTwoFactorChallenge = false;
                    UsedAutomaticTwoFactor = false;
                    pendingState = new();
                    NotifyChallenge();
                    try { await CompleteAsync(state, token); }
                    finally { IsBusy = false; }
                    return;
                }
                if (!rendered)
                {
                    QrStatusMessage = state.Message ?? "无法获取 Steam 登录二维码，请重试。";
                    HasQrError = true;
                    return;
                }
                // Once displayed, the service returns without success when its challenge expires.
                SetQrImage(null);
                QrStatusMessage = "二维码已过期，正在重新获取…";
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
        }
        catch (OperationCanceledException) when (source.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (!source.IsCancellationRequested)
            {
                QrStatusMessage = ex is InvalidOperationException ? ex.Message : "无法获取 Steam 登录二维码，请检查网络后重试。";
                HasQrError = true;
            }
        }
        finally
        {
            var canceled = source.IsCancellationRequested;
            if (ReferenceEquals(qrOperation, source))
            {
                qrOperation = null;
                IsQrBusy = false;
                SetQrImage(null);
                if (canceled) StartQrIfVisible();
            }
        }
    }

    public void ClearSavedAccounts()
    {
        SelectedSavedAccount = null;
        SavedAccounts = Array.Empty<SteamSavedLoginAccount>();
    }

    public async Task LoadSavedAccountsAsync()
    {
        if (IsBusy || Disposed || SavedAccountsProvider == null) return;
        await RunAsync(async token =>
        {
            SelectedSavedAccount = null;
            SavedAccounts = Array.Empty<SteamSavedLoginAccount>();
            var accounts = await SavedAccountsProvider(token);
            token.ThrowIfCancellationRequested();
            SavedAccounts = accounts;
            if (SavedAccounts.Count == 0)
                StatusMessage = "没有可用的交易确认登录状态。请先在令牌插件的“确认交易”中登录，然后重新读取令牌。";
        });
    }

    public async Task SavedAccountLoginAsync()
    {
        if (IsBusy || Disposed || SelectedSavedAccount == null) return;
        var selected = SelectedSavedAccount;
        await RunAsync(async token =>
        {
            var session = await selected.CreateSessionAsync(token);
            token.ThrowIfCancellationRequested();
            if (session.SteamId != selected.SteamId.ToString(CultureInfo.InvariantCulture) || string.IsNullOrWhiteSpace(session.AccessToken))
                throw new InvalidOperationException("返回的 Steam 登录状态与所选账号不匹配，请重新读取令牌。");
            if (!sessions.AddOrSetSeesion(session))
                throw new InvalidOperationException("无法创建 Steam 登录会话。");
            UsedQrCode = HadTwoFactorChallenge = UsedAutomaticTwoFactor = false;
            await CompleteAsync(new SteamLoginState
            {
                Success = true, SteamId = selected.SteamId, Username = selected.AccountName,
                AccessToken = session.AccessToken, RefreshToken = session.RefreshToken,
                Cookies = session.CookieContainer.GetAllCookies(),
                SeesionId = session.CookieContainer.GetCookies(new Uri("https://steamcommunity.com/"))["sessionid"]?.Value,
            }, token);
        });
    }

    public async Task CookieLoginAsync()
    {
        if (IsBusy || Disposed || !AllowCookieLogin) return;
        if (string.IsNullOrWhiteSpace(SessionId) || string.IsNullOrWhiteSpace(SteamLoginSecure))
        {
            StatusMessage = "请输入 sessionid 和 steamLoginSecure。";
            return;
        }
        await RunAsync(async token =>
        {
            var parts = Uri.UnescapeDataString(SteamLoginSecure.Trim()).Split("||", StringSplitOptions.None);
            if (parts.Length != 2 || !ulong.TryParse(parts[0], out var steamId) || steamId == 0 || string.IsNullOrWhiteSpace(parts[1]))
            {
                StatusMessage = "steamLoginSecure 格式无效。";
                return;
            }
            var cookies = new CookieContainer();
            foreach (var domain in new[] { "steamcommunity.com", "steampowered.com" })
            {
                cookies.Add(new Cookie("steamLoginSecure", SteamLoginSecure.Trim(), "/", domain) { Secure = true });
                cookies.Add(new Cookie("sessionid", SessionId.Trim(), "/", domain) { Secure = true });
            }
            var session = new SteamSession { SteamId = steamId.ToString(CultureInfo.InvariantCulture), AccessToken = parts[1], RefreshToken = string.Empty, CookieContainer = cookies };
            if (!sessions.AddOrSetSeesion(session))
                throw new InvalidOperationException("无法创建 Steam 登录会话。");
            UsedQrCode = false;
            HadTwoFactorChallenge = false;
            UsedAutomaticTwoFactor = false;
            await CompleteAsync(new SteamLoginState { Success = true, SteamId = steamId, AccessToken = parts[1], SeesionId = SessionId.Trim(), Cookies = cookies.GetAllCookies() }, token);
        });
    }

    async Task CompleteAsync(SteamLoginState state, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (state.SteamId == 0 || string.IsNullOrEmpty(state.AccessToken))
            throw new InvalidOperationException("Steam 未返回有效的登录会话。");
        var saved = true;
        if (RememberLogin)
        {
            var session = sessions.RentSession(state.SteamId.ToString(CultureInfo.InvariantCulture));
            if (session == null) saved = false;
            else
            {
                await sessionSaveGate.WaitAsync(token);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // Secure-storage writes cannot be cancelled. Keep this commit ordered until it finishes.
                    saved = await sessions.SaveSession(session);
                }
                finally { sessionSaveGate.Release(); }
            }
        }
        token.ThrowIfCancellationRequested();
        State.Username = state.Username;
        State.SteamId = state.SteamId;
        State.SteamIdString = state.SteamId.ToString(CultureInfo.InvariantCulture);
        State.AccessToken = state.AccessToken;
        State.RefreshToken = state.RefreshToken;
        State.Cookies = state.Cookies;
        State.SeesionId = state.SeesionId;
        State.ClientId = state.ClientId;
        State.RequestId = state.RequestId;
        State.Language = state.Language;
        State.Requires2FA = State.RequiresEmailAuth = State.RequiresCaptcha = false;
        State.Password = State.TwofactorCode = State.EmailCode = null;
        State.Message = null;
        State.Success = true;
        Password = GuardCode = null;
        pendingState = new();
        NotifyChallenge();
        StatusMessage = saved ? "Steam 登录成功。" : "Steam 登录成功，但未能保存登录状态。";
        SetQrImage(null);
        try
        {
            if (LoginSucceededAsync != null) await LoginSucceededAsync(State);
            loginCompleted = true;
        }
        catch
        {
            State.Success = false;
            throw;
        }
    }

    async Task RunAsync(Func<CancellationToken, Task> action)
    {
        using var source = new CancellationTokenSource();
        operation = source;
        IsBusy = true;
        StopQrLogin();
        StatusMessage = null;
        try { await action(source.Token); }
        catch (OperationCanceledException) when (source.IsCancellationRequested) { }
        catch (Exception ex)
        {
            // Mobile code rejection and pending token derivation must resume the same session.
            // The service retains these fields only while continuation remains possible.
            if (!requireMobileSession || !pendingState.ClientId.HasValue ||
                (!(pendingState.Requires2FA || pendingState.RequiresEmailAuth) && string.IsNullOrEmpty(pendingState.RefreshToken) && !UsedAutomaticTwoFactor))
            {
                pendingState = new();
                UsedAutomaticTwoFactor = false;
            }
            pendingState.TwofactorCode = pendingState.EmailCode = null;
            GuardCode = null;
            NotifyChallenge();
            StatusMessage = ex is InvalidOperationException ? ex.Message : "无法完成 Steam 登录，请检查网络后重试。";
        }
        finally
        {
            if (ReferenceEquals(operation, source)) operation = null;
            IsBusy = false;
            StartQrIfVisible();
        }
    }

    void SetQrImage(Stream? image)
    {
        var previous = QrCodeImage;
        QrCodeImage = image;
        previous?.Dispose();
    }

    public void Cancel()
    {
        if (operation != null) StatusMessage = "已取消 Steam 登录，可以重新选择登录方式。";
        operation?.Cancel();
        pendingState = new();
        UsedAutomaticTwoFactor = false;
        GuardCode = null;
        NotifyChallenge();
        StopQrLogin();
        StartQrIfVisible();
    }

    public override void Deactivation()
    {
        qrVisible = false;
        Cancel();
        base.Deactivation();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { qrVisible = false; Cancel(); Password = null; LoginSucceededAsync = null; AutomaticTwoFactorCode = null; SavedAccountsProvider = null; SavedAccounts = Array.Empty<SteamSavedLoginAccount>(); SelectedSavedAccount = null; }
        base.Dispose(disposing);
    }
}

/// <summary>Optional provider supplied by the authenticator plugin.</summary>
public interface ISteamSavedAccountProvider
{
    Task<IReadOnlyList<SteamSavedLoginAccount>> LoadAsync(CancellationToken cancellationToken);
}

/// <summary>A selectable saved account; secrets remain inside its session factory.</summary>
public sealed class SteamSavedLoginAccount
{
    public SteamSavedLoginAccount(string accountName, ulong steamId, Func<CancellationToken, Task<SteamSession>> createSessionAsync)
        => (AccountName, SteamId, CreateSessionAsync) = (accountName, steamId, createSessionAsync);
    public string AccountName { get; }
    public ulong SteamId { get; }
    public string DisplayName => $"{AccountName} ({SteamId})";
    internal Func<CancellationToken, Task<SteamSession>> CreateSessionAsync { get; }
}
