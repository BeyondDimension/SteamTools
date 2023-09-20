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
using ArchiSteamFarm.Steam.Integration;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.Storage;

[SuppressMessage("ReSharper", "ClassCannotBeInstantiated")]
public sealed class GlobalConfig
{
    [PublicAPI]
    public const bool DefaultAutoRestart = true;

    [PublicAPI]
    public const string? DefaultCommandPrefix = "!";

    [PublicAPI]
    public const byte DefaultConfirmationsLimiterDelay = 10;

    [PublicAPI]
    public const byte DefaultConnectionTimeout = 90;

    [PublicAPI]
    public const string? DefaultCurrentCulture = null;

    [PublicAPI]
    public const bool DefaultDebug = false;

    [PublicAPI]
    public const byte DefaultFarmingDelay = 15;

    [PublicAPI]
    public const bool DefaultFilterBadBots = true;

    [PublicAPI]
    public const byte DefaultGiftsLimiterDelay = 1;

    [PublicAPI]
    public const bool DefaultHeadless = false;

    [PublicAPI]
    public const byte DefaultIdleFarmingPeriod = 8;

    [PublicAPI]
    public const byte DefaultInventoryLimiterDelay = 4;

    [PublicAPI]
    public const bool DefaultIPC = true;

    [PublicAPI]
    public const string? DefaultIPCPassword = null;

    [PublicAPI]
    public const ArchiCryptoHelper.EHashingMethod DefaultIPCPasswordFormat = ArchiCryptoHelper.EHashingMethod.PlainText;

    [PublicAPI]
    public const byte DefaultLoginLimiterDelay = 10;

    [PublicAPI]
    public const byte DefaultMaxFarmingTime = 10;

    [PublicAPI]
    public const byte DefaultMaxTradeHoldDuration = 15;

    [PublicAPI]
    public const byte DefaultMinFarmingDelayAfterBlock = 60;

    [PublicAPI]
    public const EOptimizationMode DefaultOptimizationMode = EOptimizationMode.MaxPerformance;

    [PublicAPI]
    public const string? DefaultSteamMessagePrefix = "/me ";

    [PublicAPI]
    public const ulong DefaultSteamOwnerID = 0;

    [PublicAPI]
    public const ProtocolTypes DefaultSteamProtocols = ProtocolTypes.All;

    [PublicAPI]
    public const EUpdateChannel DefaultUpdateChannel = EUpdateChannel.Stable;

    [PublicAPI]
    public const byte DefaultUpdatePeriod = 24;

    [PublicAPI]
    public const ushort DefaultWebLimiterDelay = 300;

    [PublicAPI]
    public const string? DefaultWebProxyPassword = null;

    [PublicAPI]
    public const string? DefaultWebProxyText = null;

    [PublicAPI]
    public const string? DefaultWebProxyUsername = null;

    [PublicAPI]
    public static readonly ImmutableHashSet<uint> DefaultBlacklist = ImmutableHashSet<uint>.Empty;

    [PublicAPI]
    public static readonly Guid? DefaultLicenseID;

    private static readonly ImmutableHashSet<string> ForbiddenIPCPasswordPhrases = ImmutableHashSet.Create(StringComparer.InvariantCultureIgnoreCase, "ipc", "api", "gui", "asf-ui", "asf-gui");

    [JsonIgnore]
    [PublicAPI]
    public WebProxy? WebProxy
    {
        get
        {
            if (BackingWebProxy != null)
            {
                return BackingWebProxy;
            }

            if (string.IsNullOrEmpty(WebProxyText))
            {
                return null;
            }

            Uri uri;

            try
            {
                uri = new Uri(WebProxyText!);
            }
            catch (UriFormatException e)
            {
                ASF.ArchiLogger.LogGenericException(e);

                return null;
            }

            WebProxy proxy = new()
            {
                Address = uri,
                BypassProxyOnLocal = true,
            };

            if (!string.IsNullOrEmpty(WebProxyUsername) || !string.IsNullOrEmpty(WebProxyPassword))
            {
                NetworkCredential credentials = new();

                if (!string.IsNullOrEmpty(WebProxyUsername))
                {
                    credentials.UserName = WebProxyUsername;
                }

                if (!string.IsNullOrEmpty(WebProxyPassword))
                {
                    credentials.Password = WebProxyPassword;
                }

                proxy.Credentials = credentials;
            }

            BackingWebProxy = proxy;

            return proxy;
        }
    }

