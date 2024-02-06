using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam.Cards;
using ArchiSteamFarm.Steam.Storage;
using Newtonsoft.Json;

namespace ArchiSteamFarm.Steam;

public sealed class Bot
{
    [JsonProperty(PropertyName = "AccountFlags")]
    public EAccountFlags AccountFlags { get; private set; }

    [JsonProperty(PropertyName = "BotConfig")]
    public BotConfig BotConfig { get; private set; } = new();

    [JsonProperty(PropertyName = "AvatarHash")]
    public string? AvatarHash { get; private set; }

    [JsonProperty(PropertyName = "BotName")]
    public string BotName { get; private set; } = string.Empty;

    [JsonProperty(PropertyName = "CardsFarmer")]
    public CardsFarmer CardsFarmer { get; private set; } = new();

    [JsonProperty(PropertyName = "GamesToRedeemInBackgroundCount")]
    public uint GamesToRedeemInBackgroundCount { get; private set; }

    [JsonProperty(PropertyName = "HasMobileAuthenticator")]
    public bool HasMobileAuthenticator { get; private set; }

    [JsonProperty(PropertyName = "IsConnectedAndLoggedOn")]
    public bool IsConnectedAndLoggedOn { get; private set; }

    [JsonProperty(PropertyName = "IsPlayingPossible")]
    public bool IsPlayingPossible { get; private set; }

    [JsonProperty(PropertyName = "KeepRunning")]
    public bool KeepRunning { get; private set; }

    [JsonProperty(PropertyName = "Nickname")]
    public string? Nickname { get; private set; }

    [JsonProperty(PropertyName = "RequiredInput")]
    public ASF.EUserInputType RequiredInput { get; private set; }

    [JsonProperty(PropertyName = "SteamID")]
    public ulong SteamID { get; private set; }

    [JsonProperty(PropertyName = "WalletBalance")]
    public long WalletBalance { get; private set; }

    [JsonProperty(PropertyName = "WalletCurrency")]
    public ECurrencyCode WalletCurrency { get; private set; }

    [JsonConstructor]
    public Bot() { }
}
