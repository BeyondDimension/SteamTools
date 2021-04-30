namespace System.Application.Models._21VianetBlueCloud
{
    public class SmsOptions : IExplicitHasValue
    {
        /// <summary>
        /// ccs account name
        /// </summary>
        public string? Account { get; set; }

        /// <summary>
        /// 密钥名称
        /// </summary>
        public string? KeyName { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        public string? KeyValue { get; set; }

        /// <summary>
        /// 下发扩展码，两位纯数字
        /// </summary>
        public string ExtendCode { get; set; } = "08";

        /// <summary>
        /// 短信验证码值在模板中的变量名
        /// </summary>
        public string CodeTemplateKeyName { get; set; } = "code";

        public string? DefaultTemplate { get; set; }

        /// <summary>
        /// 开发者平台分配的模板标志
        /// </summary>
        public SmsOptionsTemplateId<string>[]? Templates { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Account) &&
                !string.IsNullOrWhiteSpace(ExtendCode) &&
                !string.IsNullOrWhiteSpace(KeyName) &&
                !string.IsNullOrWhiteSpace(KeyValue) &&
                (Templates.Any_Nullable() || DefaultTemplate != default);
        }

        bool IExplicitHasValue.ExplicitHasValue() => IsValid();
    }
}