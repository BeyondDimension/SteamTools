// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.Steam.Data;

public static class Asset
{
    public enum ERarity : byte
    {
        Unknown,
        Common,
        Uncommon,
        Rare,
    }

    public enum EType : byte
    {
        Unknown,
        BoosterPack,
        Emoticon,
        FoilTradingCard,
        ProfileBackground,
        TradingCard,
        SteamGems,
        SaleItem,
        Consumable,
        ProfileModifier,
        Sticker,
        ChatEffect,
        MiniProfileBackground,
        AvatarProfileFrame,
        AnimatedAvatar,
        KeyboardSkin,
        StartupVideo,
    }
}