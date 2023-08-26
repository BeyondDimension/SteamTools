#if WINDOWS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl : IDisposable
{
    bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                isLightOrDarkThemeWatch?.Dispose();

                IOPath.TryDeleteCacheSubDir(CacheTempDirName);

                if (IsPrivilegedProcess)
                {
                    IEnumerable<Task> ProxyDispose()
                    {
                        if (SetAsSystemProxyStatus)
                        {
                            yield return SetAsSystemProxyAsync(false, default, default);
                        }
                        if (SetAsSystemPACProxyStatus)
                        {
                            yield return SetAsSystemPACProxyAsync(false, default);
                        }
                    }
                    Task.WaitAll(ProxyDispose().ToArray());
                }
            }

            isLightOrDarkThemeWatch = null;
            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
#endif