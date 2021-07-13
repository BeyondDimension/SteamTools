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
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static System.Application.Models.GAPAuthenticatorValueDTO;

namespace WinAuth
{
    /// <summary>
    /// SteamClient for logging and getting/accepting/rejecting trade confirmations
    /// </summary>
    public partial class WinAuthSteamClient : IDisposable
    {
        /// <summary>
        /// URLs for all mobile services
        /// </summary>
        const string COMMUNITY_DOMAIN = "steamcommunity.com";
        const string COMMUNITY_BASE = "https://" + COMMUNITY_DOMAIN;
        const string WEBAPI_BASE = "https://api.steampowered.com";
        const string API_GETWGTOKEN = WEBAPI_BASE + "/IMobileAuthService/GetWGToken/v0001";
        const string API_LOGOFF = WEBAPI_BASE + "/ISteamWebUserPresenceOAuth/Logoff/v0001";
        const string API_LOGON = WEBAPI_BASE + "/ISteamWebUserPresenceOAuth/Logon/v0001";

        /// <summary>
        /// Default mobile user agent
        /// </summary>
        const string USERAGENT = "Mozilla/5.0 (Linux; U; Android 4.1.1; en-us; Google Nexus 4 - 4.1.1 - API 16 - 768x1280 Build/JRO03S) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";

