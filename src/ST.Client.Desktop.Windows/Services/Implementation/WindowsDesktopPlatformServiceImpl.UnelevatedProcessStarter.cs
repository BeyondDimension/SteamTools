using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Application.Services.Implementation
{
    partial class WindowsDesktopPlatformServiceImpl
    {
        public void UnelevatedProcessStart(string cmdArgs)
        {
            UnelevatedProcessStarter.Start(cmdArgs);
        }

        /// <summary>
        /// https://stackoverflow.com/a/49997055/15855120
        /// </summary>
        static class UnelevatedProcessStarter
        {
            public static void Start(string cmdArgs)
            {
                // 1. Get the shell
                var shell = NativeMethods.GetShellWindow();
                if (shell == IntPtr.Zero)
                {
                    throw new Exception("Could not find shell window");
                }

                // 2. Copy the access token of the process
                NativeMethods.GetWindowThreadProcessId(shell, out uint shellProcessId);
                var hShellProcess = NativeMethods.OpenProcess(0x00000400 /* QueryInformation */, false, (int)shellProcessId);
                if (!NativeMethods.OpenProcessToken(hShellProcess, 2 /* TOKEN_DUPLICATE */, out IntPtr hShellToken))
                {
                    throw new Win32Exception();
                }

                // 3. Dublicate the acess token
                uint tokenAccess = 8 /*TOKEN_QUERY*/ | 1 /*TOKEN_ASSIGN_PRIMARY*/ | 2 /*TOKEN_DUPLICATE*/ | 0x80 /*TOKEN_ADJUST_DEFAULT*/ | 0x100 /*TOKEN_ADJUST_SESSIONID*/;
                var securityAttributes = new SecurityAttributes();

                NativeMethods.DuplicateTokenEx(
                    hShellToken,
                    tokenAccess,
                    ref securityAttributes,
                    2 /* SecurityImpersonation */,
                    1 /* TokenPrimary */,
                    out IntPtr hToken);

                // 4. Create a new process with the copied token
                var si = new Startupinfo();
                si.cb = Marshal.SizeOf(si);

                if (!NativeMethods.CreateProcessWithTokenW(
                    hToken,
                    0x00000002 /* LogonNetcredentialsOnly */,
                    null,
                    cmdArgs,
                    0x00000010 /* CreateNewConsole */,
                    IntPtr.Zero,
                    null,
                    ref si,
                    out ProcessInformation _))
                {
                    throw new Win32Exception();
                }
            }

            public class NativeMethods
            {
                [DllImport("user32.dll")]
                public static extern IntPtr GetShellWindow();
                [DllImport("user32.dll", SetLastError = true)]
                public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
                [DllImport("kernel32.dll", SetLastError = true)]
                public static extern IntPtr OpenProcess(int processAccess, bool bInheritHandle, int processId);
                [DllImport("advapi32.dll", SetLastError = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                public static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);
                [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
                public static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess,
                    ref SecurityAttributes lpTokenAttributes,
                    int impersonationLevel,
                    int tokenType,
                    out IntPtr phNewToken);
                [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
                public static extern bool CreateProcessWithTokenW(
                    IntPtr hToken, int dwLogonFlags,
                    string lpApplicationName, string lpCommandLine,
                    int dwCreationFlags, IntPtr lpEnvironment,
                    string lpCurrentDirectory, [In] ref Startupinfo lpStartupInfo, out ProcessInformation lpProcessInformation);
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct ProcessInformation
            {
                public IntPtr hProcess;
                public IntPtr hThread;
                public int dwProcessId;
                public int dwThreadId;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SecurityAttributes
            {
                public int nLength;
                public IntPtr lpSecurityDescriptor;
                public int bInheritHandle;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct Startupinfo
            {
                public int cb;
                public string lpReserved;
                public string lpDesktop;
                public string lpTitle;
                public int dwX;
                public int dwY;
                public int dwXSize;
                public int dwYSize;
                public int dwXCountChars;
                public int dwYCountChars;
                public int dwFillAttribute;
                public int dwFlags;
                public short wShowWindow;
                public short cbReserved2;
                public IntPtr lpReserved2;
                public IntPtr hStdInput;
                public IntPtr hStdOutput;
                public IntPtr hStdError;
            }
        }
    }
}