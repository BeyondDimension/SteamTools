// ReSharper disable once CheckNamespace
using System.Security.Authentication;

namespace Microsoft.Extensions.DependencyInjection;

static partial class ListenOptionsExtensions
{
    /// <summary>
    /// 使用流量分析中间件
    /// </summary>
    /// <param name="listen"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ListenOptions UseFlowAnalyze(this ListenOptions listen)
    {
        var flowAnalyzer = listen.ApplicationServices.GetRequiredService<IFlowAnalyzer>();
        listen.Use(next => async context =>
        {
            var oldTransport = context.Transport;
            try
            {
                await using var loggingDuplexPipe = new FlowAnalyzeDuplexPipe(context.Transport, flowAnalyzer);
                context.Transport = loggingDuplexPipe;
                await next(context);
            }
            finally
            {
                context.Transport = oldTransport;
            }
        });
        return listen;
    }

    /// <summary>
    /// 使用 Tls 中间件
    /// </summary>
    /// <param name="listen"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ListenOptions UseTls(this ListenOptions listen)
    {
        var certService = listen.ApplicationServices.GetRequiredService<CertService>();
        listen.Use(next => context => TlsInvadeMiddleware.InvokeAsync(next, context));
        listen.UseHttps(new TlsHandshakeCallbackOptions
        {
            OnConnection = ctx =>
            {
                var o = new SslServerAuthenticationOptions
                {
                    ServerCertificate = certService.GetOrCreateServerCert(ctx.ClientHelloInfo.ServerName),
                };
                return ValueTask.FromResult(o);
            },
        });
        listen.Use(next => context => TlsRestoreMiddleware.InvokeAsync(next, context));
        return listen;
    }

    ///// <summary>
    ///// 使用 Tls 中间件
    ///// </summary>
    ///// <param name="listen"></param>
    ///// <param name="configureOptions">https配置</param>
    ///// <returns></returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //static ListenOptions UseTls(this ListenOptions listen, Action<HttpsConnectionAdapterOptions> configureOptions)
    //{
    //    listen.Use(next => context => TlsInvadeMiddleware.InvokeAsync(next, context));
    //    listen.UseHttps(configureOptions);
    //    listen.Use(next => context => TlsRestoreMiddleware.InvokeAsync(next, context));
    //    return listen;
    //}
}
