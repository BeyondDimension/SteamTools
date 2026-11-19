#if WINDOWS
using Windows.Security.Credentials.UI;
using User32 = PInvoke.User32;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    // IUserConsentVerifierInterop is available starting with Windows build 22000,
    // even though the projection also exists in the 19041 targeting pack.
    static bool CanUseWindowsHello => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) && Environment.UserInteractive;

    public async ValueTask<bool> IsSupportedBiometricAsync()
    {
        if (!CanUseWindowsHello)
            return false;

        try
        {
            return await UserConsentVerifier.CheckAvailabilityAsync().AsTask().ConfigureAwait(false)
                == UserConsentVerifierAvailability.Available;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public ValueTask<LocalUserVerificationResult> VerifyLocalUserAsync(nint owner, string message) =>
        VerifyLocalUserAsync(owner, message, CancellationToken.None);

    public async ValueTask<LocalUserVerificationResult> VerifyLocalUserAsync(
        nint owner, string message, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return LocalUserVerificationResult.Canceled;

        if (!CanUseWindowsHello || owner == 0)
            return LocalUserVerificationResult.Unavailable;

        try
        {
            var ownerThreadId = GetVerificationOwnerThreadId(owner);
            if (ownerThreadId == 0)
                return LocalUserVerificationResult.Unavailable;

            if (cancellationToken.IsCancellationRequested)
                return LocalUserVerificationResult.Canceled;

            // Request consent in the current process. Do not retry through an elevated helper
            // or the ownerless UWP method if Windows rejects this request.
            var result = await UserConsentVerifierInterop.RequestVerificationForWindowAsync(owner, message)
                .AsTask(cancellationToken).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
                return LocalUserVerificationResult.Canceled;

            return result switch
            {
                UserConsentVerificationResult.Verified => GetVerificationOwnerThreadId(owner) == ownerThreadId
                    && !cancellationToken.IsCancellationRequested
                    ? LocalUserVerificationResult.Verified
                    : LocalUserVerificationResult.Canceled,
                UserConsentVerificationResult.Canceled => LocalUserVerificationResult.Canceled,
                UserConsentVerificationResult.DeviceNotPresent or
                UserConsentVerificationResult.NotConfiguredForUser or
                UserConsentVerificationResult.DisabledByPolicy or
                UserConsentVerificationResult.DeviceBusy => LocalUserVerificationResult.Unavailable,
                _ => LocalUserVerificationResult.Failed,
            };
        }
        catch (Exception ex)
        {
            return ex is OperationCanceledException || cancellationToken.IsCancellationRequested
                ? LocalUserVerificationResult.Canceled
                : LocalUserVerificationResult.Failed;
        }
    }

    static int GetVerificationOwnerThreadId(nint owner)
    {
        var threadId = User32.GetWindowThreadProcessId(owner, out int processId);
        return threadId != 0 && processId == Environment.ProcessId
            && User32.GetAncestor(owner, User32.GetAncestorFlags.GA_ROOT) == owner ? threadId : 0;
    }
}

#endif