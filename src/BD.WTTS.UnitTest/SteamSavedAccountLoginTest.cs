using System.Net;
using System.Net.Http;
using JsonSerializer = System.Text.Json.JsonSerializer;
using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.UI.ViewModels;
using BD.WTTS.Models;
using WinAuth;
using Moq;
using ReactiveUI.Builder;
using System.Reactive.Concurrency;

namespace BD.WTTS.UnitTest;

public sealed class SteamSavedAccountLoginTest
{
    const ulong SteamId = 76561198000000001;

    [OneTimeSetUp]
    public void InitializeReactiveUI() => RxAppBuilder.CreateReactiveUIBuilder()
        .WithMainThreadScheduler(ImmediateScheduler.Instance).BuildApp();

    [Test]
    public void ActiveTradeSessionIsAvailableWithoutPersistingItAndLogoutInvalidatesIt()
    {
        const ushort id = 61001;
        var live = NewAuthenticator();
        var stored = NewAuthenticator();
        using var item = new AuthenticatorItemModel(new AuthenticatorDTO { Id = id, Name = "offline", Value = live });
        var access = Token(SteamId, ["mobile", "web"]);
        live.Client.SessionSet(new WinAuth.SteamClient.SteamSession { SteamID = SteamId, AccessToken = access }.ToString());
        var selected = AuthenticatorSavedSteamAccounts.FindUsableSession(id, stored, out var owner);
        Assert.Multiple(() =>
        {
            Assert.That(selected, Is.SameAs(live.Client.Session));
            Assert.That(owner, Is.SameAs(live));
            Assert.That(stored.SessionData, Is.Null.Or.Empty);
            Assert.That(AuthenticatorSavedSteamAccounts.IsLiveSessionCurrent(id, live, selected!, access, null), Is.True);
        });

        live.Client.Logout();
        Assert.Multiple(() =>
        {
            Assert.That(AuthenticatorSavedSteamAccounts.IsLiveSessionCurrent(id, live, selected!, access, null), Is.False);
            Assert.That(AuthenticatorSavedSteamAccounts.FindUsableSession(id, stored, out _), Is.Null);
        });
        live.Client.Dispose();
        stored.Client.Dispose();
    }

    [Test]
    public void RetiringOldTokenItemCannotRemoveItsReplacementSession()
    {
        const ushort id = 61002;
        var old = NewAuthenticator();
        var replacement = NewAuthenticator();
        var stored = NewAuthenticator();
        var oldItem = new AuthenticatorItemModel(new AuthenticatorDTO { Id = id, Name = "old", Value = old });
        using var newItem = new AuthenticatorItemModel(new AuthenticatorDTO { Id = id, Name = "new", Value = replacement });
        replacement.Client.SessionSet(new WinAuth.SteamClient.SteamSession
        {
            SteamID = SteamId, AccessToken = Token(SteamId, ["mobile", "web"]),
        }.ToString());
        oldItem.Dispose();
        var selected = AuthenticatorSavedSteamAccounts.FindUsableSession(id, stored, out var owner);
        Assert.Multiple(() =>
        {
            Assert.That(selected, Is.SameAs(replacement.Client.Session));
            Assert.That(owner, Is.SameAs(replacement));
        });
        newItem.Dispose();
        Assert.That(AuthenticatorSavedSteamAccounts.FindUsableSession(id, stored, out _), Is.Null);
        old.Client.Dispose();
        replacement.Client.Dispose();
        stored.Client.Dispose();
    }

    static SteamAuthenticator NewAuthenticator() => new()
    {
        SteamData = "{\"steamid\":76561198000000001,\"account_name\":\"offline\"}",
    };

