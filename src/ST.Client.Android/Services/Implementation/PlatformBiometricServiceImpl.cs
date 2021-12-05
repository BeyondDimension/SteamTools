using AndroidX.Biometric;
using System.Threading.Tasks;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.Services.Implementation
{
    internal sealed class PlatformBiometricServiceImpl : IBiometricService
    {
        public bool IsSupported
        {
            get
            {
                // https://developer.android.com/training/sign-in/biometric-auth#available
                var manager = BiometricManager.From(XEPlatform.AppContext);
                return manager?.CanAuthenticate(BiometricManager.Authenticators.BiometricStrong | BiometricManager.Authenticators.DeviceCredential) == BiometricManager.BiometricSuccess;
            }
        }

        ValueTask<bool> IBiometricService.IsSupportedAsync() => new(IsSupported);
    }
}