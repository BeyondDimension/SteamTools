using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Helpers;
using BD.WTTS.Models;
using BD.WTTS.UI.ViewModels;
using Moq;
using ReactiveUI.Builder;
using System.Reactive.Concurrency;
using WinAuth;

namespace BD.WTTS.UnitTest;

public sealed class SteamAuthenticatorIdentityTest
{
    const ulong SteamId = 76561198000000001;

    [OneTimeSetUp]
    public void InitializeReactiveUI() => RxAppBuilder.CreateReactiveUIBuilder()
        .WithMainThreadScheduler(ImmediateScheduler.Instance).BuildApp();

    [Test]
    public async Task MissingIdIsBoundOnlyAfterTheSameTokenCodeIsAccepted()
    {
        var token = new SteamAuthenticator
        {
            SteamData = "{\"account_name\":\"offline\",\"identity_secret\":\"keep\",\"extra\":{\"value\":42}}",
        };
        var account = new Mock<ISteamAccountService>();
        var calls = 0;
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((state, _, _) =>
            {
                if (++calls == 1)
                {
                    state.ClientId = 123;
                    state.SteamId = SteamId;
                    state.Requires2FA = true;
                    Assert.That(token.SteamId64, Is.Zero);
                }
                else
                {
                    Assert.That(state.TwofactorCode, Is.EqualTo("ABCDE"));
                    Assert.That(token.SteamId64, Is.Zero, "Identity must not be written before Steam accepts the code.");
                    state.Requires2FA = false;
                    state.Success = true;
                    state.AccessToken = "offline-access";
                }
                return Task.CompletedTask;
            });
        using var login = new SteamLoginViewModel(accountService: account.Object, sessionService: new Mock<ISteamSessionService>().Object, requireMobileSession: true);
        login.AutomaticTwoFactorCode = state =>
        {
            SteamAuthenticatorIdentity.ValidateCredentialAccount(token, state);
            return "ABCDE";
        };
        login.LoginSucceededAsync = state =>
        {
            SteamAuthenticatorIdentity.ConfirmAuthenticatedAccount(token, state, login.UsedAutomaticTwoFactor);
            return Task.CompletedTask;
        };
        login.UserName = "OFFLINE";
        login.Password = "offline-password";

        await login.LoginAsync();

        using var json = JsonDocument.Parse(token.SteamData!);
        Assert.Multiple(() =>
        {
            Assert.That(login.State.Success, Is.True);
            Assert.That(token.SteamId64, Is.EqualTo((long)SteamId));
            Assert.That(json.RootElement.GetProperty("identity_secret").GetString(), Is.EqualTo("keep"));
            Assert.That(json.RootElement.GetProperty("extra").GetProperty("value").GetInt32(), Is.EqualTo(42));
        });
    }

    [Test]
    public async Task AcceptedCodeSurvivesInterruptedPollingWithoutResubmission()
    {
        var token = new SteamAuthenticator { SteamData = "{\"account_name\":\"offline\"}" };
        var account = new Mock<ISteamAccountService>();
        var calls = 0;
        var codes = 0;
        account.Setup(x => x.DoLoginV2Async(It.IsAny<SteamLoginState>(), EAuthTokenPlatformType.KEauthTokenPlatformTypeMobileApp, It.IsAny<CancellationToken>()))
            .Returns<SteamLoginState, EAuthTokenPlatformType?, CancellationToken>((state, _, _) =>
            {
                if (++calls == 1)
                {
                    state.ClientId = 123;
                    state.SteamId = SteamId;
                    state.Requires2FA = true;
                }
                else if (calls == 2)
                {
                    Assert.That(state.TwofactorCode, Is.EqualTo("ABCDE"));
                    state.Requires2FA = false;
                    throw new HttpRequestException("offline poll failed after code acceptance");
                }
                else
                {
                    Assert.That(state.ClientId, Is.EqualTo(123));
                    Assert.That(state.TwofactorCode, Is.Null);
                    state.Success = true;
                    state.AccessToken = "offline-access";
                }
                return Task.CompletedTask;
            });
        using var login = new SteamLoginViewModel(accountService: account.Object, sessionService: new Mock<ISteamSessionService>().Object, requireMobileSession: true);
        login.AutomaticTwoFactorCode = state => { SteamAuthenticatorIdentity.ValidateCredentialAccount(token, state); codes++; return "ABCDE"; };
        login.LoginSucceededAsync = state => { SteamAuthenticatorIdentity.ConfirmAuthenticatedAccount(token, state, login.UsedAutomaticTwoFactor); return Task.CompletedTask; };
        login.UserName = "offline";
        login.Password = "offline-password";
        await login.LoginAsync();
        Assert.That(token.SteamId64, Is.Zero);
        await login.LoginAsync();
        Assert.Multiple(() =>
        {
            Assert.That(codes, Is.EqualTo(1));
            Assert.That(calls, Is.EqualTo(3));
            Assert.That(login.State.Success, Is.True);
            Assert.That(token.SteamId64, Is.EqualTo((long)SteamId));
        });
    }

    [Test]
    public void MissingIdWithoutAcceptedTokenCodeCannotBeBound()
    {
        var token = new SteamAuthenticator { SteamData = "{\"account_name\":\"offline\"}" };
        var state = new SteamLoginState { Username = "offline", SteamId = SteamId, Success = true };
        Assert.Throws<InvalidOperationException>(() => SteamAuthenticatorIdentity.ConfirmAuthenticatedAccount(token, state, false));
        Assert.That(token.SteamId64, Is.Zero);
    }

    [Test]
    public void ExistingDifferentIdIsRejectedEvenWithAcceptedCode()
    {
        var token = new SteamAuthenticator { SteamData = "{\"account_name\":\"offline\",\"steamid\":76561198000000002}" };
        var state = new SteamLoginState { Username = "offline", SteamId = SteamId, Success = true };
        Assert.Throws<InvalidOperationException>(() => SteamAuthenticatorIdentity.ValidateCredentialAccount(token, state));
        Assert.Throws<InvalidOperationException>(() => SteamAuthenticatorIdentity.ConfirmAuthenticatedAccount(token, state, true));
        Assert.That(token.SteamId64, Is.EqualTo(76561198000000002));
    }

    [TestCase(null)]
    [TestCase("another-account")]
    public void MissingIdRequiresMatchingStoredAccount(string? accountName)
    {
        var token = new SteamAuthenticator { SteamData = JsonSerializer.Serialize(new { account_name = accountName }) };
        var state = new SteamLoginState { Username = "offline", SteamId = SteamId };
        Assert.Throws<InvalidOperationException>(() => SteamAuthenticatorIdentity.ValidateCredentialAccount(token, state));
        Assert.That(token.SteamId64, Is.Zero);
    }

    [TestCase("{\"account_name\":\"offline\",\"steamid\":\"76561198000000001\"}")]
    [TestCase("{\"account_name\":\"offline\",\"Session\":{\"SteamID\":\"76561198000000001\"}}")]
    [TestCase("{\"account_name\":\"offline\",\"steamid\":76561198000000001}")]
    public void SdaImportRetainsSteamIdFromEitherLocation(string json)
    {
        var source = JsonSerializer.Deserialize<SdaFileModel>(json)!;
        var saved = new SdaFileConvertToSteamDataModel(source);
        var token = new SteamAuthenticator { SteamData = JsonSerializer.Serialize(saved) };
        Assert.That(token.SteamId64, Is.EqualTo((long)SteamId));
    }
}
