using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Extensions;
using Titanium.Web.Proxy.StreamExtended.BufferPool;

namespace Titanium.Web.Proxy.Helpers
{
    internal class TcpHelper
    {
        /// <summary>
        ///     Gets the process id by local port number.
        /// </summary>
        /// <returns>Process id.</returns>
        internal static unsafe int GetProcessIdByLocalPort(AddressFamily addressFamily, int localPort)
        {
            var tcpTable = IntPtr.Zero;
            int tcpTableLength = 0;

            int addressFamilyValue = addressFamily == AddressFamily.InterNetwork ? NativeMethods.AfInet : NativeMethods.AfInet6;
            const int allPid = (int)NativeMethods.TcpTableType.OwnerPidAll;

            if (NativeMethods.GetExtendedTcpTable(tcpTable, ref tcpTableLength, false, addressFamilyValue, allPid, 0) != 0)
            {
                try
                {
                    tcpTable = Marshal.AllocHGlobal(tcpTableLength);
                    if (NativeMethods.GetExtendedTcpTable(tcpTable, ref tcpTableLength, true, addressFamilyValue, allPid,
                            0) == 0)
                    {
                        int rowCount = *(int*)tcpTable;
                        uint portInNetworkByteOrder = toNetworkByteOrder((uint)localPort);

                        if (addressFamily == AddressFamily.InterNetwork)
                        {
                            var rowPtr = (NativeMethods.TcpRow*)(tcpTable + 4);

                            for (int i = 0; i < rowCount; ++i)
                            {
                                if (rowPtr->localPort == portInNetworkByteOrder)
                                {
                                    return rowPtr->owningPid;
                                }

                                rowPtr++;
                            }
                        }
                        else
                        {
                            var rowPtr = (NativeMethods.Tcp6Row*)(tcpTable + 4);

                            for (int i = 0; i < rowCount; ++i)
                            {
                                if (rowPtr->localPort == portInNetworkByteOrder)
                                {
                                    return rowPtr->owningPid;
                                }

                                rowPtr++;
                            }
                        }
                    }
                }
                finally
                {
                    if (tcpTable != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(tcpTable);
                    }
                }
            }

            return 0;
        }

        /// <summary>
        ///     Converts 32-bit integer from native byte order (little-endian)
        ///     to network byte order for port,
        ///     switches 0th and 1st bytes, and 2nd and 3rd bytes
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private static uint toNetworkByteOrder(uint port)
        {
            return ((port >> 8) & 0x00FF00FFu) | ((port << 8) & 0xFF00FF00u);
        }

        /// <summary>
        ///     relays the input clientStream to the server at the specified host name and port with the given httpCmd and headers
        ///     as prefix
        ///     Useful for websocket requests
        ///     Task-based Asynchronous Pattern
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="serverStream"></param>
        /// <param name="bufferPool"></param>
        /// <param name="onDataSend"></param>
        /// <param name="onDataReceive"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        private static async Task sendRawTap(Stream clientStream, Stream serverStream, IBufferPool bufferPool,
            Action<byte[], int, int>? onDataSend, Action<byte[], int, int>? onDataReceive,
            CancellationTokenSource cancellationTokenSource)
        {
            // Now async relay all server=>client & client=>server data
            var sendRelay =
                clientStream.CopyToAsync(serverStream, onDataSend, bufferPool, cancellationTokenSource.Token);
            var receiveRelay =
                serverStream.CopyToAsync(clientStream, onDataReceive, bufferPool, cancellationTokenSource.Token);

            await Task.WhenAny(sendRelay, receiveRelay);
            cancellationTokenSource.Cancel();

            await Task.WhenAll(sendRelay, receiveRelay);
        }

        /// <summary>
        ///     relays the input clientStream to the server at the specified host name and port with the given httpCmd and headers
        ///     as prefix
        ///     Useful for websocket requests
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="serverStream"></param>
        /// <param name="bufferPool"></param>
        /// <param name="onDataSend"></param>
        /// <param name="onDataReceive"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <param name="exceptionFunc"></param>
        /// <returns></returns>
        internal static Task SendRaw(Stream clientStream, Stream serverStream, IBufferPool bufferPool,
            Action<byte[], int, int>? onDataSend, Action<byte[], int, int>? onDataReceive,
            CancellationTokenSource cancellationTokenSource,
            ExceptionHandler exceptionFunc)
        {
            // todo: fix APM mode
            return sendRawTap(clientStream, serverStream, bufferPool, onDataSend, onDataReceive,
                cancellationTokenSource);
        }
    }
}
