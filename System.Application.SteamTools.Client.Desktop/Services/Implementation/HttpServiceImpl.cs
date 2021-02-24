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

        public HttpServiceImpl(ILogger logger, IHttpPlatformHelper http_helper, IHttpClientFactory clientFactory) : base(logger, http_helper, clientFactory)
        {
        }

        public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken) where T : notnull
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.ParseAdd(Accept);
                request.Headers.UserAgent.ParseAdd(http_helper.UserAgent);
                var client = CreateClient();
                using var response = await client.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken)
                    .ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    if (response.Content != null)
                    {
                        var mime = response.Content.Headers.ContentType?.MediaType ?? Accept;
                        switch (mime)
                        {
                            case MediaTypeNames.JSON:
                            case MediaTypeNames.XML:
                            case MediaTypeNames.XML_APP:
                                {
                                    using var stream = await response.Content
                                        .ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
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
            return default;
        }
    }
}