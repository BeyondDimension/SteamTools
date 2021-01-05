/*
    Copyright © 2002, The KPD-Team
    All rights reserved.
    http://www.mentalis.org/

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions
  are met:

    - Redistributions of source code must retain the above copyright
       notice, this list of conditions and the following disclaimer. 

    - Neither the name of the KPD-Team, nor the names of its contributors
       may be used to endorse or promote products derived from this
       software without specific prior written permission. 

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
  THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
  STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
  OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Titanium.Web.Proxy.ProxySocket
{
    /// <summary>
    /// Implements the SOCKS4[A] protocol.
    /// </summary>
    internal sealed class Socks4Handler : SocksHandler
    {
        /// <summary>
        /// Initializes a new instance of the SocksHandler class.
        /// </summary>
        /// <param name="server">The socket connection with the proxy server.</param>
        /// <param name="user">The username to use when authenticating with the server.</param>
        /// <exception cref="ArgumentNullException"><c>server</c> -or- <c>user</c> is null.</exception>
        public Socks4Handler(Socket server, string user) : base(server, user) { }

        /// <summary>
        /// Creates an array of bytes that has to be sent when the user wants to connect to a specific host/port combination.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="buffer">The buffer which contains the result data.</param>
        /// <returns>An array of bytes that has to be sent when the user wants to connect to a specific host/port combination.</returns>
        /// <remarks>Resolving the host name will be done at server side. Do note that some SOCKS4 servers do not implement this functionality.</remarks>
        /// <exception cref="ArgumentNullException"><c>host</c> is null.</exception>
        /// <exception cref="ArgumentException"><c>port</c> is invalid.</exception>
        private int GetHostPortBytes(string host, int port, Memory<byte> buffer)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (port <= 0 || port > 65535)
                throw new ArgumentException(nameof(port));

            int length = 10 + Username.Length + host.Length;
            Debug.Assert(buffer.Length >= length);

            var connect = buffer.Span;
            connect[0] = 4;
            connect[1] = 1;
            PortToBytes(port, connect.Slice(2));
            connect[4] = connect[5] = connect[6] = 0;
            connect[7] = 1;
            var userNameArray = Encoding.ASCII.GetBytes(Username);
            userNameArray.CopyTo(connect.Slice(8));
            connect[8 + Username.Length] = 0;
            Encoding.ASCII.GetBytes(host).CopyTo(connect.Slice(9 + Username.Length));
            connect[length - 1] = 0;
            return length;
        }

        /// <summary>
        /// Creates an array of bytes that has to be sent when the user wants to connect to a specific IPEndPoint.
        /// </summary>
        /// <param name="remoteEP">The IPEndPoint to connect to.</param>
        /// <param name="buffer">The buffer which contains the result data.</param>
        /// <returns>An array of bytes that has to be sent when the user wants to connect to a specific IPEndPoint.</returns>
        /// <exception cref="ArgumentNullException"><c>remoteEP</c> is null.</exception>
        private int GetEndPointBytes(IPEndPoint remoteEP, Memory<byte> buffer)
        {
            if (remoteEP == null)
                throw new ArgumentNullException(nameof(remoteEP));

            int length = 9 + Username.Length;
            Debug.Assert(buffer.Length >= length);

            var connect = buffer.Span;
            connect[0] = 4;
            connect[1] = 1;
            PortToBytes(remoteEP.Port, connect.Slice(2));
            remoteEP.Address.GetAddressBytes().CopyTo(connect.Slice(4));
            Encoding.ASCII.GetBytes(Username).CopyTo(connect.Slice(8));
            connect[length - 1] = 0;
            return length;
        }

        /// <summary>
        /// Starts negotiating with the SOCKS server.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <exception cref="ArgumentNullException"><c>host</c> is null.</exception>
        /// <exception cref="ArgumentException"><c>port</c> is invalid.</exception>
        /// <exception cref="ProxyException">The proxy rejected the request.</exception>
        /// <exception cref="SocketException">An operating system error occurs while accessing the Socket.</exception>
        /// <exception cref="ObjectDisposedException">The Socket has been closed.</exception>
        public override void Negotiate(string host, int port)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(10 + Username.Length + host.Length);
            try
            {
                int length = GetHostPortBytes(host, port, buffer);
                Negotiate(buffer, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Starts negotiating with the SOCKS server.
        /// </summary>
        /// <param name="remoteEP">The IPEndPoint to connect to.</param>
        /// <exception cref="ArgumentNullException"><c>remoteEP</c> is null.</exception>
        /// <exception cref="ProxyException">The proxy rejected the request.</exception>
        /// <exception cref="SocketException">An operating system error occurs while accessing the Socket.</exception>
        /// <exception cref="ObjectDisposedException">The Socket has been closed.</exception>
        public override void Negotiate(IPEndPoint remoteEP)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(9 + Username.Length);
            try
            {
                int length = GetEndPointBytes(remoteEP, buffer);
                Negotiate(buffer, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Starts negotiating with the SOCKS server.
        /// </summary>
        /// <param name="connect">The bytes to send when trying to authenticate.</param>
        /// <param name="length">The byte count to send when trying to authenticate.</param>
        /// <exception cref="ArgumentNullException"><c>connect</c> is null.</exception>
        /// <exception cref="ArgumentException"><c>connect</c> is too small.</exception>
        /// <exception cref="ProxyException">The proxy rejected the request.</exception>
        /// <exception cref="SocketException">An operating system error occurs while accessing the Socket.</exception>
        /// <exception cref="ObjectDisposedException">The Socket has been closed.</exception>
        private void Negotiate(byte[] connect, int length)
        {
            if (connect == null)
                throw new ArgumentNullException(nameof(connect));

            if (length < 2)
                throw new ArgumentException(nameof(length));

            if (Server.Send(connect, 0, length, SocketFlags.None) < length)
                throw new SocketException(10054);

            ReadBytes(connect, 8);
            if (connect[1] != 90)
            {
                Server.Close();
                throw new ProxyException("Negotiation failed.");
            }
        }

        /// <summary>
        /// Starts negotiating asynchronously with a SOCKS proxy server.
        /// </summary>
        /// <param name="host">The remote server to connect to.</param>
        /// <param name="port">The remote port to connect to.</param>
        /// <param name="callback">The method to call when the connection has been established.</param>
        /// <param name="proxyEndPoint">The IPEndPoint of the SOCKS proxy server.</param>
        /// <param name="state">The state.</param>
        /// <returns>An IAsyncProxyResult that references the asynchronous connection.</returns>
        public override IAsyncProxyResult BeginNegotiate(string host, int port, HandShakeComplete callback,
            IPEndPoint proxyEndPoint, object state)
        {
            ProtocolComplete = callback;
            Buffer = ArrayPool<byte>.Shared.Rent(10 + Username.Length + host.Length);
            BufferCount = GetHostPortBytes(host, port, Buffer);
            Server.BeginConnect(proxyEndPoint, OnConnect, Server);
            AsyncResult = new IAsyncProxyResult(state);
            return AsyncResult;
        }

        /// <summary>
        /// Starts negotiating asynchronously with a SOCKS proxy server.
        /// </summary>
        /// <param name="remoteEP">An IPEndPoint that represents the remote device.</param>
        /// <param name="callback">The method to call when the connection has been established.</param>
        /// <param name="proxyEndPoint">The IPEndPoint of the SOCKS proxy server.</param>
        /// <param name="state">The state.</param>
        /// <returns>An IAsyncProxyResult that references the asynchronous connection.</returns>
        public override IAsyncProxyResult BeginNegotiate(IPEndPoint remoteEP, HandShakeComplete callback,
            IPEndPoint proxyEndPoint, object state)
        {
            ProtocolComplete = callback;
            Buffer = ArrayPool<byte>.Shared.Rent(9 + Username.Length);
            BufferCount = GetEndPointBytes(remoteEP, Buffer);
            Server.BeginConnect(proxyEndPoint, OnConnect, Server);
            AsyncResult = new IAsyncProxyResult(state);
            return AsyncResult;
        }

        /// <summary>
        /// Called when the Socket is connected to the remote proxy server.
        /// </summary>
        /// <param name="ar">Stores state information for this asynchronous operation as well as any user-defined data.</param>
        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                Server.EndConnect(ar);
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
                return;
            }

            try
            {
                Server.BeginSend(Buffer, 0, BufferCount, SocketFlags.None, OnSent, Server);
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
            }
        }

        /// <summary>
        /// Called when the Socket has sent the handshake data.
        /// </summary>
        /// <param name="ar">Stores state information for this asynchronous operation as well as any user-defined data.</param>
        private void OnSent(IAsyncResult ar)
        {
            try
            {
                HandleEndSend(ar, BufferCount);
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
                return;
            }

            try
            {
                BufferCount = 8;
                Received = 0;
                Server.BeginReceive(Buffer, 0, BufferCount, SocketFlags.None, OnReceive, Server);
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
            }
        }

        /// <summary>
        /// Called when the Socket has received a reply from the remote proxy server.
        /// </summary>
        /// <param name="ar">Stores state information for this asynchronous operation as well as any user-defined data.</param>
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                HandleEndReceive(ar);
                if (Received == 8)
                {
                    if (Buffer[1] == 90)
                        OnProtocolComplete(null);
                    else
                    {
                        Server.Close();
                        OnProtocolComplete(new ProxyException("Negotiation failed."));
                    }
                }
                else
                {
                    Server.BeginReceive(Buffer, Received, BufferCount - Received, SocketFlags.None, OnReceive,
                        Server);
                }
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
            }
        }
    }
}
