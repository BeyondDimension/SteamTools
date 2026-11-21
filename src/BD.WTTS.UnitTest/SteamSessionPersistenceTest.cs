using System.Net;
using System.Security;
using BD.SteamClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BD.WTTS.UnitTest;

public sealed class SteamSessionPersistenceTest
{
    [Test]
    public async Task RestoresAndUpdatesLegacySecureStorageSession()
    {
        const string steamId = "76561198000000001";
        const string accessToken = "fixture-access";
        const string key = "CurrentSteamUser";
        string? storedJson = """{"steamid":"76561198000000001","access_token":"fixture-access","refresh_token":"fixture+refresh"}""";

        var storage = new Mock<ISecureStorage>(MockBehavior.Strict);
        storage.Setup(x => x.GetAsync(key)).ReturnsAsync(() => storedJson);
        storage.Setup(x => x.SetAsync(key, It.IsAny<string?>()))
            .Callback<string, string?>((_, value) => storedJson = value)
            .Returns(Task.CompletedTask);

        var account = new Mock<ISteamAccountService>(MockBehavior.Strict);
        account.Setup(x => x.IsAccessTokenValid(accessToken)).Returns(true);

        var registrations = new ServiceCollection();
        registrations.AddSingleton(storage.Object);
        registrations.AddSingleton(account.Object);
        registrations.TryAddSteamSessionService();
        registrations.AddSteamAccountService();
        using var provider = registrations.BuildServiceProvider();
        var sessions = provider.GetRequiredService<ISteamSessionService>();

        try
        {
            var session = await sessions.LoadSession();
            Assert.That(session, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(session!.SteamId, Is.EqualTo(steamId));
                Assert.That(session.AccessToken, Is.EqualTo(accessToken));
                Assert.That(session.RefreshToken, Is.EqualTo("fixture+refresh"));
                Assert.That(sessions.RentSession(steamId), Is.SameAs(session));
                var cookies = session.CookieContainer.GetCookies(new Uri("https://steamcommunity.com"));
                Assert.That(cookies["steamLoginSecure"]?.Value, Is.EqualTo("76561198000000001%7C%7Cfixture-access"));
            });

            session!.AccessToken = "updated+access";
            Assert.That(await sessions.SaveSession(session), Is.True);
            Assert.That(storedJson, Is.EqualTo("""{"steamid":"76561198000000001","access_token":"updated+access","refresh_token":"fixture+refresh"}"""));
        }
        finally
        {
            sessions.RemoveSession(steamId);
        }
    }
}
