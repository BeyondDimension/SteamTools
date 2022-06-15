using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace System;

public static class CacheExtensions
{
    public static bool Get<T>(this IMemoryCache _cache, string key, out T? value) where T : class
    {
        return _cache.TryGetValue(key, out value);
    }

    public static void Set<T>(this IMemoryCache _cache, string key, T value, int minutes) where T : class
    {
        _cache.Set(key, value, TimeSpan.FromMinutes(minutes));
    }

    public static bool Get<T>(this IMemoryCache _cache, Guid key, out T? value) where T : class
    {
        return _cache.TryGetValue(key, out value);
    }

    public static void Set<T>(this IMemoryCache _cache, Guid key, T value, int minutes) where T : class
    {
        _cache.Set(key, value, TimeSpan.FromMinutes(minutes));
    }
}
