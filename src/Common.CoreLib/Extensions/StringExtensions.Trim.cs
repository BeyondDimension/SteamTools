// ReSharper disable once CheckNamespace
namespace System;

public static partial class StringExtensions
{
    public static string TrimStart(this string s, string value)
    {
        if (s.StartsWith(value))
        {
            return s[value.Length..];
        }
        else
        {
            return s;
        }
    }

    public static string TrimEnd(this string s, string value)
    {
        if (s.EndsWith(value))
        {
            return s[..^value.Length];
        }
        else
        {
            return s;
        }
    }

    public static string TrimStart(this string s, string value, StringComparison comparisonType)
    {
        if (s.StartsWith(value, comparisonType))
        {
            return s[value.Length..];
        }
        else
        {
            return s;
        }
    }

    public static string TrimEnd(this string s, string value, StringComparison comparisonType)
    {
        if (s.EndsWith(value, comparisonType))
        {
            return s[..^value.Length];
        }
        else
        {
            return s;
        }
    }
}