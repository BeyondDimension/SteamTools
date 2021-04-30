using Microsoft.AspNetCore.Mvc.Filters;

namespace System.Application.Filters
{
    /// <summary>
    /// 添加额外响应头
    /// </summary>
    public sealed class AddResponseHeadersAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            /* https://developer.mozilla.org/zh-CN/docs/Web/HTTP/X-Frame-Options
             * X-Frame-Options HTTP 响应头是用来给浏览器指示允许一个页面可否在 <frame>, <iframe> 或者 <object> 中展现的标记。网站可以使用此功能，来确保自己网站的内容没有被嵌到别人的网站中去，也从而避免了点击劫持 (clickjacking) 的攻击。
             * SAMEORIGIN 表示该页面可以在相同域名页面的 frame 中展示。*/
            context.HttpContext.Response.Headers.Add("X-FRAME-OPTIONS", "SAMEORIGIN");

            /* https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Headers/X-Content-Type-Options
             * X-Content-Type-Options 响应首部相当于一个提示标志，被服务器用来提示客户端一定要遵循在 Content-Type 首部中对  MIME 类型 的设定，而不能对其进行修改。这就禁用了客户端的 MIME 类型嗅探行为，换句话说，也就是意味着网站管理员确定自己的设置没有问题。*/
            context.HttpContext.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        }
    }
}