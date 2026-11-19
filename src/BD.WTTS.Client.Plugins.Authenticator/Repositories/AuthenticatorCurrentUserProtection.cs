using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace BD.WTTS.Repositories;

// Direct DPAPI interop keeps the plugin Native AOT compatible without a package dependency.
internal static partial class AuthenticatorCurrentUserProtection
{
    const uint UiForbidden = 1; // No CRYPTPROTECT_LOCAL_MACHINE: DPAPI CurrentUser scope.

    [StructLayout(LayoutKind.Sequential)]
    struct DataBlob
    {
        public int Length;
        public nint Data;
    }

    [LibraryImport("crypt32.dll")]
    private static partial int CryptProtectData(ref DataBlob input, nint description, nint entropy,
        nint reserved, nint prompt, uint flags, out DataBlob output);

    [LibraryImport("crypt32.dll")]
    private static partial int CryptUnprotectData(ref DataBlob input, nint description, nint entropy,
        nint reserved, nint prompt, uint flags, out DataBlob output);

    [LibraryImport("kernel32.dll")]
    private static partial nint LocalFree(nint memory);

    internal static unsafe byte[] Transform(ReadOnlySpan<byte> data, bool protect)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();

        fixed (byte* ptr = data)
        {
            var input = new DataBlob { Length = data.Length, Data = (nint)ptr };
            DataBlob output;
            var success = protect
                ? CryptProtectData(ref input, 0, 0, 0, 0, UiForbidden, out output)
                : CryptUnprotectData(ref input, 0, 0, 0, 0, UiForbidden, out output);
            try
            {
                if (success == 0 || output.Data == 0 || output.Length < 0)
                    throw new CryptographicException("Current-user data protection failed.");
                return new ReadOnlySpan<byte>((void*)output.Data, output.Length).ToArray();
            }
            finally
            {
                if (output.Data != 0)
                {
                    if (!protect && output.Length > 0)
                        CryptographicOperations.ZeroMemory(new Span<byte>((void*)output.Data, output.Length));
                    _ = LocalFree(output.Data);
                }
            }
        }
    }
}
