using BD.WTTS.Models;
using WinAuth;

namespace BD.WTTS.UnitTest;

public sealed class SteamTradeConfirmationModelTest
{
    [Test]
    public async Task DetailFailurePreservesTheConfirmationAndItsSummary()
    {
        // The client has no installed session or HTTP transport.
        using var client = new WinAuth.SteamClient(new SteamAuthenticator());
        var confirmation = CreateTrade();

        await confirmation.LoadDetailsAsync(client, default);

        Assert.Multiple(() =>
        {
            Assert.That(confirmation.HasDetailLoadError, Is.True);
            Assert.That(confirmation.Headline, Is.EqualTo("Offline fixture"));
            Assert.That(confirmation.SendSummary, Is.EqualTo("One outgoing item"));
            Assert.That(confirmation.ReceiveSummary, Is.EqualTo("You will receive nothing"));
            Assert.That(confirmation.ReceiveNoItems, Is.True);
            Assert.That(confirmation.SendItemImageUrls, Is.Empty);
            Assert.That(confirmation.ReceiveItemImageUrls, Is.Empty);
        });
    }

    [Test]
    public async Task CancellationLeavesTheTradeDetailsUnchanged()
    {
        using var client = new WinAuth.SteamClient(new SteamAuthenticator());
        var confirmation = CreateTrade();

        await confirmation.LoadDetailsAsync(client, new CancellationToken(canceled: true));

        Assert.That(confirmation.HasDetailLoadError, Is.False);
    }

    [Test]
    public async Task OtherConfirmationTypesDoNotRequestTradeDetails()
    {
        using var client = new WinAuth.SteamClient(new SteamAuthenticator());
        var confirmation = new SteamTradeConfirmationModel(new SteamMobileTradeConf { Type = 3 });

        await confirmation.LoadDetailsAsync(client, default);

        Assert.That(confirmation.HasDetailLoadError, Is.False);
    }

    static SteamTradeConfirmationModel CreateTrade() => new(new SteamMobileTradeConf
    {
        Type = 2,
        Id = "offline-id",
        Headline = "Offline fixture",
        Summary = ["One outgoing item", "You will receive nothing"],
    });
}