        /// <summary>
        /// Regular expressions for trade confirmations
        /// </summary>
        static readonly Regex _tradesRegex = new("\"mobileconf_list_entry\"(.*?)>(.*?)\"mobileconf_list_entry_sep\"", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        static readonly Regex _tradeConfidRegex = new(@"data-confid\s*=\s*""([^""]+)""", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        static readonly Regex _tradeKeyRegex = new(@"data-key\s*=\s*""([^""]+)""", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        static readonly Regex _tradePlayerRegex = new("\"mobileconf_list_entry_icon\"(.*?)src=\"([^\"]+)\"", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        static readonly Regex _tradeDetailsRegex = new("\"mobileconf_list_entry_description\".*?<div>([^<]*)</div>[^<]*<div>([^<]*)</div>[^<]*<div>([^<]*)</div>[^<]*</div>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Number of Confirmation retries
        /// </summary>
        const int DEFAULT_CONFIRMATIONPOLLER_RETRIES = 3;

        /// <summary>
        /// Delay between trade confirmation events
        /// </summary>
        public const int CONFIRMATION_EVENT_DELAY = 1000;

        /// <summary>
        /// Action for Confirmation polling
        /// </summary>
        public enum PollerAction
        {
            None = 0,
            Notify = 1,
            AutoConfirm = 2,
            SilentAutoConfirm = 3
        }

        /// <summary>
        /// Hold the Confirmation polling data
        /// </summary>
        public class ConfirmationPoller
        {
            /// <summary>
            /// Seconds between polls
            /// </summary>
            public int Duration;

            /// <summary>
            /// Action for new Confirmation
            /// </summary>
            public PollerAction Action;

            /// <summary>
            /// List of current Confirmations ids
            /// </summary>
            public List<string>? Ids;

            /// <summary>
            /// Create new ConfirmationPoller object
            /// </summary>
            public ConfirmationPoller()
            {
            }

            /// <summary>
            /// Create a JSON string of the object
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (Duration == 0)
                {
                    return "null";
                }
                else
                {
                    List<string> props = new List<string>
                    {
                        "\"duration\":" + Duration,
                        "\"action\":" + (int)Action
                    };
                    if (Ids != null)
                    {
                        props.Add("\"ids\":[" + (Ids.Count != 0 ? "\"" + string.Join("\",\"", Ids.ToArray()) + "\"" : string.Empty) + "]");
                    }

                    return "{" + string.Join(",", props.ToArray()) + "}";
                }
            }

            /// <summary>
            /// Create a new ConfirmationPoller from a JSON string
            /// </summary>
            /// <param name="json">JSON string</param>
            /// <returns>new ConfirmationPoller or null</returns>
            public static ConfirmationPoller? FromJSON(string json)
            {
                if (string.IsNullOrEmpty(json) == true || json == "null")
                {
                    return null;
                }
                var poller = FromJSON(JObject.Parse(json));
                return (poller.Duration != 0 ? poller : null);
            }

            /// <summary>
            /// Create a new ConfirmationPoller from a JToken
            /// </summary>
            /// <param name="tokens">existing JKToken</param>
            /// <returns></returns>
            public static ConfirmationPoller? FromJSON(JToken tokens)
            {
                if (tokens == null)
                {
                    return null;
                }

                var poller = new ConfirmationPoller();

                var token = tokens.SelectToken("duration");
                if (token != null)
                {
                    poller.Duration = token.Value<int>();
                }
                token = tokens.SelectToken("action");
                if (token != null)
                {
                    poller.Action = (PollerAction)token.Value<int>();
                }
                token = tokens.SelectToken("ids");
                if (token != null)
                {
                    poller.Ids = token.ToObject<List<string>>();
                }

                return (poller.Duration != 0 ? poller : null);
            }
        }

        /// <summary>
        /// A class for a single confirmation
        /// </summary>
        public class Confirmation : ReactiveObject
        {
            public string Id { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public bool Offline { get; set; }
            public bool IsNew { get; set; }
            public string Image { get; set; } = string.Empty;

            private Task<string?>? _ImageStream;
            [Obsolete("use httpUrl in Mobile")]
            public Task<string?>? ImageStream
            {
                get => _ImageStream;
                set => this.RaiseAndSetIfChanged(ref _ImageStream, value);
            }

            private bool _ButtonEnable = true;
            public bool ButtonEnable
            {
                get => _ButtonEnable;
                set => this.RaiseAndSetIfChanged(ref _ButtonEnable, value);
            }

            private int _IsOperate;
            public int IsOperate
            {
                get => _IsOperate;
                set => this.RaiseAndSetIfChanged(ref _IsOperate, value);
            }

            public string Details { get; set; } = string.Empty;
            public string Traded { get; set; } = string.Empty;
            public string When { get; set; } = string.Empty;
        }

        /// <summary>
        /// Session state to remember logins
        /// </summary>
        public class SteamSession
        {
            /// <summary>
            /// User's steam ID
            /// </summary>
            public string? SteamId;

            /// <summary>
            /// Current cookies
            /// </summary>
            public CookieContainer Cookies = new();

            /// <summary>
            /// Authorization token
            /// </summary>
            public string? OAuthToken;

            /// <summary>
            /// UMQ id
            /// </summary>
            public string? UmqId;

            /// <summary>
            /// Message id
            /// </summary>
            public int MessageId;

            /// <summary>
            /// Current polling state
            /// </summary>
            public ConfirmationPoller? Confirmations;

            /// <summary>
            /// Create Session instance
            /// </summary>
            public SteamSession()
            {
                Clear(false);
            }

            /// <summary>
            /// Create session instance from existing json data
            /// </summary>
            /// <param name="json">json session data</param>
            public SteamSession(string? json) : this()
            {
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        FromJson(json);
                    }
                    catch (Exception)
                    {
                        // invalid json
                    }
                }
            }

            /// <summary>
            /// Clear the session
            /// </summary>
            public void Clear() => Clear(true);

            void Clear(bool setCookies)
            {
                OAuthToken = null;
                UmqId = null;
                if (setCookies) Cookies = new();
                Confirmations = null;
            }

            /// <summary>
            /// Get session data that can be saved and imported
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return "{\"steamid\":\"" + (SteamId ?? string.Empty) + "\","
                    + "\"cookies\":\"" + Cookies.ThrowIsNull(nameof(Cookies)).GetCookieHeader(new Uri(COMMUNITY_BASE + "/")) + "\","
                    + "\"oauthtoken\":\"" + (OAuthToken ?? string.Empty) + "\","
                    // + "\"umqid\":\"" + (this.UmqId ?? string.Empty) + "\","
                    + "\"confs\":" + (Confirmations != null ? Confirmations.ToString() : "null")
                    + "}";
            }

            /// <summary>
            /// Convert json data into session
            /// </summary>
            /// <param name="json"></param>
            void FromJson(string json)
            {
                var tokens = JObject.Parse(json);
                var token = tokens.SelectToken("steamid");
                if (token != null)
                {
                    SteamId = token.Value<string>();
                }
                token = tokens.SelectToken("cookies");
                if (token != null)
                {
                    Cookies = new CookieContainer();

                    // Net3.5 has a bug that prepends "." to domain, e.g. ".steamcommunity.com"
                    var uri = new Uri(COMMUNITY_BASE + "/");
                    var match = Regex.Match(token.Value<string>(), @"([^=]+)=([^;]*);?", RegexOptions.Singleline);
                    while (match.Success == true)
                    {
                        Cookies.Add(uri, new Cookie(match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim()));
                        match = match.NextMatch();
                    }
                }
                token = tokens.SelectToken("oauthtoken");
                if (token != null)
                {
                    OAuthToken = token.Value<string>();
                }
                //token = tokens.SelectToken("umqid");
                //if (token != null)
                //{
                //	this.UmqId = token.Value<string>();
                //}
                token = tokens.SelectToken("confs");
                if (token != null)
                {
                    Confirmations = ConfirmationPoller.FromJSON(token);
                }
            }
        }

        /// <summary>
        /// Login state fields
        /// </summary>
        public bool InvalidLogin;
        public bool RequiresCaptcha;
        public string? CaptchaId;
        public string? CaptchaUrl;
        public bool Requires2FA;
        public bool RequiresEmailAuth;
        public string? EmailDomain;
        public string? Error;

        /// <summary>
        /// Current session
        /// </summary>
        public SteamSession Session;

        /// <summary>
        /// Current authenticator
        /// </summary>
        public SteamAuthenticator Authenticator;

        /// <summary>
        /// Saved Html from GetConfirmations used as template for GetDetails
        /// </summary>
        string? ConfirmationsHtml;

        /// <summary>
        /// Query string from GetConfirmations used in GetDetails
        /// </summary>
        string? ConfirmationsQuery;

        /// <summary>
        /// Cancellation token for poller
        /// </summary>
        CancellationTokenSource? _pollerCancellation;

        /// <summary>
        /// Number of Confirmation retries
        /// </summary>
        public int ConfirmationPollerRetries = DEFAULT_CONFIRMATIONPOLLER_RETRIES;

        /// <summary>
        /// Create a new SteamClient
        /// </summary>
        public WinAuthSteamClient(SteamAuthenticator auth, string? session = null)
        {
            Authenticator = auth;
            Session = new SteamSession(session);

            if (Session.Confirmations != null)
            {
                if (IsLoggedIn() == false)
                {
                    Session.Confirmations = null;
                }
                else
                {
                    Task.Factory.StartNew(() =>
                    {
                        Refresh();
                        PollConfirmations(Session.Confirmations);
                    });
                }
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~WinAuthSteamClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose this object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // clear resources
            }

            if (_pollerCancellation != null)
            {
                _pollerCancellation.Cancel();
                _pollerCancellation = null;
            }
        }

        /// <summary>
        /// Clear the client state
        /// </summary>
        public void Clear()
        {
            InvalidLogin = false;
            RequiresCaptcha = false;
            CaptchaId = null;
            CaptchaUrl = null;
            RequiresEmailAuth = false;
            EmailDomain = null;
            Requires2FA = false;
            Error = null;

            Session.Clear();
        }

        /// <summary>
        /// Check if user is logged in
        /// </summary>
        /// <returns></returns>
        public bool IsLoggedIn()
        {
            return (Session != null && string.IsNullOrEmpty(Session.OAuthToken) == false);
        }

        /// <summary>
        /// Login to Steam using credentials and optional captcha
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="captchaId"></param>
        /// <param name="captchaText"></param>
        /// <returns>true if successful</returns>
        //[Obsolete("use LoginAsync")]
        public bool Login(string username, string password, string? captchaId = null, string? captchaText = null, string? language = null)
        {
            // clear error
            Error = null;

            var data = new NameValueCollection();
            string response;

            if (IsLoggedIn() == false)
            {
                // get session
                if (Session.Cookies.Count == 0)
                {
                    // .Net3.5 has a bug in CookieContainer that prepends a "." to the domain, i.e. ".steamcommunity.com"
                    var cookieuri = new Uri(COMMUNITY_BASE + "/");
                    Session.Cookies.Add(cookieuri, new Cookie("mobileClientVersion", "3067969+%282.1.3%29"));
                    Session.Cookies.Add(cookieuri, new Cookie("mobileClient", "android"));
                    Session.Cookies.Add(cookieuri, new Cookie("steamid", ""));
                    Session.Cookies.Add(cookieuri, new Cookie("steamLogin", ""));
                    Session.Cookies.Add(cookieuri, new Cookie("Steam_Language", string.IsNullOrEmpty(language) ? "english" : language));
                    Session.Cookies.Add(cookieuri, new Cookie("dob", ""));

                    NameValueCollection headers = new NameValueCollection();
                    headers.Add("X-Requested-With", "com.valvesoftware.android.steam.community");

                    response = GetString(COMMUNITY_BASE + "/mobilelogin?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client", "GET", null, headers);
                }

                // Steam strips any non-ascii chars from username and password
                username = Regex.Replace(username, @"[^\u0000-\u007F]", string.Empty);
                password = Regex.Replace(password, @"[^\u0000-\u007F]", string.Empty);

                // get the user's RSA key
                data.Add("username", username);
                response = GetString(COMMUNITY_BASE + "/mobilelogin/getrsakey", "POST", data);
                var rsaresponse = JObject.Parse(response);
                if (rsaresponse.SelectToken("success").Value<bool>() != true)
                {
                    InvalidLogin = true;
                    Error = "Unknown username";
                    return false;
                }

                // encrypt password with RSA key
                RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
                string encryptedPassword64;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var passwordBytes = Encoding.ASCII.GetBytes(password);
                    var p = rsa.ExportParameters(false);
                    p.Exponent = StringToByteArray(rsaresponse.SelectToken("publickey_exp").Value<string>());
                    p.Modulus = StringToByteArray(rsaresponse.SelectToken("publickey_mod").Value<string>());
                    rsa.ImportParameters(p);
                    byte[] encryptedPassword = rsa.Encrypt(passwordBytes, false);
                    encryptedPassword64 = Convert.ToBase64String(encryptedPassword);
                }

                // login request
                data = new NameValueCollection();
                data.Add("password", encryptedPassword64);
                data.Add("username", username);
                data.Add("twofactorcode", this.Authenticator.CurrentCode);
                //data.Add("emailauth", string.Empty);
                data.Add("loginfriendlyname", "#login_emailauth_friendlyname_mobile");
                data.Add("captchagid", (string.IsNullOrEmpty(captchaId) == false ? captchaId : "-1"));
                data.Add("captcha_text", (string.IsNullOrEmpty(captchaText) == false ? captchaText : "enter above characters"));
                //data.Add("emailsteamid", (string.IsNullOrEmpty(emailcode) == false ? this.SteamId ?? string.Empty : string.Empty));
                data.Add("rsatimestamp", rsaresponse.SelectToken("timestamp").Value<string>());
                data.Add("remember_login", "false");
                data.Add("oauth_client_id", "DE45CD61");
                data.Add("oauth_scope", "read_profile write_profile read_client write_client");
                data.Add("donotache", new DateTime().ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString());
                response = GetString(COMMUNITY_BASE + "/mobilelogin/dologin/", "POST", data);
                Dictionary<string, object> loginresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                if (loginresponse.ContainsKey("emailsteamid") == true)
                {
                    Session.SteamId = loginresponse["emailsteamid"] as string;
                }

                InvalidLogin = false;
                RequiresCaptcha = false;
                CaptchaId = null;
                CaptchaUrl = null;
                RequiresEmailAuth = false;
                EmailDomain = null;
                Requires2FA = false;

                if (loginresponse.ContainsKey("login_complete") == false || (bool)loginresponse["login_complete"] == false || loginresponse.ContainsKey("oauth") == false)
                {
                    InvalidLogin = true;

                    // require captcha
                    if (loginresponse.ContainsKey("captcha_needed") == true && (bool)loginresponse["captcha_needed"] == true)
                    {
                        RequiresCaptcha = true;
                        CaptchaId = (string)loginresponse["captcha_gid"];
                        CaptchaUrl = COMMUNITY_BASE + "/public/captcha.php?gid=" + CaptchaId;
                    }

                    // require email auth
                    if (loginresponse.ContainsKey("emailauth_needed") == true && (bool)loginresponse["emailauth_needed"] == true)
                    {
                        if (loginresponse.ContainsKey("emaildomain") == true)
                        {
                            var emaildomain = (string)loginresponse["emaildomain"];
                            if (string.IsNullOrEmpty(emaildomain) == false)
                            {
                                EmailDomain = emaildomain;
                            }
                        }
                        RequiresEmailAuth = true;
                    }

                    // require email auth
                    if (loginresponse.ContainsKey("requires_twofactor") == true && (bool)loginresponse["requires_twofactor"] == true)
                    {
                        Requires2FA = true;
                    }

                    if (loginresponse.ContainsKey("message") == true)
                    {
                        Error = (string)loginresponse["message"];
                    }

                    return false;
                }

                // get the OAuth token
                string oauth = (string)loginresponse["oauth"];
                var oauthjson = JObject.Parse(oauth);
                Session.OAuthToken = oauthjson.SelectToken("oauth_token").Value<string>();
                if (oauthjson.SelectToken("steamid") != null)
                {
                    Session.SteamId = oauthjson.SelectToken("steamid").Value<string>();
                }

                //// perform UMQ login
                //data.Clear();
                //data.Add("access_token", this.Session.OAuthToken);
                //response = GetString(API_LOGON, "POST", data);
                //loginresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                //if (loginresponse.ContainsKey("umqid") == true)
                //{
                //	this.Session.UmqId = (string)loginresponse["umqid"];
                //	if (loginresponse.ContainsKey("message") == true)
                //	{
                //		this.Session.MessageId = Convert.ToInt32(loginresponse["message"]);
                //	}
                //}
            }

            return true;
        }

        /// <summary>
        /// Logout of the current session
        /// </summary>
        //[Obsolete("use LogoutAsync")]
        public void Logout()
        {
            if (string.IsNullOrEmpty(Session.OAuthToken) == false)
            {
                PollConfirmationsStop();

                if (string.IsNullOrEmpty(Session.UmqId) == false)
                {
                    var data = new NameValueCollection();
                    data.Add("access_token", this.Session.OAuthToken);
                    data.Add("umqid", this.Session.UmqId);
                    GetString(API_LOGOFF, "POST", data);
                }
            }

            Clear();
        }

        /// <summary>
        /// Refresh the login session cookies from the OAuth code
        /// </summary>
        /// <returns>true if successful</returns>
        //[Obsolete("use RefreshAsync")]
        public bool Refresh()
        {
            try
            {
                var data = new NameValueCollection();
                data.Add("access_token", this.Session.OAuthToken);
                string response = GetString(API_GETWGTOKEN, "POST", data);
                if (string.IsNullOrEmpty(response) == true)
                {
                    return false;
                }

                var json = JObject.Parse(response);
                var token = json.SelectToken("response.token");
                if (token == null)
                {
                    return false;
                }

                var cookieuri = new Uri(COMMUNITY_BASE + "/");
                Session.Cookies.Add(cookieuri, new Cookie("steamLogin", Session.SteamId + "||" + token.Value<string>()));

                token = json.SelectToken("response.token_secure");
                if (token == null)
                {
                    return false;
                }
                Session.Cookies.Add(cookieuri, new Cookie("steamLoginSecure", Session.SteamId + "||" + token.Value<string>()));

                // perform UMQ login
                //response = GetString(API_LOGON, "POST", data);
                //var loginresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                //if (loginresponse.ContainsKey("umqid") == true)
                //{
                //	this.Session.UmqId = (string)loginresponse["umqid"];
                //	if (loginresponse.ContainsKey("message") == true)
                //	{
                //		this.Session.MessageId = Convert.ToInt32(loginresponse["message"]);
                //	}
                //}

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Perform a UMQ login
        /// </summary>
        /// <returns></returns>
        //[Obsolete("use UmqLoginAsync")]
        bool UmqLogin()
        {
            if (IsLoggedIn() == false)
            {
                return false;
            }

            var data = new NameValueCollection();
            data.Add("access_token", this.Session.OAuthToken);
            var response = GetString(API_LOGON, "POST", data);
            var loginresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            if (loginresponse.ContainsKey("umqid") == true)
            {
                Session.UmqId = (string)loginresponse["umqid"];
                if (loginresponse.ContainsKey("message") == true)
                {
                    Session.MessageId = Convert.ToInt32(loginresponse["message"]);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Delegate for Confirmation event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newconfirmation">new Confirmation</param>
        /// <param name="action">action to be taken</param>
        public delegate void ConfirmationDelegate(object sender, Confirmation newconfirmation, PollerAction action);

        /// <summary>
        /// Delegate for Confirmation error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message">error message</param>
        /// <param name="ex">optional exception</param>
        public delegate void ConfirmationErrorDelegate(object sender, string message, PollerAction action, Exception ex);

        /// <summary>
        /// Event fired for new Confirmation
        /// </summary>
        public event ConfirmationDelegate? ConfirmationEvent;

        /// <summary>
        /// Event fired for error on polling
        /// </summary>
        public event ConfirmationErrorDelegate? ConfirmationErrorEvent;

        /// <summary>
        /// Stop the current poller
        /// </summary>
        protected void PollConfirmationsStop()
        {
            // kill any existing poller
            if (_pollerCancellation != null)
            {
                _pollerCancellation.Cancel();
                _pollerCancellation = null;
            }
            Session.Confirmations = null;
        }

        /// <summary>
        /// Start a new poller
        /// </summary>
        /// <param name="poller"></param>
        public void PollConfirmations(ConfirmationPoller poller)
        {
            PollConfirmationsStop();

            if (poller == null || poller.Duration <= 0)
            {
                return;
            }

            if (Session.Confirmations == null)
            {
                Session.Confirmations = new ConfirmationPoller();
            }
            Session.Confirmations = poller;

            _pollerCancellation = new CancellationTokenSource();
            var token = _pollerCancellation.Token;
            Task.Factory.StartNew(() => PollConfirmations(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Confirmation polling task
        /// </summary>
        /// <param name="cancel"></param>
        public async void PollConfirmations(CancellationToken cancel)
        {
            //lock (this.Session.Confirmations)
            //{
            //	if (this.Session.Confirmations.Ids == null)
            //	{
            //		try
            //		{
            //			// this will update the session
            //			GetConfirmations();
            //		}
            //		catch (InvalidSteamRequestException)
            //		{
            //			// ignore in case of Steam timeout
            //		}
            //	}
            //}

            try
            {
                int retryCount = 0;
                while (!cancel.IsCancellationRequested && Session.Confirmations != null)
                {
                    try
                    {
                        //List<string> currentIds;
                        //lock (this.Session.Confirmations)
                        //{
                        //	currentIds = this.Session.Confirmations.Ids;
                        //}

                        var confs = GetConfirmations();

                        // check for new ids
                        //List<string> newIds;
                        //if (currentIds == null)
                        //{
                        //	newIds = confs.Select(t => t.Id).ToList();
                        //}
                        //else
                        //{
                        //	newIds = confs.Select(t => t.Id).Except(currentIds).ToList();
                        //}

                        // fire events if subscriber
                        if (ConfirmationEvent != null /* && newIds.Count() != 0 */)
                        {
                            var rand = new Random();
                            foreach (var conf in confs)
                            {
                                if (cancel.IsCancellationRequested)
                                {
                                    break;
                                }

                                DateTime start = DateTime.Now;

                                ConfirmationEvent(this, conf, Session.Confirmations.Action);

                                // Issue#339: add a delay for any autoconfs or notifications
                                var delay = CONFIRMATION_EVENT_DELAY + rand.Next(CONFIRMATION_EVENT_DELAY / 2); // delay is 100%-150% of CONFIRMATION_EVENT_DELAY
                                var duration = (int)DateTime.Now.Subtract(start).TotalMilliseconds;
                                if (delay > duration)
                                {
                                    Thread.Sleep(delay - duration);
                                }
                            }
                        }

                        retryCount = 0;
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        if (retryCount >= ConfirmationPollerRetries)
                        {
                            ConfirmationErrorEvent(this, "Failed to read confirmations", Session.Confirmations.Action, ex);
                        }
                        else
                        {
                            // try and reset the session
                            try
                            {
                                Refresh();
                            }
                            catch (Exception) { }
                        }
                    }

                    if (Session.Confirmations != null)
                    {
                        await Task.Delay(Session.Confirmations.Duration * 60 * 1000, cancel);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        /// <summary>
        /// Get the current trade Confirmations
        /// </summary>
        /// <returns>list of Confirmation objects</returns>
        //[Obsolete("use GetConfirmationsAsync")]
        public List<Confirmation> GetConfirmations()
        {
            long servertime = (CurrentTime + Authenticator.ServerTimeDiff) / 1000L;

            var jids = JObject.Parse(Authenticator.SteamData).SelectToken("identity_secret");
            string ids = (jids != null ? jids.Value<string>() : string.Empty);

            var timehash = CreateTimeHash(servertime, "conf", ids);

            var data = new NameValueCollection()
            {
                { "p", Authenticator.DeviceId },
                { "a", Session.SteamId },
                { "k", timehash },
                { "t", servertime.ToString() },
                { "m", "android" },
                { "tag", "conf" }
            };

            string html = GetString(COMMUNITY_BASE + "/mobileconf/conf", "GET", data);

            // save last html for confirmations details
            ConfirmationsHtml = html;
            ConfirmationsQuery = string.Join("&", Array.ConvertAll(data.AllKeys, key => String.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key]))));

            List<Confirmation> trades = new();

            // extract the trades
            Match match = _tradesRegex.Match(html);
            while (match.Success)
            {
                var tradeIds = match.Groups[1].Value;

                var trade = new Confirmation();

                var innerMatch = _tradeConfidRegex.Match(tradeIds);
                if (innerMatch.Success)
                {
                    trade.Id = innerMatch.Groups[1].Value;
                }
                innerMatch = _tradeKeyRegex.Match(tradeIds);
                if (innerMatch.Success)
                {
                    trade.Key = innerMatch.Groups[1].Value;
                }

                var traded = match.Groups[2].Value;

                innerMatch = _tradePlayerRegex.Match(traded);
                if (innerMatch.Success)
                {
                    if (innerMatch.Groups[1].Value.IndexOf("offline") != -1)
                    {
                        trade.Offline = true;
                    }
                    trade.Image = innerMatch.Groups[2].Value.Replace("32fx32f", "128fx128f");
                }

                innerMatch = _tradeDetailsRegex.Match(traded);
                if (innerMatch.Success)
                {
                    trade.Details = innerMatch.Groups[1].Value;
                    trade.Traded = innerMatch.Groups[2].Value;
                    trade.When = innerMatch.Groups[3].Value;
                }

                trades.Add(trade);

                match = match.NextMatch();
            }

            if (Session.Confirmations != null)
            {
                lock (Session.Confirmations)
                {
                    if (Session.Confirmations.Ids == null)
                    {
                        Session.Confirmations.Ids = new List<string>();
                    }
                    foreach (var conf in trades)
                    {
                        conf.IsNew = (Session.Confirmations.Ids.Contains(conf.Id) == false);
                        if (conf.IsNew == true)
                        {
                            Session.Confirmations.Ids.Add(conf.Id);
                        }
                    }
                    var newIds = trades.Select(t => t.Id).ToList();
                    foreach (var confId in Session.Confirmations.Ids.ToList())
                    {
                        if (newIds.Contains(confId) == false)
                        {
                            Session.Confirmations.Ids.Remove(confId);
                        }
                    }
                }
            }

            return trades;
        }

        /// <summary>
        /// Get details for an individual Confirmation
        /// </summary>
        /// <param name="trade">trade Confirmation</param>
        /// <returns>html string of details</returns>
        //[Obsolete("use GetConfirmationDetailsAsync")]
        public string GetConfirmationDetails(Confirmation trade)
        {
            // build details URL
            string url = COMMUNITY_BASE + "/mobileconf/details/" + trade.Id + "?" + ConfirmationsQuery;

            string response = GetString(url);
            if (response.IndexOf("success") == -1)
            {
                throw new WinAuthInvalidSteamRequestException("Invalid request from steam: " + response);
            }
            if (JObject.Parse(response).SelectToken("success").Value<bool>() == true)
            {
                string html = JObject.Parse(response).SelectToken("html").Value<string>();

                Regex detailsRegex = new Regex(@"(.*<body[^>]*>\s*<div\s+class=""[^""]+"">).*(</div>.*?</body>\s*</html>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var match = detailsRegex.Match(ConfirmationsHtml);
                if (match.Success == true)
                {
                    return match.Groups[1].Value + html + match.Groups[2].Value;
                }
            }

            return "<html><head></head><body><p>Cannot load trade confirmation details</p></body></html>";
        }

        /// <summary>
        /// Confirm or reject a specific trade confirmation
        /// </summary>
        /// <param name="id">id of trade</param>
        /// <param name="key">key for trade</param>
        /// <param name="accept">true to accept, false to reject</param>
        /// <returns>true if successful</returns>
        //[Obsolete("use ConfirmTradeAsync")]
        public bool ConfirmTrade(string id, string key, bool accept)
        {
            if (string.IsNullOrEmpty(Session.OAuthToken) == true)
            {
                return false;
            }

            long servertime = (SteamAuthenticator.CurrentTime + Authenticator.ServerTimeDiff) / 1000L;

            var jids = JObject.Parse(Authenticator.SteamData).SelectToken("identity_secret");
            string ids = (jids != null ? jids.Value<string>() : string.Empty);
            var timehash = CreateTimeHash(servertime, "conf", ids);

            var data = new NameValueCollection()
            {
                { "op", accept ? "allow" : "cancel" },
                { "p", Authenticator.DeviceId },
                { "a", Session.SteamId },
                { "k", timehash },
                { "t", servertime.ToString() },
                { "m", "android" },
                { "tag", "conf" },
                //
                { "cid", id },
                { "ck", key }
            };

            try
            {
                string response = GetString(COMMUNITY_BASE + "/mobileconf/ajaxop", "GET", data);
                if (string.IsNullOrEmpty(response) == true)
                {
                    Error = "Blank response";
                    return false;
                }

                var success = JObject.Parse(response).SelectToken("success");
                if (success == null || success.Value<bool>() == false)
                {
                    Error = "Failed";
                    return false;
                }

                if (Session.Confirmations != null)
                {
                    lock (Session.Confirmations)
                    {
                        if (Session.Confirmations.Ids.Contains(id) == true)
                        {
                            Session.Confirmations.Ids.Remove(id);
                        }
                    }
                }

                return true;
            }
            catch (WinAuthInvalidSteamRequestException ex)
            {
#if DEBUG
                Error = ex.Message + Environment.NewLine + ex.StackTrace;
#else
                this.Error = ex.Message;
#endif
                return false;
            }
        }

        /// <summary>
        /// Create the hash needed for the confirmations query string
        /// </summary>
        /// <param name="time">current time</param>
        /// <param name="tag">tag</param>
        /// <param name="secret">identity secret</param>
        /// <returns>hash string</returns>
        static string CreateTimeHash(long time, string tag, string secret)
        {
            byte[] b64secret = Convert.FromBase64String(secret);

            int bufferSize = 8;
            if (string.IsNullOrEmpty(tag) == false)
            {
                bufferSize += Math.Min(32, tag.Length);
            }
            byte[] buffer = new byte[bufferSize];

            byte[] timeArray = BitConverter.GetBytes(time);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timeArray);
            }
            Array.Copy(timeArray, buffer, 8);
            if (string.IsNullOrEmpty(tag) == false)
            {
                Array.Copy(Encoding.UTF8.GetBytes(tag), 0, buffer, 8, bufferSize - 8);
            }

            HMACSHA1 hmac = new HMACSHA1(b64secret, true);
            byte[] hash = hmac.ComputeHash(buffer);

            return Convert.ToBase64String(hash, Base64FormattingOptions.None);
        }

        #region Web Request

        /// <summary>
        /// Get binary data web request
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="method">GET or POST</param>
        /// <param name="formdata">optional form data</param>
        /// <param name="headers">optional headers</param>
        /// <returns>array of returned data</returns>
        //[Obsolete("use SendAsync")]
        public byte[] GetData(string url, string? method = null, NameValueCollection? formdata = null, NameValueCollection? headers = null)
        {
            return Request(url, method ?? "GET", formdata, headers);
        }

        /// <summary>
        /// Get string from web request
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="method">GET or POST</param>
        /// <param name="formdata">optional form data</param>
        /// <param name="headers">optional headers</param>
        /// <returns>string of returned data</returns>
        //[Obsolete("use SendAsync")]
        public string GetString(string url, string method = null, NameValueCollection formdata = null, NameValueCollection headers = null)
        {
            byte[] data = Request(url, method ?? "GET", formdata, headers);
            if (data == null || data.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                return Encoding.UTF8.GetString(data);
            }
        }

        /// <summary>
        /// Make a request to Steam URL
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="method">GET or POST</param>
        /// <param name="formdata">optional form data</param>
        /// <param name="headers">optional headers</param>
        /// <returns>returned data</returns>
        //[Obsolete("use SendAsync")]
        protected byte[]? Request(string url, string method, NameValueCollection? data, NameValueCollection? headers)
        {
            byte[]? responsedata;
            //try
            //{
            //    responsedata = Request(url, method, data, headers, true);
            //}
            //catch (Exception e)
            //{
            //    Log.Error(nameof(WinAuthSteamClient), e, "Forward Request Fail.");
            //}

            responsedata = Request(url, method, data, headers, false);

            return responsedata;
        }

        //[Obsolete("use SendAsync")]
        byte[]? Request(string url, string method, NameValueCollection? data, NameValueCollection? headers, bool enableForward)
        {
            // ensure only one request per account at a time
            lock (this)
            {
                // create form-encoded data for query or body
                string query = (data == null ? string.Empty : string.Join("&", Array.ConvertAll(data.AllKeys, key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key])))));
                if (string.Compare(method, "GET", true) == 0)
                {
                    url += (!url.Contains("?", StringComparison.CurrentCulture) ? "?" : "&") + query;
                }

                var isForward = enableForward && TryGetForwardUrl(ref url);

                // call the server
                HttpWebRequest request = WebRequest.CreateHttp(url);
                request.Timeout = GeneralHttpClientFactory.DefaultTimeoutTotalMilliseconds;
                request.Method = method;
                request.Accept = "text/javascript, text/html, application/xml, text/xml, */*";
                request.ServicePoint.Expect100Continue = false;
                request.UserAgent = USERAGENT;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                request.Referer = COMMUNITY_BASE;
                if (headers != null)
                {
                    request.Headers.Add(headers);
                }

                if (isForward) AppendForwardHeaders(request.Headers);

                request.CookieContainer = Session.Cookies;

                if (string.Compare(method, "POST", true) == 0)
                {
                    request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    request.ContentLength = query.Length;

                    StreamWriter requestStream = new(request.GetRequestStream());
                    requestStream.Write(query);
                    requestStream.Close();
                }

                try
                {
                    using HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    LogRequest(method, url, request.CookieContainer, data, response.StatusCode.ToString() + " " + response.StatusDescription);

                    // OK?
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WinAuthInvalidSteamRequestException(string.Format("{0}: {1}", (int)response.StatusCode, response.StatusDescription));
                    }

                    // load the response
                    using MemoryStream ms = new();
                    byte[] buffer = new byte[4096];
                    int read;
                    while ((read = response.GetResponseStream().Read(buffer, 0, 4096)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    byte[] responsedata = ms.ToArray();

                    LogRequest(method, url, request.CookieContainer, data, responsedata != null && responsedata.Length != 0 ? Encoding.UTF8.GetString(responsedata) : string.Empty);

                    return responsedata;
                }
                catch (Exception ex)
                {
                    LogException(method, url, request.CookieContainer, data, ex);

                    if (ex is WebException exception &&
                        exception.Response is HttpWebResponse response &&
                        response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new WinAuthUnauthorisedSteamRequestException(ex);
                    }

                    if (ex is WinAuthInvalidSteamRequestException) throw;
                    else throw new WinAuthInvalidSteamRequestException(ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Log an exception from a Request
        /// </summary>
        /// <param name="method">Get or POST</param>
        /// <param name="url">Request URL</param>
        /// <param name="cookies">cookie container</param>
        /// <param name="request">Request data</param>
        /// <param name="ex">Thrown exception</param>
        [Obsolete("use LogException(string, string, CookieContainer?, IReadOnlyDictionary{string, string}?, Exception)")]
        static void LogException(string method, string url, CookieContainer? cookies, NameValueCollection? request, Exception ex)
        {
            return;

            //StringBuilder data = new StringBuilder();
            //if (cookies != null)
            //{
            //    foreach (Cookie cookie in cookies.GetCookies(new Uri(url)))
            //    {
            //        if (data.Length == 0)
            //        {
            //            data.Append("Cookies:");
            //        }
            //        else
            //        {
            //            data.Append("&");
            //        }
            //        data.Append(cookie.Name + "=" + cookie.Value);
            //    }
            //    data.Append(" ");
            //}

            //if (request != null)
            //{
            //    foreach (var key in request.AllKeys)
            //    {
            //        if (data.Length == 0)
            //        {
            //            data.Append("Req:");
            //        }
            //        else
            //        {
            //            data.Append("&");
            //        }
            //        data.Append(key + "=" + request[key]);
            //    }
            //    data.Append(" ");
            //}
        }

        /// <summary>
        /// Log a normal response
        /// </summary>
        /// <param name="method">Get or POST</param>
        /// <param name="url">Request URL</param>
        /// <param name="cookies">cookie container</param>
        /// <param name="request">Request data</param>
        /// <param name="response">response body</param>
        [Obsolete("use LogRequest(string, string, CookieContainer?, IReadOnlyDictionary{string, string}?, string)")]
        static void LogRequest(string method, string url, CookieContainer? cookies, NameValueCollection? request, string response)
        {
            return;

            //StringBuilder data = new StringBuilder();
            //if (cookies != null)
            //{
            //    foreach (Cookie cookie in cookies.GetCookies(new Uri(url)))
            //    {
            //        if (data.Length == 0)
            //        {
            //            data.Append("Cookies:");
            //        }
            //        else
            //        {
            //            data.Append("&");
            //        }
            //        data.Append(cookie.Name + "=" + cookie.Value);
            //    }
            //    data.Append(" ");
            //}

            //if (request != null)
            //{
            //    foreach (var key in request.AllKeys)
            //    {
            //        if (data.Length == 0)
            //        {
            //            data.Append("Req:");
            //        }
            //        else
            //        {
            //            data.Append("&");
            //        }
            //        data.Append(key + "=" + request[key]);
            //    }
            //    data.Append(" ");
            //}
        }

        #endregion

        /// <summary>
        /// Convert a hex string into a byte array. E.g. "001f406a" -> byte[] {0x00, 0x1f, 0x40, 0x6a}
        /// </summary>
        /// <param name="hex">hex string to convert</param>
        /// <returns>byte[] of hex string</returns>
        static byte[] StringToByteArray(string hex)
        {
            int len = hex.Length;
            byte[] bytes = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        static string SelectTokenValueStr(JObject obj, string path)
        {
            var value = obj.SelectToken(path)?.Value<string>();
            return value.ThrowIsNull(path);
        }

        static byte[] StringToByteArray(JObject obj, string path)
        {
            return StringToByteArray(SelectTokenValueStr(obj, path));
        }
    }
}