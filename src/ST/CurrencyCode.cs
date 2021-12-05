using System.Application;
using System.Application.Columns;
using System.Globalization;

namespace System.Application
{
    /// <summary>
    /// 货币类型
    /// </summary>
    public enum CurrencyCode : byte
    {
        /// <summary>
        /// ¥ 人民币
        /// </summary>
        CNY = 1,

        /// <summary>
        /// $ 美元
        /// </summary>
        USD = 2,
    }
}

namespace System.Application.Columns
{
    public interface ICurrencyCodeString
    {
        string CurrencyCode { get; }
    }
}

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class CurrencyCodeEnumExtensions
    {
        public static int GetLCID(this CurrencyCode currencyCode) => currencyCode switch
        {
            CurrencyCode.CNY => CultureInfoExtensions.LCID_zh_CN,
            CurrencyCode.USD => CultureInfoExtensions.LCID_en_US,
            _ => throw new ArgumentOutOfRangeException(nameof(currencyCode), currencyCode, null),
        };

        public static CultureInfo GetCultureInfo(this CurrencyCode currencyCode) => new(currencyCode.GetLCID());

        public static CurrencyCode GetCurrencyCode(this ICurrencyCodeString @interface, CurrencyCode @default = CurrencyCode.CNY)
            => Enum.TryParse(@interface.CurrencyCode, true, out CurrencyCode currencyCode) ? currencyCode : @default;
    }
}
