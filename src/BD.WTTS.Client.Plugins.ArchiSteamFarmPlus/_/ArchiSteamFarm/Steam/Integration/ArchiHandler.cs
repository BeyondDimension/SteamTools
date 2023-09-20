// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.Steam.Integration;

public static class ArchiHandler
{
    internal const byte MaxGamesPlayedConcurrently = 32; // This is limit introduced by Steam Network

    public enum EUserInterfaceMode : byte
    {
        Default = 0,
        BigPicture = 1,
        Mobile = 2,
    }
}