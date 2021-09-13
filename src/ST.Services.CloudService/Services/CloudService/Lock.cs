using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public class Lock : ILock
    {
        private static ConcurrentDictionary<Guid, DateTimeOffset?> _LockUserCache = new ConcurrentDictionary<Guid, DateTimeOffset?>();
        public bool LockUser(Guid id)
        {
            var state = _LockUserCache.TryGetValue(id, out DateTimeOffset? overTime);
            if (state)
            {
                if (overTime.HasValue && overTime.Value <= DateTimeOffset.Now)
                {
                    _LockUserCache.TryUpdate(id, DateTimeOffset.Now.AddMinutes(1), overTime);
                    return true;
                }
                return false;
            }
            _LockUserCache.TryAdd(id, DateTimeOffset.Now.AddMinutes(1));
            return true;
        }

        public bool RemoveLockUser(Guid id)
        {
            return _LockUserCache.TryRemove(id, out _);
        }
    }
}
