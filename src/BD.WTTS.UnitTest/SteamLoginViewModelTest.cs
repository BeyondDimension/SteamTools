using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.UI.ViewModels;
using Moq;
using System.Net;
using SteamKit2;
using ReactiveUI.Builder;
using System.Reactive.Concurrency;

namespace BD.WTTS.UnitTest;

public sealed class SteamLoginViewModelTest
{
    [OneTimeSetUp]
    public void InitializeReactiveUI() => RxAppBuilder.CreateReactiveUIBuilder()
        .WithMainThreadScheduler(ImmediateScheduler.Instance).BuildApp();

    static SteamLoginViewModel Create(Mock<ISteamAccountService> account, SteamLoginState? result = null,
        bool cookies = false, bool remember = false, Mock<ISteamSessionService>? sessions = null)
        => new(result, cookies, remember, account.Object, (sessions ?? new Mock<ISteamSessionService>()).Object);

    [Test]
    public async Task EmptyCredentialsDoNotCallSteam()
    {
        var account = new Mock<ISteamAccountService>(MockBehavior.Strict);
        using var login = Create(account);
        await login.LoginAsync();
        Assert.That(login.StatusMessage, Is.Not.Empty);
        Assert.That(login.IsBusy, Is.False);
    }

    [Test]
    public async Task TokenEnrollmentSelectsMobileSessionsForPasswordAndQr()
    {
        var account = new Mock<ISteamAccountService>();
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        account.Setup(x => x.DoLoginWithQRCodeAsync(It.IsAny<SteamLoginState>(), It.IsAny<Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>>>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        using var login = new SteamLoginViewModel(accountService: account.Object, sessionService: new Mock<ISteamSessionService>().Object, requireMobileSession: true);
        login.UserName = "account"; login.Password = "password";
        await login.LoginAsync();
        await login.LoginWithQrCodeAsync();
        account.Verify(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>()), Times.Once);
        account.Verify(x => x.DoLoginWithQRCodeAsync(It.IsAny<SteamLoginState>(), It.IsAny<Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>>>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GuardChallengeContinuesTheSameSessionAndNotifiesOnce()
    {
        var account = new Mock<ISteamAccountService>();
        var calls = 0;
        SteamLoginState? first = null;
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), null, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((state, _, _) =>
            {
                if (++calls == 1) { first = state; state.Requires2FA = true; state.ClientId = 123; }
                else
                {
                    Assert.That(state, Is.SameAs(first));
                    Assert.That(state.TwofactorCode, Is.EqualTo("ABCDE"));
                    state.Success = true; state.Requires2FA = false; state.SteamId = 76561198000000001;
                    state.AccessToken = "access"; state.RefreshToken = "refresh";
                }
                return Task.CompletedTask;
            });
        var result = new SteamLoginState();
        using var login = Create(account, result);
        var completed = 0;
        login.LoginSucceededAsync = _ => { completed++; return Task.CompletedTask; };
        login.UserName = "account"; login.Password = "password";
        await login.LoginAsync();
        Assert.That(login.RequiresGuardCode, Is.True);
        login.GuardCode = "ABCDE";
        await login.LoginAsync();
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.RefreshToken, Is.EqualTo("refresh"));
            Assert.That(login.HadTwoFactorChallenge, Is.True);
            Assert.That(completed, Is.EqualTo(1));
            Assert.That(login.Password, Is.Null.Or.Empty);
            Assert.That(result.Password, Is.Null.Or.Empty);
        });
    }

