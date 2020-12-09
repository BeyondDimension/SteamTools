using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using WinAuth;

namespace SteamTool.Auth
{
    public static class AuthService
    {
        /// <summary>
        /// Import a file containing authenticators in the KeyUriFormat. The file might be plain text, encrypted zip or encrypted pgp.
        /// </summary>
        /// <param name="parent">parent Form</param>
        /// <param name="file">file name to import</param>
        /// <returns>list of imported authenticators</returns>
        public static List<WinAuthAuthenticator> ImportAuthenticators(string file)
        {
            List<WinAuthAuthenticator> authenticators = new List<WinAuthAuthenticator>();

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
                using (var sr = new StringReader(lines.ToString()))
                {
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
                            line = line.Substring(0, hash) + "%23" + line.Substring(hash + 1);
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
                            AutoRefresh = false
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

                        int period = 0;
                        int.TryParse(query["period"], out period);
                        if (period != 0)
                        {
                            auth.Period = period;
                        }

                        int digits = 0;
                        int.TryParse(query["digits"], out digits);
                        if (digits != 0)
                        {
                            auth.CodeDigits = digits;
                        }

                        Authenticator.HMACTypes hmactype;

                        if (Enum.TryParse<Authenticator.HMACTypes>(query["algorithm"], true, out hmactype) == true)
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
                        importedAuthenticator.Sync();

                        authenticators.Add(importedAuthenticator);
                    }
                }

                return authenticators;
            }
            catch (UriFormatException ex)
            {
                throw new ImportException(string.Format("Invalid authenticator at line {0}", linenumber), ex);
            }
            catch (Exception ex)
            {
                throw new ImportException(string.Format("Error importing at line {0}:{1}", ex.Message), ex);
            }
        }

        public static string ExportAuthenticators(IList<WinAuthAuthenticator> authenticators, string password)
        {
            // create file in memory
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
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
            }
        }

    }
}
