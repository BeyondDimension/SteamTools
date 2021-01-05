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
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Titanium.Web.Proxy.ProxySocket
{
    /// <summary>
    /// Implements the HTTPS (CONNECT) protocol.
    /// </summary>
    internal sealed class HttpsHandler : SocksHandler
    {
        /// <summary>
        /// Initializes a new HttpsHandler instance.
        /// </summary>
        /// <param name="server">The socket connection with the proxy server.</param>
        /// <exception cref="ArgumentNullException"><c>server</c>  is null.</exception>
        public HttpsHandler(Socket server) : this(server, "") { }

        /// <summary>
        /// Initializes a new HttpsHandler instance.
        /// </summary>
        /// <param name="server">The socket connection with the proxy server.</param>
        /// <param name="user">The username to use.</param>
        /// <exception cref="ArgumentNullException"><c>server</c> -or- <c>user</c> is null.</exception>
        public HttpsHandler(Socket server, string user) : this(server, user, "") { }

        /// <summary>
        /// Initializes a new HttpsHandler instance.
        /// </summary>
        /// <param name="server">The socket connection with the proxy server.</param>
        /// <param name="user">The username to use.</param>
        /// <param name="pass">The password to use.</param>
        /// <exception cref="ArgumentNullException"><c>server</c> -or- <c>user</c> -or- <c>pass</c> is null.</exception>
        public HttpsHandler(Socket server, string user, string pass) : base(server, user)
        {
            Password = pass;
        }

        /// <summary>
        /// Creates an array of bytes that has to be sent when the user wants to connect to a specific IPEndPoint.
        /// </summary>
        /// <returns>An array of bytes that has to be sent when the user wants to connect to a specific IPEndPoint.</returns>
        private byte[] GetConnectBytes(string host, int port)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("CONNECT {0}:{1} HTTP/1.1", host, port));
            sb.AppendLine(string.Format("Host: {0}:{1}", host, port));
            if (!string.IsNullOrEmpty(Username))
            {
                string auth =
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format("{0}:{1}", Username, Password)));
                sb.AppendLine(string.Format("Proxy-Authorization: Basic {0}", auth));
            }

            sb.AppendLine();
            byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());
            return buffer;
        }

        /// <summary>
        /// Verifies that proxy server successfully connected to requested host
        /// </summary>
        /// <param name="buffer">Input data array</param>
        /// <param name="length">The data count in the buffer</param>
        private void VerifyConnectHeader(byte[] buffer, int length)
        {
            string header = Encoding.ASCII.GetString(buffer, 0, length);
            if ((!header.StartsWith("HTTP/1.1 ", StringComparison.OrdinalIgnoreCase) &&
                 !header.StartsWith("HTTP/1.0 ", StringComparison.OrdinalIgnoreCase)) || !header.EndsWith(" "))
                throw new ProtocolViolationException();

            string code = header.Substring(9, 3);
            if (code != "200")
                throw new ProxyException("Invalid HTTP status. Code: " + code);
        }

        /// <summary>
        /// Starts negotiating with the SOCKS server.
        /// </summary>
        /// <param name="remoteEP">The IPEndPoint to connect to.</param>
        /// <exception cref="ArgumentNullException"><c>remoteEP</c> is null.</exception>
        /// <exception cref="ProxyException">The proxy rejected the request.</exception>
        /// <exception cref="SocketException">An operating system error occurs while accessing the Socket.</exception>
        /// <exception cref="ObjectDisposedException">The Socket has been closed.</exception>
        /// <exception cref="ProtocolViolationException">The proxy server uses an invalid protocol.</exception>
        public override void Negotiate(IPEndPoint remoteEP)
        {
            if (remoteEP == null)
                throw new ArgumentNullException();
            Negotiate(remoteEP.Address.ToString(), remoteEP.Port);
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
        /// <exception cref="ProtocolViolationException">The proxy server uses an invalid protocol.</exception>
        public override void Negotiate(string host, int port)
        {
            if (host == null)
                throw new ArgumentNullException();

            if (port <= 0 || port > 65535 || host.Length > 255)
                throw new ArgumentException();

            byte[] buffer = GetConnectBytes(host, port);
            if (Server.Send(buffer, 0, buffer.Length, SocketFlags.None) < buffer.Length)
            {
                throw new SocketException(10054);
            }

            ReadBytes(buffer, 13); // buffer is always longer than 13 bytes. Check the code in GetConnectBytes
            VerifyConnectHeader(buffer, 13);

            // Read bytes 1 by 1 until we reach "\r\n\r\n"
            int receivedNewlineChars = 0;
            while (receivedNewlineChars < 4)
            {
                int recv = Server.Receive(buffer, 0, 1, SocketFlags.None);
                if (recv == 0)
                {
                    throw new SocketException(10054);
                }

                byte b = buffer[0];
                if (b == (receivedNewlineChars % 2 == 0 ? '\r' : '\n'))
                    receivedNewlineChars++;
                else
                    receivedNewlineChars = b == '\r' ? 1 : 0;
            }
        }

        /// <summary>
        /// Starts negotiating asynchronously with the HTTPS server. 
        /// </summary>
        /// <param name="remoteEP">An IPEndPoint that represents the remote device.</param>
        /// <param name="callback">The method to call when the negotiation is complete.</param>
        /// <param name="proxyEndPoint">The IPEndPoint of the HTTPS proxy server.</param>
        /// <param name="state">The state.</param>
        /// <returns>An IAsyncProxyResult that references the asynchronous connection.</returns>
        public override IAsyncProxyResult BeginNegotiate(IPEndPoint remoteEP, HandShakeComplete callback,
            IPEndPoint proxyEndPoint, object state)
        {
            return BeginNegotiate(remoteEP.Address.ToString(), remoteEP.Port, callback, proxyEndPoint, state);
        }

        /// <summary>
        /// Starts negotiating asynchronously with the HTTPS server. 
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="callback">The method to call when the negotiation is complete.</param>
        /// <param name="proxyEndPoint">The IPEndPoint of the HTTPS proxy server.</param>
        /// <param name="state">The state.</param>
        /// <returns>An IAsyncProxyResult that references the asynchronous connection.</returns>
        public override IAsyncProxyResult BeginNegotiate(string host, int port, HandShakeComplete callback,
            IPEndPoint proxyEndPoint, object state)
        {
            ProtocolComplete = callback;
            Buffer = GetConnectBytes(host, port);
            Server.BeginConnect(proxyEndPoint, this.OnConnect, Server);
            AsyncResult = new IAsyncProxyResult(state);
            return AsyncResult;
        }

        /// <summary>
        /// Called when the socket is connected to the remote server.
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
                Server.BeginSend(Buffer, 0, Buffer.Length, SocketFlags.None, this.OnConnectSent,
                    null);
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
            }
        }

        /// <summary>
        /// Called when the connect request bytes have been sent.
        /// </summary>
        /// <param name="ar">Stores state information for this asynchronous operation as well as any user-defined data.</param>
        private void OnConnectSent(IAsyncResult ar)
        {
            try
            {
                HandleEndSend(ar, Buffer.Length);
                Buffer = new byte[13];
                Received = 0;
                Server.BeginReceive(Buffer, 0, 13, SocketFlags.None, this.OnConnectReceive, Server);
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
            }
        }

        /// <summary>
        /// Called when an connect reply has been received.
        /// </summary>
        /// <param name="ar">Stores state information for this asynchronous operation as well as any user-defined data.</param>
        private void OnConnectReceive(IAsyncResult ar)
        {
            try
            {
                HandleEndReceive(ar);
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
                return;
            }

            try
            {
                if (Received < 13)
                {
                    Server.BeginReceive(Buffer, Received, 13 - Received, SocketFlags.None,
                        this.OnConnectReceive, Server);
                }
                else
                {
                    VerifyConnectHeader(Buffer, 13);
                    ReadUntilHeadersEnd(true);
                }
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
            }
        }

        /// <summary>
        /// Reads socket buffer byte by byte until we reach "\r\n\r\n". 
        /// </summary>
        /// <param name="readFirstByte"></param>
        private void ReadUntilHeadersEnd(bool readFirstByte)
        {
            while (Server.Available > 0 && _receivedNewlineChars < 4)
            {
                if (!readFirstByte)
                    readFirstByte = false;
                else
                {
                    int recv = Server.Receive(Buffer, 0, 1, SocketFlags.None);
                    if (recv == 0)
                        throw new SocketException(10054);
                }

                if (Buffer[0] == (_receivedNewlineChars % 2 == 0 ? '\r' : '\n'))
                    _receivedNewlineChars++;
                else
                    _receivedNewlineChars = Buffer[0] == '\r' ? 1 : 0;
            }

            if (_receivedNewlineChars == 4)
            {
                OnProtocolComplete(null);
            }
            else
            {
                Server.BeginReceive(Buffer, 0, 1, SocketFlags.None, this.OnEndHeadersReceive,
                    Server);
            }
        }

        // I think we should never reach this function in practice
        // But let's define it just in case
        /// <summary>
        /// Called when additional headers have been received.
        /// </summary>
        /// <param name="ar">Stores state information for this asynchronous operation as well as any user-defined data.</param>
        private void OnEndHeadersReceive(IAsyncResult ar)
        {
            try
            {
                HandleEndReceive(ar);
                ReadUntilHeadersEnd(false);
            }
            catch (Exception e)
            {
                OnProtocolComplete(e);
            }
        }

        protected override void OnProtocolComplete(Exception? exception)
        {
            // do not return the base Buffer
            ProtocolComplete(exception);
        }

        /// <summary>
        /// Gets or sets the password to use when authenticating with the HTTPS server.
        /// </summary>
        /// <value>The password to use when authenticating with the HTTPS server.</value>
        private string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value ?? throw new ArgumentNullException();
            }
        }

        // private variables
        /// <summary>Holds the value of the Password property.</summary>
        private string _password;

        /// <summary>Holds the count of newline characters received.</summary>
        private int _receivedNewlineChars;
    }
}
