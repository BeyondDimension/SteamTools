using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 电话服务
    /// </summary>
    public interface ITelephonyService
    {
        public static ITelephonyService Instance => DI.Get<ITelephonyService>();

        /// <summary>
        /// 获取当前设备的手机号码(Only Android)
        /// </summary>
        /// <returns></returns>
        //[SupportedOSPlatform("Android")]
        Task<string?> GetPhoneNumberAsync();

        /// <summary>
        /// 设置自动填充当前设备的手机号码值(Only Android)
        /// </summary>
        /// <param name="textBoxText"></param>
        /// <returns></returns>
        //[SupportedOSPlatform("Android")]
        public static async Task<string?> GetAutoFillPhoneNumberAsync(string? textBoxText)
        {
            var value = await Instance.GetPhoneNumberAsync();
            if ((!string.IsNullOrWhiteSpace(value)) &&
                (string.IsNullOrWhiteSpace(textBoxText) ||
                    (textBoxText.Length < value.Length &&
                        value.Substring(0, textBoxText.Length) == textBoxText)))
            {
                return value;
            }
            return textBoxText;
        }
    }
}