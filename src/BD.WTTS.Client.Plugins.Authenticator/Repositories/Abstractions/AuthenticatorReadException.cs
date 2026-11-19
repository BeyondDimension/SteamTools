using static BD.WTTS.Repositories.Abstractions.IAccountPlatformAuthenticatorRepository;

namespace BD.WTTS.Repositories.Abstractions;

/// <summary>A vault read failed; no partial result was returned.</summary>
public sealed class AuthenticatorReadException : Exception
{
    public ImportResultCode ResultCode { get; }

    public AuthenticatorReadException(ImportResultCode resultCode)
        : base($"Authenticator data could not be read ({resultCode}).")
    {
        ResultCode = resultCode;
    }
}
