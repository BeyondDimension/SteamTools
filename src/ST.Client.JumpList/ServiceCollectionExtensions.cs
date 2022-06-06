using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddJumpListService(this IServiceCollection services)
    {
        if (Windows10JumpListServiceImpl.IsSupported)
        {
            services.AddSingleton<IJumpListService, Windows10JumpListServiceImpl>();
        }
        else
        {
#if AVALONIA
            services.AddSingleton<IJumpListService, JumpListServiceImpl>();
#else
            throw new System.PlatformNotSupportedException();
#endif
        }
        return services;
    }
}
