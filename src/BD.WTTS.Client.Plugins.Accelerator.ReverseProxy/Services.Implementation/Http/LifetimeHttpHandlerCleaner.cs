// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/LifetimeHttpHandlerCleaner.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// <see cref="LifetimeHttpHandler"/> 清理器
/// </summary>
sealed class LifetimeHttpHandlerCleaner
{
    /// <summary>
    /// 当前监视生命周期的记录的数量
    /// </summary>
    int trackingEntryCount = 0;

    /// <summary>
    /// 监视生命周期的记录队列
    /// </summary>
    private readonly ConcurrentQueue<TrackingEntry> trackingEntries = new();

    /// <summary>
    /// 获取或设置清理的时间间隔
    /// 默认 10s
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromSeconds(10d);

    /// <summary>
    /// 添加要清除的 <see cref="LifetimeHttpHandler"/>
    /// </summary>
    /// <param name="handler"></param>
    public void Add(LifetimeHttpHandler handler)
    {
        var entry = new TrackingEntry(handler);
        trackingEntries.Enqueue(entry);

        // 从 0 变为 1，要启动清理作业
        if (Interlocked.Increment(ref trackingEntryCount) == 1)
        {
            StartCleanup();
        }
    }

    /// <summary>
    /// 启动清理作业
    /// </summary>
    async void StartCleanup()
    {
        await Task.Yield();
        while (Cleanup() == false)
        {
            await Task.Delay(CleanupInterval);
        }
    }

    /// <summary>
    /// 清理失效的拦截器
    /// 返回是否完全清理
    /// </summary>
    /// <returns></returns>
    bool Cleanup()
    {
        var cleanCount = trackingEntries.Count;
        for (var i = 0; i < cleanCount; i++)
        {
            trackingEntries.TryDequeue(out var entry);
            Debug.Assert(entry != null);

            if (entry.CanDispose == false)
            {
                trackingEntries.Enqueue(entry);
                continue;
            }

            entry.Dispose();
            if (Interlocked.Decrement(ref trackingEntryCount) == 0)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 表示监视生命周期的记录
    /// </summary>
    sealed class TrackingEntry : IDisposable
    {
        /// <summary>
        /// 用于释放资源的对象
        /// </summary>
        readonly IDisposable disposable;

        /// <summary>
        /// 监视对象的弱引用
        /// </summary>
        readonly WeakReference weakReference;

        /// <summary>
        /// 获取是否可以释放资源
        /// </summary>
        /// <returns></returns>
        public bool CanDispose => weakReference.IsAlive == false;

        /// <summary>
        /// 监视生命周期的记录
        /// </summary>
        /// <param name="handler">激活状态的httpHandler</param>
        public TrackingEntry(LifetimeHttpHandler handler)
        {
            disposable = handler.InnerHandler!;
            weakReference = new WeakReference(handler);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}