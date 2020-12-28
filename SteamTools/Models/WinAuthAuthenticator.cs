using Livet;
using SteamTools.Win32;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Xml;
using WinAuth;

namespace SteamTools.Models
{

    /// <summary>
    /// Wrapper for real authenticator data used to save to file with other application information
    /// </summary>
    public class WinAuthAuthenticator : NotificationObject, ICloneable
    {
        /// <summary>
        /// Unique Id of authenticator saved in config
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Index for authenticator when in sorted list
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Actual authenticator data
        /// </summary>
        public Authenticator AuthenticatorData { get; set; }

        /// <summary>
        /// When this authenticator was created
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// When the item was last updated
        /// </summary>
        public DateTime LastUpdate { get; set; }

        private string _name;
        private bool _autoRefresh;
        private bool _allowCopy;
        private bool _copyOnCode;
        private bool _hideSerial;
        private int _progressBarValue;

        /// <summary>
        /// Create the authenticator wrapper
        /// </summary>
        public WinAuthAuthenticator()
        {
            Id = Guid.NewGuid();
            Created = DateTime.Now;
            _autoRefresh = true;
        }

        /// <summary>
        /// Clone this authenticator
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            WinAuthAuthenticator clone = this.MemberwiseClone() as WinAuthAuthenticator;

            clone.Id = Guid.NewGuid();
            clone.AuthenticatorData = (this.AuthenticatorData != null ? this.AuthenticatorData.Clone() as Authenticator : null);

            return clone;
        }