    [Test]
    public async Task AutomaticTwoFactorContinuesBegunSession()
    {
        var account = new Mock<ISteamAccountService>();
        SteamLoginState? begun = null;
        var calls = 0;
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((state, _, _) =>
            {
                if (++calls == 1)
                {
                    begun = state;
                    state.ClientId = 123;
                    state.SteamId = 76561198000000001;
                    state.Requires2FA = true;
                }
                else
                {
                    Assert.That(state, Is.SameAs(begun));
                    Assert.That(state.TwofactorCode, Is.EqualTo("ABCDE"));
                    state.Requires2FA = false;
                    state.Success = true;
                    state.AccessToken = "access";
                }
                return Task.CompletedTask;
            });
        using var login = new SteamLoginViewModel(accountService: account.Object, sessionService: new Mock<ISteamSessionService>().Object, requireMobileSession: true);
        var providerCalls = 0;
        login.AutomaticTwoFactorCode = state =>
        {
            providerCalls++;
            Assert.That(state, Is.SameAs(begun));
            return "ABCDE";
        };
        login.UserName = "account"; login.Password = "password";

        await login.LoginAsync();

        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(providerCalls, Is.EqualTo(1));
            Assert.That(login.State.Success, Is.True);
            Assert.That(login.HadTwoFactorChallenge, Is.True);
            Assert.That(login.UsedAutomaticTwoFactor, Is.True);
        });
    }

    [Test]
    public async Task AutomaticTwoFactorProviderErrorPreventsSubmission()
    {
        var account = new Mock<ISteamAccountService>();
        var calls = 0;
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((state, _, _) =>
            {
                calls++;
                state.ClientId = 123;
                state.SteamId = 76561198000000001;
                state.Requires2FA = true;
                return Task.CompletedTask;
            });
        using var login = new SteamLoginViewModel(accountService: account.Object, sessionService: new Mock<ISteamSessionService>().Object, requireMobileSession: true);
        login.AutomaticTwoFactorCode = _ => throw new InvalidOperationException("令牌账号不匹配，请检查令牌。");
        login.UserName = "account"; login.Password = "password";

        await login.LoginAsync();

        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(login.StatusMessage, Does.Contain("令牌账号不匹配"));
            Assert.That(login.State.Success, Is.False);
            Assert.That(login.RequiresManualGuardCode, Is.False);
            Assert.That(login.UsedAutomaticTwoFactor, Is.False);
        });
    }

    [Test]
    public async Task AutomaticTwoFactorProviderDoesNotReplaceManualEmailCode()
    {
        var account = new Mock<ISteamAccountService>();
        SteamLoginState? begun = null;
        var calls = 0;
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((state, _, _) =>
            {
                if (++calls == 1) { begun = state; state.ClientId = 123; state.RequiresEmailAuth = true; }
                else
                {
                    Assert.That(state, Is.SameAs(begun));
                    Assert.That(state.EmailCode, Is.EqualTo("email-code"));
                    state.RequiresEmailAuth = false;
                    state.Success = true;
                    state.SteamId = 76561198000000001;
                    state.AccessToken = "access";
                }
                return Task.CompletedTask;
            });
        using var login = new SteamLoginViewModel(accountService: account.Object, sessionService: new Mock<ISteamSessionService>().Object, requireMobileSession: true);
        var providerCalls = 0;
        login.AutomaticTwoFactorCode = _ => { providerCalls++; return "ABCDE"; };
        login.UserName = "account"; login.Password = "password";

        await login.LoginAsync();
        Assert.That(login.RequiresManualGuardCode, Is.True);
        login.GuardCode = "email-code";
        await login.LoginAsync();

        Assert.Multiple(() =>
        {
            Assert.That(providerCalls, Is.Zero);
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(login.UsedAutomaticTwoFactor, Is.False);
            Assert.That(login.State.Success, Is.True);
        });
    }

    [Test]
    public async Task RejectedAutomaticTwoFactorIsNotRepeated()
    {
        var account = new Mock<ISteamAccountService>();
        var calls = 0;
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((state, _, _) =>
            {
                if (++calls == 1)
                {
                    state.ClientId = 123;
                    state.SteamId = 76561198000000001;
                    state.Requires2FA = true;
                    return Task.CompletedTask;
                }
                Assert.That(state.TwofactorCode, Is.EqualTo("ABCDE"));
                throw new InvalidOperationException("Steam rejected the code.");
            });
        using var login = new SteamLoginViewModel(accountService: account.Object, sessionService: new Mock<ISteamSessionService>().Object, requireMobileSession: true);
        var providerCalls = 0;
        login.AutomaticTwoFactorCode = _ => { providerCalls++; return "ABCDE"; };
        login.UserName = "account"; login.Password = "password";

        await login.LoginAsync();

        Assert.Multiple(() =>
        {
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(providerCalls, Is.EqualTo(1));
            Assert.That(login.StatusMessage, Does.Contain("Steam rejected the code."));
            Assert.That(login.State.Success, Is.False);
            Assert.That(login.RequiresManualGuardCode, Is.False);
            Assert.That(login.UsedAutomaticTwoFactor, Is.False);
        });
    }

    [Test]
    public async Task HiddenQrIsCancelledAndRapidReshowStartsAFreshSession()
    {
        var account = new Mock<ISteamAccountService>();
        var firstStarted = new TaskCompletionSource<(SteamLoginState State, CancellationToken Token)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource<(SteamLoginState State, CancellationToken Token)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstRequest = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondRequest = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var starts = 0;
        account.Setup(x => x.DoLoginWithQRCodeAsync(It.IsAny<SteamLoginState>(), It.IsAny<Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>>>(), It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>>, string, uint, int, CancellationToken>((state, _, _, _, _, token) =>
            {
                if (Interlocked.Increment(ref starts) == 1)
                {
                    firstStarted.TrySetResult((state, token));
                    return firstRequest.Task;
                }
                secondStarted.TrySetResult((state, token));
                return secondRequest.Task;
            });
        using var login = Create(account);
        login.SetQrLoginVisible(true);
        var first = await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        login.SetQrLoginVisible(false);
        Assert.That(first.Token.IsCancellationRequested, Is.True);
        login.SetQrLoginVisible(true);
        var second = await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(second.Token.IsCancellationRequested, Is.False);

        first.State.Success = true;
        first.State.SteamId = 76561198000000001;
        first.State.AccessToken = "late";
        firstRequest.SetResult();
        await Task.Yield();
        Assert.That(login.State.Success, Is.False);
        login.SetQrLoginVisible(false);
        Assert.That(second.Token.IsCancellationRequested, Is.True);
        secondRequest.SetResult();
    }

    [Test]
    public async Task PasswordLoginCanWinWhileQrIsPolling()
    {
        var account = new Mock<ISteamAccountService>();
        var qrStarted = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        var qrRequest = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        account.Setup(x => x.DoLoginWithQRCodeAsync(It.IsAny<SteamLoginState>(), It.IsAny<Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>>>(), It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>>, string, uint, int, CancellationToken>((_, _, _, _, _, token) =>
            {
                qrStarted.TrySetResult(token);
                return qrRequest.Task;
            });
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), null, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((state, _, _) =>
            {
                state.Success = true;
                state.SteamId = 76561198000000001;
                state.AccessToken = "password-session";
                return Task.CompletedTask;
            });
        using var login = Create(account);
        login.SetQrLoginVisible(true);
        var qrToken = await qrStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        login.UserName = "account";
        login.Password = "password";
        await login.LoginAsync();

        Assert.Multiple(() =>
        {
            Assert.That(qrToken.IsCancellationRequested, Is.True);
            Assert.That(login.State.Success, Is.True);
            Assert.That(login.State.AccessToken, Is.EqualTo("password-session"));
            Assert.That(login.UsedQrCode, Is.False);
        });
        login.SetQrLoginVisible(false);
        qrRequest.SetResult();
    }

    [Test]
    public async Task CancelledQrLoginCannotPublishALateResult()
    {
        var account = new Mock<ISteamAccountService>();
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        SteamLoginState? working = null;
        account.Setup(x => x.DoLoginWithQRCodeAsync(It.IsAny<SteamLoginState>(), It.IsAny<Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>>>(), It.IsAny<string>(), It.IsAny<uint>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, Func<CAuthentication_BeginAuthSessionViaQR_Response, Task<bool>>, string, uint, int, CancellationToken>((state, _, _, _, _, _) => { working = state; return pending.Task; });
        var result = new SteamLoginState();
        using var login = Create(account, result);
        var completed = 0;
        login.LoginSucceededAsync = _ => { completed++; return Task.CompletedTask; };
        var task = login.LoginWithQrCodeAsync();
        login.Cancel();
        await task;
        working!.Success = true; working.AccessToken = "late";
        pending.SetResult();
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.AccessToken, Is.Null);
            Assert.That(completed, Is.Zero);
            Assert.That(login.IsBusy, Is.False);
        });
    }

    [Test]
    public async Task PendingMobileLoginAfterGuardKeepsTheAuthenticatedSessionForRetry()
    {
        var account = new Mock<ISteamAccountService>();
        SteamLoginState? session = null;
        var calls = 0;
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((s, _, _) =>
            {
                if (++calls == 1) { session = s; s.ClientId = 123; s.Requires2FA = true; }
                else
                {
                    Assert.That(s, Is.SameAs(session));
                    Assert.That(s.ClientId, Is.EqualTo(123));
                    s.Requires2FA = false;
                    if (calls == 3) { s.Success = true; s.SteamId = 76561198000000001; s.AccessToken = "access"; }
                }
                return Task.CompletedTask;
            });
        using var login = new SteamLoginViewModel(accountService: account.Object, sessionService: new Mock<ISteamSessionService>().Object, requireMobileSession: true);
        login.UserName = "account"; login.Password = "password";
        await login.LoginAsync();
        login.GuardCode = "ABCDE";
        await login.LoginAsync();
        Assert.That(login.State.Success, Is.False);
        await login.LoginAsync();
        Assert.That(login.State.Success, Is.True);
        Assert.That(login.HadTwoFactorChallenge, Is.True);
    }

    [Test]
    public async Task RejectedMobileGuardCanBeCorrectedWithoutRestartingTheSession()
    {
        var account = new Mock<ISteamAccountService>();
        var calls = 0;
        SteamLoginState? pending = null;
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((state, _, _) =>
            {
                calls++;
                if (calls == 1)
                {
                    pending = state;
                    state.ClientId = 123;
                    state.RequiresEmailAuth = true;
                    return Task.CompletedTask;
                }
                Assert.That(state, Is.SameAs(pending));
                if (calls == 2)
                    throw new InvalidOperationException("Steam authentication request was rejected (EResult 65).");
                Assert.That(state.EmailCode, Is.EqualTo("corrected-code"));
                state.Success = true;
                state.RequiresEmailAuth = false;
                state.SteamId = 76561198000000001;
                state.AccessToken = "access";
                return Task.CompletedTask;
            });
        using var login = new SteamLoginViewModel(accountService: account.Object, sessionService: new Mock<ISteamSessionService>().Object, requireMobileSession: true);
        login.UserName = "offline";
        login.Password = "offline-password";
        await login.LoginAsync();
        login.GuardCode = "bad-code";
        await login.LoginAsync();
        Assert.That(login.RequiresEmailAuth, Is.True);
        Assert.That(login.IsBusy, Is.False);
        login.GuardCode = "corrected-code";
        await login.LoginAsync();
        Assert.That(login.State.Success, Is.True);
        Assert.That(calls, Is.EqualTo(3));
    }

    [TestCase("invalid")]
    [TestCase("0||token")]
    [TestCase("76561198000000001||")]
    public async Task InvalidCookieDoesNotCreateSession(string cookie)
    {
        var sessions = new Mock<ISteamSessionService>(MockBehavior.Strict);
        using var login = Create(new(), cookies: true, sessions: sessions);
        login.SessionId = "session"; login.SteamLoginSecure = cookie;
        await login.CookieLoginAsync();
        Assert.That(login.State.Success, Is.False);
        Assert.That(login.StatusMessage, Is.Not.Empty);
    }

    [Test]
    public async Task CancellationDuringSaveWaitsForTheCommitAndDoesNotPublishSuccess()
    {
        var account = new Mock<ISteamAccountService>();
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), null, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((s, _, _) =>
            {
                s.Success = true; s.SteamId = 76561198000000001; s.AccessToken = "access";
                return Task.CompletedTask;
            });
        var sessions = new Mock<ISteamSessionService>();
        sessions.Setup(x => x.RentSession(It.IsAny<string>())).Returns(new SteamSession());
        var save = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        sessions.Setup(x => x.SaveSession(It.IsAny<SteamSession>())).Returns(save.Task);
        using var login = Create(account, remember: true, sessions: sessions);
        login.UserName = "account"; login.Password = "password";
        var task = login.LoginAsync();
        login.Cancel();
        Assert.That(task.IsCompleted, Is.False, "A storage commit must finish before another login can start.");
        save.SetResult(true);
        await task;
        Assert.That(login.State.Success, Is.False);
    }

    [Test]
    public async Task EncodedCookieLoginPreservesSessionAndRememberPreference()
    {
        var sessions = new Mock<ISteamSessionService>();
        SteamSession? saved = null;
        SteamSession? current = null;
        sessions.Setup(x => x.AddOrSetSeesion(It.IsAny<SteamSession>())).Returns<SteamSession>(s => { current = s; return true; });
        sessions.Setup(x => x.RentSession(It.IsAny<string>())).Returns(() => current);
        sessions.Setup(x => x.SaveSession(It.IsAny<SteamSession>())).Returns<SteamSession>(s => { saved = s; return Task.FromResult(true); });
        using var login = Create(new(), cookies: true, remember: true, sessions: sessions);
        login.SessionId = "session"; login.SteamLoginSecure = "76561198000000001%7C%7Ctoken";
        await login.CookieLoginAsync();
        Assert.Multiple(() =>
        {
            Assert.That(login.State.Success, Is.True);
            Assert.That(saved?.CookieContainer.GetCookies(new Uri("https://steamcommunity.com"))["sessionid"]?.Value, Is.EqualTo("session"));
            Assert.That(saved?.SteamId, Is.EqualTo("76561198000000001"));
        });
    }
}
