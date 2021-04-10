namespace System.Application.Services
{
    public class InputLengthConstants
    {
        public static InputLengthConstants Current { get; } = new();

        /// <inheritdoc cref="ModelValidatorProvider.Lengths.PhoneNumber"/>
        public int PhoneNumber => ModelValidatorProvider.Lengths.PhoneNumber;

        /// <inheritdoc cref="ModelValidatorProvider.Lengths.NickName"/>
        public int NickName => ModelValidatorProvider.Lengths.NickName;

        /// <inheritdoc cref="ModelValidatorProvider.Lengths.SMS_CAPTCHA"/>
        public int SMS_CAPTCHA => ModelValidatorProvider.Lengths.SMS_CAPTCHA;
    }
}