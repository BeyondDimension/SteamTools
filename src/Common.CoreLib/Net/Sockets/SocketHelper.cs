using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace System.Net.Sockets
{
    public static class SocketHelper
    {
        /// <summary>
        /// 获取一个随机的未使用的端口
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static int GetRandomUnusedPort(IPAddress address)
        {
            var listener = new TcpListener(address, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        /// <summary>
        /// 检查指定的端口是否被占用
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsUsePort(IPAddress address, int port)
        {
            try
            {
                var listener = new TcpListener(address, port);
                listener.Start();
                listener.Stop();
                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <inheritdoc cref="IsUsePort(IPAddress, int)"/>
        public static bool IsUsePort(int port)
        {
            try
            {
                return IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Any(x => x.Port == port);
            }
            catch
            {
                return IsUsePort(IPAddress.Loopback, port);
            }
        }

        /// <summary>
        /// 根据 TCP 端口号获取占用的进程
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows7.0")]
#endif
        public static Process? GetProcessByTcpPort(ushort port)
            => GetAllTcpConnections().FirstOrDefault(x => x.LocalPort == port)?.Process;

        // https://github.com/microsoftarchive/msdn-code-gallery-microsoft/blob/master/OneCodeTeam/C%23%20Sample%20to%20list%20all%20the%20active%20TCP%20and%20UDP%20connections%20using%20Windows%20Form%20appl/%5BC%23%5D-C%23%20Sample%20to%20list%20all%20the%20active%20TCP%20and%20UDP%20connections%20using%20Windows%20Form%20appl/C%23/SocketConnection/SocketConnection.cs

        /// <summary>
        /// This function reads and parses the active TCP socket connections available
        /// and stores them in a list.
        /// </summary>
        /// <returns>
        /// It returns the current set of TCP socket connections which are active.
        /// </returns>
        /// <exception cref="OutOfMemoryException">
        /// This exception may be thrown by the function Marshal.AllocHGlobal when there
        /// is insufficient memory to satisfy the request.
        /// </exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows7.0")]
#endif
        public static List<TcpProcessRecord> GetAllTcpConnections()
        {
            int bufferSize = 0;
            List<TcpProcessRecord> tcpTableRecords = new List<TcpProcessRecord>();

            // Getting the size of TCP table, that is returned in 'bufferSize' variable.
            _ = GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET,
                TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

            // Allocating memory from the unmanaged memory of the process by using the
            // specified number of bytes in 'bufferSize' variable.
            IntPtr tcpTableRecordsPtr = Marshal.AllocHGlobal(bufferSize);

            try
            {
                // The size of the table returned in 'bufferSize' variable in previous
                // call must be used in this subsequent call to 'GetExtendedTcpTable'
                // function in order to successfully retrieve the table.
                var result = GetExtendedTcpTable(tcpTableRecordsPtr, ref bufferSize, true,
                    AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL);

                // Non-zero value represent the function 'GetExtendedTcpTable' failed,
                // hence empty list is returned to the caller function.
                if (result != 0)
                    return tcpTableRecords;

                // Marshals data from an unmanaged block of memory to a newly allocated
                // managed object 'tcpRecordsTable' of type 'MIB_TCPTABLE_OWNER_PID'
                // to get number of entries of the specified TCP table structure.
                MIB_TCPTABLE_OWNER_PID tcpRecordsTable = (MIB_TCPTABLE_OWNER_PID)
                                        Marshal.PtrToStructure(tcpTableRecordsPtr,
                                        typeof(MIB_TCPTABLE_OWNER_PID));
                IntPtr tableRowPtr = (IntPtr)((long)tcpTableRecordsPtr +
                                        Marshal.SizeOf(tcpRecordsTable.dwNumEntries));

                // Reading and parsing the TCP records one by one from the table and
                // storing them in a list of 'TcpProcessRecord' structure type objects.
                for (int row = 0; row < tcpRecordsTable.dwNumEntries; row++)
                {
                    MIB_TCPROW_OWNER_PID tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.
                        PtrToStructure(tableRowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tcpTableRecords.Add(new TcpProcessRecord(
                                          new IPAddress(tcpRow.localAddr),
                                          new IPAddress(tcpRow.remoteAddr),
                                          BitConverter.ToUInt16(new byte[2]
                                          {
                                              tcpRow.localPort[1],
                                              tcpRow.localPort[0]
                                          }, 0),
                                          BitConverter.ToUInt16(new byte[2]
                                          {
                                              tcpRow.remotePort[1],
                                              tcpRow.remotePort[0]
                                          }, 0),
                                          tcpRow.owningPid, tcpRow.state));
                    tableRowPtr = (IntPtr)((long)tableRowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            catch
            {

            }
            finally
            {
                Marshal.FreeHGlobal(tcpTableRecordsPtr);
            }
            return tcpTableRecords;
        }

        // The GetExtendedTcpTable function retrieves a table that contains a list of
        // TCP endpoints available to the application. Decorating the function with
        // DllImport attribute indicates that the attributed method is exposed by an
        // unmanaged dynamic-link library 'iphlpapi.dll' as a static entry point.
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize,
            bool bOrder, int ulAf, TcpTableClass tableClass, uint reserved = 0);

        // The version of IP used by the TCP/UDP endpoint. AF_INET is used for IPv4.
        const int AF_INET = 2;

        /// <summary>
        /// The structure contains information that describes an IPv4 TCP connection with 
        /// IPv4 addresses, ports used by the TCP connection, and the specific process ID
        /// (PID) associated with connection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_TCPROW_OWNER_PID
        {
            public MibTcpState state;
            public uint localAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] localPort;
            public uint remoteAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] remotePort;
            public int owningPid;
        }

        /// <summary>
        /// The structure contains a table of process IDs (PIDs) and the IPv4 TCP links that 
        /// are context bound to these PIDs.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_TCPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public MIB_TCPROW_OWNER_PID[] table;
        }

        // Enum to define the set of values used to indicate the type of table returned by 
        // calls made to the function 'GetExtendedTcpTable'.
        enum TcpTableClass
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }
    }

    /// <summary>
    /// This class provides access an IPv4 TCP connection addresses and ports and its
    /// associated Process IDs and names.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class TcpProcessRecord
    {
        [DisplayName("Local Address")]
        public IPAddress LocalAddress { get; set; }

        [DisplayName("Local Port")]
        public ushort LocalPort { get; set; }

        [DisplayName("Remote Address")]
        public IPAddress RemoteAddress { get; set; }

        [DisplayName("Remote Port")]
        public ushort RemotePort { get; set; }

        [DisplayName("State")]
        public MibTcpState State { get; set; }

        int _ProcessId;

        [DisplayName("Process ID")]
        public int ProcessId
        {
            get => _ProcessId;
            set
            {
                _ProcessId = value;
                _Process = new(() =>
                {
                    try
                    {
                        return Process.GetProcessById(value);
                    }
                    catch
                    {

                    }
                    return null;
                });
            }
        }

        Lazy<Process?>? _Process;

        [DisplayName("Process")]
        public Process? Process => _Process?.Value;

        [DisplayName("Process Name")]
        public string? ProcessName => Process?.ProcessName;

        public TcpProcessRecord(IPAddress localIp, IPAddress remoteIp, ushort localPort,
            ushort remotePort, int pId, MibTcpState state)
        {
            LocalAddress = localIp;
            RemoteAddress = remoteIp;
            LocalPort = localPort;
            RemotePort = remotePort;
            State = state;
            ProcessId = pId;
        }
    }

    // Enum for different possible states of TCP connection
    public enum MibTcpState
    {
        CLOSED = 1,
        LISTENING = 2,
        SYN_SENT = 3,
        SYN_RCVD = 4,
        ESTABLISHED = 5,
        FIN_WAIT1 = 6,
        FIN_WAIT2 = 7,
        CLOSE_WAIT = 8,
        CLOSING = 9,
        LAST_ACK = 10,
        TIME_WAIT = 11,
        DELETE_TCB = 12,
        NONE = 0
    }
}
