using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Application.Models;
using System.ComponentModel;

namespace System.Application.Services.Implementation.PacketIntercept;

abstract class InterceptHostedService : BackgroundService
{
    readonly IInterceptor interceptor;
    readonly ILogger<InterceptHostedService> logger;
    readonly IReverseProxyConfig reverseProxyConfig;

    public InterceptHostedService(
        IInterceptor interceptor,
        ILogger<InterceptHostedService> logger,
        IReverseProxyConfig reverseProxyConfig)
    {
        this.interceptor = interceptor;
        this.logger = logger;
        this.reverseProxyConfig = reverseProxyConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await interceptor.InterceptAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 995)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InterceptAsync catch.");
            await reverseProxyConfig.Service.StopProxy();
        }
    }
}
