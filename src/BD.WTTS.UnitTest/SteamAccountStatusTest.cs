using System.Reflection;
using BD.SteamClient.Models;
using BD.SteamClient.Services.Implementation;
using BD.WTTS.Models;
using ValveKeyValue;

namespace BD.WTTS.UnitTest;

public sealed class SteamAccountStatusTest
{
    [Test]
    public void SteamAccountShowsAutoLoginAndRememberPasswordIndependently()
    {
        var user = new SteamUser { MostRecent = false, RememberPassword = true };
        var autoLogin = typeof(SteamUser).GetProperty("AutoLogin");
        Assert.That(autoLogin, Is.Not.Null, "Steam's AutoLogin must be represented separately from MostRecent.");
        autoLogin!.SetValue(user, true);
        var account = new SteamAccount(user);
        var accountAutoLogin = typeof(SteamAccount).GetProperty("AutoLogin");
        Assert.That(accountAutoLogin, Is.Not.Null);
        Assert.That(accountAutoLogin!.GetValue(account), Is.EqualTo(true));
        Assert.That(account.RememberPassword, Is.True);
        Assert.That(account.MostRecent, Is.False);
        var changed = new List<string?>();
        account.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        autoLogin.SetValue(user, false);
        user.RememberPassword = false;
        Assert.Multiple(() =>
        {
            Assert.That(account.IsRecentLogin, Is.False);
            Assert.That(account.RememberPassword, Is.False);
            Assert.That(changed, Does.Contain(nameof(SteamAccount.IsRecentLogin)));
            Assert.That(changed, Does.Contain(nameof(SteamAccount.RememberPassword)));
        });
    }

    [Test]
    public void SteamAutoLoginUsesTheNewFieldBeforeTheLegacyField()
    {
        var method = typeof(SteamServiceImpl).GetMethod("ReadAutoLogin", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        var account = new KVObject("user", new[]
        {
            new KVObject("AutoLogin", (KVValue)0),
            new KVObject("AllowAutoLogin", (KVValue)1),
        });
        Assert.That(method!.Invoke(null, new object[] { account }), Is.EqualTo(false));
        var legacyAccount = new KVObject("user", new[] { new KVObject("AllowAutoLogin", (KVValue)1) });
        Assert.That(method.Invoke(null, new object[] { legacyAccount }), Is.EqualTo(true));
        var missingFields = new KVObject("user", Array.Empty<KVObject>());
        Assert.That(method.Invoke(null, new object[] { missingFields }), Is.EqualTo(false));
    }

}
