using System;
using System.Globalization;

namespace Titanium.Web.Proxy.Extensions
{
    internal static class StringExtensions
    {
        internal static bool EqualsIgnoreCase(this string str, string? value)
        {
            return str.Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        internal static bool EqualsIgnoreCase(this ReadOnlySpan<char> str, ReadOnlySpan<char> value)
        {
            return str.Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        internal static bool ContainsIgnoreCase(this string str, string? value)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(str, value, CompareOptions.IgnoreCase) >= 0;
        }

        internal static int IndexOfIgnoreCase(this string str, string? value)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(str, value, CompareOptions.IgnoreCase);
        }
    }
}
