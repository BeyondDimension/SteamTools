using System.Security.Cryptography;
using BD.Common.Enums;
using BD.Common.Services;
using BD.WTTS.Repositories.Abstractions;
using static BD.WTTS.Repositories.Abstractions.IAccountPlatformAuthenticatorRepository;

namespace BD.WTTS.Repositories;

// V1: magic | version | mode | purpose | [16-byte salt] | body.
// Body before optional local protection: [32-byte key for local-only] | nonce | tag | ciphertext.
// GCM authenticates the entire header. When a password is set, the key is never in the body.
internal static class AuthenticatorEnvelope
{
    const byte Version = 1;
    const int HeaderLength = 7;
    const int SaltLength = 16;
    const int KeyLength = 32;
    const int NonceLength = 12;
    const int TagLength = 16;
    const int Iterations = 600_000;
    static ReadOnlySpan<byte> Magic => "WTA!"u8;
    const int LegacyMachineAes = 1; // LocalDataProtectionProviderBase.LocalDataProtectionType.AesCFB

    static bool HasMachineWrapper(ReadOnlySpan<byte> data) => data.Length > 2 * sizeof(int) &&
        BitConverter.ToInt32(data) == (int)EncryptionMode.EmbeddedAesWithLocal &&
        BitConverter.ToInt32(data[sizeof(int)..]) == LegacyMachineAes;

    [Flags]
    enum Mode : byte
    {
        Password = 1,
        WindowsUser = 2,
        Machine = 4,
        WindowsHello = 8, // DPAPI CurrentUser envelope; the app must also verify the user.
    }

    internal enum Purpose : byte
    {
        Name = 1,
        Value = 2,
    }

    internal static bool IsEnvelope(byte[]? data) => data.AsSpan().StartsWith(Magic);

    internal static bool RequiresWindowsHello(byte[]? data) => data is { Length: > HeaderLength } &&
        IsEnvelope(data) && (data[5] & (byte)Mode.WindowsHello) != 0;

    internal static byte[] NewSalt() => RandomNumberGenerator.GetBytes(SaltLength);

    static ReadOnlySpan<byte> ReadSalt(byte[]? data) =>
        IsEnvelope(data) && data!.Length >= HeaderLength + SaltLength && (data[5] & (byte)Mode.Password) != 0
            ? data.AsSpan(HeaderLength, SaltLength)
            : default;

    internal static byte[]? PasswordSalt(byte[]? data)
    {
        var salt = ReadSalt(data);
        return salt.IsEmpty ? null : salt.ToArray();
    }

    internal static bool SharePasswordSalt(byte[]? left, byte[]? right)
    {
        var salt = ReadSalt(left);
        return !salt.IsEmpty && salt.SequenceEqual(ReadSalt(right));
    }

    internal sealed class Keys(string? password, bool verifiedWithHello = false) : IDisposable
    {
        // Small operation-local cache: refuse to spend an unbounded number of KDFs on untrusted salts.
        const int MaxSalts = 8;
        readonly Dictionary<Guid, byte[]> keys = new();
        internal bool VerifiedWithHello { get; } = verifiedWithHello;


        internal byte[] Get(ReadOnlySpan<byte> salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new AuthenticatorReadException(ImportResultCode.SecondaryPasswordFail);
            var saltId = new Guid(salt);
            if (keys.TryGetValue(saltId, out var key))
                return key;
            if (keys.Count == MaxSalts)
                throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);

            key = new byte[KeyLength];
            try
            {
                Rfc2898DeriveBytes.Pbkdf2(password.AsSpan(), salt, key, Iterations, HashAlgorithmName.SHA256);
                keys.Add(saltId, key);
                return key;
            }
            catch
            {
                CryptographicOperations.ZeroMemory(key);
                throw;
            }
        }

