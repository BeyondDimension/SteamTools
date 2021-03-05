// https://bcssstorage.blob.core.chinacloudapi.cn/docs/CCS/DEMO.zip

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Application.Models;
using System.Application.Models._21VianetBlueCloud;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
using SmsOptions = System.Application.Models._21VianetBlueCloud.SmsOptions;

namespace System.Application.Services.Implementation._21VianetBlueCloud
{
    /// <summary>
    /// 短信服务提供商 - 世纪互联蓝云
    /// </summary>
    public class SmsSenderProvider : SmsSenderBase, ISmsSender
    {
        public const string Name = nameof(_21VianetBlueCloud);

        public override string Channel => Name;

        public override bool SupportCheck => false;

        readonly HttpClient httpClient;
        readonly SmsOptions options;
        readonly ILogger logger;
        readonly JsonSerializer jsonSerializer = new();

        public SmsSenderProvider(ILogger<SmsSenderProvider> logger, SmsOptions? options, HttpClient httpClient)
        {
            this.logger = logger;
            if (!options.HasValue()) throw new ArgumentException(null, nameof(options));
            this.options = options.ThrowIsNull(nameof(options));
            this.httpClient = httpClient;
        }

        const string Schema = "SharedAccessSignature";
        const string SignKey = "sig";
        const string KeyNameKey = "skn";
        const string ExpiryKey = "se";
        const string _endpoint = "https://bluecloudccs.21vbluecloud.com:443/services/sms/messages?api-version=2018-10-01";

        /// <summary>
        /// create token
        /// </summary>
        /// <param name="key">密钥：密钥分为两种：-full: 可以用于 REST API 和设备端 SDK，-device: 只能用于设备端 SDK</param>
        /// <param name="keyName">full/device</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        static string CreateSASToken(string key, string keyName, TimeSpan timeout)
        {
            var values = new Dictionary<string, string>
            {
                { KeyNameKey, keyName },
                { ExpiryKey, (DateTimeOffset.UtcNow + timeout).ToUnixTimeSeconds().ToString() }
            };

            var signContent = string.Join("&", values
                .Where(pair => pair.Key != SignKey)
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}={HttpUtility.UrlEncode(pair.Value)}"));

            string sign;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                sign = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signContent)));
            }

            return $"{Schema} {SignKey}={HttpUtility.UrlEncode(sign)}&{signContent}";
        }

        public override async Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken)
        {
            var key = options.KeyValue.ThrowIsNull(nameof(options.KeyValue));
            var keyName = options.KeyName.ThrowIsNull(nameof(options.KeyName));
            var template_name = options.Templates?.FirstOrDefault(x => x.Type == type)?.Template;

            var requestData = new RequestData
            {
                PhoneNumber = new[] { number },
                ExtendCode = options.ExtendCode,
                MessageBody = new MessageBody
                {
                    TemplateName = template_name,
                    TemplateParam = new()
                    {
                        { options.CodeTemplateKeyName, message },
                    }
                },
            };

            var jsonPayload = JsonConvert.SerializeObject(requestData);

            using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, MediaTypeNames.JSON),
            };

            request.Headers.Add("Account", options.Account);

            var token = CreateSASToken(key, keyName, TimeSpan.FromSeconds(30));
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            var isSuccess = response.IsSuccessStatusCode;
            SendSms21VianetBlueCloudResult? jsonObject = null;

            if (isSuccess)
            {
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var json = new JsonTextReader(reader);
                jsonObject = jsonSerializer.Deserialize<SendSms21VianetBlueCloudResult?>(json);
            }

            var result = new SendSmsResult<SendSms21VianetBlueCloudResult>
            {
                HttpStatusCode = (int)response.StatusCode,
                IsSuccess = isSuccess,
                Result = jsonObject,
                ResultObject = jsonObject,
            };

            return result;
        }

        public override Task<ICheckSmsResult> CheckSmsAsync(string __, string ___, CancellationToken _)
        {
            throw new NotImplementedException();
        }

        class RequestData
        {
            /// <summary>
            /// 接收手机号
            /// </summary>
            [N_JsonProperty("phoneNumber")]
            [S_JsonProperty("phoneNumber")]
            public string[]? PhoneNumber { get; set; }

            /// <summary>
            /// 下发扩展码，两位纯数字
            /// </summary>
            [N_JsonProperty("extend")]
            [S_JsonProperty("extend")]
            public string? ExtendCode { get; set; }

            [N_JsonProperty("messageBody")]
            [S_JsonProperty("messageBody")]
            public MessageBody? MessageBody { get; set; }
        }

        class MessageBody
        {
            /// <summary>
            /// 短信模板名称
            /// </summary>
            [N_JsonProperty("templateName")]
            [S_JsonProperty("templateName")]
            public string? TemplateName { get; set; }

            /// <summary>
            /// 短信模板参数，和模板中变量一一对应,没有变量则不需要
            /// </summary>
            [N_JsonProperty("templateParam")]
            [S_JsonProperty("templateParam")]
            public Dictionary<string, string> TemplateParam { get; set; } = new();
        }
    }
}