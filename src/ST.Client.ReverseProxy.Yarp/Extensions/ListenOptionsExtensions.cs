using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Application.Internals.FlowAnalyze;
using System.Application.Services;

namespace Microsoft.Extensions.DependencyInjection;

static partial class ListenOptionsExtensions
{
    /// <summary>
    /// 使用流量分析中间件
    /// </summary>
    /// <param name="listen"></param>
    /// <returns></returns>
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
}