        public void Dispose()
        {
            foreach (var key in keys.Values)
                CryptographicOperations.ZeroMemory(key);
            keys.Clear();
            password = null;
        }
    }

    internal static async ValueTask<byte[]> SealAsync(ISecurityService security, byte[] plaintext, Purpose purpose,
        bool isLocal, byte[]? salt, Keys keys, bool windowsHello = false)
    {
        var mode = salt == null ? default : Mode.Password;
        if (isLocal)
            mode |= OperatingSystem.IsWindows() ? Mode.WindowsUser : Mode.Machine;
        if (windowsHello)
        {
            if (mode != Mode.WindowsUser || !keys.VerifiedWithHello)
                throw new InvalidOperationException("Windows Hello requires verified current-user protection without a password.");
            mode |= Mode.WindowsHello;
        }
        if (mode == 0)
            throw new ArgumentException("An envelope requires a password or local binding.");

        var headerLength = HeaderLength + (salt == null ? 0 : SaltLength);
        var keyPrefix = salt == null ? KeyLength : 0;
        var envelope = new byte[headerLength + (isLocal ? 0 : NonceLength + TagLength + plaintext.Length)];
        Magic.CopyTo(envelope);
        envelope[4] = Version;
        envelope[5] = (byte)mode;
        envelope[6] = (byte)purpose;
        salt?.CopyTo(envelope, HeaderLength);
        var body = isLocal ? new byte[keyPrefix + NonceLength + TagLength + plaintext.Length] : envelope;
        var offset = isLocal ? 0 : headerLength;
        try
        {
            if (keyPrefix != 0)
                RandomNumberGenerator.Fill(body.AsSpan(0, KeyLength));
            var key = salt == null ? body.AsSpan(0, KeyLength) : keys.Get(salt).AsSpan();
            var nonceOffset = offset + keyPrefix;
            RandomNumberGenerator.Fill(body.AsSpan(nonceOffset, NonceLength));
            using (var aes = new AesGcm(key, TagLength))
                aes.Encrypt(body.AsSpan(nonceOffset, NonceLength), plaintext,
                    body.AsSpan(nonceOffset + NonceLength + TagLength), body.AsSpan(nonceOffset + NonceLength, TagLength),
                    envelope.AsSpan(0, headerLength));
            if (!isLocal)
                return envelope;

            var wrapped = (mode & Mode.WindowsUser) != 0
                ? AuthenticatorCurrentUserProtection.Transform(body, protect: true)
                : await security.EB(body, EncryptionMode.EmbeddedAesWithLocal).ConfigureAwait(false);
            if (wrapped == null || wrapped.Length == 0 || ReferenceEquals(wrapped, body) ||
                ((mode & Mode.Machine) != 0 && !HasMachineWrapper(wrapped)))
                throw new CryptographicException("Authenticator local protection failed.");
            Array.Resize(ref envelope, headerLength + wrapped.Length);
            wrapped.CopyTo(envelope, headerLength);
            return envelope;
        }
        finally
        {
            if (isLocal)
                CryptographicOperations.ZeroMemory(body);
        }
    }

    internal static async ValueTask<byte[]> OpenAsync(ISecurityService security, byte[]? envelope, Purpose purpose,
        bool isLocal, bool hasPassword, Keys keys)
    {
        if (envelope == null || !IsEnvelope(envelope) || envelope.Length <= HeaderLength ||
            envelope[4] != Version || envelope[6] != (byte)purpose)
            throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);

        var mode = (Mode)envelope[5];
        var localMode = mode & (Mode.WindowsUser | Mode.Machine);
        var windowsHello = (mode & Mode.WindowsHello) != 0;
        if ((mode & ~(Mode.Password | Mode.WindowsUser | Mode.Machine | Mode.WindowsHello)) != 0 ||
            localMode == (Mode.WindowsUser | Mode.Machine) || (localMode != 0) != isLocal ||
            ((mode & Mode.Password) != 0) != hasPassword || mode == 0 ||
            (windowsHello && mode != (Mode.WindowsUser | Mode.WindowsHello)))
            throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
        if (windowsHello && !keys.VerifiedWithHello)
            throw new AuthenticatorReadException(ImportResultCode.WindowsHelloRequired);
        var headerLength = HeaderLength + (hasPassword ? SaltLength : 0);
        if (envelope.Length <= headerLength)
            throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);

        var body = envelope;
        var offset = headerLength;
        var keyPrefix = hasPassword ? 0 : KeyLength;
        try
        {
            if (isLocal)
            {
                var wrapped = envelope.AsSpan(headerLength);
                if (localMode == Mode.WindowsUser)
                {
                    try
                    {
                        body = AuthenticatorCurrentUserProtection.Transform(wrapped, protect: false);
                    }
                    catch (Exception ex) when (ex is CryptographicException or NotSupportedException or DllNotFoundException or EntryPointNotFoundException)
                    {
                        throw new AuthenticatorReadException(ImportResultCode.LocalFail);
                    }
                }
                else
                {
                    if (OperatingSystem.IsWindows())
                        throw new AuthenticatorReadException(ImportResultCode.LocalFail);
                    // Reject the legacy local provider's pass-through subtype as well as other wrappers.
                    if (!HasMachineWrapper(wrapped))
                        throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
                    var (unwrapped, code) = await security.DB2(wrapped.ToArray()).ConfigureAwait(false);
                    if (code != BD.Common.Services.ISecurityService.DResultCode.Success || unwrapped == null)
                        throw new AuthenticatorReadException(ImportResultCode.LocalFail);
                    body = unwrapped;
                }
                offset = 0;
            }

            if (body.Length - offset < keyPrefix + NonceLength + TagLength)
                throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
            var key = hasPassword ? keys.Get(envelope.AsSpan(HeaderLength, SaltLength)).AsSpan() : body.AsSpan(0, KeyLength);
            var nonceOffset = offset + keyPrefix;
            var plaintext = new byte[body.Length - nonceOffset - NonceLength - TagLength];
            try
            {
                using var aes = new AesGcm(key, TagLength);
                aes.Decrypt(body.AsSpan(nonceOffset, NonceLength), body.AsSpan(nonceOffset + NonceLength + TagLength),
                    body.AsSpan(nonceOffset + NonceLength, TagLength), plaintext, envelope.AsSpan(0, headerLength));
                return plaintext;
            }
            catch (AuthenticationTagMismatchException)
            {
                CryptographicOperations.ZeroMemory(plaintext);
                // A password-derived authentication failure cannot distinguish damage from a wrong password.
                throw new AuthenticatorReadException(hasPassword ? ImportResultCode.SecondaryPasswordFail : ImportResultCode.IncorrectFormat);
            }
            catch
            {
                CryptographicOperations.ZeroMemory(plaintext);
                throw;
            }
        }
        finally
        {
            if (!ReferenceEquals(body, envelope))
                CryptographicOperations.ZeroMemory(body);
        }
    }
}
