using static System.Application.Services.IReverseProxyService;

namespace System.Application.Services.Implementation;

public abstract class ReverseProxyServiceImpl : IReverseProxyService
{
    protected readonly IPlatformService platformService;

    public ReverseProxyServiceImpl(IPlatformService platformService)
    {
        this.platformService = platformService;
    }

    protected virtual void DisposeCore()
    {

    }

    bool disposedValue;

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                DisposeCore();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
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
