using System.Application.Models;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 短信发送服务
    /// </summary>
    public interface ISmsSender
    {
        /// <summary>
        /// [通过手机号码注册账号]用于注册
        /// </summary>
        const int SendSmsRequestType_Register = 200;
        /// <summary>
        /// [通过手机号码重设密码]找回密码
        /// </summary>
        const int SendSmsRequestType_ForgotPassword = 201;
        /// <summary>
        /// [换新手机号要旧手机号短信验证]绑定新手机号
        /// </summary>
        const int SendSmsRequestType_ChangePhoneNumberNew = 202;
        /// <summary>
        /// [新手机号的短信验证]换绑手机（安全验证）
        /// </summary>
        const int SendSmsRequestType_ChangePhoneNumberValidation = 203;

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="number">收信人手机号码。</param>
        /// <param name="message">发送的短信内容。</param>
        /// <param name="type">发送短信用途。</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ISendSmsResult> SendSmsAsync(string number, string message, int type, CancellationToken cancellationToken = default);

        /// <summary>
        /// 校验短信验证码，某些平台要求校验短信也要调用他提供的接口
        /// </summary>
        /// <param name="number">收信人手机号码。</param>
        /// <param name="message">短信验证码内容。</param>
        /// <returns></returns>
        Task<ICheckSmsResult> CheckSmsAsync(string number, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// 短信提供渠道显示名称
        /// </summary>
        string Channel { get; }

        /// <summary>
        /// 短信提供渠道是否支持校验短信验证码
        /// </summary>
        bool SupportCheck { get; }

        /// <summary>
        /// 生成短信随机验证码
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns></returns>
        string GenerateRandomNum(int length);
    }
}