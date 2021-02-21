using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 尝试添加模型验证
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddModelValidator(this IServiceCollection services)
        {
            services.TryAddSingleton<IModelValidator, ModelValidator>();
            return services;
        }
    }
}