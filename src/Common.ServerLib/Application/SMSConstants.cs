namespace System.Application
{
    public static class SMSConstants
    {
        /// <summary>
        /// （单位秒）短信发送有效期（10分钟）+1min 。
        /// </summary>
        public const ushort SmsSendPeriodValidity = 660;

        /// <summary>
        /// 同一个手机号码一天发送短信验证码最大次数
        /// </summary>
        public const byte MaxSendSmsDay = 10;

        /// <summary>
        /// 最大校验失败作废次数
        /// </summary>
        public const int MaxCheckFailuresCount = 20;

        /// <summary>
        /// （单位秒）发送短信过于频繁，距离上一次发送的时间+此秒值大于等于现在则返回错误提示
        /// </summary>
        public const ushort SmsSendTooFrequently = 59;
    }
}