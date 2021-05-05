using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Net.Http;

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

        //public static IServiceCollection AddHttpService(this IServiceCollection services, string name)
        //{
        //    // 此函数为添加HTTP服务模板
        //    // https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0
        //    services.AddHttpClient(name);
        //    services.AddSingleton<IXXHttpService, XXHttpService>();
        //}
    }
}