    [TestCase(false)]
    [TestCase(true)]
    public async Task SavedAccountUsesVerifiedWebSessionAndHonorsRememberPreference(bool remember)
    {
        var service = new Mock<ISteamSessionService>();
        SteamSession? installed = null;
        service.Setup(x => x.AddOrSetSeesion(It.IsAny<SteamSession>()))
            .Returns<SteamSession>(session => { installed = session; return true; });
        service.Setup(x => x.RentSession(SteamId.ToString())).Returns(() => installed);
        service.Setup(x => x.SaveSession(It.IsAny<SteamSession>())).ReturnsAsync(true);
        using var login = new SteamLoginViewModel(rememberLogin: remember,
            accountService: new Mock<ISteamAccountService>().Object, sessionService: service.Object);
        login.SavedAccountsProvider = _ => Task.FromResult<IReadOnlyList<SteamSavedLoginAccount>>(
            [new("offline", SteamId, _ => Task.FromResult(CreateSession(SteamId)))]);
        var completed = 0;
        login.LoginSucceededAsync = _ => { completed++; return Task.CompletedTask; };

        await login.LoadSavedAccountsAsync();
        login.SelectedSavedAccount = login.SavedAccounts.Single();
        await login.SavedAccountLoginAsync();

        Assert.Multiple(() =>
        {
            Assert.That(login.State.Success, Is.True);
            Assert.That(login.State.SteamId, Is.EqualTo(SteamId));
            Assert.That(login.State.AccessToken, Is.EqualTo("web-access"));
            Assert.That(login.State.RefreshToken, Is.Empty);
            Assert.That(completed, Is.EqualTo(1));
        });
        service.Verify(x => x.SaveSession(It.IsAny<SteamSession>()), remember ? Times.Once : Times.Never);
    }

