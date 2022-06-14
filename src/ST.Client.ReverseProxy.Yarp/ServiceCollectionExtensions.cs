using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加由 Yarp 实现的反向代理服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddReverseProxyService(this IServiceCollection services)
    {
        services.AddSingleton<IReverseProxyService, YarpReverseProxyServiceImpl>();
        return services;
    }
}
