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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using System.Collections;
using System.Security;

#if NUNIT
using NUnit.Framework;
#endif

#if NETCF
using OpenNETCF.Security.Cryptography;
#endif

namespace WinAuth
{
  /// <summary>
  /// Class that implements base RFC 4226 an RFC 6238 authenticator
  /// </summary>
  public abstract class Authenticator : ICloneable
  {
    /// <summary>
    /// Number of bytes making up the salt
    /// </summary>
    private const int SALT_LENGTH = 8;

    /// <summary>
    /// Number of iterations in PBKDF2 key generation
    /// </summary>
    private const int PBKDF2_ITERATIONS = 2000;

    /// <summary>
    /// Size of derived PBKDF2 key
    /// </summary>
    private const int PBKDF2_KEYSIZE = 256;

    /// <summary>
    /// Version for encrpytion changes
    /// </summary>
    private static string ENCRYPTION_HEADER = Authenticator.ByteArrayToString(Encoding.UTF8.GetBytes("WINAUTH3"));

    /// <summary>
    /// Default number of digits in code
    /// </summary>
    public const int DEFAULT_CODE_DIGITS = 6;

    /// <summary>
    /// Default period of 30s
    /// </summary>
    public const int DEFAULT_PERIOD = 30;

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

    /// <summary>
    /// HMAC hashing algorithm types
    /// </summary>
    public enum HMACTypes
    {
      SHA1 = 0,
      SHA256 = 1,
      SHA512 = 2
    }

    public const HMACTypes DEFAULT_HMAC_TYPE = HMACTypes.SHA1;

    #region Authenticator data

    /// <summary>
    /// Serial number of authenticator
    /// </summary>
    //public virtual string Serial { get; set; }

    /// <summary>
    /// Secret key used for Authenticator
    /// </summary>
    public byte[] SecretKey { get; set; }

    /// <summary>
    /// Time difference in milliseconds of our machine and server
    /// </summary>
    public long ServerTimeDiff { get; set; }

    /// <summary>
    /// Time of last synced
    /// </summary>
    public long LastServerTime { get; set; }

    /// <summary>
    /// Type of password used to encrypt secretdata
    /// </summary>
    public PasswordTypes PasswordType { get; private set; }

    /// <summary>
    /// Password used to encrypt secretdata (if PasswordType == Explict)
    /// </summary>
    protected string Password { get; set; }

    /// <summary>
    /// Hash of secret data to detect changes
    /// </summary>
    protected byte[] SecretHash { get; private set; }

    public bool RequiresPassword { get; private set; }

    /// <summary>
    /// The data current saved with the current encryption and/or password (might be none)
    /// </summary>
    protected string EncryptedData { get; private set; }

    /// <summary>
    /// Number of digits returned in code (default is 6)
    /// </summary>
    public int CodeDigits { get; set; }

    /// <summary>
    /// Hashing algorithm used for OTP generation (default is SHA1)
    /// </summary>
    public HMACTypes HMACType { get; set; }

    /// <summary>
    /// Period in seconds for next code
    /// </summary>
    public int Period { get; set; }

    /// <summary>
    /// Name of issuer
    /// </summary>
    public virtual string Issuer { get; set; }

