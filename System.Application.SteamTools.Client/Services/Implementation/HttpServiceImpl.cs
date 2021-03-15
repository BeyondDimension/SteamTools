using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO;
using System.IO.FileFormats;
using System.Linq;
using System.Net.Http;
using System.Properties;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Application.ForwardHelper;

namespace System.Application.Services.Implementation
{
    internal sealed class HttpServiceImpl : GeneralHttpClientFactory, IHttpService
    {
        const string TAG = "HttpS";
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

        /// <summary>
        /// 启用转发
        /// </summary>
        static readonly bool EnableForward = !ThisAssembly.Debuggable;

        public async Task<T?> GetAsync<T>(
            string requestUri,
            string accept,
            CancellationToken cancellationToken) where T : notnull
        {
            HttpRequestMessage? request = null;
            HttpResponseMessage? response = null;
            bool notDispose = false;
            try
            {
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.ParseAdd(accept);
                request.Headers.UserAgent.ParseAdd(http_helper.UserAgent);

                if (EnableForward &&
                    allowUrls.Any(x => requestUri.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
                {
                    response = await cloud_client.Forward(request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);
                }
                else
                {
                    var client = CreateClient();
                    response = await client.SendAsync(request,
                      HttpCompletionOption.ResponseHeadersRead,
                      cancellationToken).ConfigureAwait(false);
                }

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
            }
            catch (Exception e)
            {
                Log.Warn(TAG, e, "GetAsync Fail.");
            }
            finally
            {
                if (!notDispose)
                {
                    request?.Dispose();
                    response?.Dispose();
                }
            }
            return default;
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
                    var fileStream = File.OpenRead(localCacheFilePath);
                    if (FileFormat.IsImage(fileStream, out var format))
                    {
                        if (http_helper.SupportedImageFormats.Contains(format)) // 读取缓存并且格式符合要求
                        {
                            fileStream.Close();
                            Log.Debug(TAG, "GetImageAsync localCacheFilePath: {0}", localCacheFilePath);
                            return localCacheFilePath;
                        }
                        else // 格式不准确
                        {
                            Log.Error(TAG, "GetImageAsync Not Supported ImageFormat: {0}.", format);
                        }
                    }
                    else // 未知的图片格式
                    {
                        Log.Error(TAG, "GetImageAsync Unknown ImageFormat.");
                    }
                    fileStream.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "GetImageAsync Load LocalFile Fail.");
            }

            if (localCacheFilePathExists)
            {
                File.Delete(localCacheFilePath); // 必须删除文件，重新下载覆盖
            }

            try
            {
                Log.Debug(TAG, "GetImageAsync requestUri: {0}", requestUri);
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
                            Log.Error(TAG,
                                "GetImageAsync Download Not Supported ImageFormat: {0}.", format);
                            isSupportedImageFormat = false;
                        }
                    }
                    else // 未知的图片格式
                    {
                        Log.Error(TAG, "GetImageAsync Download Unknown ImageFormat.");
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
                Log.Error(TAG, e, "GetImageAsync Fail.");
            }

            return default;
        }

        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Task<string?>>> get_image_pipeline = new();

        public async Task<Stream?> GetImageAsync(
            string requestUri,
            string channelType,
            CancellationToken cancellationToken)
        {
            if (get_image_pipeline.ContainsKey(channelType))
            {
                var pairs = get_image_pipeline[channelType];
                if (pairs.ContainsKey(requestUri))
                {
                    var findResult = await pairs[requestUri];
                    return FileOpenRead(findResult);
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
            return FileOpenRead(result);

            static Stream? FileOpenRead(string? p) => p == null ? null : File.OpenRead(p);
        }
    }
}