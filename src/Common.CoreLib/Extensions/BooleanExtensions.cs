using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

public static class BooleanExtensions
{
    public const string TrueLowerString = "true";
    public const string FalseLowerString = "false";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToLowerString(this bool value) => value ? TrueLowerString : FalseLowerString;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToLowerString(this bool? value) => value.HasValue ? value.Value.ToLowerString() : String.Empty;
}
