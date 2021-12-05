using System.Application.Services;
using Xamarin.Essentials;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    /// <inheritdoc cref="Preferences"/>
    public static class Preferences2
    {
        // https://github.com/xamarin/Essentials/blob/1.7.0/Xamarin.Essentials/Preferences/Preferences.shared.cs

        // overloads

        /// <inheritdoc cref="Preferences.ContainsKey(string)"/>
        public static bool ContainsKey(string key) =>
            ContainsKey(key, null);

        /// <inheritdoc cref="Preferences.Remove(string)"/>
        public static void Remove(string key) =>
            Remove(key, null);

        /// <inheritdoc cref="Preferences.Clear"/>
        public static void Clear() =>
            Clear(null);

        /// <inheritdoc cref="Preferences.Get(string, string)"/>
        public static string? Get(string key, string? defaultValue) =>
            Get(key, defaultValue, null);

        /// <inheritdoc cref="Preferences.Get(string, bool)"/>
        public static bool Get(string key, bool defaultValue) =>
            Get(key, defaultValue, null);

        /// <inheritdoc cref="Preferences.Get(string, int)"/>
        public static int Get(string key, int defaultValue) =>
            Get(key, defaultValue, null);

        /// <inheritdoc cref="Preferences.Get(string, double)"/>
        public static double Get(string key, double defaultValue) =>
            Get(key, defaultValue, null);

        /// <inheritdoc cref="Preferences.Get(string, float)"/>
        public static float Get(string key, float defaultValue) =>
            Get(key, defaultValue, null);

        /// <inheritdoc cref="Preferences.Get(string, long)"/>
        public static long Get(string key, long defaultValue) =>
            Get(key, defaultValue, null);

        /// <inheritdoc cref="Preferences.Set(string, string)"/>
        public static void Set(string key, string value) =>
            Set(key, value, null);

        /// <inheritdoc cref="Preferences.Set(string, bool)"/>
        public static void Set(string key, bool value) =>
            Set(key, value, null);

        /// <inheritdoc cref="Preferences.Set(string, int)"/>
        public static void Set(string key, int value) =>
            Set(key, value, null);

        /// <inheritdoc cref="Preferences.Set(string, double)"/>
        public static void Set(string key, double value) =>
            Set(key, value, null);

        /// <inheritdoc cref="Preferences.Set(string, float)"/>
        public static void Set(string key, float value) =>
            Set(key, value, null);

        /// <inheritdoc cref="Preferences.Set(string, long)"/>
        public static void Set(string key, long value) =>
            Set(key, value, null);

        // shared -> platform

        /// <inheritdoc cref="Preferences.ContainsKey(string, string)"/>
        public static bool ContainsKey(string key, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                return Preferences.ContainsKey(key, sharedName);
            }
            else
            {
                return IPreferencesPlatformService.Instance.PlatformContainsKey(key, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Remove(string, string)"/>
        public static void Remove(string key, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                Preferences.Remove(key, sharedName);
            }
            else
            {
                IPreferencesPlatformService.Instance.PlatformRemove(key, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Clear(string)"/>
        public static void Clear(string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                Preferences.Clear(sharedName);
            }
            else
            {
                IPreferencesPlatformService.Instance.PlatformClear(sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, string, string)"/>
        public static string? Get(string key, string? defaultValue, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                return Preferences.Get(key, defaultValue, sharedName);
            }
            else
            {
                return IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, bool, string)"/>
        public static bool Get(string key, bool defaultValue, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                return Preferences.Get(key, defaultValue, sharedName);
            }
            else
            {
                return IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, int, string)"/>
        public static int Get(string key, int defaultValue, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                return Preferences.Get(key, defaultValue, sharedName);
            }
            else
            {
                return IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, double, string)"/>
        public static double Get(string key, double defaultValue, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                return Preferences.Get(key, defaultValue, sharedName);
            }
            else
            {
                return IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, float, string)"/>
        public static float Get(string key, float defaultValue, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                return Preferences.Get(key, defaultValue, sharedName);
            }
            else
            {
                return IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, long, string)"/>
        public static long Get(string key, long defaultValue, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                return Preferences.Get(key, defaultValue, sharedName);
            }
            else
            {
                return IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, string, string)"/>
        public static void Set(string key, string value, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                Preferences.Set(key, value, sharedName);
            }
            else
            {
                IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, bool, string)"/>
        public static void Set(string key, bool value, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                Preferences.Set(key, value, sharedName);
            }
            else
            {
                IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, int, string)"/>
        public static void Set(string key, int value, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                Preferences.Set(key, value, sharedName);
            }
            else
            {
                IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, double, string)"/>
        public static void Set(string key, double value, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                Preferences.Set(key, value, sharedName);
            }
            else
            {
                IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, float, string)"/>
        public static void Set(string key, float value, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                Preferences.Set(key, value, sharedName);
            }
            else
            {
                IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);
            }
        }

        /// <inheritdoc cref="Preferences.Get(string, long, string)"/>
        public static void Set(string key, long value, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                Preferences.Set(key, value, sharedName);
            }
            else
            {
                IPreferencesPlatformService.Instance.PlatformSet(key, value, sharedName);
            }
        }

        // DateTime

        /// <inheritdoc cref="Preferences.Get(string, DateTime)"/>
        public static DateTime Get(string key, DateTime defaultValue) =>
            Get(key, defaultValue, null);

        /// <inheritdoc cref="Preferences.Set(string, DateTime)"/>
        public static void Set(string key, DateTime value) =>
            Set(key, value, null);

        /// <inheritdoc cref="Preferences.Get(string, DateTime, string)"/>
        public static DateTime Get(string key, DateTime defaultValue, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                return Preferences.Get(key, defaultValue, sharedName);
            }
            else
            {
                return DateTime.FromBinary(IPreferencesPlatformService.Instance.PlatformGet(key, defaultValue.ToBinary(), sharedName));
            }
        }

        /// <inheritdoc cref="Preferences.Set(string, DateTime, string)"/>
        public static void Set(string key, DateTime value, string? sharedName)
        {
            if (Essentials.IsSupported)
            {
                Preferences.Set(key, value, sharedName);
            }
            else
            {
                IPreferencesPlatformService.Instance.PlatformSet(key, value.ToBinary(), sharedName);
            }
        }
    }
}
