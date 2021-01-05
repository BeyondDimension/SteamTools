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
using System.Net.Sockets;
using System.Text;

namespace Titanium.Web.Proxy.ProxySocket.Authentication
{
    /// <summary>
    /// This class implements the 'username/password authentication' scheme.
    /// </summary>
    internal sealed class AuthUserPass : AuthMethod
    {
        /// <summary>
        /// Initializes a new AuthUserPass instance.
        /// </summary>
        /// <param name="server">The socket connection with the proxy server.</param>
        /// <param name="user">The username to use.</param>
        /// <param name="pass">The password to use.</param>
        /// <exception cref="ArgumentNullException"><c>user</c> -or- <c>pass</c> is null.</exception>
        public AuthUserPass(Socket server, string user, string pass) : base(server)
        {
            Username = user;
            Password = pass;
        }

        /// <summary>
        /// Creates an array of bytes that has to be sent if the user wants to authenticate with the username/password authentication scheme.
        /// </summary>
        /// <returns>An array of bytes that has to be sent if the user wants to authenticate with the username/password authentication scheme.</returns>
        private void GetAuthenticationBytes(Memory<byte> buffer)
        {
            var span = buffer.Span;
            span[0] = 1;
            span[1] = (byte)Username.Length;
            Encoding.ASCII.GetBytes(Username).CopyTo(span.Slice(2));
            span[Username.Length + 2] = (byte)Password.Length;
            Encoding.ASCII.GetBytes(Password).CopyTo(span.Slice(Username.Length + 3));
        }

        private int GetAuthenticationLength()
        {
            return 3 + Username.Length + Password.Length;
        }

        /// <summary>
        /// Starts the authentication process.
        /// </summary>
        public override void Authenticate()
        {
            int length = GetAuthenticationLength();
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                GetAuthenticationBytes(buffer);
                if (Server.Send(buffer, 0, length, SocketFlags.None) < length)
                {
                    throw new SocketException(10054);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            int received = 0;
            while (received != 2)
            {
                int recv = Server.Receive(buffer, received, 2 - received, SocketFlags.None);
                if (recv == 0)
                    throw new SocketException(10054);

                received += recv;
            }

            if (buffer[1] != 0)
            {
                Server.Close();
                throw new ProxyException("Username/password combination rejected.");
            }
        }

        /// <summary>
        /// Starts the asynchronous authentication process.
        /// </summary>
        /// <param name="callback">The method to call when the authentication is complete.</param>
        public override void BeginAuthenticate(HandShakeComplete callback)
        {
            int length = GetAuthenticationLength();
            Buffer = ArrayPool<byte>.Shared.Rent(length);
            GetAuthenticationBytes(Buffer);
            CallBack = callback;
            Server.BeginSend(Buffer, 0, length, SocketFlags.None, this.OnSent, Server);
        }

        /// <summary>
        /// Called when the authentication bytes have been sent.
        /// </summary>
        /// <param name="ar">Stores state information for this asynchronous operation as well as any user-defined data.</param>
        private void OnSent(IAsyncResult ar)
        {
            try
            {
                if (Server.EndSend(ar) < GetAuthenticationLength())
                    throw new SocketException(10054);

                Server.BeginReceive(Buffer, 0, 2, SocketFlags.None, this.OnReceive, Server);
            }
            catch (Exception e)
            {
                OnCallBack(e);
            }
        }

        /// <summary>
        /// Called when the socket received an authentication reply.
        /// </summary>
        /// <param name="ar">Stores state information for this asynchronous operation as well as any user-defined data.</param>
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int recv = Server.EndReceive(ar);
                if (recv <= 0)
                    throw new SocketException(10054);

                Received += recv;
                if (Received == 2)
                    if (Buffer[1] == 0)
                        OnCallBack(null);
                    else
                        throw new ProxyException("Username/password combination not accepted.");
                else
                    Server.BeginReceive(Buffer, Received, 2 - Received, SocketFlags.None,
                        this.OnReceive, Server);
            }
            catch (Exception e)
            {
                OnCallBack(e);
            }
        }

        private void OnCallBack(Exception? exception)
        {
            ArrayPool<byte>.Shared.Return(Buffer);
            CallBack(exception);
        }

        /// <summary>
        /// Gets or sets the username to use when authenticating with the proxy server.
        /// </summary>
        /// <value>The username to use when authenticating with the proxy server.</value>
        /// <exception cref="ArgumentNullException">The specified value is null.</exception>
        private string Username
        {
            get => _username;
            set => _username = value ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Gets or sets the password to use when authenticating with the proxy server.
        /// </summary>
        /// <value>The password to use when authenticating with the proxy server.</value>
        /// <exception cref="ArgumentNullException">The specified value is null.</exception>
        private string Password
        {
            get => _password;
            set => _password = value ?? throw new ArgumentNullException();
        }

        // private variables
        /// <summary>Holds the value of the Username property.</summary>
        private string _username;

        /// <summary>Holds the value of the Password property.</summary>
        private string _password;
    }
}