    [Test]
    public async Task RejectsSessionForDifferentAccountBeforeInstallingIt()
    {
        var service = new Mock<ISteamSessionService>(MockBehavior.Strict);
        using var login = new SteamLoginViewModel(accountService: new Mock<ISteamAccountService>().Object,
            sessionService: service.Object);
        login.SelectedSavedAccount = new("offline", SteamId, _ => Task.FromResult(CreateSession(SteamId + 1)));
        await login.SavedAccountLoginAsync();
        Assert.That(login.State.Success, Is.False);
        service.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CancellingAccountListDoesNotPublishLateResults()
    {
        using var login = new SteamLoginViewModel(accountService: new Mock<ISteamAccountService>().Object,
            sessionService: new Mock<ISteamSessionService>().Object);
        var pending = new TaskCompletionSource<IReadOnlyList<SteamSavedLoginAccount>>(TaskCreationOptions.RunContinuationsAsynchronously);
        login.SavedAccountsProvider = _ => pending.Task;
        var loading = login.LoadSavedAccountsAsync();
        login.Cancel();
        login.ClearSavedAccounts();
        pending.SetResult([new("late", SteamId, _ => Task.FromResult(CreateSession(SteamId)))]);
        await loading;
        Assert.That(login.SavedAccounts, Is.Empty);
    }

    [Test]
    public async Task CancellingSessionConversionDoesNotInstallLateSession()
    {
        var service = new Mock<ISteamSessionService>(MockBehavior.Strict);
        using var login = new SteamLoginViewModel(accountService: new Mock<ISteamAccountService>().Object,
            sessionService: service.Object);
        var pending = new TaskCompletionSource<SteamSession>(TaskCreationOptions.RunContinuationsAsynchronously);
        login.SelectedSavedAccount = new("offline", SteamId, _ => pending.Task);
        var operation = login.SavedAccountLoginAsync();
        login.Cancel();
        pending.SetResult(CreateSession(SteamId));
        await operation;
        Assert.That(login.State.Success, Is.False);
        service.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SavedMobileWebTokenCreatesVerifiedSessionWithoutCopyingRefreshToken()
    {
        var access = Token(SteamId, ["mobile", "web"]);
        var service = new Mock<ISteamAccountService>(MockBehavior.Strict);
        WebHandler? handler = null;
        var session = await AuthenticatorSavedSteamAccounts.CreateWebSessionAsync(SteamId, access, null, service.Object, default,
            cookies => handler = new WebHandler(cookies));
        Assert.Multiple(() =>
        {
            Assert.That(session.SteamId, Is.EqualTo(SteamId.ToString()));
            Assert.That(session.AccessToken, Is.EqualTo(access));
            Assert.That(session.RefreshToken, Is.Empty);
            Assert.That(Uri.UnescapeDataString(session.CookieContainer.GetCookies(new Uri("https://steamcommunity.com"))["steamLoginSecure"]!.Value), Is.EqualTo($"{SteamId}||{access}"));
            Assert.That(session.CookieContainer.GetCookies(new Uri("https://store.steampowered.com"))["sessionid"]?.Value,
                Is.EqualTo(session.CookieContainer.GetCookies(new Uri("https://steamcommunity.com"))["sessionid"]?.Value));
            Assert.That(handler!.VerifiedCommunity, Is.True);
        });
        service.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ExpiredMobileAccessIsRenewedBeforeWebSessionIsCreated()
    {
        var refresh = Token(SteamId, ["mobile", "derive"]);
        var renewed = Token(SteamId, ["mobile", "web"]);
        var service = new Mock<ISteamAccountService>(MockBehavior.Strict);
        service.Setup(x => x.RefreshAccessToken(SteamId, refresh)).ReturnsAsync(renewed);
        var session = await AuthenticatorSavedSteamAccounts.CreateWebSessionAsync(SteamId,
            Token(SteamId, ["mobile", "web"], expired: true), refresh, service.Object, default, cookies => new WebHandler(cookies));
        Assert.Multiple(() =>
        {
            Assert.That(session.AccessToken, Is.EqualTo(renewed));
            Assert.That(session.RefreshToken, Is.Empty);
        });
        service.Verify(x => x.RefreshAccessToken(SteamId, refresh), Times.Once);
    }

    [TestCase("other-account")]
    [TestCase("no-web")]
    [TestCase("refresh-token")]
    [TestCase("expired")]
    public void RejectsUnsuitableAccessBeforeContactingSteam(string scenario)
    {
        var id = scenario == "other-account" ? SteamId + 1 : SteamId;
        var audiences = scenario switch
        {
            "no-web" => new[] { "mobile" },
            "refresh-token" => new[] { "mobile", "web", "derive" },
            _ => new[] { "mobile", "web" },
        };
        var service = new Mock<ISteamAccountService>(MockBehavior.Strict);
        var reachedWeb = false;
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await AuthenticatorSavedSteamAccounts.CreateWebSessionAsync(SteamId,
                Token(id, audiences, expired: scenario == "expired"), null, service.Object, default,
                cookies => { reachedWeb = true; return new WebHandler(cookies); }));
        Assert.That(reachedWeb, Is.False);
    }

    [TestCase("/login/home/")]
    [TestCase("/profiles/76561198000000002")]
    [TestCase("https://evil.example/id/account")]
    public void RejectsUnverifiedCommunitySession(string redirect)
    {
        var service = new Mock<ISteamAccountService>(MockBehavior.Strict);
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await AuthenticatorSavedSteamAccounts.CreateWebSessionAsync(SteamId,
                Token(SteamId, ["mobile", "web"]), null, service.Object, default,
                cookies => new WebHandler(cookies, redirect)));
    }

    [Test]
    public async Task CancellingRefreshDoesNotContactCommunity()
    {
        var refresh = Token(SteamId, ["mobile", "derive"]);
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new Mock<ISteamAccountService>(MockBehavior.Strict);
        service.Setup(x => x.RefreshAccessToken(SteamId, refresh)).Returns(pending.Task);
        using var cancel = new CancellationTokenSource();
        var reachedWeb = false;
        var operation = AuthenticatorSavedSteamAccounts.CreateWebSessionAsync(SteamId, null, refresh, service.Object, cancel.Token,
            cookies => { reachedWeb = true; return new WebHandler(cookies); });
        cancel.Cancel();
        Assert.CatchAsync<OperationCanceledException>(async () => await operation);
        pending.SetResult(Token(SteamId, ["mobile", "web"]));
        Assert.That(reachedWeb, Is.False);
    }

    static SteamSession CreateSession(ulong id) => new()
    {
        SteamId = id.ToString(), AccessToken = "web-access", RefreshToken = string.Empty,
    };

    static string Token(ulong id, string[] audiences, bool expired = false)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = id.ToString(), aud = audiences,
            exp = DateTimeOffset.UtcNow.AddHours(expired ? -1 : 1).ToUnixTimeSeconds(),
        });
        return "e30." + Convert.ToBase64String(payload).TrimEnd('=').Replace('+', '-').Replace('/', '_') + ".offline";
    }

    sealed class WebHandler(CookieContainer cookies, string redirect = "/profiles/76561198000000001") : HttpMessageHandler
    {
        public bool VerifiedCommunity { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.RequestUri != new Uri("https://steamcommunity.com/my/") ||
                cookies.GetCookies(request.RequestUri)["steamLoginSecure"] is null ||
                cookies.GetCookies(request.RequestUri)["sessionid"] is null)
                throw new InvalidOperationException("Unexpected community verification request.");
            VerifiedCommunity = true;
            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri(redirect, UriKind.RelativeOrAbsolute);
            return Task.FromResult(response);
        }
    }
}
