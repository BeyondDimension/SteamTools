using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam.Cards;
using ArchiSteamFarm.Steam.Storage;
using Newtonsoft.Json;

namespace ArchiSteamFarm.Steam;

public sealed class Bot
{
    [JsonProperty]
    public EAccountFlags AccountFlags { get; private set; }

    [JsonProperty]
    public BotConfig BotConfig { get; private set; } = new();

    [JsonProperty]
    public string? AvatarHash { get; private set; }

    [JsonProperty]
    public string BotName { get; } = string.Empty;

    [JsonProperty]
    public CardsFarmer CardsFarmer { get; } = new();

    [JsonProperty]
    public uint GamesToRedeemInBackgroundCount { get; private set; }

    [JsonProperty]
    public bool HasMobileAuthenticator { get; private set; }

    [JsonProperty]
    public bool IsConnectedAndLoggedOn { get; private set; }

    [JsonProperty]
    public bool IsPlayingPossible { get; private set; }

    [JsonProperty]
    public bool KeepRunning { get; private set; }

    [JsonProperty]
    public string? Nickname { get; private set; }

    [JsonProperty]
    public ASF.EUserInputType RequiredInput { get; private set; }

    [JsonProperty]
    public ulong SteamID { get; private set; }

    [JsonProperty]
    public long WalletBalance { get; private set; }

    [JsonProperty]
    public ECurrencyCode WalletCurrency { get; private set; }

    [JsonConstructor]
    public Bot() { }
}
