using System.IO;
using System.Runtime.InteropServices;
#if NETSTANDARD
using JustArchiNET.Madness;
#else
using System.Runtime.Versioning;
#endif

namespace System.Application.Services
{
    partial interface IPlatformService
    {
        [Flags]
        [SupportedOSPlatform("FreeBSD")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("MacOS")]
        enum UnixPermission : ushort
        {
            OtherExecute = 0x1,
            OtherWrite = 0x2,
            OtherRead = 0x4,
            GroupExecute = 0x8,
            GroupWrite = 0x10,
            GroupRead = 0x20,
            UserExecute = 0x40,
            UserWrite = 0x80,
            UserRead = 0x100,
            Combined755 = UserRead | UserWrite | UserExecute | GroupRead | GroupExecute | OtherRead | OtherExecute,
            Combined777 = UserRead | UserWrite | UserExecute | GroupRead | GroupWrite | GroupExecute | OtherRead | OtherWrite | OtherExecute
        }

        [SupportedOSPlatform("FreeBSD")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("MacOS")]
        UnixSetFileAccessResult UnixSetFileAccess(string? path, UnixPermission permission)
        {
            if (!OperatingSystem2.IsFreeBSD &&
                !OperatingSystem2.IsLinux &&
                !OperatingSystem2.IsMacOS)
                throw new PlatformNotSupportedException();

            if (string.IsNullOrEmpty(path))
                return UnixSetFileAccessResult.PathIsNullOrEmpty;

            if (!File.Exists(path) && !Directory.Exists(path))
                return UnixSetFileAccessResult.PathInvalid;

            var result = Chmod(path, (int)permission);
            if (result == 0) return UnixSetFileAccessResult.Success;

            var lastError = Marshal.GetLastWin32Error();
            Log.Error(TAG,
                "UnixSetFileAccess Fail, permission:{3}, result: {0}, lastError: {1}, path: {2}",
                result,
                lastError,
                path,
                permission);

            return (UnixSetFileAccessResult)result;
        }

#if !NETSTANDARD
#pragma warning disable CA2101 // False positive, we can't use unicode charset on Unix, and it uses UTF-8 by default anyway
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <returns>0 on success, -1 on failure</returns>
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
        [SupportedOSPlatform("FreeBSD")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("MacOS")]
        private static extern int Chmod(string path, int mode);
#if !NETSTANDARD
#pragma warning restore CA2101 // False positive, we can't use unicode charset on Unix, and it uses UTF-8 by default anyway
#endif

        [SupportedOSPlatform("FreeBSD")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("MacOS")]
        enum UnixSetFileAccessResult
        {
            PathIsNullOrEmpty = -3,
            PathInvalid,
            Failure,
            Success,
        }
    }
}