using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace System.Application.Services.Implementation
{
    internal sealed class HttpServiceImpl : HttpService, IHttpService
    {
        const string TAG = "HttpS";
        const string Accept = MediaTypeNames.JSON;
        readonly JsonSerializer jsonSerializer = new JsonSerializer();

        public HttpServiceImpl(ILoggerFactory loggerFactory, IHttpPlatformHelper http_helper, IHttpClientFactory clientFactory) : base(loggerFactory.CreateLogger(TAG), http_helper, clientFactory)
        {
        }

        public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken) where T : notnull
        {
            HttpRequestMessage? request = null;
            HttpResponseMessage? response = null;
            bool notDispose = false;
            try
            {
                request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.ParseAdd(Accept);
                request.Headers.UserAgent.ParseAdd(http_helper.UserAgent);
                var client = CreateClient();
                response = await client.SendAsync(request,
                  HttpCompletionOption.ResponseHeadersRead,
                  cancellationToken)
                  .ConfigureAwait(false);
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
                        var mime = response.Content.Headers.ContentType?.MediaType ?? Accept;
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
                Log.Error(TAG, e, "GetAsync Fail.");
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
    }
}