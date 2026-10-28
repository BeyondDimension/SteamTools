using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Helpers;
using BD.WTTS.Models;
using Moq;
using JsonSerializer = System.Text.Json.JsonSerializer;
using WinAuth;

namespace BD.WTTS.UnitTest;

public sealed class SteamQrSavedSessionTest
{
    const ulong SteamId = 76561198000000001;

    [Test]
    public async Task MissingSavedStateDoesNotRequestLoginOrRefresh()
    {
        var service = new Mock<ISteamAccountService>(MockBehavior.Strict);
        var auth = Create(null, null);
        auth.SessionData = null;
        Assert.That(SteamQrLoginSession.CanUse(auth), Is.False);
        Assert.That(await SteamQrLoginSession.GetAccessTokenAsync(auth, service.Object, default), Is.Null);
        service.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ReusesSavedMobileAccessTokenWithoutNetworkAuthentication()
    {
        var service = new Mock<ISteamAccountService>(MockBehavior.Strict);
        var token = Token();
        var auth = Create(token, null);
        Assert.That(SteamQrLoginSession.CanUse(auth), Is.True);
        Assert.That(await SteamQrLoginSession.GetAccessTokenAsync(auth, service.Object, default), Is.EqualTo(token));
        service.VerifyNoOtherCalls();
    }

    [TestCase("web", false, SteamId)]
    [TestCase("mobile", true, SteamId)]
    [TestCase("mobile", false, SteamId + 1)]
    public async Task RejectsWebExpiredAndOtherAccountAccessTokens(string audience, bool expired, ulong subject)
    {
        var service = new Mock<ISteamAccountService>(MockBehavior.Strict);
        var auth = Create(Token(audience, expired: expired, subject: subject), null);
        Assert.That(SteamQrLoginSession.CanUse(auth), Is.False);
        Assert.That(await SteamQrLoginSession.GetAccessTokenAsync(auth, service.Object, default), Is.Null);
        service.VerifyNoOtherCalls();
    }

    [Test]
    public async Task RenewsExpiredAccessFromSavedRefreshWithoutInteractiveLogin()
    {
        var refresh = Token(refresh: true);
        var renewed = Token();
        var service = new Mock<ISteamAccountService>(MockBehavior.Strict);
        service.Setup(x => x.RefreshAccessToken(SteamId, refresh)).ReturnsAsync(renewed);
        var auth = Create(Token(expired: true), refresh);
        Assert.That(SteamQrLoginSession.CanUse(auth), Is.True);
        Assert.That(await SteamQrLoginSession.GetAccessTokenAsync(auth, service.Object, default), Is.EqualTo(renewed));
        var saved = new WinAuth.SteamClient.SteamSession(auth.SessionData!);
        Assert.Multiple(() =>
        {
            Assert.That(saved.AccessToken, Is.EqualTo(renewed));
            Assert.That(saved.RefreshToken, Is.EqualTo(refresh));
            Assert.That(saved.SessionID, Is.EqualTo("trade-session"));
        });
        service.Verify(x => x.RefreshAccessToken(SteamId, refresh), Times.Once);
        service.VerifyNoOtherCalls();
    }

    [Test]
    public async Task LogoutWinsAgainstOutstandingRefresh()
    {
        var refresh = Token(refresh: true);
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new Mock<ISteamAccountService>();
        service.Setup(x => x.RefreshAccessToken(SteamId, refresh)).Returns(pending.Task);
        var auth = Create(Token(expired: true), refresh);
        var operation = SteamQrLoginSession.GetAccessTokenAsync(auth, service.Object, default);
        auth.SessionData = null;
        pending.SetResult(Token());
        Assert.That(await operation, Is.Null);
        Assert.That(auth.SessionData, Is.Null);
    }

    [TestCase(EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryInvalid, false, false, false)]
    [TestCase(EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryNoPriorHistory, false, false, true)]
    [TestCase(EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryUsedPreviously, true, false, true)]
    [TestCase(EAuthSessionSecurityHistory.KEauthSessionSecurityHistoryUsedPreviously, false, true, true)]
    public void NewIpAndEachReportedWarningRequireExtraReview(EAuthSessionSecurityHistory history,
        bool locationMismatch, bool highUsage, bool expectedRisk)
    {
        var details = new SteamQrLoginRequestDetails(new()
        {
            LoginHistory = history,
            RequestorLocationMismatch = locationMismatch,
            HighUsageLogin = highUsage,
        }, new(1, 123));
        Assert.That(details.IsRiskReported, Is.EqualTo(expectedRisk));
    }

    [TestCase(null, null, false, false, false)]
    [TestCase(false, false, false, false, false)]
    [TestCase(true, false, true, false, true)]
    [TestCase(false, true, false, true, false)]
    [TestCase(true, true, true, true, true)]
    public void LocationAndUsageDetailsAppearOnlyForReportedAbnormalities(bool? locationMismatch, bool? highUsage,
        bool expectedLocation, bool expectedUsage, bool expectedGeolocation)
    {
        var info = new CAuthentication_GetAuthSessionInfo_Response { Country = "Example", Geoloc = "offline coordinates" };
        if (locationMismatch.HasValue) info.RequestorLocationMismatch = locationMismatch.Value;
        if (highUsage.HasValue) info.HighUsageLogin = highUsage.Value;
        var details = new SteamQrLoginRequestDetails(info, new(1, 123));

        Assert.Multiple(() =>
        {
            Assert.That(details.HasLocation, Is.True);
            Assert.That(details.HasLocationAssessment, Is.EqualTo(expectedLocation));
            Assert.That(details.HasUsageAssessment, Is.EqualTo(expectedUsage));
            Assert.That(details.HasGeolocation, Is.EqualTo(expectedGeolocation));
            Assert.That(details.HasSecuritySummary, Is.EqualTo(expectedLocation || expectedUsage));
        });
    }

    static SteamAuthenticator Create(string? access, string? refresh) => new()
    {
        SteamData = "{\"steamid\":76561198000000001,\"account_name\":\"offline\"}",
        SessionData = new WinAuth.SteamClient.SteamSession
        {
            SteamID = SteamId, AccessToken = access, RefreshToken = refresh, SessionID = "trade-session",
        }.ToString(),
    };

    static string Token(string audience = "mobile", bool refresh = false, bool expired = false, ulong subject = SteamId)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = subject.ToString(), aud = refresh ? new[] { audience, "derive" } : new[] { audience },
            exp = DateTimeOffset.UtcNow.AddHours(expired ? -1 : 1).ToUnixTimeSeconds(),
        });
        return "e30." + Convert.ToBase64String(payload).TrimEnd('=').Replace('+', '-').Replace('/', '_') + ".offline";
    }
}
