using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ITelephonyService"/>
    internal sealed class PlatformTelephonyServiceImpl : ITelephonyService
    {
        public Task<string?> GetPhoneNumberAsync()
        {
            // iOS 不支持读取当前手机号码
            return Task.FromResult((string?)null);
        }
    }
}