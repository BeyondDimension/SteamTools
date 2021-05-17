using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class NativeMethods
    {
        [DllImport("Avrt.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr AvSetMmThreadCharacteristics(string taskName, ref uint taskIndex);

        [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetProcessImageFileName(
            IntPtr hProcess,
            [Out] StringBuilder lpImageFileName,
            [In][MarshalAs(UnmanagedType.U4)] int nSize
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
             uint processAccess,
             bool bInheritHandle,
             int processId
        );
        public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags)
        {
            return OpenProcess((uint)flags, false, proc.Id);
        }

        public static string QueryFullProcessImageName(Process process)
        {
            int capacity = 1024;
            StringBuilder sb = new StringBuilder(capacity);
            IntPtr handle = OpenProcess(process, ProcessAccessFlags.QueryLimitedInformation);
            QueryFullProcessImageName(handle, 0, sb, ref capacity);
            string fullPath = sb.ToString(0, capacity);
            return fullPath;
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
    }
}