        public int ProgressBarValue
        {
            get
            {
                return _progressBarValue;
            }
            set
            {
                _progressBarValue = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get/set the name of this authenticator
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get/set auto refresh flag
        /// </summary>
        public bool AutoRefresh
        {
            get
            {
                if (this.AuthenticatorData != null && this.AuthenticatorData is HOTPAuthenticator)
                {
                    return false;
                }
                else
                {
                    return _autoRefresh;
                }
            }
            set
            {
                // HTOP must always be false
                if (this.AuthenticatorData != null && this.AuthenticatorData is HOTPAuthenticator)
                {
                    _autoRefresh = false;
                }
                else
                {
                    _autoRefresh = value;
                }
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get/set allow copy flag
        /// </summary>
        public bool AllowCopy
        {
            get
            {
                return _allowCopy;
            }
            set
            {
                _allowCopy = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get/set auto copy flag
        /// </summary>
        public bool CopyOnCode
        {
            get
            {
                return _copyOnCode;
            }
            set
            {
                _copyOnCode = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Get/set hide serial flag
        /// </summary>
        public bool HideSerial
        {
            get
            {
                return _hideSerial;
            }
            set
            {
                _hideSerial = value;
                this.RaisePropertyChanged();
            }
        }

        public string CurrentCode
        {
            get
            {
                if (this.AuthenticatorData == null)
                {
                    return null;
                }

                string code = this.AuthenticatorData.CurrentCode;
                return code;
            }
            set
            {
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Sync the current authenticator's time with its server
        /// </summary>
        public void Sync()
        {
            if (AuthenticatorData != null)
            {
                try
                {
                    AuthenticatorData.Sync();
                }
                catch (EncryptedSecretDataException)
                {
                    // reset lastsync to force sync on next decryption
                }
            }
        }

        /// <summary>
        /// Copy the current code to the clipboard
        /// </summary>
        public void CopyCodeToClipboard(string code = null, bool showError = false)
        {
            if (code == null)
            {
                code = this.CurrentCode;
            }

            bool clipRetry = false;
            do
            {
                bool failed = false;
                // check if the clipboard is locked
                IntPtr hWnd = User32Window.GetOpenClipboardWindow();
                if (hWnd != IntPtr.Zero)
                {
                    int len = User32Window.GetWindowTextLength(hWnd);
                    if (len == 0)
                    {
                        //WinAuthMain.LogException(new ApplicationException("Clipboard in use by another process"));
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder(len + 1);
                        User32Window.GetWindowText(hWnd, sb, sb.Capacity);
                        //WinAuthMain.LogException(new ApplicationException("Clipboard in use by '" + sb.ToString() + "'"));
                    }

                    failed = true;
                }
                else
                {
                    // Issue#170: can still get error copying even though it works, so just increase retries and ignore error
                    try
                    {
                        Clipboard.Clear();

                        // add delay for clip error
                        System.Threading.Thread.Sleep(100);
                        Clipboard.SetDataObject(code, true);
                    }
                    catch (ExternalException)
                    {
                    }
                }

                if (failed == true && showError == true)
                {
                    // only show an error the first time
                    //clipRetry = (MessageBox.Show(form, strings.ClipboardInUse,
                    //    WinAuthMain.APPLICATION_NAME,
                    //    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes);

                }
            }
            while (clipRetry == true);
        }

        public bool ReadXml(XmlReader reader, string password)
        {
            bool changed = false;

            if (Guid.TryParse(reader.GetAttribute("id"), out Guid id) == true)
            {
                Id = id;
            }

            string authenticatorType = reader.GetAttribute("type");
            if (string.IsNullOrEmpty(authenticatorType) == false)
            {
                Type type = typeof(Authenticator).Assembly.GetType(authenticatorType, false, true);
                this.AuthenticatorData = Activator.CreateInstance(type) as Authenticator;
            }

            //string encrypted = reader.GetAttribute("encrypted");
            //if (string.IsNullOrEmpty(encrypted) == false)
            //{
            //	// read the encrypted text from the node
            //	string data = reader.ReadElementContentAsString();
            //	// decrypt
            //	Authenticator.PasswordTypes passwordType;
            //	data = Authenticator.DecryptSequence(data, encrypted, password, out passwordType);

            //	using (MemoryStream ms = new MemoryStream(Authenticator.StringToByteArray(data)))
            //	{
            //		reader = XmlReader.Create(ms);
            //		ReadXml(reader, password);
            //	}
            //	this.PasswordType = passwordType;
            //	this.Password = password;

            //	return;
            //}

            reader.MoveToContent();

            if (reader.IsEmptyElement)
            {
                reader.Read();
                return changed;
            }

            reader.Read();
            while (reader.EOF == false)
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "name":
                            Name = reader.ReadElementContentAsString();
                            break;

                        case "created":
                            long t = reader.ReadElementContentAsLong();
                            t += Convert.ToInt64(new TimeSpan(new DateTime(1970, 1, 1).Ticks).TotalMilliseconds);
                            t *= TimeSpan.TicksPerMillisecond;
                            Created = new DateTime(t).ToLocalTime();
                            break;

                        case "autorefresh":
                            //_autoRefresh = reader.ReadElementContentAsBoolean();
                            _autoRefresh = false;
                            break;

                        case "allowcopy":
                            _allowCopy = reader.ReadElementContentAsBoolean();
                            break;

                        case "copyoncode":
                            _copyOnCode = reader.ReadElementContentAsBoolean();
                            break;

                        case "hideserial":
                            _hideSerial = reader.ReadElementContentAsBoolean();
                            break;

                        case "authenticatordata":
                            try
                            {
                                // we don't pass the password as they are locked till clicked
                                changed = this.AuthenticatorData.ReadXml(reader) || changed;
                            }
                            catch (EncryptedSecretDataException)
                            {
                                // no action needed
                            }
                            catch (BadPasswordException)
                            {
                                // no action needed
                            }
                            break;

                        // v2
                        case "authenticator":
                            this.AuthenticatorData = Authenticator.ReadXmlv2(reader, password);
                            break;
                        // v2
                        case "servertimediff":
                            this.AuthenticatorData.ServerTimeDiff = reader.ReadElementContentAsLong();
                            break;


                        default:
                            reader.Skip();
                            break;
                    }
                }
                else
                {
                    reader.Read();
                    break;
                }
            }

            return changed;
        }

        /// <summary>
        /// Write the data as xml into an XmlWriter
        /// </summary>
        /// <param name="writer">XmlWriter to write config</param>
        public void WriteXmlString(XmlWriter writer)
        {
            writer.WriteStartElement(typeof(WinAuthAuthenticator).Name);
            writer.WriteAttributeString("id", this.Id.ToString());
            if (this.AuthenticatorData != null)
            {
                writer.WriteAttributeString("type", this.AuthenticatorData.GetType().FullName);
            }

            //if (this.PasswordType != Authenticator.PasswordTypes.None)
            //{
            //	string data;

            //	using (MemoryStream ms = new MemoryStream())
            //	{
            //		XmlWriterSettings settings = new XmlWriterSettings();
            //		settings.Indent = true;
            //		settings.Encoding = Encoding.UTF8;
            //		using (XmlWriter encryptedwriter = XmlWriter.Create(ms, settings))
            //		{
            //			Authenticator.PasswordTypes savedpasswordType = PasswordType;
            //			PasswordType = Authenticator.PasswordTypes.None;
            //			WriteXmlString(encryptedwriter);
            //			PasswordType = savedpasswordType;
            //		}
            //		//data = Encoding.UTF8.GetString(ms.ToArray());
            //		data = Authenticator.ByteArrayToString(ms.ToArray());
            //	}

            //	string encryptedTypes;
            //	data = Authenticator.EncryptSequence(data, PasswordType, Password, out encryptedTypes);
            //	writer.WriteAttributeString("encrypted", encryptedTypes);
            //	writer.WriteString(data);
            //	writer.WriteEndElement();

            //	return;
            //}

            writer.WriteStartElement("name");
            writer.WriteValue(this.Name ?? string.Empty);
            writer.WriteEndElement();

            writer.WriteStartElement("created");
            writer.WriteValue(Convert.ToInt64((this.Created.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds));
            writer.WriteEndElement();

            //writer.WriteStartElement("autorefresh");
            //writer.WriteValue(this.AutoRefresh);
            //writer.WriteEndElement();
            //
            writer.WriteStartElement("allowcopy");
            writer.WriteValue(this.AllowCopy);
            writer.WriteEndElement();
            //
            writer.WriteStartElement("copyoncode");
            writer.WriteValue(this.CopyOnCode);
            writer.WriteEndElement();
            //
            writer.WriteStartElement("hideserial");
            writer.WriteValue(this.HideSerial);
            writer.WriteEndElement();
            //

            // save the authenticator to the config file
            if (this.AuthenticatorData != null)
            {
                this.AuthenticatorData.WriteToWriter(writer);

                // save script with password and generated salt
                //if (this.AutoLogin != null)
                //{
                //	this.AutoLogin.WriteXmlString(writer, this.AuthenticatorData.PasswordType, this.AuthenticatorData.Password);
                //}
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Create a KeyUriFormat compatible URL
        /// See https://code.google.com/p/google-authenticator/wiki/KeyUriFormat
        /// </summary>
        /// <returns>string</returns>
        public virtual string ToUrl(bool compat = false)
        {
            string type = "totp";
            string extraparams = string.Empty;

            Match match;
            string issuer = this.AuthenticatorData.Issuer;
            string label = this.Name;
            if (string.IsNullOrEmpty(issuer) == true && (match = Regex.Match(label, @"^([^\(]+)\s+\((.*?)\)(.*)")).Success == true)
            {
                issuer = match.Groups[1].Value;
                label = match.Groups[2].Value + match.Groups[3].Value;
            }
            if (string.IsNullOrEmpty(issuer) == false && (match = Regex.Match(label, @"^" + issuer + @"\s+\((.*?)\)(.*)")).Success == true)
            {
                label = match.Groups[1].Value + match.Groups[2].Value;
            }
            if (string.IsNullOrEmpty(issuer) == false)
            {
                extraparams += "&issuer=" + HttpUtility.UrlEncode(issuer);
            }

            if (this.AuthenticatorData.HMACType != Authenticator.DEFAULT_HMAC_TYPE)
            {
                extraparams += "&algorithm=" + this.AuthenticatorData.HMACType.ToString();
            }

            if (this.AuthenticatorData is BattleNetAuthenticator)
            {
                extraparams += "&serial=" + HttpUtility.UrlEncode(((BattleNetAuthenticator)this.AuthenticatorData).Serial.Replace("-", ""));
            }
            else if (this.AuthenticatorData is SteamAuthenticator)
            {
                if (compat == false)
                {
                    extraparams += "&deviceid=" + HttpUtility.UrlEncode(((SteamAuthenticator)this.AuthenticatorData).DeviceId);
                    extraparams += "&data=" + HttpUtility.UrlEncode(((SteamAuthenticator)this.AuthenticatorData).SteamData);
                }
            }
            else if (this.AuthenticatorData is HOTPAuthenticator)
            {
                type = "hotp";
                extraparams += "&counter=" + ((HOTPAuthenticator)this.AuthenticatorData).Counter;
            }

            string secret = HttpUtility.UrlEncode(Base32.getInstance().Encode(this.AuthenticatorData.SecretKey));

            if (this.AuthenticatorData.Period != Authenticator.DEFAULT_PERIOD)
            {
                extraparams += "&period=" + this.AuthenticatorData.Period;
            }

            var url = string.Format("otpauth://" + type + "/{0}?secret={1}&digits={2}{3}",
              (string.IsNullOrEmpty(issuer) == false ? HttpUtility.UrlPathEncode(issuer) + ":" + HttpUtility.UrlPathEncode(label) : HttpUtility.UrlPathEncode(label)),
              secret,
              this.AuthenticatorData.CodeDigits,
              extraparams);

            return url;
        }

    }

    public class JsonAuthenticator
    {
        /// <summary>
        /// Unique Id of authenticator saved in config
        /// </summary>
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public string Issuer { get; set; }

        /// <summary>
        /// Time difference in milliseconds of our machine and server
        /// </summary>
        public long ServerTimeDiff { get; set; }

        /// <summary>
        /// Time of last synced
        /// </summary>
        public long LastServerTime { get; set; }

        /// <summary>
        /// Get/set the combined secret data value
        /// </summary>
        public string SecretData { get; set; }


        public static WinAuthAuthenticator ConvertWinAuthAuthenticator(JsonAuthenticator auth)
        {
            return new WinAuthAuthenticator
            {
                Id = auth.Id,
                Name = auth.Name,
                Created = auth.Created,
            };
        }

        public static JsonAuthenticator ConvertJsonAuthenticator(WinAuthAuthenticator auth)
        {
            return new JsonAuthenticator();
        }
    }
}