    [JsonProperty(Required = Required.DisallowNull)]
    public bool AutoRestart { get; private set; } = DefaultAutoRestart;

    [JsonProperty(Required = Required.DisallowNull)]
    [SwaggerItemsMinMax(MinimumUint = 1, MaximumUint = uint.MaxValue)]
    public ImmutableHashSet<uint> Blacklist { get; private set; } = DefaultBlacklist;

    [JsonProperty]
    public string? CommandPrefix { get; private set; } = DefaultCommandPrefix;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte ConfirmationsLimiterDelay { get; private set; } = DefaultConfirmationsLimiterDelay;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(1, byte.MaxValue)]
    public byte ConnectionTimeout { get; private set; } = DefaultConnectionTimeout;

    [JsonProperty]
    public string? CurrentCulture { get; private set; } = DefaultCurrentCulture;

    [JsonProperty(Required = Required.DisallowNull)]
    public bool Debug { get; private set; } = DefaultDebug;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(1, byte.MaxValue)]
    public byte FarmingDelay { get; private set; } = DefaultFarmingDelay;

    [JsonProperty(Required = Required.DisallowNull)]
    public bool FilterBadBots { get; private set; } = DefaultFilterBadBots;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte GiftsLimiterDelay { get; private set; } = DefaultGiftsLimiterDelay;

    [JsonProperty(Required = Required.DisallowNull)]
    public bool Headless { get; private set; } = DefaultHeadless;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte IdleFarmingPeriod { get; private set; } = DefaultIdleFarmingPeriod;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte InventoryLimiterDelay { get; private set; } = DefaultInventoryLimiterDelay;

    [JsonProperty(Required = Required.DisallowNull)]
    public bool IPC { get; private set; } = DefaultIPC;

    [JsonProperty]
    public string? IPCPassword
    {
        get => BackingIPCPassword;

        internal set
        {
            IsIPCPasswordSet = true;
            BackingIPCPassword = value;
        }
    }

    [JsonProperty(Required = Required.DisallowNull)]
    public ArchiCryptoHelper.EHashingMethod IPCPasswordFormat { get; private set; } = DefaultIPCPasswordFormat;

    [JsonProperty]
    public Guid? LicenseID { get; private set; } = DefaultLicenseID;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte LoginLimiterDelay { get; private set; } = DefaultLoginLimiterDelay;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(1, byte.MaxValue)]
    public byte MaxFarmingTime { get; private set; } = DefaultMaxFarmingTime;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte MaxTradeHoldDuration { get; private set; } = DefaultMaxTradeHoldDuration;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte MinFarmingDelayAfterBlock { get; private set; } = DefaultMinFarmingDelayAfterBlock;

    [JsonProperty(Required = Required.DisallowNull)]
    public EOptimizationMode OptimizationMode { get; private set; } = DefaultOptimizationMode;

    [JsonProperty]
    [MaxLength(SteamChatMessage.MaxMessagePrefixBytes / SteamChatMessage.ReservedEscapeMessageBytes)]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "This is optional, supportive attribute, we don't care if it gets trimmed or not")]
    public string? SteamMessagePrefix { get; private set; } = DefaultSteamMessagePrefix;

    [JsonProperty(Required = Required.DisallowNull)]
    [SwaggerSteamIdentifier]
    [SwaggerValidValues(ValidIntValues = new[] { 0 })]
    public ulong SteamOwnerID { get; private set; } = DefaultSteamOwnerID;

    [JsonProperty(Required = Required.DisallowNull)]
    public ProtocolTypes SteamProtocols { get; private set; } = DefaultSteamProtocols;

    [JsonProperty(Required = Required.DisallowNull)]
    public EUpdateChannel UpdateChannel { get; private set; } = DefaultUpdateChannel;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(byte.MinValue, byte.MaxValue)]
    public byte UpdatePeriod { get; private set; } = DefaultUpdatePeriod;

    [JsonProperty(Required = Required.DisallowNull)]
    [Range(ushort.MinValue, ushort.MaxValue)]
    public ushort WebLimiterDelay { get; private set; } = DefaultWebLimiterDelay;

    [JsonProperty(nameof(WebProxy))]
    public string? WebProxyText { get; private set; } = DefaultWebProxyText;

    [JsonProperty]
    public string? WebProxyUsername { get; private set; } = DefaultWebProxyUsername;

    [JsonExtensionData]
    internal Dictionary<string, JToken>? AdditionalProperties
    {
        get;
        [UsedImplicitly]
        set;
    }

    internal bool IsIPCPasswordSet { get; private set; }

    internal bool IsWebProxyPasswordSet { get; private set; }

    internal bool Saving { get; set; }

    [JsonProperty]
    internal string? WebProxyPassword
    {
        get => BackingWebProxyPassword;

        set
        {
            IsWebProxyPasswordSet = true;
            BackingWebProxyPassword = value;
        }
    }

    private string? BackingIPCPassword = DefaultIPCPassword;
    private WebProxy? BackingWebProxy;
    private string? BackingWebProxyPassword = DefaultWebProxyPassword;

    [JsonProperty($"{SharedInfo.UlongCompatibilityStringPrefix}{nameof(SteamOwnerID)}", Required = Required.DisallowNull)]
    private string SSteamOwnerID
    {
        get => SteamOwnerID.ToString(CultureInfo.InvariantCulture);

        set
        {
            if (string.IsNullOrEmpty(value) || !ulong.TryParse(value, out ulong result))
            {
                ASF.ArchiLogger.LogGenericError(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsInvalid, nameof(SSteamOwnerID)));

                return;
            }

            SteamOwnerID = result;
        }
    }

    [JsonConstructor]
    internal GlobalConfig() { }

    [UsedImplicitly]
    public bool ShouldSerializeAutoRestart() => !Saving || (AutoRestart != DefaultAutoRestart);

    [UsedImplicitly]
    public bool ShouldSerializeBlacklist() => !Saving || ((Blacklist != DefaultBlacklist) && !Blacklist.SetEquals(DefaultBlacklist));

    [UsedImplicitly]
    public bool ShouldSerializeCommandPrefix() => !Saving || (CommandPrefix != DefaultCommandPrefix);

    [UsedImplicitly]
    public bool ShouldSerializeConfirmationsLimiterDelay() => !Saving || (ConfirmationsLimiterDelay != DefaultConfirmationsLimiterDelay);

    [UsedImplicitly]
    public bool ShouldSerializeConnectionTimeout() => !Saving || (ConnectionTimeout != DefaultConnectionTimeout);

    [UsedImplicitly]
    public bool ShouldSerializeCurrentCulture() => !Saving || (CurrentCulture != DefaultCurrentCulture);

    [UsedImplicitly]
    public bool ShouldSerializeDebug() => !Saving || (Debug != DefaultDebug);

    [UsedImplicitly]
    public bool ShouldSerializeFarmingDelay() => !Saving || (FarmingDelay != DefaultFarmingDelay);

    [UsedImplicitly]
    public bool ShouldSerializeFilterBadBots() => !Saving || (FilterBadBots != DefaultFilterBadBots);

    [UsedImplicitly]
    public bool ShouldSerializeGiftsLimiterDelay() => !Saving || (GiftsLimiterDelay != DefaultGiftsLimiterDelay);

    [UsedImplicitly]
    public bool ShouldSerializeHeadless() => !Saving || (Headless != DefaultHeadless);

    [UsedImplicitly]
    public bool ShouldSerializeIdleFarmingPeriod() => !Saving || (IdleFarmingPeriod != DefaultIdleFarmingPeriod);

    [UsedImplicitly]
    public bool ShouldSerializeInventoryLimiterDelay() => !Saving || (InventoryLimiterDelay != DefaultInventoryLimiterDelay);

    [UsedImplicitly]
    public bool ShouldSerializeIPC() => !Saving || (IPC != DefaultIPC);

    [UsedImplicitly]
    public bool ShouldSerializeIPCPassword() => Saving && IsIPCPasswordSet && (IPCPassword != DefaultIPCPassword);

    [UsedImplicitly]
    public bool ShouldSerializeIPCPasswordFormat() => !Saving || (IPCPasswordFormat != DefaultIPCPasswordFormat);

    [UsedImplicitly]
    public bool ShouldSerializeLicenseID() => !Saving || ((LicenseID != DefaultLicenseID) && (LicenseID != Guid.Empty));

    [UsedImplicitly]
    public bool ShouldSerializeLoginLimiterDelay() => !Saving || (LoginLimiterDelay != DefaultLoginLimiterDelay);

    [UsedImplicitly]
    public bool ShouldSerializeMaxFarmingTime() => !Saving || (MaxFarmingTime != DefaultMaxFarmingTime);

    [UsedImplicitly]
    public bool ShouldSerializeMaxTradeHoldDuration() => !Saving || (MaxTradeHoldDuration != DefaultMaxTradeHoldDuration);

    [UsedImplicitly]
    public bool ShouldSerializeMinFarmingDelayAfterBlock() => !Saving || (MinFarmingDelayAfterBlock != DefaultMinFarmingDelayAfterBlock);

    [UsedImplicitly]
    public bool ShouldSerializeOptimizationMode() => !Saving || (OptimizationMode != DefaultOptimizationMode);

    [UsedImplicitly]
    public bool ShouldSerializeSSteamOwnerID() => !Saving;

    [UsedImplicitly]
    public bool ShouldSerializeSteamMessagePrefix() => !Saving || (SteamMessagePrefix != DefaultSteamMessagePrefix);

    [UsedImplicitly]
    public bool ShouldSerializeSteamOwnerID() => !Saving || (SteamOwnerID != DefaultSteamOwnerID);

    [UsedImplicitly]
    public bool ShouldSerializeSteamProtocols() => !Saving || (SteamProtocols != DefaultSteamProtocols);

    [UsedImplicitly]
    public bool ShouldSerializeUpdateChannel() => !Saving || (UpdateChannel != DefaultUpdateChannel);

    [UsedImplicitly]
    public bool ShouldSerializeUpdatePeriod() => !Saving || (UpdatePeriod != DefaultUpdatePeriod);

    [UsedImplicitly]
    public bool ShouldSerializeWebLimiterDelay() => !Saving || (WebLimiterDelay != DefaultWebLimiterDelay);

    [UsedImplicitly]
    public bool ShouldSerializeWebProxyPassword() => Saving && IsWebProxyPasswordSet && (WebProxyPassword != DefaultWebProxyPassword);

    [UsedImplicitly]
    public bool ShouldSerializeWebProxyText() => !Saving || (WebProxyText != DefaultWebProxyText);

    [UsedImplicitly]
    public bool ShouldSerializeWebProxyUsername() => !Saving || (WebProxyUsername != DefaultWebProxyUsername);

    [PublicAPI]
    public enum EOptimizationMode : byte
    {
        MaxPerformance,
        MinMemoryUsage,
    }

    [PublicAPI]
    public enum EUpdateChannel : byte
    {
        None,
        Stable,
        Experimental,
    }
}
