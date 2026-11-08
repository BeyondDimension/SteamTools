using System.Net;
using BD.SteamClient.Models;
using MessagePack;

namespace BD.WTTS.UnitTest;

public sealed class SteamLoginStateSerializationTest
{
    [Test]
    public void CookieCollectionRoundTripsWithBothSerializers()
    {
        var cookies = new CookieCollection { new Cookie("session", "value", "/", "example.com") };
        var state = new SteamLoginState { Username = "test", Cookies = cookies };

        var memoryPackCopy = SteamLoginState.Parse(state.ToBytes());
        var messagePackCopy = MessagePackSerializer.Deserialize<SteamLoginState>(MessagePackSerializer.Serialize(state));

        Assert.Multiple(() =>
        {
            Assert.That(memoryPackCopy?.Username, Is.EqualTo("test"));
            Assert.That(memoryPackCopy?.Cookies?["session"]?.Value, Is.EqualTo("value"));
            Assert.That(messagePackCopy?.Username, Is.EqualTo("test"));
            Assert.That(messagePackCopy?.Cookies?["session"]?.Value, Is.EqualTo("value"));
        });
    }
}
