using Livet;
using SteamTools.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using WinAuth;
using SteamTool.Core.Common;
using System.ComponentModel;
using SteamTools.Models.Settings;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Linq;
using MetroTrilithon.Mvvm;
using SteamTool.Model;
using System.Threading.Tasks;

namespace SteamTools.Services
{
    public class AuthService : NotificationObject
    {
        /// <summary>
        /// Entry for a single SDA account
        /// </summary>
        class ImportedSDAEntry
        {
            public const int PBKDF2_ITERATIONS = 50000;
            public const int SALT_LENGTH = 8;
            public const int KEY_SIZE_BYTES = 32;
            public const int IV_LENGTH = 16;

            public string Username;
            public string SteamId;
            public string json;

            public override string ToString()
            {
                return Username + " (" + this.SteamId + ")";
            }
        }

        #region static members
        public static AuthService Current { get; } = new AuthService();

        #endregion

        //public AuthService()
        //{
        //    AuthService.Current.Subscribe(nameof(AuthService.Current.Authenticators),
        //        () => { AuthSettings.Authenticators.Value = ConvertJsonAuthenticator(AuthService.Current.Authenticators); });
        //}

        private BindingList<WinAuthAuthenticator> _Authenticators = new BindingList<WinAuthAuthenticator>();

        public BindingList<WinAuthAuthenticator> Authenticators
        {
            get => this._Authenticators;
            set
            {
                if (this._Authenticators != value)
                {
                    this._Authenticators = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public async void Initialize()
        {
            await Task.Run(() =>
            {
                StatusService.Current.Notify("正在初始化令牌数据...");
                var path = Path.Combine(AppContext.BaseDirectory, Const.AUTHDATA_FILE);
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path);
                    var auths = AuthService.LoadJsonAuthenticator(text.DecompressString());
                    Authenticators = new BindingList<WinAuthAuthenticator>(auths);
                    if (string.IsNullOrEmpty(AuthSettings.Authenticators.Value))
                    {
                        AuthSettings.Authenticators.Value = text;
                    }
                }
                else
                {
                    var auths = AuthService.LoadJsonAuthenticator(AuthSettings.Authenticators.Value.DecompressString());
                    Authenticators = new BindingList<WinAuthAuthenticator>(auths);
                    if (AuthSettings.IsCurrentDirectorySaveAuthData.Value)
                    {
                        File.WriteAllText(path, AuthSettings.Authenticators.Value);
                    }
                }
                StatusService.Current.Notify("加载令牌数据完成");
            }).ContinueWith(s => { Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow("令牌同步服务器失败，错误信息:" + s.Exception); }, TaskContinuationOptions.OnlyOnFaulted).ContinueWith(s => s.Dispose());
            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    Logger.Error(ex);
            //    WindowService.Current.ShowDialogWindow($"令牌同步服务器失败，错误信息：{ex}");
            //}
        }

        /// <summary>
        /// 导入Steam++导出的令牌数据文件
        /// </summary>
        public void ImportAuthenticatorsString(string str)
        {
            var auths = AuthService.LoadJsonAuthenticator(str);
            foreach (var auth in auths)
            {
                Authenticators.Add(auth);
            }
        }

        /// <summary>
        /// 导入Steam++导出的令牌数据文件
        /// </summary>
        public void ImportAuthenticators(string file)
        {
            var text = File.ReadAllText(file, Encoding.UTF8);
            var auths = AuthService.LoadJsonAuthenticator(text.DecompressString());
            foreach (var auth in auths)
            {
                Authenticators.Add(auth);
            }
        }

        /// <summary>
        /// Import a file containing authenticators in the KeyUriFormat. The file might be plain text, encrypted zip or encrypted pgp.
        /// </summary>
        /// <param name="parent">parent Form</param>
        /// <param name="file">file name to import</param>
        /// <returns>list of imported authenticators</returns>
        public void ImportWinAuthenticators(string file)
        {
            StringBuilder lines = new StringBuilder();
            bool retry;
            do
            {
                retry = false;
                lines.Length = 0;
                // read a plain text file
                lines.Append(File.ReadAllText(file));
            } while (retry);

            int linenumber = 0;
            try
            {
                using var sr = new StringReader(lines.ToString());
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    linenumber++;

                    // ignore blank lines or comments
                    line = line.Trim();
                    if (line.Length == 0 || line.IndexOf("#") == 0)
                    {
                        continue;
                    }

                    // bug if there is a hash before ?
                    var hash = line.IndexOf("#");
                    var qm = line.IndexOf("?");
                    if (hash != -1 && hash < qm)
                    {
                        line = $"{line.Substring(0, hash)}%23{line.Substring(hash + 1)}";
                    }

                    // parse and validate URI
                    var uri = new Uri(line);

                    // we only support "otpauth"
                    if (uri.Scheme != "otpauth")
                    {
                        throw new ApplicationException("Import only supports otpauth://");
                    }
                    // we only support totp (not hotp)
                    if (uri.Host != "totp" && uri.Host != "hotp")
                    {
                        throw new ApplicationException("Import only supports otpauth://totp/ or otpauth://hotp/");
                    }

                    // get the label and optional issuer
                    string issuer = string.Empty;
                    string label = (string.IsNullOrEmpty(uri.LocalPath) == false ? uri.LocalPath.Substring(1) : string.Empty); // skip past initial /
                    int p = label.IndexOf(":");
                    if (p != -1)
                    {
                        issuer = label.Substring(0, p);
                        label = label.Substring(p + 1);
                    }
                    // + aren't decoded
                    label = label.Replace("+", " ");

                    var query = HttpUtility.ParseQueryString(uri.Query);
                    string secret = query["secret"];
                    if (string.IsNullOrEmpty(secret) == true)
                    {
                        throw new ApplicationException("Authenticator does not contain secret");
                    }

                    string counter = query["counter"];
                    if (uri.Host == "hotp" && string.IsNullOrEmpty(counter) == true)
                    {
                        throw new ApplicationException("HOTP authenticator should have a counter");
                    }

                    WinAuthAuthenticator importedAuthenticator = new WinAuthAuthenticator
                    {
                        AutoRefresh = false,
                    };
                    //
                    Authenticator auth;
                    if (string.Compare(issuer, "BattleNet", true) == 0)
                    {
                        string serial = query["serial"];
                        if (string.IsNullOrEmpty(serial) == true)
                        {
                            throw new ApplicationException("Battle.net Authenticator does not have a serial");
                        }
                        serial = serial.ToUpper();
                        if (Regex.IsMatch(serial, @"^[A-Z]{2}-?[\d]{4}-?[\d]{4}-?[\d]{4}$") == false)
                        {
                            throw new ApplicationException("Invalid serial for Battle.net Authenticator");
                        }
                        auth = new BattleNetAuthenticator();
                        //char[] decoded = Base32.getInstance().Decode(secret).Select(c => Convert.ToChar(c)).ToArray(); // this is hex string values
                        //string hex = new string(decoded);
                        //((BattleNetAuthenticator)auth).SecretKey = Authenticator.StringToByteArray(hex);

                        ((BattleNetAuthenticator)auth).SecretKey = Base32.getInstance().Decode(secret);

                        ((BattleNetAuthenticator)auth).Serial = serial;

                        issuer = string.Empty;
                    }
                    else if (string.Compare(issuer, "Steam", true) == 0)
                    {
                        auth = new SteamAuthenticator();
                        ((SteamAuthenticator)auth).SecretKey = Base32.getInstance().Decode(secret);
                        ((SteamAuthenticator)auth).Serial = string.Empty;
                        ((SteamAuthenticator)auth).DeviceId = query["deviceid"] ?? string.Empty;
                        ((SteamAuthenticator)auth).SteamData = query["data"] ?? string.Empty;
                        issuer = string.Empty;
                    }
                    else if (uri.Host == "hotp")
                    {
                        auth = new HOTPAuthenticator();
                        ((HOTPAuthenticator)auth).SecretKey = Base32.getInstance().Decode(secret);
                        ((HOTPAuthenticator)auth).Counter = int.Parse(counter);

                        if (string.IsNullOrEmpty(issuer) == false)
                        {
                            auth.Issuer = issuer;
                        }
                    }
                    else // if (string.Compare(issuer, "Google", true) == 0)
                    {
                        auth = new GoogleAuthenticator();
                        ((GoogleAuthenticator)auth).Enroll(secret);

                        if (string.Compare(issuer, "Google", true) == 0)
                        {
                            issuer = string.Empty;
                        }
                        else if (string.IsNullOrEmpty(issuer) == false)
                        {
                            auth.Issuer = issuer;
                        }
                    }

                    int.TryParse(query["period"], out int period);
                    if (period != 0)
                    {
                        auth.Period = period;
                    }

                    int.TryParse(query["digits"], out int digits);
                    if (digits != 0)
                    {
                        auth.CodeDigits = digits;
                    }


                    if (Enum.TryParse<Authenticator.HMACTypes>(query["algorithm"], true, out Authenticator.HMACTypes hmactype) == true)
                    {
                        auth.HMACType = hmactype;
                    }

                    //
                    if (label.Length != 0)
                    {
                        importedAuthenticator.Name = (issuer.Length != 0 ? issuer + " (" + label + ")" : label);
                    }
                    else if (issuer.Length != 0)
                    {
                        importedAuthenticator.Name = issuer;
                    }
                    else
                    {
                        importedAuthenticator.Name = "Imported";
                    }
                    //
                    importedAuthenticator.AuthenticatorData = auth;

                    // sync
                    StatusService.Current.Set("正在同步令牌服务器时钟...");
                    importedAuthenticator.Sync();

                    Authenticators.Add(importedAuthenticator);
                }

            }
            catch (UriFormatException ex)
            {
                throw new UriFormatException(string.Format("Invalid authenticator at line {0}", linenumber), ex);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error importing at line {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Import an authenticator from the uuid and steamguard files
        /// </summary>
        /// <returns>true if successful</returns>
        public bool ImportSteamGuard(string name, string uuid, string steamGuard)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                //WinAuthForm.ErrorDialog(this, "Please enter the contents of the steam.uuid.xml file or your DeviceId");
                return false;
            }
            if (steamGuard.Length == 0)
            {
                //WinAuthForm.ErrorDialog(this, "Please enter the contents of your SteamGuard file");
                return false;
            }

            // check the deviceid
            string deviceId;
            if (uuid.IndexOf("?xml") != -1)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(uuid);
                    var node = doc.SelectSingleNode("//string[@name='uuidKey']");
                    if (node == null)
                    {
                        //WinAuthForm.ErrorDialog(this, "Cannot find uuidKey in xml");
                        return false;
                    }

                    deviceId = node.InnerText;
                }
                catch (Exception ex)
                {
                    //WinAuthForm.ErrorDialog(this, "Invalid uuid xml: " + ex.Message);
                    Logger.Error(ex);
                    return false;
                }
            }
            else
            {
                deviceId = uuid;
            }
            if (string.IsNullOrEmpty(deviceId) || Regex.IsMatch(deviceId, @"android:[0-9abcdef-]+", RegexOptions.Singleline | RegexOptions.IgnoreCase) == false)
            {
                //WinAuthForm.ErrorDialog(this, "Invalid deviceid, expecting \"android:NNNN...\"");
                return false;
            }

