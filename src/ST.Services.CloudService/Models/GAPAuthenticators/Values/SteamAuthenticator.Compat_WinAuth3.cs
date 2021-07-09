/*
* Copyright (C) 2015 Colin Mackie.
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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using WinAuth;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using N_JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using S_JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace System.Application.Models
{
    partial class GAPAuthenticatorValueDTO
    {
        partial class SteamAuthenticator
        {
            /// <summary>
            /// Number of minutes to ignore syncing if network error
            /// </summary>
            const int SYNC_ERROR_MINUTES = 60;

            /// <summary>
            /// Number of attempts to activate
            /// </summary>
            const int ENROLL_ACTIVATE_RETRIES = 30;

            /// <summary>
            /// Incorrect activation code
            /// </summary>
            const int INVALID_ACTIVATION_CODE = 89;

            /// <summary>
            /// Time for http request when calling Sync in ms
            /// </summary>
            const int SYNC_TIMEOUT = 30000;

            /// <summary>
            /// URLs for all mobile services
            /// </summary>
            static readonly string COMMUNITY_BASE = "https://steamcommunity.com";
            static readonly string WEBAPI_BASE = "https://api.steampowered.com";
            static readonly string SYNC_URL = "https://api.steampowered.com:443/ITwoFactorService/QueryTime/v0001";

            /// <summary>
            /// Character set for authenticator code
            /// </summary>
            static readonly char[] STEAMCHARS = new char[] {
                '2', '3', '4', '5', '6', '7', '8', '9', 'B', 'C',
                'D', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q',
                'R', 'T', 'V', 'W', 'X', 'Y'};

            /// <summary>
            /// Enrolling state
            /// </summary>
            public class EnrollState
            {
                public string? Language { get; set; }

                public string? Username { get; set; }
                public string? Password { get; set; }
                public string? CaptchaId { get; set; }
                public string? CaptchaUrl { get; set; }
                public string? CaptchaText { get; set; }
                public string? EmailDomain { get; set; }
                public string? EmailAuthText { get; set; }
                public string? ActivationCode { get; set; }
                public CookieContainer? Cookies { get; set; }

                public string? SteamId { get; set; }
                public string? OAuthToken { get; set; }

                public bool RequiresLogin { get; set; }
                public bool RequiresCaptcha { get; set; }
                public bool Requires2FA { get; set; }
                public bool RequiresEmailAuth { get; set; }
                public bool RequiresActivation { get; set; }

                public string? RevocationCode { get; set; }
                public string? SecretKey { get; set; }
                public bool Success { get; set; }

                public string? Error { get; set; }
            }

            #region Authenticator data

            /// <summary>
            /// Time of last Sync error
            /// </summary>
            static DateTime _lastSyncError = DateTime.MinValue;

            /// <summary>
            /// Current Steam client instance
            /// </summary>
            [MPIgnore, N_JsonIgnore, S_JsonIgnore]
            public WinAuthSteamClient? Client { get; protected set; }

            #endregion

            /// <summary>
            /// Expanding offsets to retry when creating first code
            /// </summary>
            readonly int[] ENROLL_OFFSETS = new int[] { 0, -30, 30, -60, 60, -90, 90, -120, 120 };

            /// <summary>
            /// Get/set the combined secret data value
            /// </summary>
            [MPIgnore, N_JsonIgnore, S_JsonIgnore]
            public override string? SecretData
            {
                get
                {
                    if (Client != null && Client.Session != null)
                    {
                        SessionData = Client.Session.ToString();
                    }

                    //if (Logger != null)
                    //{
                    //	Logger.Debug("Get Steam data: {0}, Session:{1}", (SteamData ?? string.Empty).Replace("\n"," ").Replace("\r",""), (SessionData ?? string.Empty).Replace("\n", " ").Replace("\r", ""));
                    //}

                    // this is the key |  serial | deviceid
                    return base.SecretData
                        + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(Serial))
                        + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(DeviceId))
                        + "|" + ByteArrayToString(Encoding.UTF8.GetBytes(SteamData))
                        + "|" + (string.IsNullOrEmpty(SessionData) == false ? ByteArrayToString(Encoding.UTF8.GetBytes(SessionData)) : string.Empty);
                }
                set
                {
                    // extract key + serial + deviceid
                    if (string.IsNullOrEmpty(value) == false)
                    {
                        string[] parts = value.Split('|');
                        base.SecretData = value;
                        Serial = (parts.Length > 1 ? Encoding.UTF8.GetString(StringToByteArray(parts[1])) : null);
                        DeviceId = (parts.Length > 2 ? Encoding.UTF8.GetString(StringToByteArray(parts[2])) : null);
                        SteamData = (parts.Length > 3 ? Encoding.UTF8.GetString(StringToByteArray(parts[3])) : string.Empty);

                        if (string.IsNullOrEmpty(SteamData) == false && SteamData[0] != '{')
                        {
                            // convert old recovation code into SteamData json
                            SteamData = "{\"revocation_code\":\"" + SteamData + "\"}";
                        }
                        string session = (parts.Length > 4 ? Encoding.UTF8.GetString(StringToByteArray(parts[4])) : null);

                        //if (Logger != null)
                        //{
                        //	Logger.Debug("Set Steam data: {0}, Session:{1}", (SteamData ?? string.Empty).Replace("\n", " ").Replace("\r", ""), (SessionData ?? string.Empty).Replace("\n", " ").Replace("\r", ""));
                        //}

                        if (string.IsNullOrEmpty(session) == false)
                        {
                            SessionData = session;
                        }
                    }
                    else
                    {
                        SecretKey = null;
                        Serial = null;
                        DeviceId = null;
                        SteamData = null;
                        SessionData = null;
                    }
                }
            }

            /// <summary>
            /// Get (or create) the current Steam client for this Authenticator
            /// </summary>
            /// <returns>current or new SteamClient</returns>
            public WinAuthSteamClient GetClient()
            {
                lock (this)
                {
                    if (Client == null)
                    {
                        Client = new WinAuthSteamClient(this, SessionData);
                    }

                    return Client;
                }
            }

            /// <summary>
            /// Perform a request to the Steam WebAPI service
            /// </summary>
            /// <param name="url">API url</param>
            /// <param name="method">GET or POST</param>
            /// <param name="data">Name-data pairs</param>
            /// <param name="cookies">current cookie container</param>
            /// <returns>response body</returns>
            static string Request(string url, string method, NameValueCollection? data = null, CookieContainer? cookies = null, NameValueCollection? headers = null, int timeout = 0)
            {
                // create form-encoded data for query or body
                string query = (data == null ? string.Empty : string.Join("&", Array.ConvertAll(data.AllKeys, key => String.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key])))));
                if (string.Compare(method, "GET", true) == 0)
                {
                    url += (url.IndexOf("?") == -1 ? "?" : "&") + query;
                }

                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) =>
                {
                    return true;
                });
                // call the server
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                var request = WebRequest.CreateHttp(url);
                request.Method = method;
                request.Accept = "text/javascript, text/html, application/xml, text/xml, */*";
                request.ServicePoint.Expect100Continue = false;
                request.UserAgent = "Mozilla/5.0 (Linux; U; Android 4.1.1; en-us; Google Nexus 4 - 4.1.1 - API 16 - 768x1280 Build/JRO03S) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                request.Referer = COMMUNITY_BASE; // + "/mobilelogin?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client";
                if (headers != null)
                {
                    request.Headers.Add(headers);
                }
                if (cookies != null)
                {
                    request.CookieContainer = cookies;
                }
                if (timeout != 0)
                {
                    request.Timeout = timeout;
                }

                if (string.Compare(method, "POST", true) == 0)
                {
                    request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    request.ContentLength = query.Length;

                    StreamWriter requestStream = new StreamWriter(request.GetRequestStream());
                    requestStream.Write(query);
                    requestStream.Close();
                }

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        LogRequest(method, url, cookies, data, response.StatusCode.ToString() + " " + response.StatusDescription);

                        // OK?
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new WinAuthInvalidRequestException(string.Format("{0}: {1}", (int)response.StatusCode, response.StatusDescription));
                        }

                        // load the response
                        using (StreamReader responseStream = new StreamReader(response.GetResponseStream()))
                        {
                            string responseData = responseStream.ReadToEnd();
                            LogRequest(method, url, cookies, data, responseData);
                            return responseData;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(method, url, cookies, data, ex);

                    if (ex is WebException exception && exception.Response != null && ((HttpWebResponse)exception.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new WinAuthUnauthorisedRequestException(ex);
                    }

                    throw new WinAuthInvalidRequestException(ex.Message, ex);
                }
            }

            /// <summary>
            /// Enroll the authenticator with the server
            /// </summary>
            public bool Enroll(EnrollState state)
            {
                // clear error
                state.Error = null;

                try
                {
                    var data = new NameValueCollection();
                    var cookies = state.Cookies ??= new CookieContainer();
                    string response;

                    if (string.IsNullOrEmpty(state.OAuthToken) == true)
                    {
                        // get session
                        if (cookies.Count == 0)
                        {
                            cookies.Add(new Uri(COMMUNITY_BASE + "/"), new Cookie("mobileClientVersion", "3067969+%282.1.3%29"));
                            cookies.Add(new Uri(COMMUNITY_BASE + "/"), new Cookie("mobileClient", "android"));
                            cookies.Add(new Uri(COMMUNITY_BASE + "/"), new Cookie("steamid", ""));
                            cookies.Add(new Uri(COMMUNITY_BASE + "/"), new Cookie("steamLogin", ""));
                            cookies.Add(new Uri(COMMUNITY_BASE + "/"), new Cookie("Steam_Language", state.Language));
                            cookies.Add(new Uri(COMMUNITY_BASE + "/"), new Cookie("dob", ""));

                            NameValueCollection headers = new NameValueCollection();
                            headers.Add("X-Requested-With", "com.valvesoftware.android.steam.community");

                            response = Request("https://steamcommunity.com/login?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client", "GET", null, cookies, headers);
                        }

                        // Steam strips any non-ascii chars from username and password
                        state.Username = Regex.Replace(state.Username, @"[^\u0000-\u007F]", string.Empty);
                        state.Password = Regex.Replace(state.Password, @"[^\u0000-\u007F]", string.Empty);

                        // get the user's RSA key
                        data.Add("donotache", new DateTime().ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString());
                        data.Add("username", state.Username);
                        response = Request(COMMUNITY_BASE + "/login/getrsakey", "POST", data, cookies);
                        var rsaresponse = JObject.Parse(response);
                        if (rsaresponse.SelectToken("success").Value<bool>() != true)
                        {
                            throw new WinAuthInvalidEnrollResponseException("Cannot get steam information for user: " + state.Username);
                        }

                        // encrypt password with RSA key
                        RNGCryptoServiceProvider random = new();
                        byte[] encryptedPassword;
                        using (var rsa = new RSACryptoServiceProvider())
                        {
                            var passwordBytes = Encoding.ASCII.GetBytes(state.Password);
                            var p = rsa.ExportParameters(false);
                            p.Exponent = StringToByteArray(rsaresponse.SelectToken("publickey_exp").Value<string>());
                            p.Modulus = StringToByteArray(rsaresponse.SelectToken("publickey_mod").Value<string>());
                            rsa.ImportParameters(p);
                            encryptedPassword = rsa.Encrypt(passwordBytes, false);
                        }

                        // login request
                        data = new NameValueCollection
                        {
                            { "password", Convert.ToBase64String(encryptedPassword) },
                            { "username", state.Username },
                            { "twofactorcode", "" },
                            { "emailauth", (state.EmailAuthText != null ? state.EmailAuthText : string.Empty) },
                            { "loginfriendlyname", "" },
                            { "captchagid", (state.CaptchaId != null ? state.CaptchaId : "-1") },
                            { "captcha_text", (state.CaptchaText != null ? state.CaptchaText : "") },
                            { "emailsteamid", (state.EmailAuthText != null ? state.SteamId ?? string.Empty : string.Empty) },
                            { "rsatimestamp", rsaresponse.SelectToken("timestamp").Value<string>() },
                            { "remember_login", "false" },
                            { "oauth_client_id", "DE45CD61" },
                            { "oauth_scope", "read_profile write_profile read_client write_client" },
                            { "donotache", new DateTime().ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString() }
                        };
                        response = Request(COMMUNITY_BASE + "/login/dologin/", "POST", data, cookies);
                        Dictionary<string, object> loginresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                        if (loginresponse.ContainsKey("emailsteamid") == true)
                        {
                            state.SteamId = loginresponse["emailsteamid"] as string;
                        }

                        // require captcha
                        if (loginresponse.ContainsKey("captcha_needed") == true && (bool)loginresponse["captcha_needed"] == true)
                        {
                            state.RequiresCaptcha = true;
                            state.CaptchaId = (string)loginresponse["captcha_gid"];
                            state.CaptchaUrl = COMMUNITY_BASE + "/public/captcha.php?gid=" + state.CaptchaId;
                        }
                        else
                        {
                            state.RequiresCaptcha = false;
                            state.CaptchaId = null;
                            state.CaptchaUrl = null;
                            state.CaptchaText = null;
                        }

                        // require email auth
                        if (loginresponse.ContainsKey("emailauth_needed") == true && (bool)loginresponse["emailauth_needed"] == true)
                        {
                            if (loginresponse.ContainsKey("emaildomain") == true)
                            {
                                var emaildomain = (string)loginresponse["emaildomain"];
                                if (string.IsNullOrEmpty(emaildomain) == false)
                                {
                                    state.EmailDomain = emaildomain;
                                }
                            }
                            state.RequiresEmailAuth = true;
                        }
                        else
                        {
                            state.EmailDomain = null;
                            state.RequiresEmailAuth = false;
                        }

                        // require email auth
                        if (loginresponse.ContainsKey("requires_twofactor") == true && (bool)loginresponse["requires_twofactor"] == true)
                        {
                            state.Requires2FA = true;
                        }
                        else
                        {
                            state.Requires2FA = false;
                        }

                        // if we didn't login, return the result
                        if (loginresponse.ContainsKey("login_complete") == false || (bool)loginresponse["login_complete"] == false || loginresponse.ContainsKey("oauth") == false)
                        {
                            if (loginresponse.ContainsKey("oauth") == false)
                            {
                                state.Error = "Invalid response from Steam (No OAuth token)";
                            }
                            if (loginresponse.ContainsKey("message") == true)
                            {
                                state.Error = (string)loginresponse["message"];
                            }
                            return false;
                        }

                        // get the OAuth token - is stringified json
                        string oauth = (string)loginresponse["oauth"];
                        var oauthjson = JObject.Parse(oauth);
                        state.OAuthToken = oauthjson.SelectToken("oauth_token").Value<string>();
                        if (oauthjson.SelectToken("steamid") != null)
                        {
                            state.SteamId = oauthjson.SelectToken("steamid").Value<string>();
                        }
                    }

                    // login to webapi
                    data.Clear();
                    data.Add("access_token", state.OAuthToken);
                    response = Request(WEBAPI_BASE + "/ISteamWebUserPresenceOAuth/Logon/v0001", "POST", data);

                    var sessionid = cookies.GetCookies(new Uri(COMMUNITY_BASE + "/"))?["sessionid"]?.Value;

                    if (state.RequiresActivation == false)
                    {
                        data.Clear();
                        data.Add("op", "has_phone");
                        data.Add("arg", "null");
                        data.Add("sessionid", sessionid);

                        response = Request(COMMUNITY_BASE + "/steamguard/phoneajax", "POST", data, cookies);
                        var jsonresponse = JObject.Parse(response);
                        bool hasPhone = jsonresponse.SelectToken("has_phone").Value<Boolean>();
                        if (hasPhone == false)
                        {
                            state.OAuthToken = null; // force new login
                            state.RequiresLogin = true;
                            state.Cookies = null;
                            state.Error = "您的Steam帐户必须附有支持短信的电话号码, " + Environment.NewLine + "请到Steam客户端或Steam网站的帐户详细信息添加电话号码。";
                            return false;
                        }

                        //response = Request(COMMUNITY_BASE + "/steamguard/phone_checksms?bForTwoFactor=1&bRevoke2fOnCancel=", "GET", null, cookies);

                        // add a new authenticator
                        data.Clear();
                        string deviceId = BuildRandomId();
                        data.Add("access_token", state.OAuthToken);
                        data.Add("steamid", state.SteamId);
                        data.Add("authenticator_type", "1");
                        data.Add("device_identifier", deviceId);
                        data.Add("sms_phone_id", "1");
                        response = Request(WEBAPI_BASE + "/ITwoFactorService/AddAuthenticator/v0001", "POST", data);
                        var tfaresponse = JObject.Parse(response);
                        if (response.IndexOf("status") == -1 && tfaresponse.SelectToken("response.status").Value<int>() == 84)
                        {
                            // invalid response
                            state.OAuthToken = null; // force new login
                            state.RequiresLogin = true;
                            state.Cookies = null;
                            state.Error = "无法发送短信， 检查您的电话已在您的Steam帐户中注册。";
                            return false;
                        }
                        if (response.IndexOf("shared_secret") == -1)
                        {
                            // invalid response
                            state.OAuthToken = null; // force new login
                            state.RequiresLogin = true;
                            state.Cookies = null;
                            state.Error = "Invalid response from Steam: " + response;
                            return false;
                        }

                        // save data into this authenticator
                        var secret = tfaresponse.SelectToken("response.shared_secret").Value<string>();
                        SecretKey = Convert.FromBase64String(secret);
                        Serial = tfaresponse.SelectToken("response.serial_number").Value<string>();
                        DeviceId = deviceId;
                        state.RevocationCode = tfaresponse.SelectToken("response.revocation_code").Value<string>();

                        // add the steamid into the data
                        var steamdata = JObject.Parse(tfaresponse.SelectToken("response").ToString());
                        if (steamdata.SelectToken("steamid") == null)
                        {
                            steamdata.Add("steamid", state.SteamId);
                        }
                        if (steamdata.SelectToken("steamguard_scheme") == null)
                        {
                            steamdata.Add("steamguard_scheme", "2");
                        }
                        SteamData = steamdata.ToString(Newtonsoft.Json.Formatting.None);

                        // calculate server drift
                        long servertime = tfaresponse.SelectToken("response.server_time").Value<long>() * 1000;
                        ServerTimeDiff = servertime - CurrentTime;
                        LastServerTime = DateTime.Now.Ticks;

                        state.RequiresActivation = true;

                        return false;
                    }

                    // finalize adding the authenticator
                    data.Clear();
                    data.Add("access_token", state.OAuthToken);
                    data.Add("steamid", state.SteamId);
                    data.Add("activation_code", state.ActivationCode);

                    // try and authorise
                    var retries = 0;
                    while (state.RequiresActivation == true && retries < ENROLL_ACTIVATE_RETRIES)
                    {
                        data.Add("authenticator_code", CalculateCode(false));
                        data.Add("authenticator_time", ServerTime.ToString());
                        response = Request(WEBAPI_BASE + "/ITwoFactorService/FinalizeAddAuthenticator/v0001", "POST", data);
                        var finalizeresponse = JObject.Parse(response);
                        if (response.IndexOf("status") != -1 && finalizeresponse.SelectToken("response.status").Value<int>() == INVALID_ACTIVATION_CODE)
                        {
                            state.Error = "Invalid activation code";
                            return false;
                        }

                        // reset our time
                        if (response.IndexOf("server_time") != -1)
                        {
                            long servertime = finalizeresponse.SelectToken("response.server_time").Value<long>() * 1000;
                            ServerTimeDiff = servertime - CurrentTime;
                            LastServerTime = DateTime.Now.Ticks;
                        }

                        // check success
                        if (finalizeresponse.SelectToken("response.success").Value<bool>() == true)
                        {
                            if (response.IndexOf("want_more") != -1 && finalizeresponse.SelectToken("response.want_more").Value<bool>() == true)
                            {
                                ServerTimeDiff += ((long)Period * 1000L);
                                retries++;
                                continue;
                            }
                            state.RequiresActivation = false;
                            break;
                        }

                        ServerTimeDiff += ((long)Period * 1000L);
                        retries++;
                    }
                    if (state.RequiresActivation == true)
                    {
                        state.Error = "注册时出现问题！Steam服务器可能存在问题， 请稍后再试。";
                        return false;
                    }

                    // mark and successful and return key
                    state.Success = true;
                    state.SecretKey = ByteArrayToString(SecretKey);

                    // send confirmation email
                    data.Clear();
                    data.Add("access_token", state.OAuthToken);
                    data.Add("steamid", state.SteamId);
                    data.Add("email_type", "2");
                    response = Request(WEBAPI_BASE + "/ITwoFactorService/SendEmail/v0001", "POST", data);

                    return true;
                }
                catch (WinAuthUnauthorisedRequestException ex)
                {
                    throw new WinAuthInvalidEnrollResponseException("You are not allowed to add an authenticator. Have you enabled 'community-generated content' in Family View?", ex);
                }
                catch (WinAuthInvalidRequestException ex)
                {
                    throw new WinAuthInvalidEnrollResponseException("Error enrolling new authenticator", ex);
                }
            }

            /// <summary>
            /// Synchronise this authenticator's time with Steam.
            /// </summary>
            public override void Sync()
            {
                // check if data is protected
                if (SecretKey == null && EncryptedData != null)
                {
                    throw new WinAuthEncryptedSecretDataException();
                }

                // don't retry for 5 minutes
                if (_lastSyncError >= DateTime.Now.AddMinutes(0 - SYNC_ERROR_MINUTES))
                {
                    return;
                }

                try
                {
                    var response = Request(SYNC_URL, "POST", null, null, null, SYNC_TIMEOUT);
                    var json = JObject.Parse(response);

                    // get servertime in ms
                    long servertime = json.SelectToken("response.server_time").Value<long>() * 1000;

                    // get the difference between the server time and our current time
                    ServerTimeDiff = servertime - CurrentTime;
                    LastServerTime = DateTime.Now.Ticks;

                    // clear any sync error
                    _lastSyncError = DateTime.MinValue;
                }
                catch (Exception ex)
                {
                    // don't retry for a while after error
                    _lastSyncError = DateTime.Now;
                    throw ex;
                    // set to zero to force reset
                    //ServerTimeDiff = 0;
                }
            }

            /// <summary>
            /// Calculate the current code for the authenticator.
            /// </summary>
            /// <param name="resyncTime">flag to resync time</param>
            /// <returns>authenticator code</returns>
            protected override string CalculateCode(bool resyncTime = false, long interval = -1)
            {
                // sync time if required
                if (resyncTime == true || ServerTimeDiff == 0)
                {
                    if (interval > 0)
                    {
                        ServerTimeDiff = (interval * ((long)Period * 1000L)) - CurrentTime;
                    }
                    else
                    {
                        Sync();
                    }
                }

                HMac hmac = new HMac(new Sha1Digest());
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
                int start = mac[19] & 0x0f;

                // extract those 4 bytes
                byte[] bytes = new byte[4];
                Array.Copy(mac, start, bytes, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }
                uint fullcode = BitConverter.ToUInt32(bytes, 0) & 0x7fffffff;

                // build the alphanumeric code
                StringBuilder code = new StringBuilder();
                for (var i = 0; i < CODE_DIGITS; i++)
                {
                    code.Append(STEAMCHARS[fullcode % STEAMCHARS.Length]);
                    fullcode /= (uint)STEAMCHARS.Length;
                }

                return code.ToString();
            }

            /// <summary>
            /// Create a random Device ID string for Enrolling
            /// </summary>
            /// <returns>Random string</returns>
            static string BuildRandomId()
            {
                return "android:" + Guid.NewGuid().ToString();
            }

            /// <summary>
            /// Log an exception from a Request
            /// </summary>
            /// <param name="method">Get or POST</param>
            /// <param name="url">Request URL</param>
            /// <param name="cookies">cookie container</param>
            /// <param name="request">Request data</param>
            /// <param name="ex">Thrown exception</param>
            static void LogException(string method, string url, CookieContainer cookies, NameValueCollection request, Exception ex)
            {
                StringBuilder data = new StringBuilder();
                if (cookies != null)
                {
                    foreach (Cookie cookie in cookies.GetCookies(new Uri(url)))
                    {
                        if (data.Length == 0)
                        {
                            data.Append("Cookies:");
                        }
                        else
                        {
                            data.Append('&');
                        }
                        data.Append(cookie.Name + "=" + cookie.Value);
                    }
                    data.Append(' ');
                }

                if (request != null)
                {
                    foreach (var key in request.AllKeys)
                    {
                        if (data.Length == 0)
                        {
                            data.Append("Req:");
                        }
                        else
                        {
                            data.Append("&");
                        }
                        data.Append(key + "=" + request[key]);
                    }
                    data.Append(" ");
                }
            }

            /// <summary>
            /// Log a normal response
            /// </summary>
            /// <param name="method">Get or POST</param>
            /// <param name="url">Request URL</param>
            /// <param name="cookies">cookie container</param>
            /// <param name="request">Request data</param>
            /// <param name="response">response body</param>
            static void LogRequest(string method, string url, CookieContainer cookies, NameValueCollection request, string response)
            {
                StringBuilder data = new StringBuilder();
                if (cookies != null)
                {
                    foreach (Cookie cookie in cookies.GetCookies(new Uri(url)))
                    {
                        if (data.Length == 0)
                        {
                            data.Append("Cookies:");
                        }
                        else
                        {
                            data.Append("&");
                        }
                        data.Append(cookie.Name + "=" + cookie.Value);
                    }
                    data.Append(" ");
                }

                if (request != null)
                {
                    foreach (var key in request.AllKeys)
                    {
                        if (data.Length == 0)
                        {
                            data.Append("Req:");
                        }
                        else
                        {
                            data.Append("&");
                        }
                        data.Append(key + "=" + request[key]);
                    }
                    data.Append(" ");
                }
            }
        }
    }
}