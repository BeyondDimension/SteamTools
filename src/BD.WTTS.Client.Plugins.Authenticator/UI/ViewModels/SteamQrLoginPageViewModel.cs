using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Helpers;
using BD.WTTS.Services;
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
    public string AccountName => authenticator.AccountName ?? Strings.L10nAuthIdle_Text030;
    public string SteamId => authenticator.SteamId64?.ToString(CultureInfo.InvariantCulture) ?? Strings.L10nAuthIdle_Text031;
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
    [Reactive] public string Status { get; private set; } = Strings.L10nAuthIdle_Text032;

    void SetStatus(string text) => Status = text;

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
        SetStatus(message);
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
            SetStatus(Strings.L10nAuthIdle_Text082);
            if (!await EnsureSavedAccessTokenAsync())
                return;
            SetStatus(Strings.L10nAuthIdle_Text083);
            var results = await SteamLoginQrScreenScanner.CaptureAsync(lifetimeToken);
            lifetimeToken.ThrowIfCancellationRequested();
            foreach (var result in results)
                Challenges.Add(result);
            SelectedChallenge = results.FirstOrDefault();
            SetStatus(results.Count switch
            {
                0 => Strings.L10nAuthIdle_Text084,
                1 => Strings.L10nAuthIdle_Text085,
                _ => Strings.L10nAuthIdle_Text086,
            });
            if (results.Count == 1)
            {
                try { await ReviewSelectedAsync(); }
                catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested) { }
                catch
                {
                    ClearReview();
                    SetStatus(Strings.L10nAuthIdle_Text087);
                }
            }
        }
        catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested) { }
        catch (PlatformNotSupportedException)
        {
            SetStatus(Strings.L10nAuthIdle_Text047);
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            SetStatus(string.Format(Strings.L10nAuthIdle_Text088, ex.NativeErrorCode));
        }
        catch (FileNotFoundException ex)
        {
            var missing = string.IsNullOrWhiteSpace(ex.FileName) ? null : Path.GetFileName(ex.FileName.Split(',')[0]);
            SetStatus(string.Format(Strings.L10nAuthIdle_Text090, missing ?? Strings.L10nAuthIdle_Text089));
        }
        catch
        {
            SetStatus(Strings.L10nAuthIdle_Text091);
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
            SetStatus(Strings.L10nAuthIdle_Text092);
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
        SetStatus(Strings.L10nAuthIdle_Text093);
        this.RaisePropertyChanged(nameof(CanApprove));
    }

    async Task ApproveAsync()
    {
        if (!CanApprove || Disposed || authenticator.SteamId64 is not > 0 || authenticator.SecretKey == null)
            return;
        if (authorizedSessionData != authenticator.SessionData)
        {
            ClearReview();
            SetStatus(Strings.L10nAuthIdle_Text094);
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
            SetStatus(Strings.L10nAuthIdle_Text095);
            Close?.Invoke(true);
        }
        catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested) { }
        catch
        {
            ClearReview();
            SetStatus(Strings.L10nAuthIdle_Text096);
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
