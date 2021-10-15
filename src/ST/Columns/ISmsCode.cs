namespace System.Application.Columns
{
    /// <summary>
    /// 列 - 短信验证码
    /// </summary>
    public interface ISmsCode
    {
        /// <summary>
        /// 短信验证码
        /// </summary>
        string? SmsCode { get; set; }
    }

    /// <summary>
    /// 列(只读) - 短信验证码
    /// </summary>
    public interface IReadOnlySmsCode
    {
        /// <inheritdoc cref="ISmsCode.SmsCode"/>
        string? SmsCode { get; }
    }
}