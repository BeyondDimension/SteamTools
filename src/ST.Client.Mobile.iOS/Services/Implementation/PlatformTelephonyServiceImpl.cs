using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ITelephonyService"/>
    internal sealed class PlatformTelephonyServiceImpl : ITelephonyService
    {
        public Task<string?> GetPhoneNumberAsync()
        {
            return Task.FromResult((string?)null);
        }
    }
}