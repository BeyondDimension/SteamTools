// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class StringExtensions
    {
        public static string TrimStart(this string s, string trimString)
        {
            if (trimString.StartsWith(trimString))
            {
                return s[trimString.Length..];
            }
            else
            {
                return s;
            }
        }

        public static string TrimEnd(this string s, string trimString)
        {
            if (trimString.EndsWith(trimString))
            {
                return s.Substring(0, s.Length - trimString.Length);
            }
            else
            {
                return s;
            }
        }

        public static string TrimStart(this string s, string trimString, StringComparison comparisonType)
        {
            if (trimString.StartsWith(trimString, comparisonType))
            {
                return s[trimString.Length..];
            }
            else
            {
                return s;
            }
        }

        public static string TrimEnd(this string s, string trimString, StringComparison comparisonType)
        {
            if (trimString.EndsWith(trimString, comparisonType))
            {
                return s.Substring(0, s.Length - trimString.Length);
            }
            else
            {
                return s;
            }
        }
    }
}