//     _                _      _  ____   _                           _____
//    / \    _ __  ___ | |__  (_)/ ___| | |_  ___   __ _  _ __ ___  |  ___|__ _  _ __  _ __ ___
//   / _ \  | '__|/ __|| '_ \ | |\___ \ | __|/ _ \ / _` || '_ ` _ \ | |_  / _` || '__|| '_ ` _ \
//  / ___ \ | |  | (__ | | | || | ___) || |_|  __/| (_| || | | | | ||  _|| (_| || |   | | | | | |
// /_/   \_\|_|   \___||_| |_||_||____/  \__|\___| \__,_||_| |_| |_||_|   \__,_||_|   |_| |_| |_|
// |
// Copyright 2015-2023 Łukasz "JustArchi" Domeradzki
// Contact: JustArchi@JustArchi.net
// |
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// |
// http://www.apache.org/licenses/LICENSE-2.0
// |
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers;
using ArchiSteamFarm.IPC.Integration;
using ArchiSteamFarm.Steam.Data;
using ArchiSteamFarm.Steam.Integration;
using JetBrains.Annotations;
using STJ = System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.Steam.Storage;

[SuppressMessage("ReSharper", "ClassCannotBeInstantiated")]
public sealed class BotConfig
{
    [PublicAPI]
    public const bool DefaultAcceptGifts = false;

    [PublicAPI]
    public const bool DefaultAutoSteamSaleEvent = false;

    [PublicAPI]
    public const EBotBehaviour DefaultBotBehaviour = EBotBehaviour.None;

    [PublicAPI]
    public const string? DefaultCustomGamePlayedWhileFarming = null;

    [PublicAPI]
    public const string? DefaultCustomGamePlayedWhileIdle = null;

    [PublicAPI]
    public const bool DefaultEnabled = false;

    [PublicAPI]
    public const bool DefaultFarmPriorityQueueOnly = false;

    [PublicAPI]
    public const byte DefaultHoursUntilCardDrops = 3;

    [PublicAPI]
    public const EPersonaStateFlag DefaultOnlineFlags = 0;

    [PublicAPI]
    public const EPersonaState DefaultOnlineStatus = EPersonaState.Online;

    [PublicAPI]
    public const ArchiCryptoHelper.ECryptoMethod DefaultPasswordFormat = ArchiCryptoHelper.ECryptoMethod.PlainText;

    [PublicAPI]
    public const bool DefaultPaused = false;

    [PublicAPI]
    public const ERedeemingPreferences DefaultRedeemingPreferences = ERedeemingPreferences.None;

    [PublicAPI]
    public const ERemoteCommunication DefaultRemoteCommunication = ERemoteCommunication.All;

    [PublicAPI]
    public const bool DefaultSendOnFarmingFinished = false;

    [PublicAPI]
    public const byte DefaultSendTradePeriod = 0;

    [PublicAPI]
    public const bool DefaultShutdownOnFarmingFinished = false;

    [PublicAPI]
    public const bool DefaultSkipRefundableGames = false;

    [PublicAPI]
    public const string? DefaultSteamLogin = null;

    [PublicAPI]
    public const ulong DefaultSteamMasterClanID = 0;

    [PublicAPI]
    public const string? DefaultSteamParentalCode = null;

    [PublicAPI]
    public const string? DefaultSteamPassword = null;

    [PublicAPI]
    public const string? DefaultSteamTradeToken = null;

    [PublicAPI]
    public const byte DefaultTradeCheckPeriod = 60;

    [PublicAPI]
    public const ETradingPreferences DefaultTradingPreferences = ETradingPreferences.None;

    [PublicAPI]
    public const bool DefaultUseLoginKeys = true;

    [PublicAPI]
    public const ArchiHandler.EUserInterfaceMode DefaultUserInterfaceMode = ArchiHandler.EUserInterfaceMode.Default;

    internal const byte SteamParentalCodeLength = 4;
    internal const byte SteamTradeTokenLength = 8;

    [PublicAPI]
    public static readonly ImmutableHashSet<Asset.EType> DefaultCompleteTypesToSend = ImmutableHashSet<Asset.EType>.Empty;

    [PublicAPI]
    public static readonly ImmutableList<EFarmingOrder> DefaultFarmingOrders = ImmutableList<EFarmingOrder>.Empty;

