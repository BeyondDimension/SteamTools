using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers;
using ArchiSteamFarm.IPC.Requests;
using ArchiSteamFarm.IPC.Responses;
using ArchiSteamFarm.Steam.Data;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Storage;
using SteamKit2;
using JsonException = System.Text.Json.JsonException;

namespace BD.WTTS.Services.Implementation;

internal static class ArchiSteamFarmJson
{
    internal static JsonSerializerOptions Options { get; } = CreateOptions();

    internal static JsonTypeInfo<T> TypeInfo<T>() => (JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T));

    internal static JsonTypeInfo TypeInfo(Type type) => Options.GetTypeInfo(type);

    internal static async Task<T?> DeserializeResponseAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        where T : class
    {
        // Json.NET returned null for empty/whitespace content as well as a JSON null.
        // Read the first top-level value without buffering another copy of the response.
        await foreach (var value in System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable(
            stream, TypeInfo<T>(), topLevelValues: true, cancellationToken))
        {
            return value;
        }
        return null;
    }

    static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = ArchiSteamFarmJsonContext.Default.WithAddedModifier(ConfigureContract),
        };
        options.Converters.Add(new AsfOrderedDictionaryConverter());
        options.Converters.Add(new AsfEnumConverter<EAccountFlags>(options));
        options.Converters.Add(new AsfEnumConverter<ECurrencyCode>(options));
        options.Converters.Add(new AsfEnumConverter<ASF.EUserInputType>(options));
        options.Converters.Add(new AsfEnumConverter<Asset.EType>(options));
        options.Converters.Add(new AsfEnumConverter<BotConfig.EBotBehaviour>(options));
        options.Converters.Add(new AsfEnumConverter<BotConfig.EFarmingOrder>(options));
        options.Converters.Add(new AsfEnumConverter<BotConfig.EAccess>(options));
        options.Converters.Add(new AsfEnumConverter<BotConfig.ERedeemingPreferences>(options));
        options.Converters.Add(new AsfEnumConverter<BotConfig.ERemoteCommunication>(options));
        options.Converters.Add(new AsfEnumConverter<BotConfig.ETradingPreferences>(options));
        options.Converters.Add(new AsfEnumConverter<EPersonaStateFlag>(options));
        options.Converters.Add(new AsfEnumConverter<EPersonaState>(options));
        options.Converters.Add(new AsfEnumConverter<ArchiCryptoHelper.ECryptoMethod>(options));
        options.Converters.Add(new AsfEnumConverter<ArchiCryptoHelper.EHashingMethod>(options));
        options.Converters.Add(new AsfEnumConverter<ArchiHandler.EUserInterfaceMode>(options));
        options.Converters.Add(new AsfEnumConverter<GlobalConfig.EOptimizationMode>(options));
        options.Converters.Add(new AsfEnumConverter<GlobalConfig.EUpdateChannel>(options));
        options.Converters.Add(new AsfEnumConverter<ProtocolTypes>(options));
        options.MakeReadOnly();
        return options;
    }

    static void ConfigureContract(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type == typeof(BotConfig))
        {
            foreach (var property in typeInfo.Properties)
            {
                property.ShouldSerialize = property.Name switch
                {
                    "AcceptGifts" => static (owner, _) => ((BotConfig)owner).ShouldSerializeAcceptGifts(),
                    "AutoSteamSaleEvent" => static (owner, _) => ((BotConfig)owner).ShouldSerializeAutoSteamSaleEvent(),
                    "BotBehaviour" => static (owner, _) => ((BotConfig)owner).ShouldSerializeBotBehaviour(),
                    "CompleteTypesToSend" => static (owner, _) => ((BotConfig)owner).ShouldSerializeCompleteTypesToSend(),
                    "CustomGamePlayedWhileFarming" => static (owner, _) => ((BotConfig)owner).ShouldSerializeCustomGamePlayedWhileFarming(),
                    "CustomGamePlayedWhileIdle" => static (owner, _) => ((BotConfig)owner).ShouldSerializeCustomGamePlayedWhileIdle(),
                    "Enabled" => static (owner, _) => ((BotConfig)owner).ShouldSerializeEnabled(),
                    "FarmingOrders" => static (owner, _) => ((BotConfig)owner).ShouldSerializeFarmingOrders(),
                    "FarmPriorityQueueOnly" => static (owner, _) => ((BotConfig)owner).ShouldSerializeFarmPriorityQueueOnly(),
                    "GamesPlayedWhileIdle" => static (owner, _) => ((BotConfig)owner).ShouldSerializeGamesPlayedWhileIdle(),
                    "HoursUntilCardDrops" => static (owner, _) => ((BotConfig)owner).ShouldSerializeHoursUntilCardDrops(),
                    "LootableTypes" => static (owner, _) => ((BotConfig)owner).ShouldSerializeLootableTypes(),
                    "MatchableTypes" => static (owner, _) => ((BotConfig)owner).ShouldSerializeMatchableTypes(),
                    "OnlineFlags" => static (owner, _) => ((BotConfig)owner).ShouldSerializeOnlineFlags(),
                    "OnlineStatus" => static (owner, _) => ((BotConfig)owner).ShouldSerializeOnlineStatus(),
                    "PasswordFormat" => static (owner, _) => ((BotConfig)owner).ShouldSerializePasswordFormat(),
                    "Paused" => static (owner, _) => ((BotConfig)owner).ShouldSerializePaused(),
                    "RedeemingPreferences" => static (owner, _) => ((BotConfig)owner).ShouldSerializeRedeemingPreferences(),
                    "RemoteCommunication" => static (owner, _) => ((BotConfig)owner).ShouldSerializeRemoteCommunication(),
                    "SendOnFarmingFinished" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSendOnFarmingFinished(),
                    "SendTradePeriod" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSendTradePeriod(),
                    "ShutdownOnFarmingFinished" => static (owner, _) => ((BotConfig)owner).ShouldSerializeShutdownOnFarmingFinished(),
                    "SkipRefundableGames" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSkipRefundableGames(),
                    "s_SteamMasterClanID" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSSteamMasterClanID(),
                    "SteamLogin" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSteamLogin(),
                    "SteamMasterClanID" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSteamMasterClanID(),
                    "SteamParentalCode" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSteamParentalCode(),
                    "SteamPassword" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSteamPassword(),
                    "SteamTradeToken" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSteamTradeToken(),
                    "SteamUserPermissions" => static (owner, _) => ((BotConfig)owner).ShouldSerializeSteamUserPermissions(),
                    "TradeCheckPeriod" => static (owner, _) => ((BotConfig)owner).ShouldSerializeTradeCheckPeriod(),
                    "TradingPreferences" => static (owner, _) => ((BotConfig)owner).ShouldSerializeTradingPreferences(),
                    "TransferableTypes" => static (owner, _) => ((BotConfig)owner).ShouldSerializeTransferableTypes(),
                    "UseLoginKeys" => static (owner, _) => ((BotConfig)owner).ShouldSerializeUseLoginKeys(),
                    "UserInterfaceMode" => static (owner, _) => ((BotConfig)owner).ShouldSerializeUserInterfaceMode(),
                    _ => null,
                };
            }
        }

        if (typeInfo.Type == typeof(GlobalConfig))
        {
            foreach (var property in typeInfo.Properties)
            {
                property.ShouldSerialize = property.Name switch
                {
                    "AutoRestart" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeAutoRestart(),
                    "Blacklist" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeBlacklist(),
                    "CommandPrefix" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeCommandPrefix(),
                    "ConfirmationsLimiterDelay" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeConfirmationsLimiterDelay(),
                    "ConnectionTimeout" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeConnectionTimeout(),
                    "CurrentCulture" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeCurrentCulture(),
                    "Debug" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeDebug(),
                    "FarmingDelay" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeFarmingDelay(),
                    "FilterBadBots" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeFilterBadBots(),
                    "GiftsLimiterDelay" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeGiftsLimiterDelay(),
                    "Headless" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeHeadless(),
                    "IdleFarmingPeriod" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeIdleFarmingPeriod(),
                    "InventoryLimiterDelay" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeInventoryLimiterDelay(),
                    "IPC" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeIPC(),
                    "IPCPassword" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeIPCPassword(),
                    "IPCPasswordFormat" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeIPCPasswordFormat(),
                    "LicenseID" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeLicenseID(),
                    "LoginLimiterDelay" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeLoginLimiterDelay(),
                    "MaxFarmingTime" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeMaxFarmingTime(),
                    "MaxTradeHoldDuration" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeMaxTradeHoldDuration(),
                    "MinFarmingDelayAfterBlock" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeMinFarmingDelayAfterBlock(),
                    "OptimizationMode" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeOptimizationMode(),
                    "s_SteamOwnerID" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeSSteamOwnerID(),
                    "SteamMessagePrefix" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeSteamMessagePrefix(),
                    "SteamOwnerID" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeSteamOwnerID(),
                    "SteamProtocols" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeSteamProtocols(),
                    "UpdateChannel" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeUpdateChannel(),
                    "UpdatePeriod" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeUpdatePeriod(),
                    "WebLimiterDelay" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeWebLimiterDelay(),
                    "WebProxyPassword" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeWebProxyPassword(),
                    "WebProxy" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeWebProxyText(),
                    "WebProxyUsername" => static (owner, _) => ((GlobalConfig)owner).ShouldSerializeWebProxyUsername(),
                    _ => null,
                };
            }
        }

        // Newtonsoft Required.Always also forbids null; DisallowNull permits omission.
        // JsonRequired supplies presence checks. Only the original non-null contracts reject null.
        foreach (var property in typeInfo.Properties)
        {
            bool disallowNull =
            (typeInfo.Type == typeof(BotConfig) && property.Name is "CompleteTypesToSend" or "FarmingOrders" or "GamesPlayedWhileIdle" or "LootableTypes" or "MatchableTypes" or "SteamUserPermissions" or "TransferableTypes" or "s_SteamMasterClanID") ||
            (typeInfo.Type == typeof(GlobalConfig) && property.Name is "Blacklist" or "s_SteamOwnerID") ||
            (typeInfo.Type == typeof(ASFResponse) && property.Name is "BuildVariant" or "GlobalConfig" or "Version") ||
            (typeInfo.Type == typeof(ASFEncryptRequest) && property.Name is "StringToEncrypt") ||
            (typeInfo.Type == typeof(ASFHashRequest) && property.Name is "StringToHash") ||
            (typeInfo.Type == typeof(ASFRequest) && property.Name is "GlobalConfig") ||
            (typeInfo.Type == typeof(BotGamesToRedeemInBackgroundRequest) && property.Name is "GamesToRedeemInBackground") ||
            (typeInfo.Type == typeof(BotInputRequest) && property.Name is "Value") ||
            (typeInfo.Type == typeof(BotRedeemRequest) && property.Name is "KeysToRedeem") ||
            (typeInfo.Type == typeof(BotRenameRequest) && property.Name is "NewName") ||
            (typeInfo.Type == typeof(BotRequest) && property.Name is "BotConfig") ||
            (typeInfo.Type == typeof(CommandRequest) && property.Name is "Command");
            if (disallowNull && property.Set is { } setter)
            {
                property.Set = (owner, value) =>
                {
                    if (value is null)
                        throw new JsonException($"{property.Name} must not be null.");
                    setter(owner, value);
                };
            }
        }
    }
}
