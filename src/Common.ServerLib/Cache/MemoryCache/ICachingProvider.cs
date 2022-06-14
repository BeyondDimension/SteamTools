using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Cache
{
    /// <summary>
    /// 简单的缓存接口，只有查询和添加，以后会进行扩展
    /// </summary>
    public interface ICachingProvider
    {
        bool Get<T>(string cacheKey, out T? obj) where T : class;

        void Set<T>(string cacheKey, T cacheValue, int timeSpan) where T : class;

        bool Get<T>(Guid cacheKey, out T? obj) where T : class;

        void Set<T>(Guid cacheKey, T cacheValue, int timeSpan) where T : class;
    }
}
