using SmsCodeTypeEnum = System.Application.SmsCodeType;

namespace System.Application.Columns
{
    /// <summary>
    /// 列 - 短信验证码类型
    /// </summary>
    public interface ISmsCodeType
    {
        /// <inheritdoc cref="SmsCodeTypeEnum"/>
        SmsCodeTypeEnum SmsCodeType { get; set; }
    }

    /// <summary>
    /// 列(只读) - 短信验证码类型
    /// </summary>
    public interface IReadOnlySmsCodeType
    {
        /// <inheritdoc cref="SmsCodeTypeEnum"/>
        SmsCodeTypeEnum SmsCodeType { get; }
    }
}