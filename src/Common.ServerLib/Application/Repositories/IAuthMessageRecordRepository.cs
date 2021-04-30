using System.Application.Entities;
using System.Application.Services;
using System.Threading.Tasks;

namespace System.Application.Repositories
{
    public interface IAuthMessageRecordRepository<TAuthMessageRecord, TSendSmsRequestType, TAuthMessageType> : IRepository<TAuthMessageRecord, Guid>
        where TAuthMessageRecord : AuthMessageRecord<TSendSmsRequestType, TAuthMessageType>
        where TSendSmsRequestType : struct, Enum
        where TAuthMessageType : struct, Enum
    {
        /// <summary>
        /// 获取最近的没有校验和没有废弃的一条验证码
        /// </summary>
        /// <param name="type">验证码类型</param>
        /// <param name="phoneNumberOrEmail">手机号码或邮箱地址</param>
        /// <param name="content">验证码内容</param>
        /// <returns></returns>
        Task<TAuthMessageRecord?> GetMostRecentVerificationCodeWithoutChecksumAndMoDiscard(
            TAuthMessageType type,
            string phoneNumberOrEmail,
            TSendSmsRequestType useType);

        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="smsSender"></param>
        /// <param name="phoneNumberOrEmail"></param>
        /// <param name="message"></param>
        /// <param name="useType"></param>
        /// <param name="type">验证码类型</param>
        /// <returns></returns>
        Task<TAuthMessageRecord?> CheckAuthMessageAsync(ISmsSender smsSender, string phoneNumberOrEmail, string message, TSendSmsRequestType useType, TAuthMessageType? type = null);

        /// <summary>
        /// 获取手机号上次发送验证码的时间
        /// </summary>
        /// <param name="phoneNumberOrEmail">手机号码</param>
        /// <param name="requestType">验证码请求类型</param>
        /// <param name="type">验证码类型</param>
        /// <returns></returns>
        Task<DateTimeOffset?> GetLastSendSmsTime(string phoneNumberOrEmail, TSendSmsRequestType? requestType = null, TAuthMessageType? type = null);

        /// <summary>
        /// 根据手机号获取今天是否超过了最大的短信次数限制
        /// </summary>
        /// <param name="phoneNum">手机号码</param>
        /// <param name="maxSendSmsDay">一天内最大发送次数</param>
        /// <param name="type">验证码类型</param>
        /// <returns></returns>
        Task<bool> IsMaxSendSmsDay(string phoneNum, byte? maxSendSmsDay = null, TAuthMessageType? type = null);
    }
}