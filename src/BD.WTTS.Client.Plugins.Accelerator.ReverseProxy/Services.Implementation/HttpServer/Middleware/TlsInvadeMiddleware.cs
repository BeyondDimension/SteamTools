// https://github.com/dotnetcore/FastGithub/blob/58f79ddc19410c92b18e8d4de1c4b61376e97be7/FastGithub.HttpServer/TlsMiddlewares/TlsInvadeMiddleware.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// Https 入侵中间件
/// </summary>
static class TlsInvadeMiddleware
{
    /// <summary>
    /// 执行中间件
    /// </summary>
    /// <param name="next"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task InvokeAsync(ConnectionDelegate next, ConnectionContext context)
    {
        // 连接不是 tls
        if (await IsTlsConnectionAsync(context) == false)
        {
            // 没有任何 tls 中间件执行过
            if (context.Features.Get<ITlsConnectionFeature>() == null)
            {
                // 设置假的 ITlsConnectionFeature，迫使 https 中间件跳过自身的工作
                context.Features.Set<ITlsConnectionFeature>(FakeTlsConnectionFeature.Instance);
            }
        }
        await next(context);
    }

    /// <summary>
    /// 是否为 tls 协议
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    static async Task<bool> IsTlsConnectionAsync(ConnectionContext context)
    {
        try
        {
            var result = await context.Transport.Input.ReadAtLeastAsync(2, context.ConnectionClosed);
            var state = IsTlsProtocol(result);
            context.Transport.Input.AdvanceTo(result.Buffer.Start);
            return state;
        }
        catch
        {
            return false;
        }

        static bool IsTlsProtocol(ReadResult result)
        {
            var reader = new SequenceReader<byte>(result.Buffer);
            return reader.TryRead(out var firstByte) &&
                reader.TryRead(out var nextByte) &&
                firstByte == 0x16 &&
                nextByte == 0x3;
        }
    }
}