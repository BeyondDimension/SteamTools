namespace System.Application.Columns
{
    /// <summary>
    /// 手机号码
    /// </summary>
    public interface IPhoneNumber
    {
        /// <inheritdoc cref="IPhoneNumber"/>
        string? PhoneNumber { get; set; }

        public const int Db_MaxLength_PhoneNumber = 20;
    }

    /// <inheritdoc cref="IPhoneNumber"/>
    public interface IReadOnlyPhoneNumber
    {
        /// <inheritdoc cref="IPhoneNumber"/>
        string? PhoneNumber { get; }
    }
}