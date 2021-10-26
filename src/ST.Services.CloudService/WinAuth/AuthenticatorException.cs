/*
 * Copyright (C) 2010 Colin Mackie.
 * This software is distributed under the terms of the GNU General Public License.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace WinAuth
{
    /// <summary>
    /// Base Authenticator exception class
    /// </summary>
    public class WinAuthException : ApplicationException
    {
        public WinAuthException()
          : base()
        {
        }

        public WinAuthException(string? msg)
          : base(msg)
        {
        }

        public WinAuthException(string? msg, Exception? ex)
          : base(msg, ex)
        {
        }
    }

    /// <summary>
    /// Exception for reading invalid config data
    /// </summary>
    public class WinAuthInvalidConfigDataException : WinAuthException
    {
        public WinAuthInvalidConfigDataException() : base()
        {
        }
    }

    /// <summary>
    /// Exception for invalid HMAC algorithm configuration
    /// </summary>
    public class WinAuthInvalidHMACAlgorithmException : WinAuthException
    {
        public WinAuthInvalidHMACAlgorithmException() : base()
        {
        }
    }

    /// <summary>
    /// Exception for invalid user decryption
    /// </summary>
    public class WinAuthInvalidUserDecryptionException : WinAuthException
    {
        public WinAuthInvalidUserDecryptionException() : base()
        {
        }
    }

    /// <summary>
    /// Exception for invalid machine decryption
    /// </summary>
    public class WinAuthInvalidMachineDecryptionException : WinAuthException
    {
        public WinAuthInvalidMachineDecryptionException() : base()
        {
        }
    }

    /// <summary>
    /// Exception for error or unexpected return from server for enroll
    /// </summary>
    public class WinAuthInvalidEnrollResponseException : WinAuthException
    {
        public WinAuthInvalidEnrollResponseException(string? msg = null, Exception? ex = null) : base(msg, ex)
        {
        }
    }

    /// <summary>
    /// Exception for error or unexpected return from server for trades
    /// </summary>
    public class WinAuthInvalidTradesResponseException : WinAuthException
    {
        public WinAuthInvalidTradesResponseException(string? msg = null, Exception? ex = null) : base(msg, ex)
        {
        }
    }

    /// <summary>
    /// Exception for error or unexpected return from server for sync
    /// </summary>
    public class WinAuthInvalidSyncResponseException : WinAuthException
    {
        public WinAuthInvalidSyncResponseException(string msg) : base(msg)
        {
        }
    }

    /// <summary>
    /// Config has been encrypted and we need a key
    /// </summary>
    public class WinAuthEncryptedSecretDataException : WinAuthException
    {
        public WinAuthEncryptedSecretDataException() : base()
        {
        }
    }

    /// <summary>
    /// Config decryption bad password
    /// </summary>
    public class WinAuthBadPasswordException : WinAuthException
    {
        public WinAuthBadPasswordException(string? msg = null, Exception? ex = null) : base(msg, ex)
        {
        }
    }

    public class WinAuthInvalidRestoreResponseException : WinAuthException
    {
        public WinAuthInvalidRestoreResponseException(string msg) : base(msg)
        {
        }
    }

    public class WinAuthInvalidRestoreCodeException : WinAuthInvalidRestoreResponseException
    {
        public WinAuthInvalidRestoreCodeException(string msg) : base(msg)
        {
        }
    }

    /// <summary>
    /// Invalid encryption detected
    /// </summary>
    public class WinAuthInvalidEncryptionException : WinAuthException
    {
        public WinAuthInvalidEncryptionException(string plain, string? password, string encrypted, string decrypted) : base()
        {
            Plain = plain;
            Password = password;
            Encrypted = encrypted;
            Decrypted = decrypted;
        }

        public string Plain { get; set; }
        public string? Password { get; set; }
        public string Encrypted { get; set; }
        public string Decrypted { get; set; }
    }

    /// <summary>
    /// Error on setting secret data (invalid decoding) caused by corruption or wrong password
    /// </summary>
    public class WinAuthInvalidSecretDataException : WinAuthException
    {
        public WinAuthInvalidSecretDataException(Exception inner, string password, string encType, List<string> decrypted)
          : base("Error decoding Secret Data", inner)
        {
            Password = password;
            EncType = encType;
            Decrypted = decrypted;
        }

        public string Password { get; set; }
        public string EncType { get; set; }
        public List<string> Decrypted { get; set; }
    }

    public class WinAuthBase32DecodingException : WinAuthException
    {
        public WinAuthBase32DecodingException(string msg)
            : base(msg)
        {
        }
    }

    /// <summary>
    /// Our custom exception for the internal Http Request
    /// </summary>
    public class WinAuthInvalidRequestException : WinAuthException
    {
        public WinAuthInvalidRequestException(string? msg = null, Exception? ex = null) : base(msg, ex)
        {
        }
    }

    /// <summary>
    /// 403 forbidden responses
    /// </summary>
    public class WinAuthUnauthorisedRequestException : WinAuthInvalidRequestException
    {
        public WinAuthUnauthorisedRequestException(Exception? ex = null) : base("Unauthorised", ex)
        {
        }
    }

    /// <summary>
    /// Our custom exception for the internal Http Request
    /// </summary>
    public class WinAuthInvalidSteamRequestException : WinAuthException
    {
        public WinAuthInvalidSteamRequestException(string? msg = null, Exception? ex = null) : base(msg, ex)
        {
        }
    }

    public class WinAuthUnauthorisedSteamRequestException : WinAuthInvalidSteamRequestException
    {
        public WinAuthUnauthorisedSteamRequestException(Exception? ex = null) : base("Unauthorised", ex)
        {
        }
    }
}