    /// <summary>
    /// Get/set the combined secret data value
    /// </summary>
    public virtual string SecretData
    {
      get
      {
        // this is the secretkey
        return Authenticator.ByteArrayToString(SecretKey) + "\t" + this.CodeDigits.ToString() + "\t" + this.HMACType.ToString() + "\t" + this.Period.ToString();
      }
      set
      {
        if (string.IsNullOrEmpty(value) == false)
        {
          string[] parts = value.Split('|')[0].Split('\t');
          SecretKey = Authenticator.StringToByteArray(parts[0]);
          if (parts.Length > 1)
          {
            int digits;
            if (int.TryParse(parts[1], out digits) == true)
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
            int period;
            if (int.TryParse(parts[3], out period) == true)
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

    /// <summary>
    /// Advanced script saved with authenticator so it is also encrypted
    /// </summary>
    //public string Script {get; set;}

    /// <summary>
    /// Get the server time since 1/1/70
    /// </summary>
    public long ServerTime
    {
      get
      {
        return CurrentTime + ServerTimeDiff;
      }
    }

    /// <summary>
    /// Calculate the code interval based on the calculated server time
    /// </summary>
    public long CodeInterval
    {
      get
      {
        // calculate the code interval; the server's time div 30,000
        return (CurrentTime + ServerTimeDiff) / ((long)this.Period * 1000L);
      }
    }

    /// <summary>
    /// Get the current code for the authenticator.
    /// </summary>
    /// <returns>authenticator code</returns>
    public string CurrentCode
    {
      get
      {
        if (this.SecretKey == null && this.EncryptedData != null)
        {
          throw new EncryptedSecretDataException();
        }

        return CalculateCode(false);
      }
    }

    #endregion

    /// <summary>
    /// Static initializer
    /// </summary>
    static Authenticator()
    {
      // Issue#71: remove the default .net expect header, which can cause issues (http://stackoverflow.com/questions/566437/)
      System.Net.ServicePointManager.Expect100Continue = false;
    }

    /// <summary>
    /// Create a new Authenticator object
    /// </summary>
    public Authenticator(int codeDigits = DEFAULT_CODE_DIGITS, HMACTypes hmacType = HMACTypes.SHA1, int period = DEFAULT_PERIOD)
    {
      CodeDigits = codeDigits;
      HMACType = hmacType;
      Period = period;
    }

    /// <summary>
    /// Calculate the current code for the authenticator.
    /// </summary>
    /// <param name="resyncTime">flag to resync time</param>
    /// <returns>authenticator code</returns>
    protected virtual string CalculateCode(bool resync = false, long interval = -1)
    {
      // sync time if required
      if (resync == true || ServerTimeDiff == 0)
      {
        if (interval > 0)
        {
          ServerTimeDiff = (interval * ((long)this.Period * 1000L)) - CurrentTime;
        }
        else
        {
          Sync();
        }
      }

      HMac hmac;
      switch (HMACType)
      {
        case HMACTypes.SHA1:
          hmac = new HMac(new Sha1Digest());
          break;
        case HMACTypes.SHA256:
          hmac = new HMac(new Sha256Digest());
          break;
        case HMACTypes.SHA512:
          hmac = new HMac(new Sha512Digest());
          break;
        default:
          throw new InvalidHMACAlgorithmException();
      }
      hmac.Init(new KeyParameter(SecretKey));

      byte[] codeIntervalArray = BitConverter.GetBytes(CodeInterval);
      if (BitConverter.IsLittleEndian)
      {
        Array.Reverse(codeIntervalArray);
      }
      hmac.BlockUpdate(codeIntervalArray, 0, codeIntervalArray.Length);

      byte[] mac = new byte[hmac.GetMacSize()];
      hmac.DoFinal(mac, 0);

      // the last 4 bits of the mac say where the code starts (e.g. if last 4 bit are 1100, we start at byte 12)
      int start = mac.Last() & 0x0f;

      // extract those 4 bytes
      byte[] bytes = new byte[4];
      Array.Copy(mac, start, bytes, 0, 4);
      if (BitConverter.IsLittleEndian)
      {
        Array.Reverse(bytes);
      }
      uint fullcode = BitConverter.ToUInt32(bytes, 0) & 0x7fffffff;

      // we use the last 8 digits of this code in radix 10
      uint codemask = (uint)Math.Pow(10, CodeDigits);
      string format = new string('0', CodeDigits);
      string code = (fullcode % codemask).ToString(format);

      return code;
    }

    /// <summary>
    /// Synchorise this authenticator's time with server time. We update our data record with the difference from our UTC time.
    /// </summary>
    public abstract void Sync();

    #region Load / Save

    public static Authenticator ReadXmlv2(XmlReader reader, string password = null)
    {
      Authenticator authenticator = null;
      string authenticatorType = reader.GetAttribute("type");
      if (string.IsNullOrEmpty(authenticatorType) == false)
      {
        authenticatorType = authenticatorType.Replace("WindowsAuthenticator.", "WinAuth.");
        Type type = System.Reflection.Assembly.GetExecutingAssembly().GetType(authenticatorType, false, true);
        authenticator = Activator.CreateInstance(type) as Authenticator;
      }
      if (authenticator == null)
      {
        authenticator = new BattleNetAuthenticator();
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
              string encrypted = reader.GetAttribute("encrypted");
              string data = reader.ReadElementContentAsString();

              PasswordTypes passwordType = DecodePasswordTypes(encrypted);

              if (passwordType != PasswordTypes.None)
              {
                // this is an old version so there is no hash
                data = DecryptSequence(data, passwordType, password, null);
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
    public static PasswordTypes DecodePasswordTypes(string passwordTypes)
    {
      PasswordTypes passwordType = PasswordTypes.None;
      if (string.IsNullOrEmpty(passwordTypes) == true)
      {
        return passwordType;
      }

      char[] types = passwordTypes.ToCharArray();
      for (int i = types.Length - 1; i >= 0; i--)
      {
        char type = types[i];
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
      StringBuilder encryptedTypes = new StringBuilder();
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

    public void SetEncryption(PasswordTypes passwordType, string password = null)
    {
      // check if still encrpyted
      if (this.RequiresPassword == true)
      {
        // have to decrypt to be able to re-encrypt
        throw new EncryptedSecretDataException();
      }

      if (passwordType == PasswordTypes.None)
      {
        this.RequiresPassword = false;
        this.EncryptedData = null;
        this.PasswordType = passwordType;
      }
      else
      {
        using (MemoryStream ms = new MemoryStream())
        {
          // get the plain version
          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Indent = true;
          settings.Encoding = Encoding.UTF8;
          using (XmlWriter encryptedwriter = XmlWriter.Create(ms, settings))
          {
            string encrpytedData = this.EncryptedData;
            Authenticator.PasswordTypes savedpasswordType = PasswordType;
            try
            {
              PasswordType = Authenticator.PasswordTypes.None;
              EncryptedData = null;
              WriteToWriter(encryptedwriter);
            }
            finally
            {
              this.PasswordType = savedpasswordType;
              this.EncryptedData = encrpytedData;
            }
          }
          string data = Authenticator.ByteArrayToString(ms.ToArray());

          // update secret hash
          using (SHA1 sha1 = SHA1.Create())
          {
            this.SecretHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(this.SecretData));
          }

          // encrypt
          this.EncryptedData = Authenticator.EncryptSequence(data, passwordType, password, null);
          this.PasswordType = passwordType;
          if (this.PasswordType == PasswordTypes.Explicit)
          {
            this.SecretData = null;
            this.RequiresPassword = true;
          }
        }
      }
    }

    public void Protect()
    {
      if (this.PasswordType != PasswordTypes.None)
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

        this.SecretData = null;
        this.RequiresPassword = true;
        this.Password = null;
      }
    }

    public bool Unprotect(string password)
    {
      PasswordTypes passwordType = this.PasswordType;
      if (passwordType == PasswordTypes.None)
      {
        throw new InvalidOperationException("Cannot Unprotect a non-encrypted authenticator");
      }

      // decrypt
      bool changed = false;
      try
      {
        string data = Authenticator.DecryptSequence(this.EncryptedData, this.PasswordType, password, null);
        using (MemoryStream ms = new MemoryStream(Authenticator.StringToByteArray(data)))
        {
          XmlReader reader = XmlReader.Create(ms);
          changed = this.ReadXml(reader, password) || changed;
        }
        this.RequiresPassword = false;
        // calculate hash of current secretdata
        using (SHA1 sha1 = SHA1.Create())
        {
          this.SecretHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(this.SecretData));
        }
        // keep the password until we reprotect in case data changes
        this.Password = password;

        if (changed == true)
        {
          // we need to encrypt changed secret data
          using (MemoryStream ms = new MemoryStream())
          {
            // get the plain version
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            using (XmlWriter encryptedwriter = XmlWriter.Create(ms, settings))
            {
              WriteToWriter(encryptedwriter);
            }
            string encrypteddata = Authenticator.ByteArrayToString(ms.ToArray());

            // update secret hash
            using (SHA1 sha1 = SHA1.Create())
            {
              this.SecretHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(this.SecretData));
            }

            // encrypt
            this.EncryptedData = Authenticator.EncryptSequence(encrypteddata, passwordType, password, null);
          }
        }

        return changed;
      }
      catch (EncryptedSecretDataException)
      {
        this.RequiresPassword = true;
        throw;
      }
      finally
      {
        this.PasswordType = passwordType;
      }
    }

    public bool ReadXml(XmlReader reader, string password = null)
    {
      // decode the password type
      string encrypted = reader.GetAttribute("encrypted");
      PasswordTypes passwordType = DecodePasswordTypes(encrypted);
      this.PasswordType = passwordType;

      if (passwordType != PasswordTypes.None)
      {
        // read the encrypted text from the node
        this.EncryptedData = reader.ReadElementContentAsString();
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
      if (this is HOTPAuthenticator)
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
      string encrypted = EncodePasswordTypes(this.PasswordType);
      if (string.IsNullOrEmpty(encrypted) == false)
      {
        writer.WriteAttributeString("encrypted", encrypted);
      }

      if (this.PasswordType != PasswordTypes.None)
      {
        writer.WriteRaw(this.EncryptedData);
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

      RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

      byte[] randomblock = new byte[length];

      SHA1 sha1 = SHA1.Create();
      int i = 0;
      do
      {
        byte[] hashBlock = new byte[128];
        random.GetBytes(hashBlock);

        byte[] key = sha1.ComputeHash(hashBlock, 0, hashBlock.Length);
        if (key.Length >= randomblock.Length)
        {
          Array.Copy(key, 0, randomblock, i, randomblock.Length);
          break;
        }
        Array.Copy(key, 0, randomblock, i, key.Length);
        i += key.Length;
      } while (true);

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
      int len = hex.Length;
      byte[] bytes = new byte[len / 2];
      for (int i = 0; i < len; i += 2)
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
    /// <param name="yubidata">optional yubi data</param>
    /// <param name="decode"></param>
    /// <returns>decrypted string sequence</returns>
    public static string DecryptSequence(string data, PasswordTypes encryptedTypes, string password, YubiKey yubi, bool decode = false)
    {
      // check for encrpytion header
      if (data.Length < ENCRYPTION_HEADER.Length || data.IndexOf(ENCRYPTION_HEADER) != 0)
      {
        return DecryptSequenceNoHash(data, encryptedTypes, password, yubi, decode);
      }

      // extract salt and hash
      //using (var sha = new SHA256Managed())
      using (var sha = SafeHasher("SHA256"))
      {
        // jump header
        int datastart = ENCRYPTION_HEADER.Length;
        string salt = data.Substring(datastart, Math.Min(SALT_LENGTH * 2, data.Length - datastart));
        datastart += salt.Length;
        string hash = data.Substring(datastart, Math.Min(sha.HashSize / 8 * 2, data.Length - datastart));
        datastart += hash.Length;
        data = data.Substring(datastart);

        data = DecryptSequenceNoHash(data, encryptedTypes, password, yubi);

        // check the hash
        byte[] compareplain = StringToByteArray(salt + data);
        string comparehash = ByteArrayToString(sha.ComputeHash(compareplain));
        if (string.Compare(comparehash, hash) != 0)
        {
          throw new BadPasswordException();
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
    /// <param name="yubidata">optional yubi data</param>
    /// <param name="decode"></param>
    /// <returns>decrypted string sequence</returns>
    private static string DecryptSequenceNoHash(string data, PasswordTypes encryptedTypes, string password, YubiKey yubi, bool decode = false)
    {
      try
      {
        // reverse order they were encrypted
        if ((encryptedTypes & PasswordTypes.Machine) != 0)
        {
          // we are going to decrypt with the Windows local machine key
          byte[] cipher = Authenticator.StringToByteArray(data);
          byte[] plain = ProtectedData.Unprotect(cipher, null, DataProtectionScope.LocalMachine);
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
          byte[] cipher = StringToByteArray(data);
          byte[] plain = ProtectedData.Unprotect(cipher, null, DataProtectionScope.CurrentUser);
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
            throw new EncryptedSecretDataException();
          }
          data = Authenticator.Decrypt(data, password, true);
          if (decode == true)
          {
            byte[] plain = Authenticator.StringToByteArray(data);
            data = Encoding.UTF8.GetString(plain, 0, plain.Length);
          }
        }
        if ((encryptedTypes & PasswordTypes.YubiKeySlot1) != 0 || (encryptedTypes & PasswordTypes.YubiKeySlot2) != 0)
        {
          if (string.IsNullOrEmpty(yubi.Info.Error) == false)
          {
            throw new BadYubiKeyException("Unable to detect YubiKey");
          }
          if (yubi.Info.Status.VersionMajor == 0)
          {
            throw new BadYubiKeyException("Please insert your YubiKey");
          }
          int slot = ((encryptedTypes & PasswordTypes.YubiKeySlot1) != 0 ? 1 : 2);

          string seed = data.Substring(0, SALT_LENGTH * 2);
          data = data.Substring(seed.Length);
          byte[] key = yubi.ChallengeResponse(slot, StringToByteArray(seed));

          data = Authenticator.Decrypt(data, key);
          if (decode == true)
          {
            byte[] plain = Authenticator.StringToByteArray(data);
            data = Encoding.UTF8.GetString(plain, 0, plain.Length);
          }

          yubi.YubiData.Seed = seed;
          yubi.YubiData.Data = key;
        }
      }
      catch (EncryptedSecretDataException)
      {
        throw;
      }
      catch (BadYubiKeyException)
      {
        throw;
      }
      catch (ChallengeResponseException ex)
      {
        throw new BadYubiKeyException("Please check your YubiKey or touch the flashing button", ex);
      }
      catch (Exception ex)
      {
        throw new BadPasswordException(ex.Message, ex);
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

    public static string EncryptSequence(string data, PasswordTypes passwordType, string password, YubiKey yubi)
    {
      // get hash of original
      var random = new RNGCryptoServiceProvider();
      byte[] saltbytes = new byte[SALT_LENGTH];
      random.GetBytes(saltbytes);
      string salt = ByteArrayToString(saltbytes);

      string hash;
      //using (var sha = new SHA256Managed())
      using (var sha = SafeHasher("SHA256"))
      {
        byte[] plain = StringToByteArray(salt + data);
        hash = ByteArrayToString(sha.ComputeHash(plain));
      }

      if ((passwordType & PasswordTypes.YubiKeySlot1) != 0 || (passwordType & PasswordTypes.YubiKeySlot2) != 0)
      {
        if (yubi.YubiData.Length == 0)
        {
          byte[] seed = new byte[SALT_LENGTH];
          random = new RNGCryptoServiceProvider();
          random.GetBytes(seed);

          // we encrypt the data using the hash of a random string from the YubiKey
          int slot = ((passwordType & PasswordTypes.YubiKeySlot1) != 0 ? 1 : 2);
          yubi.YubiData.Data = yubi.ChallengeResponse(slot, seed);
          yubi.YubiData.Seed = Authenticator.ByteArrayToString(seed);
        }

        byte[] key = yubi.YubiData.Data;
        string encrypted = Encrypt(data, key);

        // test the encryption
        string decrypted = Decrypt(encrypted, key);
        if (string.Compare(data, decrypted) != 0)
        {
          throw new InvalidEncryptionException(data, password, encrypted, decrypted);
        }
        data = yubi.YubiData.Seed + encrypted;
      }
      if ((passwordType & PasswordTypes.Explicit) != 0)
      {
        string encrypted = Encrypt(data, password);

        // test the encryption
        string decrypted = Decrypt(encrypted, password, true);
        if (string.Compare(data, decrypted) != 0)
        {
          throw new InvalidEncryptionException(data, password, encrypted, decrypted);
        }
        data = encrypted;
      }
      if ((passwordType & PasswordTypes.User) != 0)
      {
        // we encrypt the data using the Windows User account key
        byte[] plain = StringToByteArray(data);
        byte[] cipher = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
        data = ByteArrayToString(cipher);
      }
      if ((passwordType & PasswordTypes.Machine) != 0)
      {
        // we encrypt the data using the Local Machine account key
        byte[] plain = StringToByteArray(data);
        byte[] cipher = ProtectedData.Protect(plain, null, DataProtectionScope.LocalMachine);
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
    public static string Encrypt(string plain, string password)
    {
      byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

      // build a new salt
      RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider();
      byte[] saltbytes = new byte[SALT_LENGTH];
      rg.GetBytes(saltbytes);
      string salt = Authenticator.ByteArrayToString(saltbytes);

      // build our PBKDF2 key
#if NETCF
			PBKDF2 kg = new PBKDF2(passwordBytes, saltbytes, PBKDF2_ITERATIONS);
#else
      Rfc2898DeriveBytes kg = new Rfc2898DeriveBytes(passwordBytes, saltbytes, PBKDF2_ITERATIONS);
#endif
      byte[] key = kg.GetBytes(PBKDF2_KEYSIZE);

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
      byte[] inBytes = Authenticator.StringToByteArray(plain);

      // get our cipher
      BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new BlowfishEngine(), new ISO10126d2Padding());
      cipher.Init(true, new KeyParameter(key));

      // encrypt data
      int osize = cipher.GetOutputSize(inBytes.Length);
      byte[] outBytes = new byte[osize];
      int olen = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
      olen += cipher.DoFinal(outBytes, olen);
      if (olen < osize)
      {
        byte[] t = new byte[olen];
        Array.Copy(outBytes, 0, t, 0, olen);
        outBytes = t;
      }

      // return encoded byte->hex string
      return Authenticator.ByteArrayToString(outBytes);
    }

    /// <summary>
    /// Decrypt a hex-coded string using our MD5 or PBKDF2 generated key
    /// </summary>
    /// <param name="data">data string to be decrypted</param>
    /// <param name="key">decryption key</param>
    /// <param name="PBKDF2">flag to indicate we are using PBKDF2 to generate derived key</param>
    /// <returns>hex coded decrypted string</returns>
    public static string Decrypt(string data, string password, bool PBKDF2)
    {
      byte[] key;
      byte[] saltBytes = Authenticator.StringToByteArray(data.Substring(0, SALT_LENGTH * 2));

      if (PBKDF2 == true)
      {
        // extract the salt from the data
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        // build our PBKDF2 key
#if NETCF
			PBKDF2 kg = new PBKDF2(passwordBytes, saltbytes, 2000);
#else
        Rfc2898DeriveBytes kg = new Rfc2898DeriveBytes(passwordBytes, saltBytes, PBKDF2_ITERATIONS);
#endif
        key = kg.GetBytes(PBKDF2_KEYSIZE);
      }
      else
      {
        // extract the salt from the data
        byte[] passwordBytes = Encoding.Default.GetBytes(password);
        key = new byte[saltBytes.Length + passwordBytes.Length];
        Array.Copy(saltBytes, key, saltBytes.Length);
        Array.Copy(passwordBytes, 0, key, saltBytes.Length, passwordBytes.Length);
        // build out combined key
        MD5 md5 = MD5.Create();
        key = md5.ComputeHash(key);
      }

      // extract the actual data to be decrypted
      byte[] inBytes = Authenticator.StringToByteArray(data.Substring(SALT_LENGTH * 2));

      // get cipher
      BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new BlowfishEngine(), new ISO10126d2Padding());
      cipher.Init(false, new KeyParameter(key));

      // decrypt the data
      int osize = cipher.GetOutputSize(inBytes.Length);
      byte[] outBytes = new byte[osize];
      try
      {
        int olen = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
        olen += cipher.DoFinal(outBytes, olen);
        if (olen < osize)
        {
          byte[] t = new byte[olen];
          Array.Copy(outBytes, 0, t, 0, olen);
          outBytes = t;
        }
      }
      catch (Exception)
      {
        // an exception is due to bad password
        throw new BadPasswordException();
      }

      // return encoded string
      return Authenticator.ByteArrayToString(outBytes);
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
      byte[] inBytes = Authenticator.StringToByteArray(data);

      // get cipher
      BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new BlowfishEngine(), new ISO10126d2Padding());
      cipher.Init(false, new KeyParameter(key));

      // decrypt the data
      int osize = cipher.GetOutputSize(inBytes.Length);
      byte[] outBytes = new byte[osize];
      try
      {
        int olen = cipher.ProcessBytes(inBytes, 0, inBytes.Length, outBytes, 0);
        olen += cipher.DoFinal(outBytes, olen);
        if (olen < osize)
        {
          byte[] t = new byte[olen];
          Array.Copy(outBytes, 0, t, 0, olen);
          outBytes = t;
        }
      }
      catch (Exception)
      {
        // an exception is due to bad password
        throw new BadPasswordException();
      }

      // return encoded string
      return Authenticator.ByteArrayToString(outBytes);
    }

    /// <summary>
    /// Wrapped TryParse for compatibility with NETCF35 to simulate long.TryParse
    /// </summary>
    /// <param name="s">string of value to parse</param>
    /// <param name="val">out long value</param>
    /// <returns>true if value was parsed</returns>
    protected internal static bool LongTryParse(string s, out long val)
    {
#if NETCF
			try
			{
				val = long.Parse(s);
				return true;
			}
			catch (Exception )
			{
				val = 0;
				return false;
			}
#else
      return long.TryParse(s, out val);
#endif
    }

    #endregion

    #region ICloneable

    /// <summary>
    /// Clone the current object
    /// </summary>
    /// <returns>return clone</returns>
    public object Clone()
    {
      // we only need to do shallow copy
      return this.MemberwiseClone();
    }

    #endregion

#if NETCF
		/// <summary>
		/// Private class that implements PBKDF2 needed for older NETCF. Implemented from http://en.wikipedia.org/wiki/PBKDF2.
		/// </summary>
		private class PBKDF2
		{
			/// <summary>
			/// Our digest
			/// </summary>
			private HMac m_mac;

			/// <summary>
			/// Digest length
			/// </summary>
			private int m_hlen;

			/// <summary>
			/// Base password
			/// </summary>
			private byte[] m_password;

			/// <summary>
			/// Salt
			/// </summary>
			private byte[] m_salt;

			/// <summary>
			/// Number of iterations
			/// </summary>
			private int m_iterations;

			/// <summary>
			/// Create a new PBKDF2 object
			/// </summary>
			public PBKDF2(byte[] password, byte[] salt, int iterations)
			{
				m_password = password;
				m_salt = salt;
				m_iterations = iterations;

				m_mac = new HMac(new Sha1Digest());
				m_hlen = m_mac.GetMacSize();
			}

			/// <summary>
			/// Calculate F.
			/// F(P,S,c,i) = U1 ^ U2 ^ ... ^ Uc
			/// Where F is an xor of c iterations of chained PRF. First iteration of PRF uses master password P as PRF key and salt concatenated to i. Second and greater PRF uses P and output of previous PRF computation:
			/// </summary>
			/// <param name="P"></param>
			/// <param name="S"></param>
			/// <param name="c"></param>
			/// <param name="i"></param>
			/// <param name="DK"></param>
			/// <param name="DKoffset"></param>
			private void F(byte[] P, byte[] S, int c, byte[] i, byte[] DK, int DKoffset)
			{
				// first iteration (ses master password P as PRF key and salt concatenated to i)
				byte[] buf = new byte[m_hlen];
				ICipherParameters param = new KeyParameter(P);
				m_mac.Init(param);
				m_mac.BlockUpdate(S, 0, S.Length);
				m_mac.BlockUpdate(i, 0, i.Length);
				m_mac.DoFinal(buf, 0);
				Array.Copy(buf, 0, DK, DKoffset, buf.Length);

				// remaining iterations (uses P and output of previous PRF computation)
				for (int iter = 1; iter < c; iter++)
				{
					m_mac.Init(param);
					m_mac.BlockUpdate(buf, 0, buf.Length);
					m_mac.DoFinal(buf, 0);

					for (int j=buf.Length-1; j >= 0; j--)
					{
						DK[DKoffset + j] ^= buf[j];
					}
				}
			}

			/// <summary>
			/// Calculate a derived key of dkLen bytes long from our initial password and salt
			/// </summary>
			/// <param name="dkLen">Length of desired key to be returned</param>
			/// <returns>derived key of dkLen bytes</returns>
			public byte[] GetBytes(int dkLen)
			{
				// For each hLen-bit block Ti of derived key DK, computing is as follows:
				//  DK = T1 || T2 || ... || Tdklen/hlen
				//  Ti = F(P,S,c,i)
				int chunks = (dkLen + m_hlen - 1) / m_hlen;
				byte[] DK = new byte[chunks * m_hlen];
				byte[] idata = new byte[4];
				for (int i = 1; i <= chunks; i++)
				{
					idata[0] = (byte)((uint)i >> 24);
					idata[1] = (byte)((uint)i >> 16);
					idata[2] = (byte)((uint)i >> 8);
					idata[3] = (byte)i; 

					F(m_password, m_salt, m_iterations, idata, DK, (i-1) * m_hlen);
				}
				if (DK.Length > dkLen)
				{
					byte[] reduced = new byte[dkLen];
					Array.Copy(DK, 0, reduced, 0, dkLen);
					DK = reduced;
				}

				return DK;
			}
		}

#if NUNIT
		/// Test against standard test vectors and one of our own of the correct iterations.
		/// http://tools.ietf.org/html/draft-josefsson-pbkdf2-test-vectors-00
		[Test]
		public static void TestPBKDF2()
		{
			PBKDF2 kg;
			byte[] DK;

			byte[] tv1 = new byte[] { 0x0c, 0x60, 0xc8, 0x0f, 0x96, 0x1f, 0x0e, 0x71, 0xf3, 0xa9, 0xb5, 0x24, 0xaf, 0x60, 0x12, 0x06, 0x2f, 0xe0, 0x37, 0xa6 };
			kg = new PBKDF2(Encoding.Default.GetBytes("password"), Encoding.Default.GetBytes("salt"), 1);
			DK = kg.GetBytes(20);
			Assert.AreEqual(DK, tv1);

			byte[] tv2 = new byte[] { 0xea, 0x6c, 0x01, 0x4d, 0xc7, 0x2d, 0x6f, 0x8c, 0xcd, 0x1e, 0xd9, 0x2a, 0xce, 0x1d, 0x41, 0xf0, 0xd8, 0xde, 0x89, 0x57 };
			kg = new PBKDF2(Encoding.Default.GetBytes("password"), Encoding.Default.GetBytes("salt"), 2);
			DK = kg.GetBytes(20);
			Assert.AreEqual(DK, tv2);

			byte[] tv3 = new byte[] { 0x4b, 0x00, 0x79, 0x01, 0xb7, 0x65, 0x48, 0x9a, 0xbe, 0xad, 0x49, 0xd9, 0x26, 0xf7, 0x21, 0xd0, 0x65, 0xa4, 0x29, 0xc1 };
			kg = new PBKDF2(Encoding.Default.GetBytes("password"), Encoding.Default.GetBytes("salt"), 4096);
			DK = kg.GetBytes(20);
			Assert.AreEqual(DK, tv3);

			byte[] tv4 = new byte[] { 0x2f, 0x25, 0x5b, 0x3a, 0x95, 0x46, 0x3c, 0x76, 0x62, 0x1f, 0x06, 0x80, 0xa2, 0xb3, 0x35, 0xad, 0x90, 0x3b, 0x85, 0xde };
			kg = new PBKDF2(Encoding.Default.GetBytes("VXFr[24c=6(D8He"), Encoding.Default.GetBytes("salt"), 1000);
			DK = kg.GetBytes(20);
			Assert.AreEqual(DK, tv4);
		}
#endif

#endif

  }
}
