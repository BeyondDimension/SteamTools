namespace System.Application.Columns
{
    /// <summary>
    /// 短信验证码
    /// </summary>
    public interface ISmsCode
    {
        /// <inheritdoc cref="ISmsCode"/>
        string? SmsCode { get; set; }
    }

    /// <inheritdoc cref="ISmsCode"/>
    public interface IReadOnlySmsCode
    {
        /// <inheritdoc cref="ISmsCode"/>
        string? SmsCode { get; }
    }
}