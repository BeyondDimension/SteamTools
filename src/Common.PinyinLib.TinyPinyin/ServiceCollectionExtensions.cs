using System.Application.Services;
using System.Application.Services.Implementation;
using TinyPinyin;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加使用 <see cref="PinyinHelper"/>(https://github.com/promeG/TinyPinyin) or (https://github.com/hueifeng/TinyPinyin.Net) 实现的拼音功能
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTinyPinyin(this IServiceCollection services)
        {
            services.AddSingleton<IPinyin, PinyinImpl>();
            return services;
        }

        /// <inheritdoc cref="AddTinyPinyin(IServiceCollection)"/>
        public static IServiceCollection AddPinyin(this IServiceCollection services) => services.AddTinyPinyin();
    }
}