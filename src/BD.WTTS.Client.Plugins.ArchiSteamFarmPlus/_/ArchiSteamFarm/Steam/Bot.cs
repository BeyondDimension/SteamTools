using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam.Cards;
using ArchiSteamFarm.Steam.Storage;
using STJ = System.Text.Json.Serialization;

namespace ArchiSteamFarm.Steam;

public sealed class Bot
{
    [STJ.JsonInclude]
    public EAccountFlags AccountFlags { get; private set; }

    [STJ.JsonInclude]
    public BotConfig BotConfig { get; private set; } = new();

    [STJ.JsonInclude]
    public string? AvatarHash { get; private set; }

    [STJ.JsonInclude]
    public string BotName { get; private set; } = string.Empty;

    [STJ.JsonInclude]
    public CardsFarmer CardsFarmer { get; private set; } = new();

    [STJ.JsonInclude]
    public uint GamesToRedeemInBackgroundCount { get; private set; }

    [STJ.JsonInclude]
    public bool HasMobileAuthenticator { get; private set; }

    [STJ.JsonInclude]
    public bool IsConnectedAndLoggedOn { get; private set; }

    [STJ.JsonInclude]
    public bool IsPlayingPossible { get; private set; }

    [STJ.JsonInclude]
    public bool KeepRunning { get; private set; }

    [STJ.JsonInclude]
    public string? Nickname { get; private set; }

    [STJ.JsonInclude]
    public ASF.EUserInputType RequiredInput { get; private set; }

    [STJ.JsonInclude]
    public ulong SteamID { get; private set; }

    [STJ.JsonInclude]
    public long WalletBalance { get; private set; }

    [STJ.JsonInclude]
    public ECurrencyCode WalletCurrency { get; private set; }

    [STJ.JsonConstructor]
    public Bot() { }
}
