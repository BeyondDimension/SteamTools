/*
 * Copyright (C) 2013 Colin Mackie.
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
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

#if NUNIT
using NUnit.Framework;
#endif

#if NETCF
using OpenNETCF.Security.Cryptography;
#endif

namespace WinAuth
{
    /// <summary>
    /// Class that implements Battle.net Mobile Authenticator v1.1.0.
    /// </summary>
    public class BattleNetAuthenticator : Authenticator
    {
        /// <summary>
        /// Size of model string
        /// </summary>
        private const int MODEL_SIZE = 16;

        /// <summary>
        /// String of possible chars we use in our random model string
        /// </summary>
        private const string MODEL_CHARS = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890";

        /// <summary>
        /// Buffer size used on Http responses
        /// </summary>
        private const int RESPONSE_BUFFER_SIZE = 64;

        /// <summary>
        /// Expect size of return data from enroll
        /// </summary>
        private const int ENROLL_RESPONSE_SIZE = 45;

        /// <summary>
        /// Expected size of return data from time sync
        /// </summary>
        private const int SYNC_RESPONSE_SIZE = 8;

        /// <summary>
        /// Buffer size used on Restore call
        /// </summary>
        private const int RESTOREINIT_BUFFER_SIZE = 32;

        /// <summary>
        /// Buffer size used on Restore Validation call
        /// </summary>
        private const int RESTOREVALIDATE_BUFFER_SIZE = 20;

        /// <summary>
        /// Number of digits in code
        /// </summary>
        private const int CODE_DIGITS = 8;

        /// <summary>
        /// The public key modulus used to encrypt our data
        /// </summary>
        private const string ENROLL_MODULUS =
            "955e4bd989f3917d2f15544a7e0504eb9d7bb66b6f8a2fe470e453c779200e5e" +
            "3ad2e43a02d06c4adbd8d328f1a426b83658e88bfd949b2af4eaf30054673a14" +
            "19a250fa4cc1278d12855b5b25818d162c6e6ee2ab4a350d401d78f6ddb99711" +
            "e72626b48bd8b5b0b7f3acf9ea3c9e0005fee59e19136cdb7c83f2ab8b0a2a99";

        /// <summary>
        /// Public key exponent used to encrypt our data
        /// </summary>
        private const string ENROLL_EXPONENT =
            "0101";

        /// <summary>
        /// Number of minutes to ignore syncing if network error
        /// </summary>
        private const int SYNC_ERROR_MINUTES = 5;

        private const string BATTLENET_ISSUER = "BattleNet";

        /// <summary>
        /// URLs for all mobile services
        /// </summary>
        private static string REGION_US = "US";
        private static string REGION_EU = "EU";
        private static string REGION_KR = "KR";
        private static string REGION_CN = "CN";
        public static Dictionary<string, string> MOBILE_URLS = new Dictionary<string, string>
        {
            {REGION_US, "http://mobile-service.blizzard.com"},
            {REGION_EU, "http://mobile-service.blizzard.com"},
            {REGION_KR, "http://mobile-service.blizzard.com"},
            {REGION_CN, "http://mobile-service.battlenet.com.cn"}
        };
        private static string ENROLL_PATH = "/enrollment/enroll2.htm";
        private static string SYNC_PATH = "/enrollment/time.htm";
        private static string RESTORE_PATH = "/enrollment/initiatePaperRestore.htm";
        private static string RESTOREVALIDATE_PATH = "/enrollment/validatePaperRestore.htm";

        /// <summary>
        /// Set of ISO3166 EU countries
        /// </summary>
        private static List<string> EU_COUNTRIES = new List<string> {
            "AL", "AD", "AM", "AT", "AZ", "BY", "BE", "BA", "BG", "HR",
            "CY", "CZ", "DK", "EE", "FI", "FR", "GE", "DE", "GR", "HU",
            "IS", "IE", "IT", "KV", "XK", "LV", "LI", "LT", "LU", "MK",
            "MT", "MD", "MC", "ME", "NL", "NO", "PL", "PT", "RO", "RU",
            "SM", "RS", "SK", "ES", "SE", "CH", "TR", "UA", "UK", "GB",
            "VA"
        };

        /// <summary>
        /// Set of ISO3166 KR countries
        /// </summary>
        private static List<string> KR_COUNTRIES = new List<string> {
            "KR", "KP", "TW", "HK", "MO"
        };

        /// <summary>
        /// URL for GEO IP lookup to determine region
        /// </summary>
        private static string GEOIPURL = "http://geoiplookup.wikimedia.org";

        /// <summary>
        /// Time of last Sync error
        /// </summary>
        private static DateTime _lastSyncError = DateTime.MinValue;

        #region Authenticator data

        /// <summary>
        /// Region for authenticator taken from first 2 chars of serial
        /// </summary>
        public string Region
        {
            get
            {
                return (string.IsNullOrEmpty(Serial) == false ? Serial.Substring(0, 2) : string.Empty);
            }
        }

        public string Serial { get; set; }

        /// <summary>
        /// Get/set the combined secret data value
        /// </summary>
        public override string SecretData
        {
            get
            {
                // for Battle.net, this is the key + serial
                return base.SecretData + "|" + Authenticator.ByteArrayToString(Encoding.UTF8.GetBytes(Serial));
            }
            set
            {
                // for Battle.net, extract key + serial
                if (string.IsNullOrEmpty(value) == false)
                {
                    string[] parts = value.Split('|');
                    if (parts.Length <= 1)
                    {
                        // old WinAuth2 version with secretdata + serial
                        SecretKey = Authenticator.StringToByteArray(value.Substring(0, 40));
                        Serial = Encoding.UTF8.GetString(Authenticator.StringToByteArray(value.Substring(40)));
                    }
                    else if (parts.Length == 3) // alpha 3.0.6
                    {
                        // secret|script|serial
                        base.SecretData = value;
                        Serial = (parts.Length > 2 ? Encoding.UTF8.GetString(Authenticator.StringToByteArray(parts[2])) : null);
                    }
                    else
                    {
                        // secret|serial
                        base.SecretData = value;
                        Serial = (parts.Length > 1 ? Encoding.UTF8.GetString(Authenticator.StringToByteArray(parts[1])) : null);
                    }
                }
                else
                {
                    SecretKey = null;
                    Serial = null;
                }
            }
        }

        /// <summary>
        /// We can check if the restore code is valid and rememeber so don't have to do it again
        /// </summary>
        public bool RestoreCodeVerified { get; set; }

        /// <summary>
        /// Get the restore code for an authenticator used to recover a lost authenticator along with the serial number.
        /// </summary>
        /// <returns>restore code (10 chars)</returns>
        public string RestoreCode
        {
            get
            {
                return BuildRestoreCode();
            }
        }

        #endregion

        /// <summary>
        /// Create a new Authenticator object
        /// </summary>
        public BattleNetAuthenticator()
            : base(CODE_DIGITS)
        {
            Issuer = BATTLENET_ISSUER;
        }

        /// <summary>
        /// Enroll the authenticator with the server.
        /// </summary>
        public void Enroll()
        {
            // default to US
            string region = REGION_US;
            string country = REGION_US;

            // Battle.net does a GEO IP lookup anyway so there is no need to pass the region
            // however China has its own URL so we must still do our own GEO IP lookup to find the country
            HttpWebRequest georequest = (HttpWebRequest)WebRequest.Create(GEOIPURL);
            georequest.Method = "GET";
            georequest.ContentType = "application/json";
            georequest.Timeout = 10000;
            // get response
            string responseString = null;
            try
            {
                using (HttpWebResponse georesponse = (HttpWebResponse)georequest.GetResponse())
                {
                    // OK?
                    if (georesponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (Stream bs = georesponse.GetResponseStream())
                            {
                                byte[] temp = new byte[RESPONSE_BUFFER_SIZE];
                                int read;
                                while ((read = bs.Read(temp, 0, RESPONSE_BUFFER_SIZE)) != 0)
                                {
                                    ms.Write(temp, 0, read);
                                }
                                responseString = Encoding.UTF8.GetString(ms.ToArray());
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            if (string.IsNullOrEmpty(responseString) == false)
            {
                // not worth a full json parser, just regex it
                Match match = Regex.Match(responseString, ".*\"country\":\"([^\"]*)\".*", RegexOptions.IgnoreCase);
                if (match.Success == true)
                {
                    // match the correct region
                    country = match.Groups[1].Value.ToUpper();

                    if (EU_COUNTRIES.Contains(country) == true)
                    {
                        region = REGION_EU;
                    }
                    else if (KR_COUNTRIES.Contains(country) == true)
                    {
                        region = REGION_KR;
                    }
                    else if (country == REGION_CN)
                    {
                        region = REGION_CN;
                    }
                    else
                    {
                        region = REGION_US;
                    }
                }
            }

            // allow override of country for CN using US from app.config
            //System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();
            //try
            //{
            //	string configcountry = config.GetValue("BattleNetAuthenticator.Country", typeof(string)) as string;
            //	if (string.IsNullOrEmpty(configcountry) == false)
            //	{
            //		country = configcountry;
            //	}
            //}
            //catch (InvalidOperationException ) { }
            //try
            //{
            //	string configregion = config.GetValue("BattleNetAuthenticator.Region", typeof(string)) as string;
            //	if (string.IsNullOrEmpty(configregion) == false)
            //	{
            //		region = configregion;
            //	}
            //}
            //catch (InvalidOperationException ) {}

            // generate byte array of data:
            //  00 byte[20] one-time key used to decrypt data when returned;
            //  20 byte[2] country code, e.g. US, GB, FR, KR, etc
            //  22 byte[16] model string for this device;
            //	38 END
            byte[] data = new byte[38];
            byte[] oneTimePad = CreateOneTimePad(20);
            Array.Copy(oneTimePad, data, oneTimePad.Length);
            // add country
            byte[] countrydata = Encoding.UTF8.GetBytes(country);
            Array.Copy(countrydata, 0, data, 20, Math.Min(countrydata.Length, 2));
            // add model name
            byte[] model = Encoding.UTF8.GetBytes(GeneralRandomModel());
            Array.Copy(model, 0, data, 22, Math.Min(model.Length, 16));

            // encrypt the data with BMA public key
            RsaEngine rsa = new RsaEngine();
            rsa.Init(true, new RsaKeyParameters(false, new Org.BouncyCastle.Math.BigInteger(ENROLL_MODULUS, 16), new Org.BouncyCastle.Math.BigInteger(ENROLL_EXPONENT, 16)));
            byte[] encrypted = rsa.ProcessBlock(data, 0, data.Length);

            // call the enroll server
            byte[] responseData = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetMobileUrl(region) + ENROLL_PATH);
                request.Method = "POST";
                request.ContentType = "application/octet-stream";
                request.ContentLength = encrypted.Length;
                request.Timeout = 10000;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(encrypted, 0, encrypted.Length);
                requestStream.Close();
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // OK?
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new InvalidEnrollResponseException(string.Format("{0}: {1}", (int)response.StatusCode, response.StatusDescription));
                    }

                    // load back the buffer - should only be a byte[45]
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //using (BufferedStream bs = new BufferedStream(response.GetResponseStream()))
                        using (Stream bs = response.GetResponseStream())
                        {
                            byte[] temp = new byte[RESPONSE_BUFFER_SIZE];
                            int read;
                            while ((read = bs.Read(temp, 0, RESPONSE_BUFFER_SIZE)) != 0)
                            {
                                ms.Write(temp, 0, read);
                            }
                            responseData = ms.ToArray();

                            // check it is correct size
                            if (responseData.Length != ENROLL_RESPONSE_SIZE)
                            {
                                throw new InvalidEnrollResponseException(string.Format("Invalid response data size (expected 45 got {0})", responseData.Length));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidEnrollResponseException("Cannot contact Battle.net servers: " + ex.Message);
            }
            // return data:
            // 00-07 server time (Big Endian)
            // 08-24 serial number (17)
            // 25-44 secret key encrpyted with our pad
            // 45 END

            // extract the server time
            byte[] serverTime = new byte[8];
            Array.Copy(responseData, serverTime, 8);
            if (BitConverter.IsLittleEndian == true)
            {
                Array.Reverse(serverTime);
            }
            // get the difference between the server time and our current time
            ServerTimeDiff = BitConverter.ToInt64(serverTime, 0) - CurrentTime;

            // get the secret key
            byte[] secretKey = new byte[20];
            Array.Copy(responseData, 25, secretKey, 0, 20);
            // decrypt the initdata with a simple xor with our key
            for (int i = oneTimePad.Length - 1; i >= 0; i--)
            {
                secretKey[i] ^= oneTimePad[i];
            }
            SecretKey = secretKey;

            // get the serial number
            Serial = Encoding.Default.GetString(responseData, 8, 17);
        }

#if DEBUG
        /// <summary>
        /// Debug version of enroll that just returns a known test authenticator
        /// </summary>
        /// <param name="testmode"></param>
        public void Enroll(bool testmode)
        {
            if (!testmode)
            {
                Enroll();
            }
            else
            {
                ServerTimeDiff = 0;
                SecretKey = new byte[20] { 0x7B, 0x0B, 0xFA, 0x82, 0x30, 0xE5, 0x44, 0x24, 0xAB, 0x51, 0x77, 0x7D, 0xAD, 0xBF, 0xD5, 0x37, 0x41, 0x43, 0xE3, 0xB0 };
                Serial = "US-1306-2525-4376"; // Restore: CR24KPKF51
            }
        }
#endif

        /// <summary>
        /// Synchronise this authenticator's time with server time. We update our data record with the difference from our UTC time.
        /// </summary>
        public override void Sync()
        {
            // check if data is protected
            if (SecretKey == null && EncryptedData != null)
            {
                throw new EncryptedSecretDataException();
            }

            // don't retry for 5 minutes
            if (_lastSyncError >= DateTime.Now.AddMinutes(0 - SYNC_ERROR_MINUTES))
            {
                return;
            }

            try
            {
                // create a connection to time sync server
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetMobileUrl(Region) + SYNC_PATH);
                request.Method = "GET";
                request.Timeout = 5000;

                // get response
                byte[] responseData = null;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // OK?
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new ApplicationException(string.Format("{0}: {1}", (int)response.StatusCode, response.StatusDescription));
                    }

                    // load back the buffer - should only be a byte[8]
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // using (BufferedStream bs = new BufferedStream(response.GetResponseStream()))
                        using (Stream bs = response.GetResponseStream())
                        {
                            byte[] temp = new byte[RESPONSE_BUFFER_SIZE];
                            int read;
                            while ((read = bs.Read(temp, 0, RESPONSE_BUFFER_SIZE)) != 0)
                            {
                                ms.Write(temp, 0, read);
                            }
                            responseData = ms.ToArray();

                            // check it is correct size
                            if (responseData.Length != SYNC_RESPONSE_SIZE)
                            {
                                throw new InvalidSyncResponseException(string.Format("Invalid response data size (expected " + SYNC_RESPONSE_SIZE + " got {0}", responseData.Length));
                            }
                        }
                    }
                }

                // return data:
                // 00-07 server time (Big Endian)

                // extract the server time
                if (BitConverter.IsLittleEndian == true)
                {
                    Array.Reverse(responseData);
                }
                // get the difference between the server time and our current time
                long serverTimeDiff = BitConverter.ToInt64(responseData, 0) - CurrentTime;

                // update the Data object
                ServerTimeDiff = serverTimeDiff;
                LastServerTime = DateTime.Now.Ticks;

                // clear any sync error
                _lastSyncError = DateTime.MinValue;
            }
            catch (WebException)
            {
                // don't retry for a while after error
                _lastSyncError = DateTime.Now;

                // set to zero to force reset
                ServerTimeDiff = 0;
            }
        }

        /// <summary>
        /// Restore an authenticator from the serial number and restore code.
        /// </summary>
        /// <param name="serial">serial code, e.g. US-1234-5678-1234</param>
        /// <param name="restoreCode">restore code given on enroll, 10 chars.</param>
        public void Restore(string serial, string restoreCode)
        {
            // get the serial data
            byte[] serialBytes = Encoding.UTF8.GetBytes(serial.ToUpper().Replace("-", string.Empty));

            // send the request to the server to get our challenge
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetMobileUrl(serial) + RESTORE_PATH);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = serialBytes.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(serialBytes, 0, serialBytes.Length);
            requestStream.Close();
            byte[] challenge = null;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // OK?
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new InvalidRestoreResponseException(string.Format("{0}: {1}", (int)response.StatusCode, response.StatusDescription));
                    }

                    // load back the buffer - should only be a byte[32]
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (Stream bs = response.GetResponseStream())
                        {
                            byte[] temp = new byte[RESPONSE_BUFFER_SIZE];
                            int read;
                            while ((read = bs.Read(temp, 0, RESPONSE_BUFFER_SIZE)) != 0)
                            {
                                ms.Write(temp, 0, read);
                            }
                            challenge = ms.ToArray();

                            // check it is correct size
                            if (challenge.Length != RESTOREINIT_BUFFER_SIZE)
                            {
                                throw new InvalidRestoreResponseException(string.Format("Invalid response data size (expected 32 got {0})", challenge.Length));
                            }
                        }
                    }
                }
            }
            catch (WebException we)
            {
                int code = (int)((HttpWebResponse)we.Response).StatusCode;
                if (code >= 500 && code < 600)
                {
                    throw new InvalidRestoreResponseException(string.Format("No response from server ({0}). Perhaps maintainence?", code));
                }
                else
                {
                    throw new InvalidRestoreResponseException(string.Format("Error communicating with server: {0} - {1}", code, ((HttpWebResponse)we.Response).StatusDescription));
                }
            }

            // only take the first 10 bytes of the restore code and encode to byte taking count of the missing chars
            byte[] restoreCodeBytes = new byte[10];
            char[] arrayOfChar = restoreCode.ToUpper().ToCharArray();
            for (int i = 0; i < 10; i++)
            {
                restoreCodeBytes[i] = ConvertRestoreCodeCharToByte(arrayOfChar[i]);
            }

            // build the response to the challenge
            HMac hmac = new HMac(new Sha1Digest());
            hmac.Init(new KeyParameter(restoreCodeBytes));
            byte[] hashdata = new byte[serialBytes.Length + challenge.Length];
            Array.Copy(serialBytes, 0, hashdata, 0, serialBytes.Length);
            Array.Copy(challenge, 0, hashdata, serialBytes.Length, challenge.Length);
            hmac.BlockUpdate(hashdata, 0, hashdata.Length);
            byte[] hash = new byte[hmac.GetMacSize()];
            hmac.DoFinal(hash, 0);

            // create a random key
            byte[] oneTimePad = CreateOneTimePad(20);

            // concatanate the hash and key
            byte[] hashkey = new byte[hash.Length + oneTimePad.Length];
            Array.Copy(hash, 0, hashkey, 0, hash.Length);
            Array.Copy(oneTimePad, 0, hashkey, hash.Length, oneTimePad.Length);

            // encrypt the data with BMA public key
            RsaEngine rsa = new RsaEngine();
            rsa.Init(true, new RsaKeyParameters(false, new Org.BouncyCastle.Math.BigInteger(ENROLL_MODULUS, 16), new Org.BouncyCastle.Math.BigInteger(ENROLL_EXPONENT, 16)));
            byte[] encrypted = rsa.ProcessBlock(hashkey, 0, hashkey.Length);

            // prepend the serial to the encrypted data
            byte[] postbytes = new byte[serialBytes.Length + encrypted.Length];
            Array.Copy(serialBytes, 0, postbytes, 0, serialBytes.Length);
            Array.Copy(encrypted, 0, postbytes, serialBytes.Length, encrypted.Length);

            // send the challenge response back to the server
            request = (HttpWebRequest)WebRequest.Create(GetMobileUrl(serial) + RESTOREVALIDATE_PATH);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = postbytes.Length;
            requestStream = request.GetRequestStream();
            requestStream.Write(postbytes, 0, postbytes.Length);
            requestStream.Close();
            byte[] secretKey = null;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // OK?
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new InvalidRestoreResponseException(string.Format("{0}: {1}", (int)response.StatusCode, response.StatusDescription));
                    }

                    // load back the buffer - should only be a byte[32]
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (Stream bs = response.GetResponseStream())
                        {
                            byte[] temp = new byte[RESPONSE_BUFFER_SIZE];
                            int read;
                            while ((read = bs.Read(temp, 0, RESPONSE_BUFFER_SIZE)) != 0)
                            {
                                ms.Write(temp, 0, read);
                            }
                            secretKey = ms.ToArray();

                            // check it is correct size
                            if (secretKey.Length != RESTOREVALIDATE_BUFFER_SIZE)
                            {
                                throw new InvalidRestoreResponseException(string.Format("Invalid response data size (expected " + RESTOREVALIDATE_BUFFER_SIZE + " got {0})", secretKey.Length));
                            }
                        }
                    }
                }
            }
            catch (WebException we)
            {
                int code = (int)((HttpWebResponse)we.Response).StatusCode;
                if (code >= 500 && code < 600)
                {
                    throw new InvalidRestoreResponseException(string.Format("No response from server ({0}). Perhaps maintainence?", code));
                }
                else if (code >= 600 && code < 700)
                {
                    throw new InvalidRestoreCodeException("Invalid serial number or restore code.");
                }
                else
                {
                    throw new InvalidRestoreResponseException(string.Format("Error communicating with server: {0} - {1}", code, ((HttpWebResponse)we.Response).StatusDescription));
                }
            }

            // xor the returned data key with our pad to get the actual secret key
            for (int i = oneTimePad.Length - 1; i >= 0; i--)
            {
                secretKey[i] ^= oneTimePad[i];
            }

            // set the authenticator data
            SecretKey = secretKey;
            if (serial.Length == 14)
            {
                Serial = serial.Substring(0, 2).ToUpper() + "-" + serial.Substring(2, 4) + "-" + serial.Substring(6, 4) + "-" + serial.Substring(10, 4);
            }
            else
            {
                Serial = serial.ToUpper();
            }
            // restore code is ok
            RestoreCodeVerified = true;
            // sync the time
            ServerTimeDiff = 0L;
            Sync();
        }

        /// <summary>
        /// Read any extra tags from the Xml
        /// </summary>
        /// <param name="reader">XmlReader</param>
        /// <param name="name">name of tag</param>
        /// <returns>true if read and processed the tag</returns>
        public override bool ReadExtraXml(XmlReader reader, string name)
        {
            switch (name)
            {
                case "restorecodeverified":
                    RestoreCodeVerified = reader.ReadElementContentAsBoolean();
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Add extra tags into the XmlWriter
        /// </summary>
        /// <param name="writer">XmlWriter to write data</param>
        protected override void WriteExtraXml(XmlWriter writer)
        {
            if (RestoreCodeVerified == true)
            {
                writer.WriteStartElement("restorecodeverified");
                writer.WriteString(bool.TrueString.ToLower());
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Get the base mobil url based on the region
        /// </summary>
        /// <param name="region">two letter region code, i.e US or CN</param>
        /// <returns>string of Url for region</returns>
        private static string GetMobileUrl(string region)
        {
            string upperregion = region.ToUpper();
            if (upperregion.Length > 2)
            {
                upperregion = upperregion.Substring(0, 2);
            }
            if (MOBILE_URLS.ContainsKey(upperregion) == true)
            {
                return MOBILE_URLS[upperregion];
            }
            else
            {
                return MOBILE_URLS[REGION_US];
            }
        }

        /// <summary>
        /// Calculate the restore code for an authenticator. This is taken from the last 10 bytes of a digest of the serial and secret key,
        /// which is then specially encoded to alphanumerics.
        /// </summary>
        /// <returns>restore code for authenticator (always 10 chars)</returns>
        protected string BuildRestoreCode()
        {
            // return if not set
            if (string.IsNullOrEmpty(Serial) == true || SecretKey == null)
            {
                return string.Empty;
            }

            // get byte array of serial
            byte[] serialdata = Encoding.UTF8.GetBytes(Serial.ToUpper().Replace("-", string.Empty));
            byte[] secretdata = SecretKey;

            // combine serial data and secret data
            byte[] combined = new byte[serialdata.Length + secretdata.Length];
            Array.Copy(serialdata, 0, combined, 0, serialdata.Length);
            Array.Copy(secretdata, 0, combined, serialdata.Length, secretdata.Length);

            // create digest of combined data
            IDigest digest = new Sha1Digest();
            digest.BlockUpdate(combined, 0, combined.Length);
            byte[] digestdata = new byte[digest.GetDigestSize()];
            digest.DoFinal(digestdata, 0);

            // take last 10 chars of hash and convert each byte to our encoded string that doesn't use I,L,O,S
            StringBuilder code = new StringBuilder();
            int startpos = digestdata.Length - 10;
            for (int i = 0; i < 10; i++)
            {
                code.Append(ConvertRestoreCodeByteToChar(digestdata[startpos + i]));
            }

            return code.ToString();
        }

        /// <summary>
        /// Create a random Model string for initialization to armor the init string sent over the wire
        /// </summary>
        /// <returns>Random model string</returns>
        private static string GeneralRandomModel()
        {
            // seed a new RNG
            RNGCryptoServiceProvider randomSeedGenerator = new RNGCryptoServiceProvider();
            byte[] seedBuffer = new byte[4];
            randomSeedGenerator.GetBytes(seedBuffer);
            Random random = new Random(BitConverter.ToInt32(seedBuffer, 0));

            // create a model string with available characters
            StringBuilder model = new StringBuilder(MODEL_SIZE);
            for (int i = MODEL_SIZE; i > 0; i--)
            {
                model.Append(MODEL_CHARS[random.Next(MODEL_CHARS.Length)]);
            }

            return model.ToString();
        }

        #region Utility functions

        /// <summary>
        /// Convert a char to a byte but with appropriate mapping to exclude I,L,O and S. E.g. A=10 but J=18 not 19 (as I is missing)
        /// </summary>
        /// <param name="c">char to convert.</param>
        /// <returns>byte value of restore code char</returns>
        private static byte ConvertRestoreCodeCharToByte(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }
            else
            {
                byte index = (byte)(c + 10 - 65);
                if (c >= 'I')
                {
                    index--;
                }
                if (c >= 'L')
                {
                    index--;
                }
                if (c >= 'O')
                {
                    index--;
                }
                if (c >= 'S')
                {
                    index--;
                }

                return index;
            }
        }

        /// <summary>
        /// Convert a byte to a char but with appropriate mapping to exclude I,L,O and S.
        /// </summary>
        /// <param name="b">byte to convert.</param>
        /// <returns>char value of restore code value</returns>
        private static char ConvertRestoreCodeByteToChar(byte b)
        {
            int index = b & 0x1f;
            if (index <= 9)
            {
                return (char)(index + 48);
            }
            else
            {
                index = (index + 65) - 10;
                if (index >= 73)
                {
                    index++;
                }
                if (index >= 76)
                {
                    index++;
                }
                if (index >= 79)
                {
                    index++;
                }
                if (index >= 83)
                {
                    index++;
                }
                return (char)index;
            }
        }

        #endregion
    }
}