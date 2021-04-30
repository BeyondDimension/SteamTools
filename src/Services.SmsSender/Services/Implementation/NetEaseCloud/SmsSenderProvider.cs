using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Application.Models;
using System.Application.Models.Abstractions;
using System.Application.Models.NetEaseCloud;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSR = System.Application.Models.CheckSmsResult<System.Application.Models.NetEaseCloud.NetEaseCloudResult>;
using SmsOptions = System.Application.Models.NetEaseCloud.SmsOptions;
using SSR = System.Application.Models.SendSmsResult<System.Application.Models.NetEaseCloud.SendSmsNetEaseCloudResult>;
using SSRR = System.Application.Models.NetEaseCloud.SendSmsNetEaseCloudResult;

/*
返回说明
http 响应:json
发送成功则返回相关信息。msg字段表示此次发送的sendid；obj字段表示此次发送的验证码。
"Content-Type": "application/json; charset=utf-8"
{
"code": 200,
"msg": "88",
"obj": "1908"
}
主要的返回码
200、315、403、414、416、500
200	操作成功
315	IP限制
403	非法操作或没有权限
414	参数错误
416	频率控制
500	服务器内部错误
 */

namespace System.Application.Services.Implementation.NetEaseCloud
{
    /// <summary>
    /// 短信服务（网易云实现） 需要实现校验接口
    /// <para>参考资料：</para>
    /// <para>http://dev.netease.im/docs/product/%E7%9F%AD%E4%BF%A1/%E7%9F%AD%E4%BF%A1%E6%8E%A5%E5%8F%A3%E6%8C%87%E5%8D%97</para>
    /// <para>http://dev.netease.im/docs/product/IM%E5%8D%B3%E6%97%B6%E9%80%9A%E8%AE%AF/%E6%9C%8D%E5%8A%A1%E7%AB%AFAPI%E6%96%87%E6%A1%A3?#接口概述</para>
    /// </summary>
    public class SmsSenderProvider : SmsSenderBase, ISmsSender
    {
        public const string Name = nameof(NetEaseCloud);

        public override string Channel => Name;

        public override bool SupportCheck => true;

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

        #region 常量

        /// <summary>
        /// 接口地址。
        /// </summary>
        const string SmsSendApiUrl = "https://api.netease.im/sms/sendcode.action";

        const string SmsCheckApiUrl = "https://api.netease.im/sms/verifycode.action";

        #endregion 常量

        #region helpers

        /// <summary>
        /// 当前UTC时间戳，从1970年1月1日0点0 分0 秒开始到现在的秒数(String)
        /// </summary>
        /// <returns></returns>
        static string CurTime() => UnixTimestamp.ToTimestampS(DateTime.UtcNow).ToString();

        static string Nonce() => Guid.NewGuid().ToStringN() + DateTime.Now.Ticks;

        async Task<T> PostAsync<T, TResult>(string requestUri, Dictionary<string, string?> args, CancellationToken cancellationToken)
           where T : SmsResult<TResult, T>, new()
           where TResult : NetEaseCloudResult<TResult>
        {
            var nonce = Nonce();
            var curTime = CurTime();
            var checkSum = Hashs.String.SHA1(options.AppSecret + nonce + curTime);
            // ↑ SHA1(AppSecret + Nonce + CurTime),三个参数拼接的字符串，
            // 进行SHA1哈希计算，转化成16进制字符(String，小写)。
#pragma warning disable CS8620 // 由于引用类型的可为 null 性差异，实参不能用于形参。
            var content = new FormUrlEncodedContent(args);
#pragma warning restore CS8620 // 由于引用类型的可为 null 性差异，实参不能用于形参。
            content.Headers.ContentType.ThrowIsNull(nameof(content.Headers.ContentType)).CharSet = "utf-8";
            // ↑ application/x-www-form-urlencoded;charset=utf-8

            var request_args = new
            {
                body = args,
                headers = new Dictionary<string, string> {
                    { nameof(options.AppKey), options.AppKey.ThrowIsNull(nameof(options.AppKey)) },
                    { nameof(Nonce), nonce },
                    { nameof(CurTime), curTime },
                    { "CheckSum", checkSum },
                },
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content,
            };

            foreach (var header in request_args.headers)
                request.Headers.Add(header.Key, header.Value);

            var isSuccess = false;
            TResult? jsonObject = null;
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var json = new JsonTextReader(reader);
                jsonObject = jsonSerializer.Deserialize<TResult?>(json);
                isSuccess = jsonObject != default && jsonObject.IsOK();
            }

            return new T
            {
                HttpStatusCode = (int)response.StatusCode,
                IsSuccess = isSuccess,
                Result = jsonObject,
                ResultObject = jsonObject,
            };
        }

        #endregion helpers

        public override async Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken)
        {
            var dictionary = new Dictionary<string, string?> { { "mobile", number } };
            var template_id = options.Templates?.FirstOrDefault(x => x.Type == type)?.Template ?? options.DefaultTemplate;
            if (template_id.HasValue)
                dictionary.Add("templateid", template_id.Value.ToString());
            dictionary.Add("authCode", message);

            var result = await PostAsync<SSR, SSRR>(SmsSendApiUrl, dictionary, cancellationToken);
            if (!result.IsSuccess)
            {
                logger.LogError(
                    $"调用网易云短信接口失败，" +
                    $"手机号码：{PhoneNumberHelper.ToStringHideMiddleFour(number)}，" +
                    $"短信内容：{message}，" +
                    $"短信类型：{type}，" +
                    $"HTTP状态码：{result.HttpStatusCode}");
            }

            return result;
        }

        public override async Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken)
        {
            var dictionary = new Dictionary<string, string?> { { "mobile", number }, { "code", message } };

            var result = await PostAsync<CSR, NetEaseCloudResult>(SmsCheckApiUrl, dictionary, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Result != default && result.Result.IsCheckSmsFail())
                {
                    result.IsCheckSuccess = true;
                }
                else
                {
                    logger.LogError(
                        $"调用网易云短信验证接口失败，" +
                        $"手机号码：{PhoneNumberHelper.ToStringHideMiddleFour(number)}，" +
                        $"短信内容：{message}，" +
                        $"HTTP状态码：{result.HttpStatusCode}");
                }
            }

            return result;
        }
    }
}