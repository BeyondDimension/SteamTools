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
        /// 发送短信验证码
        /// </summary>
        /// <param name="number">收信人手机号码</param>
        /// <param name="message">发送的短信内容</param>
        /// <param name="type">发送短信用途</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ISendSmsResult> SendSmsAsync(string number, string message, ushort type, CancellationToken cancellationToken = default);

        /// <summary>
        /// 校验短信验证码，某些平台要求校验短信也要调用他提供的接口
        /// </summary>
        /// <param name="number">收信人手机号码</param>
        /// <param name="message">短信验证码内容</param>
        /// <param name="cancellationToken"></param>
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