    [PublicAPI]
    public static readonly ImmutableList<uint> DefaultGamesPlayedWhileIdle = ImmutableList<uint>.Empty;

    [PublicAPI]
    public static readonly ImmutableHashSet<Asset.EType> DefaultLootableTypes = ImmutableHashSet.Create(Asset.EType.BoosterPack, Asset.EType.FoilTradingCard, Asset.EType.TradingCard);

    [PublicAPI]
    public static readonly ImmutableHashSet<Asset.EType> DefaultMatchableTypes = ImmutableHashSet.Create(Asset.EType.TradingCard);

    [PublicAPI]
    public static readonly ImmutableDictionary<ulong, EAccess> DefaultSteamUserPermissions = ImmutableDictionary<ulong, EAccess>.Empty;

    [PublicAPI]
    public static readonly ImmutableHashSet<Asset.EType> DefaultTransferableTypes = ImmutableHashSet.Create(Asset.EType.BoosterPack, Asset.EType.FoilTradingCard, Asset.EType.TradingCard);

    [STJ.JsonInclude]
    public bool AcceptGifts { get; private set; } = DefaultAcceptGifts;

    [STJ.JsonInclude]
    public bool AutoSteamSaleEvent { get; private set; } = DefaultAutoSteamSaleEvent;

    [STJ.JsonInclude]
    public EBotBehaviour BotBehaviour { get; private set; } = DefaultBotBehaviour;

    [STJ.JsonInclude]
    [SwaggerValidValues(ValidIntValues = new[] { (int)Asset.EType.FoilTradingCard, (int)Asset.EType.TradingCard })]
    public ImmutableHashSet<Asset.EType> CompleteTypesToSend { get; private set; } = DefaultCompleteTypesToSend;

    [STJ.JsonInclude]
    public string? CustomGamePlayedWhileFarming { get; private set; } = DefaultCustomGamePlayedWhileFarming;

    [STJ.JsonInclude]
    public string? CustomGamePlayedWhileIdle { get; private set; } = DefaultCustomGamePlayedWhileIdle;

    [STJ.JsonInclude]
    public bool Enabled { get; private set; } = DefaultEnabled;

    [STJ.JsonInclude]
    public ImmutableList<EFarmingOrder> FarmingOrders { get; private set; } = DefaultFarmingOrders;

    [STJ.JsonInclude]
    public bool FarmPriorityQueueOnly { get; private set; } = DefaultFarmPriorityQueueOnly;

