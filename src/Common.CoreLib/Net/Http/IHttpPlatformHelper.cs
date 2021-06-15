using System.IO;
using System.IO.FileFormats;

namespace System.Net.Http
{
    /// <summary>
    /// 由平台实现的HTTP工具助手
    /// </summary>
    public interface IHttpPlatformHelper
    {
        /// <summary>
        /// 用户代理
        /// <para>https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Headers/User-Agent</para>
        /// <para>通常可以从平台的WebView控件中调用js获取</para>
        /// </summary>
        string UserAgent { get; }

        string AcceptImages { get; }

        string AcceptLanguage { get; }

        ImageFormat[] SupportedImageFormats { get; }

        /// <summary>
        /// 尝试将需要上传的文件流[处理]后保存到临时路径中并返回信息
        /// <para>[处理]由客户端平台原生实现</para>
        /// <para>例如：文件压缩、格式转换(heic)</para>
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        (string filePath, string mime)? TryHandleUploadFile(Stream fileStream);

        /// <summary>
        /// 是否有网络链接
        /// </summary>
        bool IsConnected { get; }
    }
}