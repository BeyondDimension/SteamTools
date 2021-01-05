//
// Nancy.Authentication.Ntlm.Protocol.Type3Message - Authentication
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// References
// a.	NTLM Authentication Scheme for HTTP, Ronald Tschalär
//	http://www.innovation.ch/java/ntlm.html
// b.	The NTLM Authentication Protocol, Copyright © 2003 Eric Glass
//	http://davenport.sourceforge.net/ntlm.html
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;

namespace Titanium.Web.Proxy.Network.WinAuth.Security
{
    internal class Message
    {
        private static readonly byte[] header = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00 };

        private readonly int type;

        internal Message(byte[] message)
        {
            type = 3;

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Length < 12)
            {
                string msg = "Minimum Type3 message length is 12 bytes.";
                throw new ArgumentOutOfRangeException(nameof(message), message.Length, msg);
            }

            if (!CheckHeader(message))
            {
                string msg = "Invalid Type3 message header.";
                throw new ArgumentException(msg, nameof(message));
            }

            if (LittleEndian.ToUInt16(message, 56) != message.Length)
            {
                string msg = "Invalid Type3 message length.";
                throw new ArgumentException(msg, nameof(message));
            }

            if (message.Length >= 64)
            {
                Flags = (Common.NtlmFlags)LittleEndian.ToUInt32(message, 60);
            }
            else
            {
                Flags = (Common.NtlmFlags)0x8201;
            }

            int domLen = LittleEndian.ToUInt16(message, 28);
            int domOff = LittleEndian.ToUInt16(message, 32);

            Domain = DecodeString(message, domOff, domLen);

            int userLen = LittleEndian.ToUInt16(message, 36);
            int userOff = LittleEndian.ToUInt16(message, 40);

            Username = DecodeString(message, userOff, userLen);
        }

        /// <summary>
        ///     Domain name
        /// </summary>
        internal string Domain { get; private set; }

        /// <summary>
        ///     Username
        /// </summary>
        internal string Username { get; private set; }

        internal Common.NtlmFlags Flags { get; set; }

        private string DecodeString(byte[] buffer, int offset, int len)
        {
            if ((Flags & Common.NtlmFlags.NegotiateUnicode) != 0)
            {
                return Encoding.Unicode.GetString(buffer, offset, len);
            }

            return Encoding.ASCII.GetString(buffer, offset, len);
        }

        protected bool CheckHeader(byte[] message)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (message[i] != header[i])
                {
                    return false;
                }
            }

            return LittleEndian.ToUInt32(message, 8) == type;
        }
    }
}
