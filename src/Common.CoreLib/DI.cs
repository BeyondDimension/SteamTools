#if !NOT_DI
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
#endif

// https://github.com/CommunityToolkit/dotnet/blob/v8.0.0-preview3/CommunityToolkit.Mvvm/DependencyInjection/Ioc.cs

namespace System;

/// <summary>
/// 依赖注入服务组(DependencyInjection)
/// </summary>
public static partial class DI
{
#if !NOT_DI

    static volatile IServiceProvider? value;

    internal static bool IsConfigured => value != null;

    public static void Dispose()
    {
        if (value is IDisposable disposable) disposable.Dispose();
    }

    public static async ValueTask DisposeAsync()
    {
        if (value is IAsyncDisposable disposable) await disposable.DisposeAsync();
        else Dispose();
    }

    /// <summary>
    /// 初始化依赖注入服务组(通过配置服务项的方式)
    /// </summary>
    /// <param name="configureServices"></param>
    public static void ConfigureServices(Action<IServiceCollection> configureServices)
    {
        var services = new ServiceCollection();
        configureServices(services);
        ConfigureServices(services.BuildServiceProvider());
    }

    /// <summary>
    /// 初始化依赖注入服务组(直接赋值)
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        Interlocked.CompareExchange(ref value, serviceProvider, null);
    }

#endif

#if !NOT_DI

    static Exception GetDIGetFailException(Type serviceType)
    {
        var msg = $"DI.Get* fail, serviceType: {serviceType}";
        Debug.WriteLine(msg);
        return new(msg);
    }

    /// <summary>
    /// 获取依赖注入服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Get<T>() where T : notnull
    {
        if (value == null)
        {
            throw GetDIGetFailException(typeof(T));
        }
        return value.GetRequiredService<T>();
    }

    /// <inheritdoc cref="Get{T}"/>
    public static T? Get_Nullable<T>() where T : notnull
    {
        if (value == null) return default;
        return value.GetService<T>();
    }

    /// <inheritdoc cref="Get{T}"/>
    public static object Get(Type serviceType)
    {
        if (value == null)
        {
            throw GetDIGetFailException(serviceType);
        }
        return value.GetRequiredService(serviceType);
    }

    /// <inheritdoc cref="Get_Nullable{T}"/>
    public static object? Get_Nullable(Type serviceType)
    {
        if (value == null) return default;
        return value.GetService(serviceType);
    }

    public static IServiceScope CreateScope()
    {
        if (value == null)
        {
            var msg = "DI.CreateScope fail.";
            Debug.WriteLine(msg);
            throw new Exception(msg);
        }
        return value.CreateScope();
    }

#endif
}