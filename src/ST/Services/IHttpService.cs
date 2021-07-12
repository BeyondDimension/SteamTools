using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IHttpService
    {
        protected const string TAG = "HttpService";

        static IHttpService Instance => DI.Get<IHttpService>();

        JsonSerializer Serializer { get; }
        IHttpClientFactory Factory { get; }
        IHttpPlatformHelper PlatformHelper { get; }

        Task<T?> SendAsync<T>(
            string? requestUri,
            HttpRequestMessage request,
            string? accept,
            bool enableForward,
            CancellationToken cancellationToken,
            Action<HttpResponseMessage>? handlerResponse = null,
            Action<HttpResponseMessage>? handlerResponseByIsNotSuccessStatusCode = null,
            string? clientName = null) where T : notnull;

        /// <summary>
        /// 通过 Get 请求 API 内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="accept"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T?> GetAsync<T>(string requestUri,
            string accept = MediaTypeNames.JSON,
            CancellationToken cancellationToken = default, string? cookie = null) where T : notnull;

        /// <summary>
        /// (带本地缓存)通过 Get 请求 Image Stream
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="channelType">渠道类型，根据不同的类型建立不同的缓存文件夹</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Stream?> GetImageStreamAsync(
            string requestUri,
            string channelType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// (带本地缓存)通过 Get 请求 Image FilePath
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="channelType">渠道类型，根据不同的类型建立不同的缓存文件夹</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string?> GetImageAsync(
            string requestUri,
            string channelType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// (无本地缓存)通过 Get 请求 Image 内容
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Obsolete]
        Task<Stream?> GetImageStreamAsync(string requestUri, CancellationToken cancellationToken = default);

        static string GetImagesCacheDirectory(string? channelType)
        {
            const string dirName = "Images";
            var dirPath = string.IsNullOrWhiteSpace(channelType) ?
                Path.Combine(IOPath.CacheDirectory, dirName, channelType) :
                Path.Combine(IOPath.CacheDirectory, dirName);
            IOPath.DirCreateByNotExists(dirPath);
            return dirPath;
        }
    }

#if DEBUG

    [Obsolete("use IHttpService.Instance", true)]
    public class HttpServices
    {
    }

#endif
}