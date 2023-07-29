using BD.WTTS.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加通用日志实现
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddGeneralLogging(this IServiceCollection services)
    {
        // 移动端平台日志与文件日志对性能影响较大，仅 Debug 时使用平台日志，文件日志应由设置项启用
        services.AddLogging(IApplication.ConfigureLogging());
        //services.AddSingleton(new NLogDisposable());
        return services;
    }

    //sealed class NLogDisposable : IDisposable
    //{
    //    bool disposedValue;

    //    void Dispose(bool disposing)
    //    {
    //        if (!disposedValue)
    //        {
    //            if (disposing)
    //            {
    //                // 释放托管状态(托管对象)
    //                NLogManager.Shutdown();
    //            }

    //            // 释放未托管的资源(未托管的对象)并重写终结器
    //            // 将大型字段设置为 null
    //            disposedValue = true;
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //        Dispose(disposing: true);
    //        GC.SuppressFinalize(this);
    //    }
    //}
}