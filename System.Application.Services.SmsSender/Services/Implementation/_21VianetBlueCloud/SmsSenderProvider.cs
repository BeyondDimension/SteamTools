using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Application.Models;
using System.Application.Models._21VianetBlueCloud;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation._21VianetBlueCloud
{
    /// <summary>
    /// 短信服务提供商 - 世纪互联蓝云
    /// </summary>
    public class SmsSenderProvider : AbstractSmsSender, ISmsSender
    {
        public const string Name = nameof(_21VianetBlueCloud);

        public override string Channel => Name;

        public override bool SupportCheck => false;

        readonly HttpClient httpClient;
        readonly SmsOptions options;
        readonly ILogger logger;
        readonly JsonSerializer jsonSerializer = new();

        public SmsSenderProvider(ILogger<SmsSenderProvider> logger, SmsOptions options, HttpClient httpClient)
        {
            this.logger = logger;
            this.options = options;
            this.httpClient = httpClient;
        }

        public override Task<ISendSmsResult> SendSmsAsync(string number, string message, int type, CancellationToken cancellationToken)
        {
            var dictionary = new Dictionary<string, object?> {
                { "phoneNumber", new[] { number } },
                { "extend", options.ExtendCode },
                { "messageBody", options.ExtendCode },
            };

            // 明天再写
            throw new NotImplementedException();
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
            [JsonProperty("phoneNumber")]
            public string[]? PhoneNumber { get; set; }

            /// <summary>
            /// 下发扩展码，两位纯数字
            /// </summary>
            [JsonProperty("extend")]
            public string? ExtendCode { get; set; }

            [JsonProperty("messageBody")]
            public MessageBody? MessageBody { get; set; }
        }

        class MessageBody
        {
            /// <summary>
            /// 短信模板名称
            /// </summary>
            [JsonProperty("templateName")]
            public string? TemplateName { get; set; }

            /// <summary>
            /// 短信模板参数，和模板中变量一一对应,没有变量则不需要
            /// </summary>
            [JsonProperty("templateParam")]
            public Dictionary<string, string>? TemplateParam { get; set; }
        }
    }
}