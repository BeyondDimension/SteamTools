using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Cache
{
    /// <summary>
    /// 实例化缓存接口ICaching
    /// </summary>
    public class MemoryCaching : ICachingProvider
    {
        private readonly IMemoryCache _cache;

        public MemoryCaching(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool Get<T>(string cacheKey, out T? obj) where T : class
        {
            return _cache.TryGetValue(cacheKey, out obj);
        }

        public void Set<T>(string cacheKey, T cacheValue, int timeSpan) where T : class
        {
            _cache.Set(cacheKey, cacheValue, TimeSpan.FromMinutes(timeSpan));
        }

        public bool Get<T>(Guid cacheKey, out T? obj) where T : class
        {
            return _cache.TryGetValue(cacheKey, out obj);
        }

        public void Set<T>(Guid cacheKey, T cacheValue, int timeSpan) where T : class
        {
            _cache.Set(cacheKey, cacheValue, TimeSpan.FromMinutes(timeSpan));
        }
    }
}
