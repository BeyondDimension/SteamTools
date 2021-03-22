namespace System.Application.Columns
{
    /// <inheritdoc cref="SmsCodeType"/>
    public interface ISmsCodeType
    {
        /// <inheritdoc cref="SmsCodeType"/>
        SmsCodeType SmsCodeType { get; set; }
    }

    /// <inheritdoc cref="SmsCodeType"/>
    public interface IReadOnlySmsCodeType
    {
        /// <inheritdoc cref="SmsCodeType"/>
        SmsCodeType SmsCodeType { get; }
    }
}