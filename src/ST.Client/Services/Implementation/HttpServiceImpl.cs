using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO;
using System.IO.FileFormats;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Application.ForwardHelper;
using static System.Application.Services.IHttpService;

namespace System.Application.Services.Implementation
{
    internal sealed class HttpServiceImpl : GeneralHttpClientFactory, IHttpService
    {
        readonly JsonSerializer jsonSerializer = new();
        readonly ICloudServiceClient cloud_client;

        public HttpServiceImpl(
            ICloudServiceClient cloud_client,
            ILoggerFactory loggerFactory,
            IHttpPlatformHelper http_helper,
            IHttpClientFactory clientFactory)
            : base(loggerFactory.CreateLogger(TAG), http_helper, clientFactory)
        {
            this.cloud_client = cloud_client;
        }

        public async Task<T?> SendAsync<T>(
            string? requestUri,
            HttpRequestMessage request,
            string? accept,
            bool enableForward,
            CancellationToken cancellationToken,
            Action<HttpResponseMessage>? handlerResponse = null,
            Action<HttpResponseMessage>? handlerResponseByIsNotSuccessStatusCode = null,
            string? clientName = null) where T : notnull
        {
            HttpResponseMessage? response = null;
            bool notDispose = false;
            try
            {
                requestUri ??= request.RequestUri.ToString();
                if (enableForward && IsAllowUrl(requestUri))
                {
                    try
                    {
                        response = await cloud_client.Forward(request,
                            HttpCompletionOption.ResponseHeadersRead,
                            cancellationToken);
                    }
                    catch (Exception e)
                    {
                        logger.LogWarning(e, "CloudService Forward Fail.");
                        response = null;
                    }
                }

                if (response == null)
                {
                    var client = CreateClient(clientName);
                    response = await client.SendAsync(request,
                      HttpCompletionOption.ResponseHeadersRead,
                      cancellationToken).ConfigureAwait(false);
                }

                handlerResponse?.Invoke(response);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Content != null)
                    {
                        var rspContentClrType = typeof(T);
                        if (rspContentClrType == typeof(string))
                        {
                            return (T)(object)await response.Content.ReadAsStringAsync();
                        }
                        else if (rspContentClrType == typeof(byte[]))
                        {
                            return (T)(object)await response.Content.ReadAsByteArrayAsync();
                        }
                        else if (rspContentClrType == typeof(Stream))
                        {
                            notDispose = true;
                            return (T)(object)await response.Content.ReadAsStreamAsync();
                        }
                        var mime = response.Content.Headers.ContentType?.MediaType ?? accept;
                        switch (mime)
                        {
                            case MediaTypeNames.JSON:
                            case MediaTypeNames.XML:
                            case MediaTypeNames.XML_APP:
                                {
                                    using var stream = await response.Content
                                        .ReadAsStreamAsync().ConfigureAwait(false);
                                    using var reader = new StreamReader(stream, Encoding.UTF8);
                                    switch (mime)
                                    {
                                        case MediaTypeNames.JSON:
                                            {
                                                using var json = new JsonTextReader(reader);
                                                return jsonSerializer.Deserialize<T>(json);
                                            }
                                        case MediaTypeNames.XML:
                                        case MediaTypeNames.XML_APP:
                                            {
                                                var xmlSerializer = new XmlSerializer(typeof(T));
                                                return (T)xmlSerializer.Deserialize(reader);
                                            }
                                    }
                                }
                                break;
                        }
                    }
                }
                else
                {
                    handlerResponseByIsNotSuccessStatusCode?.Invoke(response);
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "SendAsync Fail.");
            }
            finally
            {
                if (!notDispose)
                {
                    request.Dispose();
                    response?.Dispose();
                }
            }
            return default;
        }

        public Task<T?> GetAsync<T>(
            string requestUri,
            string accept,
            CancellationToken cancellationToken) where T : notnull
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.ParseAdd(accept);
            request.Headers.UserAgent.ParseAdd(http_helper.UserAgent);
            return SendAsync<T>(requestUri, request, accept, true, cancellationToken);
        }

        async Task<string?> GetImageAsync_(
            string requestUri,
            string localCacheFilePath,
            CancellationToken cancellationToken)
        {
            var localCacheFilePathExists = File.Exists(localCacheFilePath);

            try
            {
                if (localCacheFilePathExists) // 存在缓存文件
                {
                    using var fileStream = IOPath.OpenRead(localCacheFilePath);
                    if (FileFormat.IsImage(fileStream, out var format))
                    {
                        if (http_helper.SupportedImageFormats.Contains(format)) // 读取缓存并且格式符合要求
                        {
                            fileStream.Close();
                            //logger.LogDebug("GetImageAsync localCacheFilePath: {0}", localCacheFilePath);
                            return localCacheFilePath;
                        }
                        else // 格式不准确
                        {
                            logger.LogError("GetImageAsync Not Supported ImageFormat: {0}.", format);
                        }
                    }
                    else // 未知的图片格式
                    {
                        logger.LogError("GetImageAsync Unknown ImageFormat.");
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "GetImageAsync_ Load LocalFile Fail.");
            }

            if (localCacheFilePathExists)
            {
                File.Delete(localCacheFilePath); // 必须删除文件，重新下载覆盖
            }

            try
            {
                //logger.LogDebug("GetImageAsync requestUri: {0}", requestUri);
                //#if DEBUG
                //                var now = DateTime.Now;
                //                var filePath = Path.Combine(IOPath.CacheDirectory, $"{now.ToString(DateTimeFormat.Connect)}-{Hashs.String.Crc32(requestUri)}");
                //                File.WriteAllText(filePath, now.ToString());
                //#endif
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.ParseAdd(http_helper.AcceptImages);
                request.Headers.UserAgent.ParseAdd(http_helper.UserAgent);
                var client = CreateClient();
                var response = await client.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var localCacheFilePath2 = localCacheFilePath + ".download-cache";
                    var fileStream = File.Create(localCacheFilePath2);
                    cancellationToken.Register(() =>
                    {
                        fileStream.Close();
                        IOPath.FileTryDelete(localCacheFilePath2);
                    });
                    await stream.CopyToAsync(fileStream, cancellationToken);
                    await fileStream.FlushAsync();
                    bool isSupportedImageFormat;
                    if (FileFormat.IsImage(fileStream, out var format))
                    {
                        if (http_helper.SupportedImageFormats.Contains(format)) // 读取缓存并且格式符合要求
                        {
                            isSupportedImageFormat = true;
                        }
                        else // 格式不准确
                        {
                            logger.LogError("GetImageAsync Download Not Supported ImageFormat: {0}.", format);
                            isSupportedImageFormat = false;
                        }
                    }
                    else // 未知的图片格式
                    {
                        logger.LogError("GetImageAsync Download Unknown ImageFormat.");
                        isSupportedImageFormat = false;
                    }
                    fileStream.Close();
                    if (isSupportedImageFormat)
                    {
                        File.Move(localCacheFilePath2, localCacheFilePath);
                        return localCacheFilePath;
                    }
                    else
                    {
                        IOPath.FileTryDelete(localCacheFilePath2);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "GetImageAsync_ Fail.");
            }

            return default;
        }

        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Task<string?>>> get_image_pipeline = new();

        public async Task<Stream?> GetImageStreamAsync(string requestUri, string channelType, CancellationToken cancellationToken)
        {
            var file = await GetImageAsync(requestUri, channelType, cancellationToken);
            return string.IsNullOrEmpty(file) ? null : File.OpenRead(file);
        }

        public async Task<string?> GetImageAsync(string requestUri, string channelType, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(requestUri)) return null;

            if (get_image_pipeline.ContainsKey(channelType))
            {
                var pairs = get_image_pipeline[channelType];
                if (pairs.ContainsKey(requestUri))
                {
                    var findResult = await pairs[requestUri];
                    return findResult;
                }
            }
            else
            {
                get_image_pipeline.TryAdd(channelType, new ConcurrentDictionary<string, Task<string?>>());
            }

            var dirPath = Path.Combine(IOPath.CacheDirectory, "Images", channelType);
            IOPath.DirCreateByNotExists(dirPath);
            var fileName = Hashs.String.SHA256(requestUri) + FileEx.ImageSource;
            var localCacheFilePath = Path.Combine(dirPath, fileName);

            var pairs2 = get_image_pipeline[channelType];
            var value = GetImageAsync_(requestUri, localCacheFilePath, cancellationToken);
            pairs2.TryAdd(requestUri, value);
            var result = await value;
            pairs2.TryRemove(requestUri, out var _);
            return result;
        }

        public async Task<Stream?> GetImageStreamAsync(string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.ParseAdd(http_helper.AcceptImages);
                request.Headers.UserAgent.ParseAdd(http_helper.UserAgent);
                var client = CreateClient();
                var response = await client.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    //var dirPath = Path.Combine(IOPath.CacheDirectory, "Images", "Temp");
                    //IOPath.DirCreateByNotExists(dirPath);
                    //var filePath = Path.Combine(dirPath, Path.GetTempFileName());
                    //IOPath.FileIfExistsItDelete(filePath);
                    var stream = await response.Content.ReadAsStreamAsync();
                    //var fileStream = File.Create(filePath);
                    //await stream.CopyToAsync(fileStream, cancellationToken);
                    return stream;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "GetImageStreamAsync Fail.");
            }
            return default;
        }
    }
}