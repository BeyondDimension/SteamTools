// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// CRL（证书吊销列表）服务中间件，用于对外提供本地 MITM 证书的吊销分发点
/// </summary>
sealed class CrlMiddleware
{
    readonly CertService certService;

    public CrlMiddleware(CertService certService)
    {
        this.certService = certService;
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (HttpMethods.IsGet(context.Request.Method) &&
            context.Request.Path.Equals(new PathString("/crl"), StringComparison.OrdinalIgnoreCase))
        {
            var crlBytes = certService.CrlBytes;
            if (crlBytes == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            context.Response.ContentType = "application/pkix-crl";
            await context.Response.Body.WriteAsync(crlBytes);
        }
        else
        {
            await next(context);
        }
    }
}
