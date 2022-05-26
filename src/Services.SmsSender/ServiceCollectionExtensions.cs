using System.Application.Models;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加短信发送提供商(仅单选一个提供商)
    /// </summary>
    /// <typeparam name="TSmsSettings"></typeparam>
    /// <param name="services"></param>
    /// <param name="name">提供商唯一名称，见 <see cref="ISmsSettings.SmsOptions"/> 中 PropertyName</param>
    /// <returns></returns>
    public static IServiceCollection AddSmsSenderProvider<TSmsSettings>(
        this IServiceCollection services,
        string? name)
        where TSmsSettings : class, ISmsSettings
        => SmsSenderBase.Add<TSmsSettings>(services, name);
}