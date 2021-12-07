using SteamKit2;
using System.Application;
using System.Globalization;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class CurrencyCodeEnumExtensions
    {
        public static ECurrencyCode Convert(this CurrencyCode currencyCode)
        {
            var value = currencyCode.ToString();
            if (Enum.TryParse<ECurrencyCode>(value, out var result)) return result;
            throw new ArgumentOutOfRangeException(nameof(currencyCode), currencyCode, null);
        }

        public static CurrencyCode Convert(this ECurrencyCode eCurrencyCode)
        {
            var value = eCurrencyCode.ToString();
            if (Enum.TryParse<CurrencyCode>(value, out var result)) return result;
            throw new ArgumentOutOfRangeException(nameof(eCurrencyCode), eCurrencyCode, null);
        }

        public static CultureInfo? GetCultureInfo(this ECurrencyCode eCurrencyCode)
        {
            if (eCurrencyCode != ECurrencyCode.Invalid)
            {
                var cultureInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                  .FirstOrDefault(culture => new RegionInfo(culture.LCID).ISOCurrencySymbol == eCurrencyCode.ToString());
                return cultureInfo;
            }
            return null;
        }
    }
}
