// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// Returns whether local user verification is currently available, without prompting.
    /// Windows Hello may use a PIN rather than biometrics. Availability is not verification.
    /// </summary>
    /// <remarks>On Windows this requires an interactive session and build 22000 or later. Availability can change before the request.</remarks>
    ValueTask<bool> IsSupportedBiometricAsync() => new(false);

    /// <summary>
    /// Requests OS verification of the local user. Only <see cref="LocalUserVerificationResult.Verified"/> is success.
    /// This is a consent check, not a credential or a way to decrypt data.
    /// A caller that can abandon the request should use the cancellation-token overload and discard stale results.
    /// </summary>
    /// <param name="owner">The live top-level native window handle from the current process (an HWND on Windows). Call from its UI thread.</param>
    /// <param name="message">The prompt text shown by the OS. Do not include secrets.</param>
    ValueTask<LocalUserVerificationResult> VerifyLocalUserAsync(nint owner, string message) =>
        VerifyLocalUserAsync(owner, message, CancellationToken.None);

    /// <inheritdoc cref="VerifyLocalUserAsync(nint, string)"/>
    /// <param name="cancellationToken">Cancel this request, including when its owner closes or the caller no longer needs verification.</param>
    ValueTask<LocalUserVerificationResult> VerifyLocalUserAsync(nint owner, string message, CancellationToken cancellationToken) =>
        new(cancellationToken.IsCancellationRequested ? LocalUserVerificationResult.Canceled : LocalUserVerificationResult.Unavailable);
}

public enum LocalUserVerificationResult
{
    /// <summary>No supported verification path or owner, or verification is currently unavailable.</summary>
    Unavailable = 0,

    /// <summary>The OS explicitly verified the local user.</summary>
    Verified,

    /// <summary>The prompt was dismissed, the request canceled, or its owner was lost.</summary>
    Canceled,

    /// <summary>Verification failed, including exhausted retries or an unexpected platform error.</summary>
    Failed,
}