            // check the steamguard
            byte[] secret;
            string serial;
            try
            {
                var json = JObject.Parse(steamGuard);

                var node = json.SelectToken("shared_secret");
                if (node == null)
                {
                    throw new ApplicationException("no shared_secret");
                }
                secret = Convert.FromBase64String(node.Value<string>());

                node = json.SelectToken("serial_number");
                if (node == null)
                {
                    throw new ApplicationException("no serial_number");
                }
                serial = node.Value<string>();
            }
            catch (Exception ex)
            {
                //WinAuthForm.ErrorDialog(this, "Invalid SteamGuard JSON contents: " + ex.Message);
                Logger.Error(ex);
                return false;
            }

            SteamAuthenticator auth = new SteamAuthenticator
            {
                SecretKey = secret,
                Serial = serial,
                SteamData = steamGuard,
                DeviceId = deviceId
            };

            //this.Authenticator.AuthenticatorData = auth;
            WinAuthAuthenticator winAuth = new WinAuthAuthenticator
            {
                Name = name,
                AuthenticatorData = auth,
                AutoRefresh = false,
            };
            Authenticators.Add(winAuth);
            return true;
        }

        public bool ImportSDAFile(string mafile, string password = null, string steamid = null, string iv = null, string salt = null)
        {
            string data;
            if (File.Exists(mafile) == false || (data = File.ReadAllText(mafile)) == null)
            {
                throw new ApplicationException("Cannot read file " + mafile);
            }

            // decrypt
            if (string.IsNullOrEmpty(password) == false)
            {
                byte[] ciphertext = Convert.FromBase64String(data);
                using Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 50000);
                byte[] key = pbkdf2.GetBytes(32);

                using RijndaelManaged aes256 = new RijndaelManaged
                {
                    IV = Convert.FromBase64String(iv),
                    Key = key,
                    Padding = PaddingMode.PKCS7,
                    Mode = CipherMode.CBC
                };

                try
                {
                    using ICryptoTransform decryptor = aes256.CreateDecryptor(aes256.Key, aes256.IV);
                    using MemoryStream ms = new MemoryStream(ciphertext);
                    using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                    using StreamReader sr = new StreamReader(cs);
                    data = sr.ReadToEnd();
                }
                catch (CryptographicException)
                {
                    throw new ApplicationException("Invalid password");
                }
            }

