/*
 * Copyright (C) 2011 Colin Mackie.
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using WinAuth;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        /// <summary>
        /// Number of bytes making up the salt
        /// </summary>
        const int SALT_LENGTH = 8;

        /// <summary>
        /// Number of iterations in PBKDF2 key generation
        /// </summary>
        const int PBKDF2_ITERATIONS = 2000;

        /// <summary>
        /// Size of derived PBKDF2 key
        /// </summary>
        const int PBKDF2_KEYSIZE = 256;

        /// <summary>
        /// Version for encrpytion changes
        /// </summary>
        static readonly string ENCRYPTION_HEADER = ByteArrayToString(Encoding.UTF8.GetBytes("WINAUTH3"));

        /// <summary>
        /// Type of password to use to encrypt secret data
        /// </summary>
        public enum PasswordTypes
        {
            None = 0,
            Explicit = 1,
            User = 2,
            Machine = 4,
            YubiKeySlot1 = 8,
            YubiKeySlot2 = 16
        }

        public const HMACTypes DEFAULT_HMAC_TYPE = HMACTypes.SHA1;

        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        public string? EncryptedData { get; set; }

        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        public virtual string? SecretData
        {
            get
            {
                // this is the secretkey
                return ByteArrayToString(SecretKey.ThrowIsNull(nameof(SecretKey))) + "\t" + CodeDigits.ToString() + "\t" + HMACType.ToString() + "\t" + Period.ToString();
            }

            set
            {
                if (string.IsNullOrEmpty(value) == false)
                {
                    var parts = value.Split('|')[0].Split('\t');
                    SecretKey = StringToByteArray(parts[0]);
                    if (parts.Length > 1)
                    {
                        if (int.TryParse(parts[1], out var digits) == true)
                        {
                            CodeDigits = digits;
                        }
                    }
                    if (parts.Length > 2)
                    {
                        HMACType = (HMACTypes)Enum.Parse(typeof(HMACTypes), parts[2]);
                    }
                    if (parts.Length > 3)
                    {
                        if (int.TryParse(parts[3], out var period) == true)
                        {
                            Period = period;
                        }
                    }
                }
                else
                {
                    SecretKey = null;
                }
            }
        }

        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        public PasswordTypes PasswordType { get; set; }

        /// <summary>
        /// Password used to encrypt secretdata (if PasswordType == Explict)
        /// </summary>
        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        protected string? Password { get; set; }

        /// <summary>
        /// Hash of secret data to detect changes
        /// </summary>
        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        protected byte[]? SecretHash { get; private set; }

        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        public bool RequiresPassword { get; private set; }

        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        public long ServerTime
        {
            get
            {
                return CurrentTime + ServerTimeDiff;
            }
        }

        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        public long CodeInterval
        {
            get
            {
                // calculate the code interval; the server's time div 30,000
                return (CurrentTime + ServerTimeDiff) / (Period * 1000L);
            }
        }

        [MPIgnore, N_JsonIgnore, S_JsonIgnore]
        public string CurrentCode
        {
            get
            {
                if (SecretKey == null && EncryptedData != null)
                {
                    return string.Empty;
                }

                return CalculateCode(false);
            }
        }

        /// <summary>
        /// Calculate the current code for the authenticator.
        /// </summary>
        /// <param name="resyncTime">flag to resync time</param>
        /// <returns>authenticator code</returns>
        protected virtual string CalculateCode(bool resync = false, long interval = -1)
        {
            // sync time if required
            if (resync || ServerTimeDiff == 0)
            {
                if (interval > 0)
                {
                    ServerTimeDiff = (interval * Period * 1000L) - CurrentTime;
                }
                else
                {
                    Sync();
                }
            }

            var hmac = HMACType switch
            {
                HMACTypes.SHA1 => new HMac(new Sha1Digest()),
                HMACTypes.SHA256 => new HMac(new Sha256Digest()),
                HMACTypes.SHA512 => new HMac(new Sha512Digest()),
                _ => throw new WinAuthInvalidHMACAlgorithmException(),
            };
            hmac.Init(new KeyParameter(SecretKey));

            var codeIntervalArray = BitConverter.GetBytes(CodeInterval);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(codeIntervalArray);
            }
            hmac.BlockUpdate(codeIntervalArray, 0, codeIntervalArray.Length);

            var mac = new byte[hmac.GetMacSize()];
            hmac.DoFinal(mac, 0);

            // the last 4 bits of the mac say where the code starts (e.g. if last 4 bit are 1100, we start at byte 12)
            var start = mac.Last() & 0x0f;

            // extract those 4 bytes
            var bytes = new byte[4];
            Array.Copy(mac, start, bytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            var fullcode = BitConverter.ToUInt32(bytes, 0) & 0x7fffffff;

            // we use the last 8 digits of this code in radix 10
            var codemask = (uint)Math.Pow(10, CodeDigits);
            var format = new string('0', CodeDigits);
            var code = (fullcode % codemask).ToString(format);

            return code;
        }

        public abstract void Sync();

        #region Load / Save

        public static GAPAuthenticatorValueDTO? ReadXmlv2(XmlReader reader, string? password = null)
        {
            GAPAuthenticatorValueDTO? authenticator = null;
            var authenticatorType = reader.GetAttribute("type");
            if (!string.IsNullOrEmpty(authenticatorType))
            {
                authenticatorType = authenticatorType.Replace(
                    "WindowsAuthenticator.",
                    typeof(GAPAuthenticatorValueDTO).FullName + ".");
                var authenticatorType_ = Assembly.GetExecutingAssembly().GetType(authenticatorType, false, true);
                authenticator = Activator.CreateInstance(authenticatorType_.ThrowIsNull(nameof(authenticatorType_))) as GAPAuthenticatorValueDTO;
            }
            if (authenticator == null)
            {
                return null;
            }

            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return null;
            }

            reader.Read();
            while (reader.EOF == false)
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "servertimediff":
                            authenticator.ServerTimeDiff = reader.ReadElementContentAsLong();
                            break;

                        //case "restorecodeverified":
                        //	authenticator.RestoreCodeVerified = reader.ReadElementContentAsBoolean();
                        //	break;

                        case "secretdata":
                            var encrypted = reader.GetAttribute("encrypted");
                            var data = reader.ReadElementContentAsString();

                            var passwordType = DecodePasswordTypes(encrypted);

                            if (passwordType != PasswordTypes.None)
                            {
                                // this is an old version so there is no hash
                                data = DecryptSequence(data, passwordType, password);
                            }

                            authenticator.PasswordType = PasswordTypes.None;
                            authenticator.SecretData = data;

                            break;

                        default:
                            if (authenticator.ReadExtraXml(reader, reader.Name) == false)
                            {
                                reader.Skip();
                            }
                            break;
                    }
                }
                else
                {
                    reader.Read();
                    break;
                }
            }

            return authenticator;
        }

        public virtual bool ReadExtraXml(XmlReader reader, string name)
        {
            return false;
        }

        /// <summary>
        /// Convert the string password types into the PasswordTypes type
        /// </summary>
        /// <param name="passwordTypes">string version of password types</param>
        /// <returns>PasswordTypes value</returns>
        public static PasswordTypes DecodePasswordTypes(string? passwordTypes)
        {
            var passwordType = PasswordTypes.None;
            if (string.IsNullOrEmpty(passwordTypes))
            {
                return passwordType;
            }

            var types = passwordTypes.ToCharArray();
            for (var i = types.Length - 1; i >= 0; i--)
            {
                var type = types[i];
                switch (type)
                {
                    case 'u':
                        passwordType |= PasswordTypes.User;
                        break;
                    case 'm':
                        passwordType |= PasswordTypes.Machine;
                        break;
                    case 'y':
                        passwordType |= PasswordTypes.Explicit;
                        break;
                    case 'a':
                        passwordType |= PasswordTypes.YubiKeySlot1;
                        break;
                    case 'b':
                        passwordType |= PasswordTypes.YubiKeySlot2;
                        break;
                    default:
                        break;
                }
            }

            return passwordType;
        }

        /// <summary>
        /// Encode the PasswordTypes type into a string for storing in config
        /// </summary>
        /// <param name="passwordType">PasswordTypes value</param>
        /// <returns>string version</returns>
        public static string EncodePasswordTypes(PasswordTypes passwordType)
        {
            var encryptedTypes = new StringBuilder();
            if ((passwordType & PasswordTypes.Explicit) != 0)
            {
                encryptedTypes.Append("y");
            }
            if ((passwordType & PasswordTypes.User) != 0)
            {
                encryptedTypes.Append("u");
            }
            if ((passwordType & PasswordTypes.Machine) != 0)
            {
                encryptedTypes.Append("m");
            }

            return encryptedTypes.ToString();
        }

        public void SetEncryption(PasswordTypes passwordType, string? password = null)
        {
            // check if still encrpyted
            if (RequiresPassword == true)
            {
                // have to decrypt to be able to re-encrypt
                throw new WinAuthEncryptedSecretDataException();
            }

            if (passwordType == PasswordTypes.None)
            {
                RequiresPassword = false;
                EncryptedData = null;
                PasswordType = passwordType;
            }
            else
            {
                using var ms = new MemoryStream();
                // get the plain version
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;
                using (var encryptedwriter = XmlWriter.Create(ms, settings))
                {
                    var encrpytedData = EncryptedData;
                    var savedpasswordType = PasswordType;
                    try
                    {
                        PasswordType = PasswordTypes.None;
                        EncryptedData = null;
                        WriteToWriter(encryptedwriter);
                    }
                    finally
                    {
                        PasswordType = savedpasswordType;
                        EncryptedData = encrpytedData;
                    }
                }
                var data = ByteArrayToString(ms.ToArray());

                // update secret hash
                using (var sha1 = SHA1.Create())
                {
                    SecretHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(SecretData.ThrowIsNull(nameof(SecretData))));
                }

                // encrypt
                EncryptedData = EncryptSequence(data, passwordType, password);
                PasswordType = passwordType;
                if (PasswordType == PasswordTypes.Explicit)
                {
                    SecretData = null;
                    RequiresPassword = true;
                }
            }
        }

        public void Protect()
        {
            if (PasswordType != PasswordTypes.None)
            {
                // check if the data has changed
                //if (this.SecretData != null)
                //{
                //	using (SHA1 sha1 = SHA1.Create())
                //	{
                //		byte[] secretHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(this.SecretData));
                //		if (this.SecretHash == null || secretHash.SequenceEqual(this.SecretHash) == false)
                //		{
                //			// we need to encrypt changed secret data
                //			SetEncryption(this.PasswordType, this.Password);
                //		}
                //	}
                //}

                SecretData = null;
                RequiresPassword = true;
                Password = null;
            }
        }

        public bool Unprotect(string? password)
        {
            var passwordType = PasswordType;
            if (passwordType == PasswordTypes.None)
            {
                throw new InvalidOperationException("Cannot Unprotect a non-encrypted authenticator");
            }

            // decrypt
            var changed = false;
            try
            {
                var data = DecryptSequence(
                    EncryptedData.ThrowIsNull(nameof(EncryptedData)),
                    PasswordType, password);
                using (var ms = new MemoryStream(StringToByteArray(data)))
                {
                    var reader = XmlReader.Create(ms);
                    changed = ReadXml(reader, password) || changed;
                }
                RequiresPassword = false;
                // calculate hash of current secretdata
                using (var sha1 = SHA1.Create())
                {
                    SecretHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(SecretData.ThrowIsNull(nameof(SecretData))));
                }
                // keep the password until we reprotect in case data changes
                Password = password;

                if (changed == true)
                {
                    // we need to encrypt changed secret data
                    using var ms = new MemoryStream();
                    // get the plain version
                    var settings = new XmlWriterSettings
                    {
                        Indent = true,
                        Encoding = Encoding.UTF8
                    };
                    using (var encryptedwriter = XmlWriter.Create(ms, settings))
                    {
                        WriteToWriter(encryptedwriter);
                    }
                    var encrypteddata = ByteArrayToString(ms.ToArray());

                    // update secret hash
                    using (var sha1 = SHA1.Create())
                    {
                        SecretHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(SecretData.ThrowIsNull(nameof(SecretData))));
                    }

                    // encrypt
                    EncryptedData = EncryptSequence(encrypteddata, passwordType, password);
                }

                return changed;
            }
            catch (WinAuthEncryptedSecretDataException)
            {
                RequiresPassword = true;
                throw;
            }
            finally
            {
                PasswordType = passwordType;
            }
        }

        public bool ReadXml(XmlReader reader, string? password = null)
        {
            // decode the password type
            var encrypted = reader.GetAttribute("encrypted");
            var passwordType = DecodePasswordTypes(encrypted);
            PasswordType = passwordType;

            if (passwordType != PasswordTypes.None)
            {
                // read the encrypted text from the node
                EncryptedData = reader.ReadElementContentAsString();
                return Unprotect(password);

                //// decrypt
                //try
                //{
                //	string data = Authenticator.DecryptSequence(this.EncryptedData, passwordType, password);
                //	using (MemoryStream ms = new MemoryStream(Authenticator.StringToByteArray(data)))
                //	{
                //		reader = XmlReader.Create(ms);
                //		this.ReadXml(reader, password);
                //	}
                //}
                //catch (EncryptedSecretDataException)
                //{
                //	this.RequiresPassword = true;
                //	throw;
                //}
                //finally
                //{
                //	this.PasswordType = passwordType;
                //}
            }

            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return false;
            }

            reader.Read();
            while (reader.EOF == false)
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "lastservertime":
                            LastServerTime = reader.ReadElementContentAsLong();
                            break;

                        case "servertimediff":
                            ServerTimeDiff = reader.ReadElementContentAsLong();
                            break;

                        case "secretdata":
                            SecretData = reader.ReadElementContentAsString();
                            break;

                        default:
                            if (ReadExtraXml(reader, reader.Name) == false)
                            {
                                reader.Skip();
                            }
                            break;
                    }
                }
                else
                {
                    reader.Read();
                    break;
                }
            }

            // check if we need to sync, or if it's been a day
#pragma warning disable CS0612 // 类型或成员已过时
            if (this is HOTPAuthenticator)
#pragma warning restore CS0612 // 类型或成员已过时
            {
                // no time sync
                return true;
            }
            else if (ServerTimeDiff == 0 || LastServerTime == 0 || LastServerTime < DateTime.Now.AddHours(-24).Ticks)
            {
                Sync();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Write this authenticator into an XmlWriter
        /// </summary>
        /// <param name="writer">XmlWriter to receive authenticator</param>
        public void WriteToWriter(XmlWriter writer)
        {
            writer.WriteStartElement("authenticatordata");
            //writer.WriteAttributeString("type", this.GetType().FullName);
            var encrypted = EncodePasswordTypes(PasswordType);
            if (string.IsNullOrEmpty(encrypted) == false)
            {
                writer.WriteAttributeString("encrypted", encrypted);
            }

            if (PasswordType != PasswordTypes.None)
            {
                writer.WriteRaw(EncryptedData.ThrowIsNull(nameof(EncryptedData)));
            }
            else
            {
                writer.WriteStartElement("servertimediff");
                writer.WriteString(ServerTimeDiff.ToString());
                writer.WriteEndElement();
                //
                writer.WriteStartElement("lastservertime");
                writer.WriteString(LastServerTime.ToString());
                writer.WriteEndElement();
                //
                writer.WriteStartElement("secretdata");
                writer.WriteString(SecretData);
                writer.WriteEndElement();

                WriteExtraXml(writer);
            }

            /*
                  if (passwordType != Authenticator.PasswordTypes.None)
                  {
                    //string data = this.EncryptedData;
                    //if (data == null)
                    //{
                    //	using (MemoryStream ms = new MemoryStream())
                    //	{
                    //		XmlWriterSettings settings = new XmlWriterSettings();
                    //		settings.Indent = true;
                    //		settings.Encoding = Encoding.UTF8;
                    //		using (XmlWriter encryptedwriter = XmlWriter.Create(ms, settings))
                    //		{
                    //			Authenticator.PasswordTypes savedpasswordType = PasswordType;
                    //			PasswordType = Authenticator.PasswordTypes.None;
                    //			WriteToWriter(encryptedwriter);
                    //			PasswordType = savedpasswordType;
                    //		}
                    //		data = Authenticator.ByteArrayToString(ms.ToArray());
                    //	}

                    //	data = Authenticator.EncryptSequence(data, PasswordType, Password);
                    //}

                    writer.WriteString(this.EncryptedData);
                    writer.WriteEndElement();

                    return;
                  }

                  //
                  writer.WriteStartElement("servertimediff");
                  writer.WriteString(ServerTimeDiff.ToString());
                  writer.WriteEndElement();
                  //
                  writer.WriteStartElement("secretdata");
                  writer.WriteString(SecretData);
                  writer.WriteEndElement();

                  WriteExtraXml(writer);
            */

            writer.WriteEndElement();
        }

        /*
            /// <summary>
            /// Write this authenticator into an XmlWriter
            /// </summary>
            /// <param name="writer">XmlWriter to receive authenticator</param>
            protected void WriteToWriter(XmlWriter writer, PasswordTypes passwordType)
            {
              if (passwordType != Authenticator.PasswordTypes.None)
              {
                writer.WriteStartElement("authenticatordata");
                writer.WriteAttributeString("encrypted", EncodePasswordTypes(this.PasswordType));
                writer.WriteString(this.EncryptedData);
                writer.WriteEndElement();
              }
              else
              {
                writer.WriteStartElement("servertimediff");
                writer.WriteString(ServerTimeDiff.ToString());
                writer.WriteEndElement();
                //
                writer.WriteStartElement("secretdata");
                writer.WriteString(SecretData);
                writer.WriteEndElement();

                WriteExtraXml(writer);
              }
            }
        */

        /// <summary>
        /// Virtual function to write any class specific xml nodes into the writer
        /// </summary>
        /// <param name="writer">XmlWriter to write data</param>
        protected virtual void WriteExtraXml(XmlWriter writer)
        {
        }

        #endregion

        #region Utility functions

        /// <summary>
        /// Create a one-time pad by generating a random block and then taking a hash of that block as many times as needed.
        /// </summary>
        /// <param name="length">desired pad length</param>
        /// <returns>array of bytes conatining random data</returns>
        protected internal static byte[] CreateOneTimePad(int length)
        {
            // There is a MITM vulnerability from using the standard Random call
            // see https://docs.google.com/document/edit?id=1pf-YCgUnxR4duE8tr-xulE3rJ1Hw-Bm5aMk5tNOGU3E&hl=en
            // in http://code.google.com/p/winauth/issues/detail?id=2
            // so we switch out to use RNGCryptoServiceProvider instead of Random

            var random = new RNGCryptoServiceProvider();

            var randomblock = new byte[length];

            var sha1 = SHA1.Create();
            var i = 0;
            do
            {
                var hashBlock = new byte[128];
                random.GetBytes(hashBlock);

                var key = sha1.ComputeHash(hashBlock, 0, hashBlock.Length);
                if (key.Length >= randomblock.Length)
                {
                    Array.Copy(key, 0, randomblock, i, randomblock.Length);
                    break;
                }
                Array.Copy(key, 0, randomblock, i, key.Length);
                i += key.Length;
            }
            while (true);

            return randomblock;
        }

        /// <summary>
        /// Get the milliseconds since 1/1/70 (same as Java currentTimeMillis)
        /// </summary>
        public static long CurrentTime
        {
            get
            {
                return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
            }
        }

        /// <summary>
        /// Convert a hex string into a byte array. E.g. "001f406a" -> byte[] {0x00, 0x1f, 0x40, 0x6a}
        /// </summary>
        /// <param name="hex">hex string to convert</param>
        /// <returns>byte[] of hex string</returns>
        public static byte[] StringToByteArray(string hex)
        {
            var len = hex.Length;
            var bytes = new byte[len / 2];
            for (var i = 0; i < len; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Convert a byte array into a ascii hex string, e.g. byte[]{0x00,0x1f,0x40,ox6a} -> "001f406a"
        /// </summary>
        /// <param name="bytes">byte array to convert</param>
        /// <returns>string version of byte array</returns>
        public static string ByteArrayToString(byte[] bytes)
        {
            // Use BitConverter, but it sticks dashes in the string
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        /// <summary>
        /// Decrypt a string sequence using the selected encryption types
        /// </summary>
        /// <param name="data">hex coded string sequence to decrypt</param>
        /// <param name="encryptedTypes">Encryption types</param>
        /// <param name="password">optional password</param>
        /// <param name="decode"></param>
        /// <returns>decrypted string sequence</returns>
        public static string DecryptSequence(string data, PasswordTypes encryptedTypes, string? password, bool decode = false)
        {
            // check for encrpytion header
            if (data.Length < ENCRYPTION_HEADER.Length || data.IndexOf(ENCRYPTION_HEADER) != 0)
            {
                return DecryptSequenceNoHash(data, encryptedTypes, password, decode);
            }

            // extract salt and hash
            //using (var sha = new SHA256Managed())
            using (var sha = SafeHasher("SHA256"))
            {
                // jump header
                var datastart = ENCRYPTION_HEADER.Length;
                var salt = data.Substring(datastart, Math.Min(SALT_LENGTH * 2, data.Length - datastart));
                datastart += salt.Length;
                var hash = data.Substring(datastart, Math.Min(sha.HashSize / 8 * 2, data.Length - datastart));
                datastart += hash.Length;
                data = data.Substring(datastart);

                data = DecryptSequenceNoHash(data, encryptedTypes, password);

                // check the hash
                var compareplain = StringToByteArray(salt + data);
                var comparehash = ByteArrayToString(sha.ComputeHash(compareplain));
                if (string.Compare(comparehash, hash) != 0)
                {
                    throw new WinAuthBadPasswordException();
                }
            }

            return data;
        }

        /// <summary>
        /// Decrypt a string sequence using the selected encryption types
        /// </summary>
        /// <param name="data">hex coded string sequence to decrypt</param>
        /// <param name="encryptedTypes">Encryption types</param>
        /// <param name="password">optional password</param>
        /// <param name="decode"></param>
        /// <returns>decrypted string sequence</returns>
        private static string DecryptSequenceNoHash(string data, PasswordTypes encryptedTypes, string? password, bool decode = false)
        {
            try
            {
                // reverse order they were encrypted
                if ((encryptedTypes & PasswordTypes.Machine) != 0)
                {
                    // we are going to decrypt with the Windows local machine key
                    var cipher = StringToByteArray(data);
                    var plain = IProtectedData.Instance.Unprotect(cipher, null, IProtectedData.DataProtectionScope.LocalMachine);
                    if (decode == true)
                    {
                        data = Encoding.UTF8.GetString(plain, 0, plain.Length);
                    }
                    else
                    {
                        data = ByteArrayToString(plain);
                    }
                }
                if ((encryptedTypes & PasswordTypes.User) != 0)
                {
                    // we are going to decrypt with the Windows User account key
                    var cipher = StringToByteArray(data);
                    var plain = IProtectedData.Instance.Unprotect(cipher, null, IProtectedData.DataProtectionScope.CurrentUser);
                    if (decode == true)
                    {
                        data = Encoding.UTF8.GetString(plain, 0, plain.Length);
                    }
                    else
                    {
                        data = ByteArrayToString(plain);
                    }
                }
                if ((encryptedTypes & PasswordTypes.Explicit) != 0)
                {
                    // we use an explicit password to encrypt data
                    if (string.IsNullOrEmpty(password) == true)
                    {
                        throw new WinAuthEncryptedSecretDataException();
                    }
                    data = Decrypt(data, password, true);
                    if (decode == true)
                    {
                        var plain = StringToByteArray(data);
                        data = Encoding.UTF8.GetString(plain, 0, plain.Length);
                    }
                }
                if ((encryptedTypes & PasswordTypes.YubiKeySlot1) != 0 || (encryptedTypes & PasswordTypes.YubiKeySlot2) != 0)
                {
                    throw new NotSupportedException();
                }
            }
            catch (WinAuthEncryptedSecretDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WinAuthBadPasswordException(ex.Message, ex);
            }

            return data;
        }

        /// <summary>
        /// Downgrade SHA256 or MD5 to SHA1 to be FIPS compliant
        /// </summary>
        public static HashAlgorithm SafeHasher(string name)
        {
            try
            {
                if (name == "SHA512")
                {
                    return SHA512.Create();
                }
                if (name == "SHA256")
                {
                    return SHA256.Create();
                }
                if (name == "MD5")
                {
                    return MD5.Create();
                }

                return SHA1.Create();
            }
            catch (Exception)
            {
                // FIPS only allows SHA1 before Windows 10
                return SHA1.Create();
            }
        }

        public static string EncryptSequence(string data, PasswordTypes passwordType, string? password)
        {
            // get hash of original
            var random = new RNGCryptoServiceProvider();
            var saltbytes = new byte[SALT_LENGTH];
            random.GetBytes(saltbytes);
            var salt = ByteArrayToString(saltbytes);

            string hash;
            //using (var sha = new SHA256Managed())
            using (var sha = SafeHasher("SHA256"))
            {
                var plain = StringToByteArray(salt + data);
                hash = ByteArrayToString(sha.ComputeHash(plain));
            }

            if ((passwordType & PasswordTypes.YubiKeySlot1) != 0 || (passwordType & PasswordTypes.YubiKeySlot2) != 0)
            {
                throw new NotSupportedException();
            }
            if ((passwordType & PasswordTypes.Explicit) != 0)
            {
                var encrypted = Encrypt(data, password);

                // test the encryption
                var decrypted = Decrypt(encrypted, password, true);
                if (string.Compare(data, decrypted) != 0)
                {
                    throw new WinAuthInvalidEncryptionException(data, password, encrypted, decrypted);
                }
                data = encrypted;
            }
            if ((passwordType & PasswordTypes.User) != 0)
            {
                // we encrypt the data using the Windows User account key
                var plain = StringToByteArray(data);
                var cipher = IProtectedData.Instance.Protect(plain, null, IProtectedData.DataProtectionScope.CurrentUser);
                data = ByteArrayToString(cipher);
            }
            if ((passwordType & PasswordTypes.Machine) != 0)
            {
                // we encrypt the data using the Local Machine account key
                var plain = StringToByteArray(data);
                var cipher = IProtectedData.Instance.Protect(plain, null, IProtectedData.DataProtectionScope.LocalMachine);
                data = ByteArrayToString(cipher);
            }

            // prepend the salt + hash
            return ENCRYPTION_HEADER + salt + hash + data;
        }

        /// <summary>
        /// Encrypt a string with a given key
        /// </summary>
        /// <param name="plain">data to encrypt - hex representation of byte array</param>
        /// <param name="password">key to use to encrypt</param>
        /// <returns>hex coded encrypted string</returns>
        public static string Encrypt(string plain, string? password)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password.ThrowIsNull(nameof(password)));

            // build a new salt
            var rg = new RNGCryptoServiceProvider();
            var saltbytes = new byte[SALT_LENGTH];
            rg.GetBytes(saltbytes);
            var salt = ByteArrayToString(saltbytes);

            // build our PBKDF2 key
            Rfc2898DeriveBytes kg = new(passwordBytes, saltbytes, PBKDF2_ITERATIONS);
            var key = kg.GetBytes(PBKDF2_KEYSIZE);

            return salt + Encrypt(plain, key);
        }

        /// <summary>
        /// Encrypt a string with a byte array key
        /// </summary>
        /// <param name="plain">data to encrypt - hex representation of byte array</param>
        /// <param name="passwordBytes">key to use to encrypt</param>
        /// <returns>hex coded encrypted string</returns>
        public static string Encrypt(string plain, byte[] key)
        {
            var inBytes = StringToByteArray(plain);

            // get our cipher
            BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new BlowfishEngine(), new ISO10126d2Padding());
            cipher.Init(true, new KeyParameter(key));

            // encrypt data
            var osize = cipher.GetOutputSize(inBytes.Length);
            var outBytes = new byte[osize];
            var olen = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
            olen += cipher.DoFinal(outBytes, olen);
            if (olen < osize)
            {
                var t = new byte[olen];
                Array.Copy(outBytes, 0, t, 0, olen);
                outBytes = t;
            }

            // return encoded byte->hex string
            return ByteArrayToString(outBytes);
        }

        /// <summary>
        /// Decrypt a hex-coded string using our MD5 or PBKDF2 generated key
        /// </summary>
        /// <param name="data">data string to be decrypted</param>
        /// <param name="key">decryption key</param>
        /// <param name="PBKDF2">flag to indicate we are using PBKDF2 to generate derived key</param>
        /// <returns>hex coded decrypted string</returns>
        public static string Decrypt(string data, string? password, bool PBKDF2)
        {
            byte[] key;
            var saltBytes = StringToByteArray(data.Substring(0, SALT_LENGTH * 2));

            if (PBKDF2 == true)
            {
                // extract the salt from the data
                var passwordBytes = Encoding.UTF8.GetBytes(password.ThrowIsNull(nameof(password)));

                // build our PBKDF2 key
#if NETCF
			PBKDF2 kg = new PBKDF2(passwordBytes, saltbytes, 2000);
#else
                var kg = new Rfc2898DeriveBytes(passwordBytes, saltBytes, PBKDF2_ITERATIONS);
#endif
                key = kg.GetBytes(PBKDF2_KEYSIZE);
            }
            else
            {
                // extract the salt from the data
                var passwordBytes = Encoding.Default.GetBytes(password.ThrowIsNull(nameof(password)));
                key = new byte[saltBytes.Length + passwordBytes.Length];
                Array.Copy(saltBytes, key, saltBytes.Length);
                Array.Copy(passwordBytes, 0, key, saltBytes.Length, passwordBytes.Length);
                // build out combined key
                var md5 = MD5.Create();
                key = md5.ComputeHash(key);
            }

            // extract the actual data to be decrypted
            var inBytes = StringToByteArray(data[(SALT_LENGTH * 2)..]);

            // get cipher
            BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new BlowfishEngine(), new ISO10126d2Padding());
            cipher.Init(false, new KeyParameter(key));

            // decrypt the data
            var osize = cipher.GetOutputSize(inBytes.Length);
            var outBytes = new byte[osize];
            try
            {
                var olen = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
                olen += cipher.DoFinal(outBytes, olen);
                if (olen < osize)
                {
                    var t = new byte[olen];
                    Array.Copy(outBytes, 0, t, 0, olen);
                    outBytes = t;
                }
            }
            catch (Exception)
            {
                // an exception is due to bad password
                throw new WinAuthBadPasswordException();
            }

            // return encoded string
            return ByteArrayToString(outBytes);
        }

        /// <summary>
        /// Decrypt a hex-encoded string with a byte array key
        /// </summary>
        /// <param name="data">hex-encoded string</param>
        /// <param name="key">key for decryption</param>
        /// <returns>hex-encoded plain text</returns>
        public static string Decrypt(string data, byte[] key)
        {
            // the actual data to be decrypted
            var inBytes = StringToByteArray(data);

            // get cipher
            BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new BlowfishEngine(), new ISO10126d2Padding());
            cipher.Init(false, new KeyParameter(key));

            // decrypt the data
            var osize = cipher.GetOutputSize(inBytes.Length);
            var outBytes = new byte[osize];
            try
            {
                var olen = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
                olen += cipher.DoFinal(outBytes, olen);
                if (olen < osize)
                {
                    var t = new byte[olen];
                    Array.Copy(outBytes, 0, t, 0, olen);
                    outBytes = t;
                }
            }
            catch (Exception)
            {
                // an exception is due to bad password
                throw new WinAuthBadPasswordException();
            }

            // return encoded string
            return ByteArrayToString(outBytes);
        }

        /// <summary>
        /// Wrapped TryParse for compatibility with NETCF35 to simulate long.TryParse
        /// </summary>
        /// <param name="s">string of value to parse</param>
        /// <param name="val">out long value</param>
        /// <returns>true if value was parsed</returns>
        protected internal static bool LongTryParse(string s, out long val)
        {
            return long.TryParse(s, out val);
        }

        protected static int GetStatusCode(WebException webException)
        {
            var webResponse = webException.Response;
            return GetStatusCode(webResponse.ThrowIsNull(nameof(webResponse)));
        }

        protected static int GetStatusCode(WebResponse webResponse)
        {
            if (webResponse is HttpWebResponse httpWebResponse)
            {
                return (int)httpWebResponse.StatusCode;
            }
            else
            {
                throw new ArgumentException(nameof(webResponse));
            }
        }

        protected static string GetStatusDescription(WebException webException)
        {
            var webResponse = webException.Response;
            return GetStatusDescription(webResponse.ThrowIsNull(nameof(webResponse)));
        }

        protected static string GetStatusDescription(WebResponse webResponse)
        {
            if (webResponse is HttpWebResponse httpWebResponse)
            {
                return httpWebResponse.StatusDescription;
            }
            else
            {
                throw new ArgumentException(nameof(webResponse));
            }
        }

        #endregion
    }
}