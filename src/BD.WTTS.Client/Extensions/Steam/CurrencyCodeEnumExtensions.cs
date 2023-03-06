#if WINDOWS || MACCATALYST || MACOS || LINUX

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

public static partial class CurrencyCodeEnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ECurrencyCode Convert(this CurrencyCode currencyCode)
    {
        var value = currencyCode.ToString();
        if (Enum.TryParse<ECurrencyCode>(value, out var result)) return result;
        throw new ArgumentOutOfRangeException(nameof(currencyCode), currencyCode, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CurrencyCode Convert(this ECurrencyCode eCurrencyCode)
    {
        var value = eCurrencyCode.ToString();
        if (Enum.TryParse<CurrencyCode>(value, out var result)) return result;
        throw new ArgumentOutOfRangeException(nameof(eCurrencyCode), eCurrencyCode, null);
    }
}

#endif