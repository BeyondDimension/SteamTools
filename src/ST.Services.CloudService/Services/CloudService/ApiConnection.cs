using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using System;
using System.Application.Models;
using System.Application.Models.Internals;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.FileFormats;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace System.Application.Services.CloudService
{
    internal sealed class ApiConnection : IApiConnection
    {
        readonly ILogger logger;
        readonly IHttpPlatformHelper http_helper;
        readonly IApiConnectionPlatformHelper conn_helper;
        readonly Lazy<JsonSerializer> jsonSerializer = new(() => new JsonSerializer());
        readonly IModelValidator validator;

        public ApiConnection(
            ILogger logger,
            IApiConnectionPlatformHelper conn_helper,
            IHttpPlatformHelper http_helper,
            IModelValidator validator)
        {
            this.logger = logger;
            this.conn_helper = conn_helper;
            this.http_helper = http_helper;
            this.validator = validator;
        }

        async ValueTask<JWTEntity?> SetRequestHeaderAuthorization(HttpRequestMessage request)
        {
            var authToken = await conn_helper.Auth.GetAuthTokenAsync();
            var authHeaderValue = conn_helper.GetAuthenticationHeaderValue(authToken);
            if (authHeaderValue != null)
            {
                request.Headers.Authorization = authHeaderValue;
                return authToken;
            }
            return null;
        }

        /// <summary>
        /// 根据异常获取响应
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        (ApiResponseCode code, string? msg) GetRspByExceptionWithLog(Exception ex, string requestUri)
        {
            if (ex is ApiResponseCodeException apiResponseCodeException)
            {
                return (apiResponseCodeException.Code, null);
            }
            var isCanceled = ex.IsCanceledException();
            if (isCanceled)
            {
                return (ApiResponseCode.Canceled, null);
            }
            var code = ApiResponseCode.ClientException;
            logger.LogError(ex,
                $"ApiConn Fail({(int)code})，Url：{requestUri}");
            var exMsg = ex.GetAllMessage();
            return (code, ApiResponse.GetMessage(code, exMsg));
        }

        /// <summary>
        /// 根据异常获取响应并写入日志
        /// </summary>
        /// <typeparam name="TApiResponse"></typeparam>
        /// <param name="ex"></param>
        /// <param name="requestUri"></param>
        /// <param name="new"></param>
        /// <returns></returns>
        TApiResponse GetRspByExceptionWithLog<TApiResponse>(Exception ex, string requestUri, Func<ApiResponseCode, string?, TApiResponse> @new)
        {
            (var code, var msg) = GetRspByExceptionWithLog(ex, requestUri);
            var rsp = @new(code, msg);
            if (rsp is ApiResponseImplBase rspImpl) rspImpl.ClientException = ex;
            return rsp;
        }

        /// <summary>
        /// 返回 HTTP 401 未授权，清空当前 AuthToken，并调用 SignOut
        /// </summary>
        /// <param name="requestUri"></param>
        async Task Unauthorized(HttpMethod method, string requestUri)
        {
            logger.LogCritical("Unauthorized method: {0}, requestUri: {1}", method, requestUri);
            await conn_helper.Auth.SignOutAsync();
        }

        /// <summary>
        /// 生成请求内容
        /// </summary>
        /// <typeparam name="TRequestModel">请求模型类型</typeparam>
        /// <param name="serializableImplType">序列化实现类型，如果为上传文件，则此参数无效</param>
        /// <param name="cancellationToken">传播应取消操作的通知</param>
        /// <param name="request">请求模型</param>
        /// <returns></returns>
        HttpContent? GetRequestContent<TRequestModel>(
            bool isSecurity,
            Aes? aes,
            Serializable.ImplType serializableImplType,
            TRequestModel? request,
            CancellationToken cancellationToken)
        {
            if (request == null) return null;
            if (isSecurity && aes == null) throw new ArgumentNullException(nameof(aes));

            HttpContent? httpContent;
            if (request is IUploadFileSource uploadFile) // 上传单个文件
            {
                httpContent = GetMultipartFormDataContent1(uploadFile);
            }
            else if (request is IEnumerable<IUploadFileSource> uploadFiles) // 上传多个文件
            {
                httpContent = GetMultipartFormDataContent2(uploadFiles);
            }
            else
            {
                if (isSecurity && serializableImplType != Serializable.ImplType.MessagePack)
                {
                    serializableImplType = Serializable.ImplType.MessagePack;
                }
                switch (serializableImplType) // 序列化模型
                {
                    case Serializable.ImplType.NewtonsoftJson:
                        httpContent = GetJsonContent(Serializable.SJSON(Serializable.JsonImplType.NewtonsoftJson, request));
                        break;
                    case Serializable.ImplType.MessagePack:
                        var byteArray = Serializable.SMP(request, cancellationToken);
                        if (byteArray == null) return null;
                        if (isSecurity)
                        {
                            byteArray = aes.ThrowIsNull(nameof(aes)).Encrypt(byteArray);
                        }
                        httpContent = new ByteArrayContent(byteArray);
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue(isSecurity ? MediaTypeNames.Security : MediaTypeNames.MessagePack);
                        break;
                    case Serializable.ImplType.SystemTextJson:
                        httpContent = GetJsonContent(Serializable.SJSON(Serializable.JsonImplType.SystemTextJson, request));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(serializableImplType), serializableImplType, null);
                }
            }
            return httpContent;
            static StringContent? GetJsonContent(string? jsonStr)
            {
                if (jsonStr == null) return null;
                return new StringContent(jsonStr, Encoding.UTF8, MediaTypeNames.JSON);
            }
            MultipartFormDataContent GetMultipartFormDataContent1(params IUploadFileSource[] uploadFiles)
            {
                var uploadFiles_ = uploadFiles.AsEnumerable();
                return GetMultipartFormDataContent2(uploadFiles_);
            }
            MultipartFormDataContent GetMultipartFormDataContent2(IEnumerable<IUploadFileSource> uploadFiles)
            {
                var multipartFormDataContent = new MultipartFormDataContent();
                var index = 0;
                foreach (var item in uploadFiles)
                {
                    if (item.HasValue() && item.Available)
                    {
                        var uploadFile = item;
                        var stream = uploadFile.OpenRead();
                        void ThrowUnsupportedUploadFileMediaType() // 未知的上传媒体类型
                        {
                            stream?.Dispose();
                            multipartFormDataContent.Dispose();
                            // ↑ 释放未压缩的文件流 与 总内容(MultipartFormDataContent) 当前文件源不释放
                            var msg = $"Unsupported Upload File MediaType, filePath: {uploadFile.FilePath}, index: {index}";
                            logger.LogError(msg);
                            throw new ApiResponseCodeException(
                                ApiResponseCode.UnsupportedUploadFileMediaType, msg);
                        }
                        var needHandle = true;
                        switch (uploadFile.UploadFileType) // 上传文件类型
                        {
                            case UploadFileType.Image: // 图片
                                if (uploadFile.IsCompressed) // 文件源已压缩过
                                {
                                    if (string.IsNullOrEmpty(uploadFile.MIME)) // 无MIME，则检测
                                    {
                                        if (FileFormat.IsImage(stream, out var format))
                                        {
                                            if (format.IsAllow()) // 属于允许的格式，则不处理
                                            {
                                                uploadFile.MIME = format.GetMIME();
                                                needHandle = false;
                                            }
                                        }
                                    }
                                    else if (FileFormat.AllowImageMediaTypeNames.Contains(uploadFile.MIME))
                                    {
                                        // 有MIME值，且在允许的范围内，则不处理
                                        needHandle = false;
                                    }
                                }
                                break;
                            //case UploadFileType.Voice: // 音频
                            //    break;
                            //case UploadFileType.Video: // 视频
                            //    break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(uploadFile.UploadFileType), uploadFile.UploadFileType, null);
                        }
                        if (needHandle)
                        {
                            var result = http_helper.TryHandleUploadFile(stream);
                            if (!result.HasValue) // 处理失败
                            {
                                ThrowUnsupportedUploadFileMediaType();
                            }
                            else
                            {
                                stream.Dispose();
                                uploadFile.Dispose();
                                // ↑ 释放未压缩的文件流 与 文件源
                                uploadFile = new UploadFileSource
                                {
                                    FilePath = result.Value.filePath,
                                    MIME = result.Value.mime,
                                    IsCompressed = true,
                                    IsCache = true,
                                    UploadFileType = uploadFile.UploadFileType,
                                };
                                stream = uploadFile.OpenRead();
                                // 生成新的文件源 并 重新打开文件流
                            }
                        }
                        var content = new UploadFileContent(uploadFile, stream);
                        if (string.IsNullOrWhiteSpace(uploadFile.FilePath))
                        {
                            multipartFormDataContent.Add(content, "file");
                        }
                        else
                        {
                            var fileName = Path.GetFileName(uploadFile.FilePath);
                            multipartFormDataContent.Add(content, "file", fileName);
                        }
                    }
                    index++;
                }
                if (!multipartFormDataContent.Any())
                {
                    throw new ApiResponseCodeException(ApiResponseCode.LackAvailableUploadFile);
                }
                return multipartFormDataContent;
            }
        }

        void ShowResponseErrorMessage(IApiResponse response, string? errorAppendText = null)
            => conn_helper.ShowResponseErrorMessage(response, errorAppendText);

        async Task GlobalResponseIntercept(
            HttpMethod method,
            string requestUri,
            IApiResponse response,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null)
        {
            if (!response.IsSuccess)
            {
                if (isShowResponseErrorMessage)
                {
                    ShowResponseErrorMessage(response, errorAppendText);
                }

                if (response.Code == ApiResponseCode.Unauthorized)
                {
                    await Unauthorized(method, requestUri);
                }
            }

            if (response is ApiResponseImplBase rspImpl)
            {
                rspImpl.Url = requestUri;
            }
        }

        async Task GlobalResponseIntercept<TResponseModel>(
            bool isApi,
            HttpMethod method,
            string requestUri,
            object? request,
            IApiResponse<TResponseModel> response,
            bool responseContentMaybeNull,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null)
        {
            if (response.IsSuccess)
            {
                if (!responseContentMaybeNull && response.Content == null)
                {
                    response.Code = ApiResponseCode.NoResponseContent;
                }
                else
                {
                    if (isApi)
                    {
                        if (!responseContentMaybeNull && response is IApiResponse<IExplicitHasValue> explicitHasValue && !explicitHasValue.Content.HasValue())
                        {
                            response.Code = ApiResponseCode.NoResponseContentValue;
                        }
                        else
                        {
                            if (response is IApiResponse<ILoginResponse> loginResponse
                                  && loginResponse.Content != null)
                            {
                                //IReadOnlyPhoneNumber? phoneNumber;
                                //if (loginResponse.Content is IReadOnlyPhoneNumber phoneNumber1)
                                //    phoneNumber = phoneNumber1;
                                //else if (request is IReadOnlyPhoneNumber phoneNumber2)
                                //    phoneNumber = phoneNumber2;
                                //else
                                //    phoneNumber = null;
                                //await conn_helper.OnLoginedAsync(phoneNumber, loginResponse.Content);
                                await conn_helper.OnLoginedAsync(loginResponse.Content, loginResponse.Content);
                            }
                            else if (response is IApiResponse<IReadOnlyAuthToken> authTokenResponse
                                && authTokenResponse.Content != null)
                            {
                                var authToken = authTokenResponse.Content.AuthToken;
                                if (authToken.HasValue())
                                {
                                    await conn_helper.SaveAuthTokenAsync(
                                        authToken.ThrowIsNull(nameof(authToken)));
                                }
                            }
                            else if (response is IApiResponse<Guid[]> guidsResponse
                                && guidsResponse.Content != null)
                            {
                                if (request is IUploadFileSource uploadFile) // 上传单个文件
                                {
                                    HandleUploadFile(uploadFile);
                                }
                                else if (request is IEnumerable<IUploadFileSource> uploadFiles) // 上传多个文件
                                {
                                    HandleUploadFiles(uploadFiles);
                                }
                                void HandleUploadFile(params IUploadFileSource[] uploadFiles)
                                {
                                    var uploadFiles_ = uploadFiles.AsEnumerable();
                                    HandleUploadFiles(uploadFiles_);
                                }
                                void HandleUploadFiles(IEnumerable<IUploadFileSource> uploadFiles)
                                {
                                    var items = uploadFiles.Where(x => x.HasValue() && x.Available).ToArray();
                                    if (items.Length != guidsResponse.Content.Length)
                                    {
                                        var msg = $"Unequal Length Upload File " +
                                            $"request: {items.Length}, response: {guidsResponse.Content.Length}";
                                        logger.LogError(msg);
                                        throw new ApiResponseCodeException(
                                            ApiResponseCode.UnequalLengthUploadFile, msg);
                                    }
                                    else
                                    {
                                        // 上传后将此缓存文件移动到下载图片文件夹中
                                        throw new NotImplementedException();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            await GlobalResponseIntercept(method, requestUri, response, isShowResponseErrorMessage, errorAppendText);
        }

        bool GlobalBeforeIntercept<TResponseModel>(
            [NotNullWhen(true)] out IApiResponse<TResponseModel>? responseResult,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null)
        {
            responseResult = null;

            #region NetworkAccess

            if (!http_helper.IsConnected)
            {
                responseResult = ApiResponse.Code<TResponseModel>(ApiResponseCode.NetworkConnectionInterruption, Constants.NetworkConnectionInterruption);
            }

            #endregion

            if (isShowResponseErrorMessage && responseResult != null && !responseResult.IsSuccess)
            {
                ShowResponseErrorMessage(responseResult, errorAppendText);
            }

            return responseResult != null;
        }

        public Task<bool>? RefreshTokenAndAutoSaveTask { get; private set; }

        async Task<bool> RefreshTokenAndAutoSave(JWTEntity jwt)
        {
            var rsp = await conn_helper.RefreshToken(jwt);

            if (rsp.IsSuccess && rsp.Content != null)
            {
                await conn_helper.SaveAuthTokenAsync(rsp.Content);
                return true;
            }
            else if (rsp.Code != ApiResponseCode.Unauthorized)
            {
                logger.LogWarning("RefreshToken Fail, Code: {0}", rsp.Code);
            }
            return false;
        }

        async Task<bool> RefreshToken(JWTEntity jwt)
        {
            if (RefreshTokenAndAutoSaveTask == null)
            {
                RefreshTokenAndAutoSaveTask = RefreshTokenAndAutoSave(jwt);
            }
            var r = await RefreshTokenAndAutoSaveTask;
            return r;
        }

        bool IsAppObsolete(HttpResponseHeaders headers)
            => headers.TryGetValues(Constants.Headers.Response.AppObsolete, out var values) &&
            values.Contains(bool.TrueString, StringComparer.OrdinalIgnoreCase);

        void HandleAppObsolete(HttpResponseHeaders headers)
        {
            if (IsAppObsolete(headers))
            {
                throw new ApiResponseCodeException(ApiResponseCode.AppObsolete);
            }
        }

        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            var client = conn_helper.CreateClient();

            HandleHttpRequest(request);

            var response = await client.SendAsync(request,
                completionOption,
                cancellationToken).ConfigureAwait(false);

            HandleAppObsolete(response.Headers);

            return response;
        }

        void HandleHttpRequest(HttpRequestMessage request)
        {
            request.Headers.AcceptLanguage.ParseAdd(http_helper.AcceptLanguage);
        }

        async Task<IApiResponse<TResponseModel>> SendCoreAsync<TRequestModel, TResponseModel>(
            bool isAnonymous,
            bool isApi,
            CancellationToken cancellationToken,
            HttpMethod method,
            string requestUri,
            TRequestModel? requestModel,
            bool responseContentMaybeNull,
            bool isSecurity,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null)
        {
            #region ModelValidator

            if (!IApiConnection.DisableModelValidator && isApi &&
                requestModel != null &&
                typeof(TRequestModel) != typeof(object))
            {
                if (!validator.Validate(requestModel, out var errorMessage))
                {
                    var validate_fail_r = ApiResponse.Code<TResponseModel>(
                        ApiResponseCode.RequestModelValidateFail, errorMessage);
                    if (isShowResponseErrorMessage) ShowResponseErrorMessage(validate_fail_r, errorAppendText);
                    return validate_fail_r;
                }
            }

            #endregion

            if (GlobalBeforeIntercept<TResponseModel>(out var globalBeforeInterceptResponse, isShowResponseErrorMessage, errorAppendText))
            {
                return globalBeforeInterceptResponse;
            }

            IApiResponse<TResponseModel> responseResult;

            Aes? aes = null;

            try
            {
                if (isSecurity)
                {
                    // 行业标准加密
                    aes = AESUtils.Create();
                }

                var serializableImplType = Serializable.ImplType.MessagePack;

                var request = new HttpRequestMessage(method, requestUri)
                {
                    Version = HttpVersion.Version20,
                    Content = GetRequestContent(
                       isSecurity,
                       aes,
                       serializableImplType,
                       requestModel,
                       cancellationToken),
                };

                switch (serializableImplType)
                {
                    case Serializable.ImplType.NewtonsoftJson:
                    case Serializable.ImplType.SystemTextJson:
                        request.Headers.Accept.ParseAdd(MediaTypeNames.JSON);
                        break;
                    case Serializable.ImplType.MessagePack:
                        if (isSecurity)
                        {
                            request.Headers.Accept.ParseAdd(MediaTypeNames.Security);
                        }
                        else
                        {
                            request.Headers.Accept.ParseAdd(MediaTypeNames.MessagePack);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(serializableImplType), serializableImplType, null);
                }

                if (isSecurity)
                {
                    var skey_bytes = aes.ThrowIsNull(nameof(aes)).ToParamsByteArray();
                    var skey_str = conn_helper.RSA.EncryptToString(skey_bytes);
                    request.Headers.Add(Constants.Headers.Request.SecurityKey, skey_str);
                }

                JWTEntity? jwt = null;

                if (!isAnonymous)
                {
                    jwt = await SetRequestHeaderAuthorization(request);
                }

                var client = conn_helper.CreateClient();

                HandleHttpRequest(request);

                var response = await client.SendAsync(request,
                   HttpCompletionOption.ResponseHeadersRead,
                   cancellationToken)
                   .ConfigureAwait(false);

                HandleAppObsolete(response.Headers);

                var code = (ApiResponseCode)response.StatusCode;

                if (!isAnonymous && code == ApiResponseCode.Unauthorized && jwt != null)
                {
                    var resultRefreshToken = await RefreshToken(jwt);
                    if (resultRefreshToken)
                    {
                        var resultRecursion = await SendCoreAsync<TRequestModel, TResponseModel>(
                            isAnonymous,
                            isApi,
                            cancellationToken,
                            method,
                            requestUri,
                            requestModel,
                            responseContentMaybeNull,
                            isSecurity,
                            isShowResponseErrorMessage,
                            errorAppendText);
                        return resultRecursion;
                    }
                }

                if (response.Content == null)
                {
                    responseResult = ApiResponse.Code<TResponseModel>(code);
                }
                else
                {
                    if (!isApi && typeof(TResponseModel) == typeof(byte[]))
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                        responseResult = ApiResponse.Code(code, null, (TResponseModel)(object)bytes);
                    }
                    else if (!isApi && typeof(TResponseModel) == typeof(string))
                    {
                        var str = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        responseResult = ApiResponse.Code(code, null, (TResponseModel)(object)str);
                    }
                    else
                    {
                        var rspIsCiphertext = false;

                        var mime = response.Content.Headers.ContentType?.MediaType;

                        if (mime == MediaTypeNames.Security)
                        {
                            mime = MediaTypeNames.MessagePack;
                            rspIsCiphertext = true;
                        }

                        switch (mime)
                        {
                            case MediaTypeNames.JSON:
                                {
                                    if (rspIsCiphertext)
                                    {
                                        throw new NotSupportedException("At present, JSON does not implement security on the server side, so this cannot happen.");
                                    }
                                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                                    using var reader = new StreamReader(stream, Encoding.UTF8);
                                    using var json = new JsonTextReader(reader);
                                    responseResult = ApiResponse.Deserialize<TResponseModel>(jsonSerializer.Value, json);
                                }
                                break;
                            case MediaTypeNames.MessagePack:
                                {
                                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                                    using var cryptoStream = rspIsCiphertext ? new CryptoStream(stream, aes.ThrowIsNull(nameof(aes)).CreateDecryptor(), CryptoStreamMode.Read) : null;
                                    responseResult = await ApiResponse.DeserializeAsync<TResponseModel>(rspIsCiphertext ? cryptoStream.ThrowIsNull(nameof(cryptoStream)) : stream, cancellationToken);
                                }
                                break;
#if DEBUG
                            case MediaTypeNames.HTML:
                            case MediaTypeNames.TXT:
                                var htmlString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                                responseResult = ApiResponse.Code<TResponseModel>(response.IsSuccessStatusCode ? ApiResponseCode.UnsupportedResponseMediaType : code, htmlString);
                                break;
#endif
                            default:
                                responseResult = ApiResponse.Code<TResponseModel>(response.IsSuccessStatusCode ? ApiResponseCode.UnsupportedResponseMediaType : code);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseResult = GetRspByExceptionWithLog(ex, requestUri, ApiResponse.Code<TResponseModel>);
            }
            finally
            {
                aes?.Dispose();
            }
            await GlobalResponseIntercept(
                isApi,
                method,
                requestUri,
                requestModel,
                responseResult,
                responseContentMaybeNull);
            return responseResult;
        }

        #region Polly

        const int numRetries = 10;

        static bool PollyHandleResultPredicate<TResponse>(TResponse response) where TResponse : IApiResponse
            => response is ApiResponseImpl impl &&
                impl.ClientException != null &&
                impl.Code == ApiResponseCode.ClientException;

        static TimeSpan PollyRetryAttempt(int attemptNumber)
        {
            var powY = attemptNumber % numRetries;
            var timeSpan = TimeSpan.FromMilliseconds(Math.Pow(2, powY));
            int addS = attemptNumber / numRetries;
            if (addS > 0) timeSpan = timeSpan.Add(TimeSpan.FromSeconds(addS));
            return timeSpan;
        }

        #endregion

        async Task<IApiResponse<TResponseModel>> SendCoreAsync<TRequestModel, TResponseModel>(
            bool isPolly,
            bool isAnonymous,
            bool isApi,
            CancellationToken cancellationToken,
            HttpMethod method,
            string requestUri,
            TRequestModel? requestModel,
            bool responseContentMaybeNull,
            bool isSecurity,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null)
        {
            IApiResponse<TResponseModel> response;
            Task<IApiResponse<TResponseModel>> _SendCoreAsync()
                => SendCoreAsync<TRequestModel, TResponseModel>(
                    isAnonymous,
                    isApi,
                    cancellationToken,
                    method,
                    requestUri,
                    requestModel,
                    responseContentMaybeNull,
                    isSecurity,
                    !isPolly && isShowResponseErrorMessage,
                    errorAppendText);
            if (isPolly)
            {
                response = await Policy.HandleResult<IApiResponse<TResponseModel>>(PollyHandleResultPredicate)
                    .WaitAndRetryAsync(numRetries, PollyRetryAttempt)
                    .ExecuteAsync(_SendCoreAsync);
                if (!response.IsSuccess)
                {
                    if (isShowResponseErrorMessage)
                    {
                        ShowResponseErrorMessage(response, errorAppendText);
                    }
                }
            }
            else
            {
                response = await _SendCoreAsync();
            }
            return response;
        }

        const int bufferSize = 4096;

        async Task<IApiResponse> DownloadAsync(
           CancellationToken cancellationToken,
           string requestUri,
           string cacheFilePath,
           IProgress<float>? progress,
           bool isAnonymous,
           bool isShowResponseErrorMessage = true,
           string? errorAppendText = null)
        {
            var cacheDirPath = Path.GetDirectoryName(cacheFilePath);
            if (cacheDirPath == null) throw new ArgumentNullException(nameof(cacheDirPath));
            IOPath.DirCreateByNotExists(cacheDirPath);

            if (GlobalBeforeIntercept<object>(out var globalBeforeInterceptResponse, isShowResponseErrorMessage, errorAppendText))
            {
                return globalBeforeInterceptResponse;
            }

            var method = HttpMethod.Get;
            IApiResponse responseResult;
            try
            {
                var request = new HttpRequestMessage(method, requestUri)
                {
                    Version = HttpVersion.Version20,
                };

                JWTEntity? jwt = null;

                if (!isAnonymous)
                {
                    jwt = await SetRequestHeaderAuthorization(request);
                }

                var client = conn_helper.CreateClient();

                HandleHttpRequest(request);

                var response = await client.SendAsync(request,
                   HttpCompletionOption.ResponseHeadersRead,
                   cancellationToken)
                   .ConfigureAwait(false);

                var code = (ApiResponseCode)response.StatusCode;

                if (!isAnonymous && code == ApiResponseCode.Unauthorized && jwt != null)
                {
                    var resultRefreshToken = await RefreshToken(jwt);
                    if (resultRefreshToken)
                    {
                        var resultRecursion = await DownloadAsync(
                            cancellationToken,
                            requestUri,
                            cacheFilePath,
                            progress,
                            isAnonymous,
                            isShowResponseErrorMessage,
                            errorAppendText);
                        return resultRecursion;
                    }
                }

                responseResult = ApiResponse.Code(code);
                if (responseResult.IsSuccess)
                {
                    var total = response.Content.Headers.ContentLength ?? -1L;
                    if (total > 0)
                    {
                        var canReportProgress = progress != null;
                        IOPath.FileIfExistsItDelete(cacheFilePath);
                        using var fileStream = new FileStream(cacheFilePath,
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.None,
                            bufferSize,
                            true);
                        using var stream = await response.Content.ReadAsStreamAsync();
                        var totalRead = 0L;
                        var buffer = new byte[bufferSize];
                        var isMoreToRead = true;
                        var lastProgressValue = 0f;
                        do
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var readAsyncToken = CancellationTokenSource.
                                CreateLinkedTokenSource(cancellationToken);
                            readAsyncToken.CancelAfter(5000);
                            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), readAsyncToken.Token);
                            if (read == 0)
                            {
                                isMoreToRead = false;
                            }
                            else
                            {
                                await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                                totalRead += read;
                                if (canReportProgress)
                                {
                                    var progressValue = MathF.Round((float)totalRead / total, 2, MidpointRounding.AwayFromZero);
                                    if (progressValue != lastProgressValue)
                                    {
                                        progress?.Report(progressValue);
                                        lastProgressValue = progressValue;
                                    }
                                }
                            }
                        } while (isMoreToRead);
                    }
                    else
                    {
                        responseResult.Code = ApiResponseCode.NoResponseContent;
                    }
                }
            }
            catch (Exception ex)
            {
                responseResult = GetRspByExceptionWithLog(ex, requestUri, ApiResponse.Code);
            }
            await GlobalResponseIntercept(
                method,
                requestUri,
                responseResult,
                isShowResponseErrorMessage,
                errorAppendText);
            return responseResult;
        }

        public async Task<IApiResponse> DownloadAsync(
            CancellationToken cancellationToken,
            string requestUri,
            string cacheFilePath,
            IProgress<float>? progress,
            bool isAnonymous,
            bool isShowResponseErrorMessage = true,
            string? errorAppendText = null,
            bool isPolly = true)
        {
            IApiResponse response;
            Task<IApiResponse> _DownloadAsync()
                => DownloadAsync(
                    cancellationToken,
                    requestUri,
                    cacheFilePath,
                    progress,
                    isAnonymous,
                    !isPolly && isShowResponseErrorMessage,
                    errorAppendText);
            if (isPolly)
            {
                response = await Policy.HandleResult<IApiResponse>(PollyHandleResultPredicate)
                    .WaitAndRetryAsync(numRetries, PollyRetryAttempt)
                    .ExecuteAsync(_DownloadAsync);
                if (!response.IsSuccess)
                {
                    if (isShowResponseErrorMessage)
                    {
                        ShowResponseErrorMessage(response, errorAppendText);
                    }
                }
            }
            else
            {
                response = await _DownloadAsync();
            }
            return response;
        }

        public async Task<IApiResponse<TResponseModel>> SendAsync<TRequestModel, TResponseModel>(CancellationToken cancellationToken, HttpMethod method, string requestUri, TRequestModel? request, bool responseContentMaybeNull, bool isSecurity, bool isAnonymous, bool isShowResponseErrorMessage = true, string? errorAppendText = null, bool isPolly = false)
        {
            var rsp = await SendCoreAsync<TRequestModel, TResponseModel>(
                isPolly: isPolly,
                isAnonymous: isAnonymous,
                isApi: true,
                cancellationToken,
                method,
                requestUri,
                requestModel: request,
                responseContentMaybeNull,
                isSecurity,
                isShowResponseErrorMessage,
                errorAppendText);
            return rsp;
        }

        public async Task<IApiResponse<byte[]>> GetRaw(CancellationToken cancellationToken, string requestUri, bool isAnonymous, bool isShowResponseErrorMessage = true, string? errorAppendText = null, bool isPolly = true)
        {
            var rsp = await SendCoreAsync<object, byte[]>(
                isPolly: isPolly,
                isAnonymous: isAnonymous,
                isApi: false,
                cancellationToken,
                HttpMethod.Get,
                requestUri,
                requestModel: null,
                responseContentMaybeNull: false,
                isSecurity: false,
                isShowResponseErrorMessage,
                errorAppendText);
            return rsp;
        }

        public async Task<IApiResponse<string>> GetHtml(CancellationToken cancellationToken, string requestUri, bool isAnonymous, bool isShowResponseErrorMessage = true, string? errorAppendText = null, bool isPolly = true)
        {
            var rsp = await SendCoreAsync<object, string>(
                isPolly: isPolly,
                isAnonymous: isAnonymous,
                isApi: false,
                cancellationToken,
                HttpMethod.Get,
                requestUri,
                requestModel: null,
                responseContentMaybeNull: false,
                isSecurity: false,
                isShowResponseErrorMessage,
                errorAppendText);
            return rsp;
        }

        public async Task<IApiResponse> SendAsync<TRequestModel>(CancellationToken cancellationToken, HttpMethod method, string requestUri, TRequestModel? request, bool isSecurity, bool isAnonymous, bool isShowResponseErrorMessage = true, string? errorAppendText = null, bool isPolly = false)
        {
            var rsp = await SendCoreAsync<TRequestModel, object>(
                isPolly: isPolly,
                isAnonymous: isAnonymous,
                isApi: true,
                cancellationToken,
                method,
                requestUri,
                requestModel: request,
                responseContentMaybeNull: true,
                isSecurity,
                isShowResponseErrorMessage,
                errorAppendText);
            return rsp;
        }

        public async Task<IApiResponse> SendAsync(CancellationToken cancellationToken, HttpMethod method, string requestUri, bool isAnonymous, bool isShowResponseErrorMessage = true, string? errorAppendText = null, bool isPolly = false)
        {
            var rsp = await SendCoreAsync<object, object>(
                isPolly: isPolly,
                isAnonymous: isAnonymous,
                isApi: true,
                cancellationToken,
                method,
                requestUri,
                requestModel: null,
                responseContentMaybeNull: true,
                isSecurity: false,
                isShowResponseErrorMessage,
                errorAppendText);
            return rsp;
        }

        public async Task<IApiResponse<TResponseModel>> SendAsync<TResponseModel>(CancellationToken cancellationToken, HttpMethod method, string requestUri, bool responseContentMaybeNull, bool isSecurity, bool isAnonymous, bool isShowResponseErrorMessage = true, string? errorAppendText = null, bool isPolly = false)
        {
            var rsp = await SendCoreAsync<object, TResponseModel>(
                isPolly: isPolly,
                isAnonymous: isAnonymous,
                isApi: true,
                cancellationToken,
                method,
                requestUri,
                requestModel: null,
                responseContentMaybeNull,
                isSecurity,
                isShowResponseErrorMessage,
                errorAppendText);
            return rsp;
        }
    }
}