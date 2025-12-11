using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.Core;

public static class ASF
{
    [PublicAPI]
    public static readonly ArchiLogger ArchiLogger = new(SharedInfo.ASF);

    public static bool IsReady { get; set; }

    public enum EUserInputType : byte
    {
        None,
        Login,
        Password,
        SteamGuard,
        SteamParentalCode,
        TwoFactorAuthentication,
        Cryptkey,
        DeviceConfirmation,
    }

    internal enum EFileType : byte
    {
        Config,
        Database,
    }
}