    [STJ.JsonInclude]
    [MaxLength(ArchiHandler.MaxGamesPlayedConcurrently)]
    [SwaggerItemsMinMax(MinimumUint = 1, MaximumUint = uint.MaxValue)]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "This is optional, supportive attribute, we don't care if it gets trimmed or not")]
    public ImmutableList<uint> GamesPlayedWhileIdle { get; private set; } = DefaultGamesPlayedWhileIdle;

    [STJ.JsonInclude]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte HoursUntilCardDrops { get; private set; } = DefaultHoursUntilCardDrops;

    [STJ.JsonInclude]
    public ImmutableHashSet<Asset.EType> LootableTypes { get; private set; } = DefaultLootableTypes;

    [STJ.JsonInclude]
    public ImmutableHashSet<Asset.EType> MatchableTypes { get; private set; } = DefaultMatchableTypes;

    [STJ.JsonInclude]
    public EPersonaStateFlag OnlineFlags { get; private set; } = DefaultOnlineFlags;

    [STJ.JsonInclude]
    public EPersonaState OnlineStatus { get; private set; } = DefaultOnlineStatus;

    [STJ.JsonInclude]
    public ArchiCryptoHelper.ECryptoMethod PasswordFormat { get; internal set; } = DefaultPasswordFormat;

    [STJ.JsonInclude]
    public bool Paused { get; private set; } = DefaultPaused;

    [STJ.JsonInclude]
    public ERedeemingPreferences RedeemingPreferences { get; private set; } = DefaultRedeemingPreferences;

    [STJ.JsonInclude]
    public ERemoteCommunication RemoteCommunication { get; private set; } = DefaultRemoteCommunication;

    [STJ.JsonInclude]
    public bool SendOnFarmingFinished { get; private set; } = DefaultSendOnFarmingFinished;

    [STJ.JsonInclude]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte SendTradePeriod { get; private set; } = DefaultSendTradePeriod;

    [STJ.JsonInclude]
    public bool ShutdownOnFarmingFinished { get; private set; } = DefaultShutdownOnFarmingFinished;

    [STJ.JsonInclude]
    public bool SkipRefundableGames { get; private set; } = DefaultSkipRefundableGames;

    [STJ.JsonInclude]
    public string? SteamLogin
    {
        get => BackingSteamLogin;

        internal set
        {
            IsSteamLoginSet = true;
            BackingSteamLogin = value;
        }
    }

    [STJ.JsonInclude]
    [SwaggerSteamIdentifier(AccountType = EAccountType.Clan)]
    [SwaggerValidValues(ValidIntValues = new[] { 0 })]
    public ulong SteamMasterClanID { get; private set; } = DefaultSteamMasterClanID;

    [STJ.JsonInclude]
    [MaxLength(SteamParentalCodeLength)]
    [MinLength(SteamParentalCodeLength)]
    [SwaggerValidValues(ValidStringValues = new[] { "0" })]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "This is optional, supportive attribute, we don't care if it gets trimmed or not")]
    public string? SteamParentalCode
    {
        get => BackingSteamParentalCode;

        internal set
        {
            IsSteamParentalCodeSet = true;
            BackingSteamParentalCode = value;
        }
    }

    [STJ.JsonInclude]
    public string? SteamPassword
    {
        get => BackingSteamPassword;

        internal set
        {
            IsSteamPasswordSet = true;
            BackingSteamPassword = value;
        }
    }

    [STJ.JsonInclude]
    [MaxLength(SteamTradeTokenLength)]
    [MinLength(SteamTradeTokenLength)]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "This is optional, supportive attribute, we don't care if it gets trimmed or not")]
    public string? SteamTradeToken { get; private set; } = DefaultSteamTradeToken;

    [STJ.JsonInclude]
    public ImmutableDictionary<ulong, EAccess> SteamUserPermissions { get; private set; } = DefaultSteamUserPermissions;

    [STJ.JsonInclude]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte TradeCheckPeriod { get; private set; } = DefaultTradeCheckPeriod;

    [STJ.JsonInclude]
    public ETradingPreferences TradingPreferences { get; private set; } = DefaultTradingPreferences;

    [STJ.JsonInclude]
    public ImmutableHashSet<Asset.EType> TransferableTypes { get; private set; } = DefaultTransferableTypes;

    [STJ.JsonInclude]
    public bool UseLoginKeys { get; private set; } = DefaultUseLoginKeys;

    [STJ.JsonInclude]
    public ArchiHandler.EUserInterfaceMode UserInterfaceMode { get; private set; } = DefaultUserInterfaceMode;

    [STJ.JsonInclude, STJ.JsonExtensionData]
    internal Dictionary<string, System.Text.Json.JsonElement>? AdditionalProperties
    {
        get;
        [UsedImplicitly]
        set;
    }

    internal bool IsSteamLoginSet { get; set; }

    internal bool IsSteamParentalCodeSet { get; set; }

    internal bool IsSteamPasswordSet { get; set; }

    internal bool Saving { get; set; }

    private string? BackingSteamLogin = DefaultSteamLogin;
    private string? BackingSteamParentalCode = DefaultSteamParentalCode;
    private string? BackingSteamPassword = DefaultSteamPassword;

    [STJ.JsonInclude]
    [STJ.JsonPropertyName("s_SteamMasterClanID")]
    private string SSteamMasterClanID
    {
        get => SteamMasterClanID.ToString(CultureInfo.InvariantCulture);

        set
        {
            if (string.IsNullOrEmpty(value) || !ulong.TryParse(value, out ulong result))
            {
                ASF.ArchiLogger.LogGenericError(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsInvalid, nameof(SSteamMasterClanID)));

                return;
            }

            SteamMasterClanID = result;
        }
    }

    [STJ.JsonConstructor]
    public BotConfig() { }

    [UsedImplicitly]
    public bool ShouldSerializeAcceptGifts() => !Saving || (AcceptGifts != DefaultAcceptGifts);

    [UsedImplicitly]
    public bool ShouldSerializeAutoSteamSaleEvent() => !Saving || (AutoSteamSaleEvent != DefaultAutoSteamSaleEvent);

    [UsedImplicitly]
    public bool ShouldSerializeBotBehaviour() => !Saving || (BotBehaviour != DefaultBotBehaviour);

    [UsedImplicitly]
    public bool ShouldSerializeCompleteTypesToSend() => !Saving || ((CompleteTypesToSend != DefaultCompleteTypesToSend) && !CompleteTypesToSend.SetEquals(DefaultCompleteTypesToSend));

    [UsedImplicitly]
    public bool ShouldSerializeCustomGamePlayedWhileFarming() => !Saving || (CustomGamePlayedWhileFarming != DefaultCustomGamePlayedWhileFarming);

    [UsedImplicitly]
    public bool ShouldSerializeCustomGamePlayedWhileIdle() => !Saving || (CustomGamePlayedWhileIdle != DefaultCustomGamePlayedWhileIdle);

    [UsedImplicitly]
    public bool ShouldSerializeEnabled() => !Saving || (Enabled != DefaultEnabled);

    [UsedImplicitly]
    public bool ShouldSerializeFarmingOrders() => !Saving || ((FarmingOrders != DefaultFarmingOrders) && !FarmingOrders.SequenceEqual(DefaultFarmingOrders));

    [UsedImplicitly]
    public bool ShouldSerializeFarmPriorityQueueOnly() => !Saving || (FarmPriorityQueueOnly != DefaultFarmPriorityQueueOnly);

    [UsedImplicitly]
    public bool ShouldSerializeGamesPlayedWhileIdle() => !Saving || ((GamesPlayedWhileIdle != DefaultGamesPlayedWhileIdle) && !GamesPlayedWhileIdle.SequenceEqual(DefaultGamesPlayedWhileIdle));

    [UsedImplicitly]
    public bool ShouldSerializeHoursUntilCardDrops() => !Saving || (HoursUntilCardDrops != DefaultHoursUntilCardDrops);

    [UsedImplicitly]
    public bool ShouldSerializeLootableTypes() => !Saving || ((LootableTypes != DefaultLootableTypes) && !LootableTypes.SetEquals(DefaultLootableTypes));

    [UsedImplicitly]
    public bool ShouldSerializeMatchableTypes() => !Saving || ((MatchableTypes != DefaultMatchableTypes) && !MatchableTypes.SetEquals(DefaultMatchableTypes));

    [UsedImplicitly]
    public bool ShouldSerializeOnlineFlags() => !Saving || (OnlineFlags != DefaultOnlineFlags);

    [UsedImplicitly]
    public bool ShouldSerializeOnlineStatus() => !Saving || (OnlineStatus != DefaultOnlineStatus);

    [UsedImplicitly]
    public bool ShouldSerializePasswordFormat() => !Saving || (PasswordFormat != DefaultPasswordFormat);

    [UsedImplicitly]
    public bool ShouldSerializePaused() => !Saving || (Paused != DefaultPaused);

    [UsedImplicitly]
    public bool ShouldSerializeRedeemingPreferences() => !Saving || (RedeemingPreferences != DefaultRedeemingPreferences);

    [UsedImplicitly]
    public bool ShouldSerializeRemoteCommunication() => !Saving || (RemoteCommunication != DefaultRemoteCommunication);

    [UsedImplicitly]
    public bool ShouldSerializeSendOnFarmingFinished() => !Saving || (SendOnFarmingFinished != DefaultSendOnFarmingFinished);

    [UsedImplicitly]
    public bool ShouldSerializeSendTradePeriod() => !Saving || (SendTradePeriod != DefaultSendTradePeriod);

    [UsedImplicitly]
    public bool ShouldSerializeShutdownOnFarmingFinished() => !Saving || (ShutdownOnFarmingFinished != DefaultShutdownOnFarmingFinished);

    [UsedImplicitly]
    public bool ShouldSerializeSkipRefundableGames() => !Saving || (SkipRefundableGames != DefaultSkipRefundableGames);

    [UsedImplicitly]
    public bool ShouldSerializeSSteamMasterClanID() => !Saving;

    [UsedImplicitly]
    public bool ShouldSerializeSteamLogin() => Saving && IsSteamLoginSet && (SteamLogin != DefaultSteamLogin);

    [UsedImplicitly]
    public bool ShouldSerializeSteamMasterClanID() => !Saving || (SteamMasterClanID != DefaultSteamMasterClanID);

    [UsedImplicitly]
    public bool ShouldSerializeSteamParentalCode() => Saving && IsSteamParentalCodeSet && (SteamParentalCode != DefaultSteamParentalCode);

    [UsedImplicitly]
    public bool ShouldSerializeSteamPassword() => Saving && IsSteamPasswordSet && (SteamPassword != DefaultSteamPassword);

    [UsedImplicitly]
    public bool ShouldSerializeSteamTradeToken() => !Saving || (SteamTradeToken != DefaultSteamTradeToken);

    [UsedImplicitly]
    public bool ShouldSerializeSteamUserPermissions() => !Saving || ((SteamUserPermissions != DefaultSteamUserPermissions) && ((SteamUserPermissions.Count != DefaultSteamUserPermissions.Count) || SteamUserPermissions.Except(DefaultSteamUserPermissions).Any()));

    [UsedImplicitly]
    public bool ShouldSerializeTradeCheckPeriod() => !Saving || (TradeCheckPeriod != DefaultTradeCheckPeriod);

    [UsedImplicitly]
    public bool ShouldSerializeTradingPreferences() => !Saving || (TradingPreferences != DefaultTradingPreferences);

    [UsedImplicitly]
    public bool ShouldSerializeTransferableTypes() => !Saving || ((TransferableTypes != DefaultTransferableTypes) && !TransferableTypes.SetEquals(DefaultTransferableTypes));

    [UsedImplicitly]
    public bool ShouldSerializeUseLoginKeys() => !Saving || (UseLoginKeys != DefaultUseLoginKeys);

    [UsedImplicitly]
    public bool ShouldSerializeUserInterfaceMode() => !Saving || (UserInterfaceMode != DefaultUserInterfaceMode);

    [PublicAPI]
    public enum EAccess : byte
    {
        None,
        FamilySharing,
        Operator,
        Master,
    }

    [Flags]
    [PublicAPI]
    public enum EBotBehaviour : byte
    {
        None = 0,
        RejectInvalidFriendInvites = 1,
        RejectInvalidTrades = 2,
        RejectInvalidGroupInvites = 4,
        DismissInventoryNotifications = 8,
        MarkReceivedMessagesAsRead = 16,
        MarkBotMessagesAsRead = 32,
        All = RejectInvalidFriendInvites | RejectInvalidTrades | RejectInvalidGroupInvites | DismissInventoryNotifications | MarkReceivedMessagesAsRead | MarkBotMessagesAsRead,
    }

    [PublicAPI]
    public enum EFarmingOrder : byte
    {
        Unordered,
        AppIDsAscending,
        AppIDsDescending,
        CardDropsAscending,
        CardDropsDescending,
        HoursAscending,
        HoursDescending,
        NamesAscending,
        NamesDescending,
        Random,
        BadgeLevelsAscending,
        BadgeLevelsDescending,
        RedeemDateTimesAscending,
        RedeemDateTimesDescending,
        MarketableAscending,
        MarketableDescending,
    }

    [Flags]
    [PublicAPI]
    public enum ERedeemingPreferences : byte
    {
        None = 0,
        Forwarding = 1,
        Distributing = 2,
        KeepMissingGames = 4,
        AssumeWalletKeyOnBadActivationCode = 8,
        All = Forwarding | Distributing | KeepMissingGames | AssumeWalletKeyOnBadActivationCode,
    }

    [Flags]
    [PublicAPI]
    public enum ERemoteCommunication : byte
    {
        None = 0,
        SteamGroup = 1,
        PublicListing = 2,
        All = SteamGroup | PublicListing,
    }

    [Flags]
    [PublicAPI]
    public enum ETradingPreferences : byte
    {
        None = 0,
        AcceptDonations = 1,
        SteamTradeMatcher = 2,
        MatchEverything = 4,
        DontAcceptBotTrades = 8,
        MatchActively = 16,
        All = AcceptDonations | SteamTradeMatcher | MatchEverything | DontAcceptBotTrades | MatchActively,
    }
}
