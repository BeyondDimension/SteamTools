using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Titanium.Web.Proxy.Helpers
{
    internal partial class NativeMethods
    {
        internal const int AfInet = 2;
        internal const int AfInet6 = 23;

        /// <summary>
        ///     <see href="http://msdn2.microsoft.com/en-us/library/aa365928.aspx" />
        /// </summary>
        [DllImport("iphlpapi.dll", SetLastError = true)]
        internal static extern uint GetExtendedTcpTable(IntPtr tcpTable, ref int size, bool sort, int ipVersion,
            int tableClass, int reserved);

        internal enum TcpTableType
        {
            BasicListener,
            BasicConnections,
            BasicAll,
            OwnerPidListener,
            OwnerPidConnections,
            OwnerPidAll,
            OwnerModuleListener,
            OwnerModuleConnections,
            OwnerModuleAll
        }

        /// <summary>
        ///     <see href="http://msdn2.microsoft.com/en-us/library/aa366913.aspx" />
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct TcpRow
        {
            public TcpState state;
            public uint localAddr;
            public uint localPort; // in network byte order (order of bytes - 1,0,3,2)
            public uint remoteAddr;
            public uint remotePort; // in network byte order (order of bytes - 1,0,3,2)
            public int owningPid;
        }

        /// <summary>
        ///     <see href="https://msdn.microsoft.com/en-us/library/aa366896.aspx" />
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct Tcp6Row
        {
            public fixed byte localAddr[16];
            public uint localScopeId;
            public uint localPort; // in network byte order (order of bytes - 1,0,3,2)
            public fixed byte remoteAddr[16];
            public uint remoteScopeId;
            public uint remotePort; // in network byte order (order of bytes - 1,0,3,2)
            public TcpState state;
            public int owningPid;
        }
    }
}
