using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IHttpService
    {
        public static IHttpService Instance => DI.Get<IHttpService>();

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
            CancellationToken cancellationToken = default) where T : notnull;

        /// <summary>
        /// 通过 Get 请求 Image 内容
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="channelType">渠道类型，根据不同的类型建立不同的缓存文件夹</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Stream?> GetImageAsync(
            string requestUri,
            string channelType,
            CancellationToken cancellationToken = default);

    }

#if DEBUG

    [Obsolete("use IHttpService.Instance", true)]
    public class HttpServices
    {
    }

#endif
}