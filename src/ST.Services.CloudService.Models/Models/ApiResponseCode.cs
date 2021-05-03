using System.Net;

namespace System.Application.Models
{
    /// <summary>
    /// MAS 定义响应码
    /// <para>200~299代表成功，三位数对应HTTP状态码，四位数为本业务自定义响应码</para>
    /// </summary>
    public enum ApiResponseCode
    {
        #region HttpStatusCode 100 ~ 511

        /// <summary>
        /// 成功
        /// </summary>
        OK = HttpStatusCode.OK,

        Unauthorized = HttpStatusCode.Unauthorized,

        NotFound = HttpStatusCode.NotFound,

        BadRequest = HttpStatusCode.BadRequest,

        InternalServerError = HttpStatusCode.InternalServerError,

        #endregion

        #region CustomCode 自定义响应码，1000~?

        /// <summary>
        /// 无响应正文内容
        /// </summary>
        NoResponseContent = 1000,

        /// <summary>
        /// 无响应正文内容值, <see cref="ExplicitHasValueExtensions.HasValue(IExplicitHasValue?)"/> 验证失败
        /// </summary>
        NoResponseContentValue,

        /// <summary>
        /// 失败
        /// </summary>
        Fail,

        /// <summary>
        /// 客户端反序列化失败
        /// </summary>
        ClientDeserializeFail,

        /// <summary>
        /// 不支持的响应媒体类型
        /// </summary>
        UnsupportedResponseMediaType,

        /// <summary>
        /// 不支持的上传文件媒体类型
        /// </summary>
        UnsupportedUploadFileMediaType,

        /// <summary>
        /// 客户端抛出异常
        /// </summary>
        ClientException,

        /// <summary>
        /// 被取消
        /// </summary>
        Canceled,

        /// <summary>
        /// 找不到HTTP请求授权头
        /// </summary>
        MissingAuthorizationHeader,

        /// <summary>
        /// HTTP请求授权声明不正确
        /// </summary>
        AuthSchemeNotCorrect,

        /// <summary>
        /// 用户被封禁
        /// </summary>
        UserIsBan,

        /// <summary>
        /// 找不到用户
        /// </summary>
        UserNotFound,

        /// <summary>
        /// 文件上传失败，缺少需要上传的有效文件
        /// </summary>
        LackAvailableUploadFile,

        /// <summary>
        /// 文件上传失败，上传的文件数与服务端响应结果数不相等
        /// </summary>
        UnequalLengthUploadFile,

        /// <summary>
        /// 请求模型验证失败
        /// </summary>
        RequestModelValidateFail,

        /// <summary>
        /// 网络连接中断
        /// </summary>
        NetworkConnectionInterruption,

        /// <summary>
        /// 请求头中缺少 User-Agent
        /// </summary>
        EmptyUserAgent,

        /// <summary>
        /// 必须使用安全传输模式
        /// </summary>
        RequiredSecurityKey,

        /// <summary>
        /// 当前运行程序不为官方渠道包
        /// </summary>
        IsNotOfficialChannelPackage,

        /// <summary>
        /// 客户端版本已弃用，需要更新版本
        /// </summary>
        AppObsolete,

        RequiredWebViwe3,

        /// <summary>
        /// 短信服务故障
        /// </summary>
        SMSServerError = 5001,

        #endregion
    }
}