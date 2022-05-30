using System.Application.Services;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Application;

/// <inheritdoc cref="Preferences"/>
public static class Preferences2
{
    // https://github.com/xamarin/Essentials/blob/1.7.0/Xamarin.Essentials/Preferences/Preferences.shared.cs

    // overloads

    /// <inheritdoc cref="Preferences.ContainsKey(string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsKey(string key) =>
        ContainsKey(key, null);

    /// <inheritdoc cref="Preferences.Remove(string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Remove(string key) =>
        Remove(key, null);

    /// <inheritdoc cref="Preferences.Clear"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear() =>
        Clear(null);

    /// <inheritdoc cref="Preferences.Get(string, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? Get(string key, string? defaultValue) =>
        Get(key, defaultValue, null);

    /// <inheritdoc cref="Preferences.Get(string, bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Get(string key, bool defaultValue) =>
        Get(key, defaultValue, null);

    /// <inheritdoc cref="Preferences.Get(string, int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Get(string key, int defaultValue) =>
        Get(key, defaultValue, null);

    /// <inheritdoc cref="Preferences.Get(string, double)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Get(string key, double defaultValue) =>
        Get(key, defaultValue, null);

    /// <inheritdoc cref="Preferences.Get(string, float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Get(string key, float defaultValue) =>
        Get(key, defaultValue, null);

    /// <inheritdoc cref="Preferences.Get(string, long)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Get(string key, long defaultValue) =>
        Get(key, defaultValue, null);

    /// <inheritdoc cref="Preferences.Set(string, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, string value) =>
        Set(key, value, null);

    /// <inheritdoc cref="Preferences.Set(string, bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, bool value) =>
        Set(key, value, null);

    /// <inheritdoc cref="Preferences.Set(string, int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, int value) =>
        Set(key, value, null);

    /// <inheritdoc cref="Preferences.Set(string, double)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, double value) =>
        Set(key, value, null);

    /// <inheritdoc cref="Preferences.Set(string, float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, float value) =>
        Set(key, value, null);

    /// <inheritdoc cref="Preferences.Set(string, long)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, long value) =>
        Set(key, value, null);

    // shared -> platform

    /// <inheritdoc cref="Preferences.ContainsKey(string, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsKey(string key, string? sharedName) => IPreferencesPlatformService.Instance.PlatformContainsKey(key, sharedName);

    /// <inheritdoc cref="Preferences.Remove(string, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Remove(string key, string? sharedName) => IPreferencesPlatformService.Instance.PlatformRemove(key, sharedName);

    /// <inheritdoc cref="Preferences.Clear(string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear(string? sharedName) => IPreferencesPlatformService.Instance.PlatformClear(sharedName);

    /// <inheritdoc cref="Preferences.Get(string, string, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? Get(string key, string? defaultValue, string? sharedName) => IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, bool, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Get(string key, bool defaultValue, string? sharedName) => IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, int, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Get(string key, int defaultValue, string? sharedName) => IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, double, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Get(string key, double defaultValue, string? sharedName) => IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, float, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Get(string key, float defaultValue, string? sharedName) => IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, long, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Get(string key, long defaultValue, string? sharedName) => IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, string, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, string value, string? sharedName) => IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, bool, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, bool value, string? sharedName) => IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, int, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, int value, string? sharedName) => IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, double, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, double value, string? sharedName) => IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, float, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, float value, string? sharedName) => IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);

    /// <inheritdoc cref="Preferences.Get(string, long, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, long value, string? sharedName) => IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);

    // DateTime

    /// <inheritdoc cref="Preferences.Get(string, DateTime)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime Get(string key, DateTime defaultValue) =>
        Get(key, defaultValue, null);

    /// <inheritdoc cref="Preferences.Set(string, DateTime)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, DateTime value) =>
        Set(key, value, null);

    /// <inheritdoc cref="Preferences.Get(string, DateTime, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime Get(string key, DateTime defaultValue, string? sharedName) => DateTime.FromBinary(IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue.ToBinary(), sharedName));

    /// <inheritdoc cref="Preferences.Set(string, DateTime, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(string key, DateTime value, string? sharedName) => IPreferencesPlatformService.Instance.PlatformSet(key, value.ToBinary(), sharedName);
}