            var token = JObject.Parse(data);
            var sdaentry = new ImportedSDAEntry
            {
                Username = token.SelectToken("account_name")?.Value<string>(),
                SteamId = steamid
            };
            if (string.IsNullOrEmpty(sdaentry.SteamId) == true)
            {
                sdaentry.SteamId = token.SelectToken("Session.SteamID")?.Value<string>();
            }
            if (string.IsNullOrEmpty(sdaentry.SteamId) == true)
            {
                sdaentry.SteamId = mafile.Split('.')[0];
            }
            sdaentry.json = data;

            //importSDAList.Items.Add(sdaentry);
            SteamAuthenticator auth = new SteamAuthenticator();
            WinAuthAuthenticator winAuth = new WinAuthAuthenticator();
            foreach (var prop in token.Root.Children().ToList())
            {
                var child = token.SelectToken(prop.Path);

                string lkey = prop.Path.ToLower();
                if (lkey == "fully_enrolled" || lkey == "session")
                {
                    prop.Remove();
                }
                else if (lkey == "device_id")
                {
                    auth.DeviceId = child.Value<string>();
                    prop.Remove();
                }
                else if (lkey == "serial_number")
                {
                    auth.Serial = child.Value<string>();
                }
                else if (lkey == "account_name")
                {
                    //if (this.nameField.Text.Length == 0)
                    //{
                    //    this.nameField.Text = "Steam (" + child.Value<string>() + ")";
                    //}
                    winAuth.Name = "Steam (" + child.Value<string>() + ")";
                }
                else if (lkey == "shared_secret")
                {
                    auth.SecretKey = Convert.FromBase64String(child.Value<string>());
                }
            }
            auth.SteamData = token.ToString(Newtonsoft.Json.Formatting.None);
            winAuth.AuthenticatorData = auth;
            winAuth.AutoRefresh = false;
            Authenticators.Add(winAuth);

