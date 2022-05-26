using Microsoft.Extensions.DependencyInjection;
using System.Application.Models;

namespace System.Application.Services.Implementation;

public abstract class SmsSenderBase : ISmsSender
{
    public abstract string Channel { get; }

    public abstract bool SupportCheck { get; }

    public abstract Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken);

    public abstract Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken);

    /// <summary>
    /// 生成随机短信验证码值，某些平台可能提供了随机生成，可以重写该函数替换
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public virtual string GenerateRandomNum(int length)
    {
        return Random2.GenerateRandomNum((ushort)length).ToString();
    }

    public static IServiceCollection Add<TSmsSender, TSmsSettings>(
        IServiceCollection services)
        where TSmsSender : class, ISmsSender
        where TSmsSettings : class, ISmsSettings
    {
        services.AddHttpClient<TSmsSender>();
        services.AddScoped<DebugSmsSenderProvider>();
        services.AddScoped<ISmsSender, SmsSenderWrapper<TSmsSender, TSmsSettings>>();
        return services;
    }

    internal static IServiceCollection Add<TSmsSettings>(
        IServiceCollection services,
        string? name)
        where TSmsSettings : class, ISmsSettings => name switch
        {
            null => services.AddScoped<ISmsSender, DebugSmsSenderProvider>(),
            nameof(ISmsSettings.SmsOptions._21VianetBlueCloud)
                => Add<_21VianetBlueCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.AlibabaCloud)
                => Add<AlibabaCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.NetEaseCloud)
                => Add<NetEaseCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
        };
}