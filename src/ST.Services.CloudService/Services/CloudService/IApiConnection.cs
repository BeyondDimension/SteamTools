using System.Application.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public interface IApiConnection
    {
        /// <summary>
        /// 禁用客户端本地模型验证
        /// </summary>
        public static bool DisableModelValidator { get; set; }

        /// <summary>
        /// 获取服务端接口的原始内容(ByteArray)
        /// </summary>
        /// <param name="cancellationToken">传播应取消操作的通知</param>
        /// <param name="requestUri">服务端接口URL地址</param>
        /// <param name="isAnonymous">是否使用匿名身份访问</param>
        /// <returns></returns>
        Task<IApiResponse<byte[]>> GetRaw(CancellationToken cancellationToken, string requestUri, bool isAnonymous = true, bool isShowResponseErrorMessage = true, string? errorAppendText = null, bool isPolly = true);

        /// <summary>
        /// 获取服务端接口的HTML内容(String)
        /// </summary>
        /// <param name="cancellationToken">传播应取消操作的通知</param>
        /// <param name="requestUri">服务端接口URL地址</param>
        /// <param name="isAnonymous">是否使用匿名身份访问</param>
        /// <returns></returns>
        Task<IApiResponse<string>> GetHtml(CancellationToken cancellationToken, string requestUri, bool isAnonymous = true, bool isShowResponseErrorMessage = true, string? errorAppendText = null, bool isPolly = true);

        /// <summary>
        /// 下载服务端接口内容
        /// <para>注意事项：在完成后根据是否成功以及业务需求需要调用 progress 报告进度满值</para>
        /// </summary>
        /// <param name="cancellationToken">传播应取消操作的通知</param>
        /// <param name="requestUri">服务端接口URL地址</param>
        /// <param name="cacheFilePath">缓存文件路径</param>
        /// <param name="progress">进度报告</param>
        /// <param name="isAnonymous">是否使用匿名身份访问</param>
        /// <returns></returns>
        Task<IApiResponse> DownloadAsync(CancellationToken cancellationToken, string requestUri, string cacheFilePath, IProgress<float>? progress, bool isAnonymous = true, bool isShowResponseErrorMessage = true, string? errorAppendText = null, bool isPolly = true);

        #region SendAsync

        Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken);

        /// <summary>
        /// RequestModel+ResponseModel(调用服务端接口)
        /// </summary>
        /// <typeparam name="TRequestModel">请求模型类型</typeparam>
        /// <typeparam name="TResponseModel">响应模型类型</typeparam>
        /// <param name="cancellationToken">传播应取消操作的通知</param>
        /// <param name="method">HTTP方法</param>
        /// <param name="requestUri">服务端接口URL地址</param>
        /// <param name="request">请求模型</param>
        /// <param name="responseContentMaybeNull">响应内容是否能为<see langword="null"/></param>
        /// <param name="isAnonymous">是否使用匿名身份访问</param>
        /// <returns></returns>
        Task<IApiResponse<TResponseModel>> SendAsync<TRequestModel, TResponseModel>(
            CancellationToken cancellationToken,
            HttpMethod method,
            string requestUri,
            TRequestModel? request,
            bool responseContentMaybeNull = false,
            bool isSecurity = false,
            bool isAnonymous = false,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null,
            bool isPolly = false);

        /// <summary>
        /// RequestModel(调用服务端接口)
        /// </summary>
        /// <typeparam name="TRequestModel">请求模型类型</typeparam>
        /// <param name="cancellationToken">传播应取消操作的通知</param>
        /// <param name="method">HTTP方法</param>
        /// <param name="requestUri">服务端接口URL地址</param>
        /// <param name="request">请求模型</param>
        /// <param name="isAnonymous">是否使用匿名身份访问</param>
        /// <returns></returns>
        Task<IApiResponse> SendAsync<TRequestModel>(
            CancellationToken cancellationToken,
            HttpMethod method,
            string requestUri,
            TRequestModel? request,
            bool isSecurity = false,
            bool isAnonymous = false,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null,
            bool isPolly = false);

        /// <summary>
        /// NoModel(调用服务端接口)
        /// </summary>
        /// <param name="cancellationToken">传播应取消操作的通知</param>
        /// <param name="method">HTTP方法</param>
        /// <param name="requestUri">服务端接口URL地址</param>
        /// <param name="isAnonymous">是否使用匿名身份访问</param>
        /// <returns></returns>
        Task<IApiResponse> SendAsync(
            CancellationToken cancellationToken,
            HttpMethod method,
            string requestUri,
            bool isAnonymous = false,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null,
            bool isPolly = false);

        /// <summary>
        /// ResponseModel(调用服务端接口)
        /// </summary>
        /// <typeparam name="TResponseModel">响应模型类型</typeparam>
        /// <param name="cancellationToken">传播应取消操作的通知</param>
        /// <param name="method">HTTP方法</param>
        /// <param name="requestUri">服务端接口URL地址</param>
        /// <param name="isAnonymous">是否使用匿名身份访问</param>
        /// <returns></returns>
        Task<IApiResponse<TResponseModel>> SendAsync<TResponseModel>(
            CancellationToken cancellationToken,
            HttpMethod method,
            string requestUri,
            bool responseContentMaybeNull = false,
            bool isSecurity = false,
            bool isAnonymous = false,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null,
            bool isPolly = false);

        #endregion
    }
}