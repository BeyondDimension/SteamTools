using Microsoft.EntityFrameworkCore;
using System.Application.Entities;
using System.Application.Services;
using System.Linq;
using System.Threading.Tasks;
using SMS = System.Application.SMSConstants;

namespace System.Application.Repositories.Implementation
{
    public abstract class AuthMessageRecordRepository<TDbContext, TAuthMessageRecord, TSendSmsRequestType, TAuthMessageType> : Repository<TDbContext, TAuthMessageRecord, Guid>, IAuthMessageRecordRepository<TAuthMessageRecord, TSendSmsRequestType, TAuthMessageType>
        where TDbContext : DbContext
        where TAuthMessageRecord : AuthMessageRecord<TSendSmsRequestType, TAuthMessageType>
        where TSendSmsRequestType : struct, Enum
        where TAuthMessageType : struct, Enum
    {
        public AuthMessageRecordRepository(TDbContext dbContext) : base(dbContext)
        {
        }

        protected abstract TAuthMessageType GetDefaultAuthMessageType();

        /// <summary>
        /// <see langword="true"/> IsPhoneNumber
        /// <para>|</para>
        /// <see langword="false"/> IsEmail
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected abstract bool IsPhoneNumberOrEmail(TAuthMessageType type);

        public async Task<TAuthMessageRecord?> GetMostRecentVerificationCodeWithoutChecksumAndMoDiscard(TAuthMessageType type, string phoneNumberOrEmail, TSendSmsRequestType useType)
        {
            var _30_minutes_before = DateTimeOffset.Now.AddSeconds(-SMS.SmsSendPeriodValidity); // 30分钟之前
            var query = Entity.OrderByDescending(x => x.CreationTime) // 时间倒排序
                .Where(x => x.RequestType.Equals(useType) && x.CreationTime >= _30_minutes_before && !x.Abandoned && !x.CheckSuccess && x.Type.Equals(type));
            // 过滤条件 30分钟之前创建的短信 没有废弃的(!Abandoned) 并且是 没有成功的(!CheckSuccess)

            TAuthMessageRecord? item = default;

            var isPhoneNumberOrEmail = IsPhoneNumberOrEmail(type);

            if (isPhoneNumberOrEmail)
            {
                item = await query.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumberOrEmail);
            }
            else
            {
                item = await query.FirstOrDefaultAsync(x => x.Email == phoneNumberOrEmail);
            }

            return item;
        }

        public async Task<TAuthMessageRecord?> CheckAuthMessageAsync(ISmsSender smsSender, string phoneNumberOrEmail, string message, TSendSmsRequestType useType, TAuthMessageType? type = null)
        {
            var type_ = type ?? GetDefaultAuthMessageType();

            var lastEffectiveRecord = await GetMostRecentVerificationCodeWithoutChecksumAndMoDiscard(type_, phoneNumberOrEmail, useType);
            if (lastEffectiveRecord == null || lastEffectiveRecord.Content != message) return null;
            // ↑ 上一条有效纪录不存在或者验证码值不相同，返回null

            if (!lastEffectiveRecord.EverCheck) // 如果这条纪录没有校验过
            {
                lastEffectiveRecord.EverCheck = true; // 设置此条纪录已经校验过
            }

            if (lastEffectiveRecord.CheckFailuresCount >= SMS.MaxCheckFailuresCount)
            {
                lastEffectiveRecord.Abandoned = true;
            }
            else
            {
                if (IsPhoneNumberOrEmail(type_) && smsSender != null && smsSender.SupportCheck)
                {
                    /* var check_provide_result =*/
                    await smsSender.CheckSmsAsync(phoneNumberOrEmail, message);
                    // 第三方提供商要求调用验证短信接口，返回值可忽略，由内部实现纪录错误日志
                }

                lastEffectiveRecord.CheckSuccess = true;
            }

            if (!lastEffectiveRecord.CheckSuccess)
            {
                lastEffectiveRecord.CheckFailuresCount++;
                if (lastEffectiveRecord.CheckFailuresCount >= SMS.MaxCheckFailuresCount)
                {
                    lastEffectiveRecord.Abandoned = true; // 验证错误次数太多，此条纪录标记废弃
                }
            }

            await db.SaveChangesAsync();
            return lastEffectiveRecord;
        }

        public async Task<DateTimeOffset?> GetLastSendSmsTime(string phoneNumberOrEmail, TSendSmsRequestType? requestType = null, TAuthMessageType? type = null)
        {
            IQueryable<TAuthMessageRecord> query = Entity;

            if (requestType.HasValue)
            {
                var requestTypeValue = requestType.Value;
                query = query.Where(x => x.RequestType.Equals(requestTypeValue));
            }

            var type_ = type ?? GetDefaultAuthMessageType();

            var item = await query
                .OrderByDescending(x => x.CreationTime)
                .Select(x => new { x.PhoneNumber, x.CreationTime, x.Type })
                .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumberOrEmail && x.Type.Equals(type_));

            return item?.CreationTime;
        }

        public async Task<bool> IsMaxSendSmsDay(string phoneNum, byte? maxSendSmsDay = null, TAuthMessageType? type = null)
        {
            var type_ = type ?? GetDefaultAuthMessageType();
            var maxSendSmsDay_ = maxSendSmsDay ?? SMS.MaxSendSmsDay;
            var today = DateTimeOffset.Now.Date;
            var tomorrow = today.AddDays(1);

            var count = await Entity
                .CountAsync(x => x.PhoneNumber == phoneNum &&
                x.CreationTime >= today &&
                x.CreationTime < tomorrow &&
                x.Type.Equals(type_));

            return count > maxSendSmsDay_;
        }
    }
}