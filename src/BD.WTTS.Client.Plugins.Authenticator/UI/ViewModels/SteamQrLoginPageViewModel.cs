using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Helpers;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public sealed class SteamQrLoginPageViewModel : WindowViewModel
{
    readonly AuthenticatorItemModel item;
    readonly SteamAuthenticator authenticator;
    readonly ISteamAccountService accountService;
    readonly CancellationTokenSource lifetime = new();
    readonly CancellationToken lifetimeToken;
    string? accessToken;
    string? authorizedSessionData;
    SteamLoginQrChallenge? reviewedChallenge;

    public SteamQrLoginPageViewModel(AuthenticatorItemModel item)
    {
        this.item = item;
        authenticator = item.AuthData.Value as SteamAuthenticator
            ?? throw new ArgumentException("A Steam authenticator is required.", nameof(item));
        accountService = Ioc.Get<ISteamAccountService>();
        lifetimeToken = lifetime.Token;
        var scan = ReactiveCommand.CreateFromTask(ScanScreenAsync);
        var approve = ReactiveCommand.CreateFromTask(ApproveAsync);
        var review = ReactiveCommand.CreateFromTask(ReviewAsync);
        scan.AddTo(this);
        approve.AddTo(this);
        review.AddTo(this);
        ScanScreenCommand = scan;
        ApproveCommand = approve;
        ReviewCommand = review;
        Challenges.CollectionChanged += (_, _) => this.RaisePropertyChanged(nameof(HasMultipleChallenges));
        this.WhenAnyValue(x => x.SelectedChallenge)
            .Subscribe(_ => ClearReview())
            .AddTo(this);
        this.WhenAnyValue(x => x.IsBusy, x => x.IsApproved, x => x.HasSavedSession)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(CanApprove));
                this.RaisePropertyChanged(nameof(CanScan));
            })
            .AddTo(this);
        ValidateSavedSession();
    }

    public string AuthenticatorName => item.AuthName;
    public string AccountName => authenticator.AccountName ?? "未命名 Steam 账号";
    public string SteamId => authenticator.SteamId64?.ToString(CultureInfo.InvariantCulture) ?? "SteamID 缺失";
    public bool IsScreenCaptureSupported => SteamLoginQrScreenScanner.IsSupported;
    public ICommand ScanScreenCommand { get; }
    public ICommand ApproveCommand { get; }
    public ICommand ReviewCommand { get; }
    public ObservableCollection<SteamLoginQrChallenge> Challenges { get; } = new();

    [Reactive] public bool IsBusy { get; set; }
    [Reactive] public bool IsApproved { get; set; }
    [Reactive] public bool HasSavedSession { get; private set; }
    [Reactive] public bool PersistentLogin { get; set; }
    [Reactive] public SteamLoginQrChallenge? SelectedChallenge { get; set; }
    [Reactive] public SteamQrLoginRequestDetails? RequestDetails { get; private set; }
    [Reactive] public string Status { get; private set; } = "准备就绪，请扫描屏幕二维码。";

    public bool HasMultipleChallenges => Challenges.Count > 1;
    public bool NeedsReview => SelectedChallenge != null && RequestDetails == null;
    public bool CanScan => !IsBusy && HasSavedSession && IsScreenCaptureSupported;
    public bool CanApprove => !IsBusy && !IsApproved && HasSavedSession && RequestDetails != null &&
        reviewedChallenge != null && reviewedChallenge == SelectedChallenge;

    void ClearReview()
    {
        reviewedChallenge = null;
        RequestDetails = null;
        IsApproved = false;
        PersistentLogin = false;
        this.RaisePropertyChanged(nameof(NeedsReview));
        this.RaisePropertyChanged(nameof(CanApprove));
    }

    bool ValidateSavedSession()
    {
        HasSavedSession = SteamQrLoginSession.CanUse(authenticator);
        if (HasSavedSession)
            return true;
        InvalidateSession(SteamQrLoginSession.LoginRequiredMessage);
        return false;
    }

    void InvalidateSession(string message)
    {
        accessToken = null;
        authorizedSessionData = null;
        HasSavedSession = false;
        ClearReview();
        Status = message;
    }

    async Task<bool> EnsureSavedAccessTokenAsync()
    {
        if (!ValidateSavedSession())
            return false;
        accessToken = await SteamQrLoginSession.GetAccessTokenAsync(authenticator, accountService, lifetimeToken);
        lifetimeToken.ThrowIfCancellationRequested();
        if (accessToken == null)
        {
            InvalidateSession(SteamQrLoginSession.SessionExpiredMessage);
            return false;
        }
        authorizedSessionData = authenticator.SessionData;
        return true;
    }

    public async Task ScanScreenAsync()
    {
        if (IsBusy || Disposed || !ValidateSavedSession())
            return;
        IsBusy = true;
        ClearReview();
        Challenges.Clear();
        SelectedChallenge = null;
        try
        {
            Status = "正在检查交易确认保存的登录状态…";
            if (!await EnsureSavedAccessTokenAsync())
                return;
            Status = "正在扫描屏幕…请勿关闭或刷新 Steam 登录二维码。";
            var results = await SteamLoginQrScreenScanner.CaptureAsync(lifetimeToken);
            lifetimeToken.ThrowIfCancellationRequested();
            foreach (var result in results)
                Challenges.Add(result);
            SelectedChallenge = results.FirstOrDefault();
            Status = results.Count switch
            {
                0 => "未发现 Steam 登录二维码。请将二维码完整显示，避免被其他窗口遮挡，然后重试。",
                1 => "已识别二维码，正在读取目标设备信息…",
                _ => "发现多个 Steam 登录二维码，请选择要确认的请求，再读取设备信息。",
            };
            if (results.Count == 1)
            {
                try { await ReviewSelectedAsync(); }
                catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested) { }
                catch
                {
                    ClearReview();
                    Status = "已识别二维码，但读取登录请求失败。请刷新二维码重试；若交易确认状态已失效，请先到交易确认重新登录。";
                }
            }
        }
        catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested) { }
        catch (PlatformNotSupportedException)
        {
            Status = "当前屏幕扫码仅支持 Windows。";
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            Status = $"Windows 屏幕截图失败（错误 {ex.NativeErrorCode}）。请在已解锁的本地桌面重试。";
        }
        catch (FileNotFoundException ex)
        {
            var missing = string.IsNullOrWhiteSpace(ex.FileName) ? "未知程序集" : Path.GetFileName(ex.FileName.Split(',')[0]);
            Status = $"缺少运行依赖 {missing}。请使用完整构建或发布目录启动，不要仅替换令牌插件 DLL。";
        }
        catch
        {
            Status = "检查交易确认状态或屏幕扫码失败。请确认网络与程序依赖完整；本页面不会要求重新登录。";
        }
        finally { IsBusy = false; }
    }

    async Task ReviewAsync()
    {
        if (IsBusy || Disposed || SelectedChallenge == null || !ValidateSavedSession())
            return;
        IsBusy = true;
        try { await ReviewSelectedAsync(); }
        catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested) { }
        catch
        {
            ClearReview();
            Status = "读取登录请求失败，二维码可能已过期。请刷新二维码；若登录状态失效，请到交易确认重新登录。";
        }
        finally { IsBusy = false; }
    }

    async Task ReviewSelectedAsync()
    {
        var challenge = SelectedChallenge;
        ClearReview();
        if (challenge == null || !await EnsureSavedAccessTokenAsync())
            return;
        var info = await accountService.GetQrAuthSessionInfoAsync(accessToken!, challenge, lifetimeToken);
        lifetimeToken.ThrowIfCancellationRequested();
        if (SelectedChallenge != challenge || authorizedSessionData != authenticator.SessionData)
            return;
        RequestDetails = new SteamQrLoginRequestDetails(info, challenge);
        this.RaisePropertyChanged(nameof(NeedsReview));
        reviewedChallenge = challenge;
        Status = "请核对以下登录请求。只有点击“确认登录 Steam”才会批准；不是本人发起请关闭窗口。";
        this.RaisePropertyChanged(nameof(CanApprove));
    }

    async Task ApproveAsync()
    {
        if (!CanApprove || Disposed || authenticator.SteamId64 is not > 0 || authenticator.SecretKey == null)
            return;
        if (authorizedSessionData != authenticator.SessionData)
        {
            ClearReview();
            Status = "交易确认的登录状态已变化，请重新读取登录请求后再确认。";
            return;
        }
        var challenge = reviewedChallenge!;
        IsBusy = true;
        try
        {
            if (!await EnsureSavedAccessTokenAsync())
                return;
            if (reviewedChallenge != challenge || SelectedChallenge != challenge)
                return;
            await accountService.ApproveQrAuthSessionAsync(accessToken!, challenge, (ulong)authenticator.SteamId64.Value,
                authenticator.SecretKey, PersistentLogin, lifetimeToken);
            lifetimeToken.ThrowIfCancellationRequested();
            if (reviewedChallenge != challenge || SelectedChallenge != challenge)
                return;
            IsApproved = true;
            reviewedChallenge = null;
            Status = "已确认登录。请回到 Steam 客户端或网页查看结果。";
            Close?.Invoke(true);
        }
        catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested) { }
        catch
        {
            ClearReview();
            Status = "确认登录失败，二维码可能已过期。请重新扫描；若登录状态失效，请到交易确认重新登录。";
        }
        finally { IsBusy = false; }
    }

    public override void OnClosing(object? sender, CancelEventArgs e)
    {
        base.OnClosing(sender, e);
        if (!e.Cancel && !Disposed)
            lifetime.Cancel();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            lifetime.Cancel();
            lifetime.Dispose();
            accessToken = null;
            authorizedSessionData = null;
        }
        base.Dispose(disposing);
    }
}