            return true;
        }

        public static string ExportAuthenticators(IList<WinAuthAuthenticator> authenticators)
        {
            // create file in memory
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            List<WinAuthAuthenticator> unprotected = new List<WinAuthAuthenticator>();
            foreach (var auth in authenticators)
            {
                unprotected.Add(auth);
                string line = auth.ToUrl();
                sw.WriteLine(line);
            }

            // reprotect
            foreach (var auth in unprotected)
            {
                auth.AuthenticatorData.Protect();
            }

            // reset and write stream out to disk or as zip
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            var result = Encoding.UTF8.GetString(ms.ToArray());
            return result;
        }

        public static string ConvertJsonAuthenticator(IList<WinAuthAuthenticator> authenticators)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb);
            writer.WriteStartDocument();
            writer.WriteStartElement("Auth");
            foreach (var auth in authenticators)
            {
                auth.WriteXmlString(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
            return sb.ToString();
        }

        public static IList<WinAuthAuthenticator> LoadJsonAuthenticator(string authString)
        {
            var list = new List<WinAuthAuthenticator>();
            if (!string.IsNullOrEmpty(authString))
            {
                XmlReader reader = XmlReader.Create(new StringReader(authString));
                reader.Read();
                while (reader.EOF == false && reader.IsEmptyElement == true)
                {
                    reader.Read();
                }
                reader.MoveToContent();
                while (reader.EOF == false)
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "Auth")
                        {
                            reader.Read();
                        }
                        if (reader.Name == "WinAuthAuthenticator")
                        {
                            var wa = new WinAuthAuthenticator() { AutoRefresh = false };
                            wa.ReadXml(reader, null);
                            list.Add(wa);
                        }
                    }
                    else
                    {
                        reader.Read();
                        break;
                    }
                }
            }
            return list;
        }

        public void SaveCurrentAuth()
        {
            AuthSettings.Authenticators.Value = ConvertJsonAuthenticator(Authenticators).CompressString();
        }
    }
}
