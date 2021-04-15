using Newtonsoft.Json.Linq;
using System;
using System.Application.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static System.Application.Models.GAPAuthenticatorValueDTO;

namespace WinAuth
{
    partial class WinAuthSteamClient
    {
        #region HttpClient

        protected async Task<T?> SendAsync<T>(string url, HttpMethod method, IReadOnlyDictionary<string, string?>? data, IReadOnlyDictionary<string, string?>? headers, CancellationToken cancellationToken, bool enableForward = true) where T : notnull
        {
            var query = data == null ? string.Empty : string.Join("&", data.Select(x => string.Format("{0}={1}", HttpUtility.UrlEncode(x.Key), HttpUtility.UrlEncode(x.Value))));
            if (method == HttpMethod.Get)
            {
                url += (!url.Contains("?", StringComparison.CurrentCulture) ? "?" : "&") + query;
            }

            var request = new HttpRequestMessage(method, url);
            var requestUri = request.RequestUri.ThrowIsNull(nameof(request.RequestUri));
            request.Headers.Accept.ParseAdd("text/javascript, text/html, application/xml, text/xml, */*");
            request.Headers.UserAgent.ParseAdd(USERAGENT);
            request.Headers.Referrer = new Uri(COMMUNITY_BASE);
            if (headers != null)
            {
                foreach (var item in headers.Keys)
                {
                    if (item == null) continue;
                    request.Headers.Add(item, headers[item]);
                }
            }

            var cookieHeader = Session.Cookies.GetCookieHeader(requestUri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }

            if (method == HttpMethod.Post)
            {
                var requestStream = new StreamWriter(new MemoryStream());
                requestStream.Write(query);
                request.Content = new StreamContent(requestStream.BaseStream);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded; charset=UTF-8");
                request.Content.Headers.ContentLength = query.Length;
            }

            HttpStatusCode? statusCode = null;
            try
            {
                var response = await IHttpService.Instance.SendAsync<T>(url, request, null, enableForward, cancellationToken,
                    handlerResponse: rsp =>
                    {
                        statusCode = rsp.StatusCode;
                        if (rsp.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
                        {
                            var setCookieValue = setCookieValues?.ToString() ?? string.Empty;
                            Session.Cookies.SetCookies(requestUri, setCookieValue);
                        }
                    },
                    handlerResponseByIsNotSuccessStatusCode: rsp =>
                    {
                        throw new WinAuthInvalidSteamRequestException(string.Format("{0}: {1}", (int)rsp.StatusCode, rsp.StatusCode.ToString()));
                    });

                LogRequest(method.ToString(), url, Session.Cookies, data,
                    statusCode.HasValue ? $"{(int)statusCode.Value} {statusCode.Value}" : string.Empty);

                return response;
            }
            catch (Exception ex)
            {
                LogException(method.ToString(), url, Session.Cookies, data, ex);

                if (statusCode == HttpStatusCode.Forbidden)
                {
                    throw new WinAuthUnauthorisedSteamRequestException(ex);
                }

                if (ex is WinAuthInvalidSteamRequestException) throw;
                else throw new WinAuthInvalidSteamRequestException(ex.Message, ex);
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
        static void LogException(string method, string url, CookieContainer? cookies, IReadOnlyDictionary<string, string?>? request, Exception ex)
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
            //    foreach (var key in request.Keys)
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
        static void LogRequest(string method, string url, CookieContainer? cookies, IReadOnlyDictionary<string, string?>? request, string response)
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
            //    foreach (var key in request.Keys)
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
        /// Login to Steam using credentials and optional captcha
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="captchaId"></param>
        /// <param name="captchaText"></param>
        /// <param name="language"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> LoginAsync(string username, string password, string? captchaId = null, string? captchaText = null, string? language = null, CancellationToken cancellationToken = default)
        {
            // clear error
            Error = null;

            var data = new Dictionary<string, string?>();
            string? response;

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

                    var headers = new Dictionary<string, string?>
                    {
                        { "X-Requested-With", "com.valvesoftware.android.steam.community" }
                    };

                    response = await SendAsync<string>(COMMUNITY_BASE + "/mobilelogin?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client", HttpMethod.Get, null, headers, cancellationToken);
                }

                // Steam strips any non-ascii chars from username and password
                username = Regex.Replace(username, @"[^\u0000-\u007F]", string.Empty);
                password = Regex.Replace(password, @"[^\u0000-\u007F]", string.Empty);

                // get the user's RSA key
                data.Add("username", username);
                response = await SendAsync<string>(COMMUNITY_BASE + "/mobilelogin/getrsakey", HttpMethod.Post, data, null, cancellationToken);
                var rsaresponse = response == null ? null : JObject.Parse(response);
                if (rsaresponse?.SelectToken("success")?.Value<bool>() != true)
                {
                    InvalidLogin = true;
                    Error = "Unknown username";
                    return false;
                }

                // encrypt password with RSA key
                //RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
                string encryptedPassword64;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var passwordBytes = Encoding.ASCII.GetBytes(password);
                    var p = rsa.ExportParameters(false);
                    p.Exponent = StringToByteArray(rsaresponse, "publickey_exp");
                    p.Modulus = StringToByteArray(rsaresponse, "publickey_mod");
                    rsa.ImportParameters(p);
                    byte[] encryptedPassword = rsa.Encrypt(passwordBytes, false);
                    encryptedPassword64 = Convert.ToBase64String(encryptedPassword);
                }

                // login request
                data = new Dictionary<string, string?>
                {
                    { "password", encryptedPassword64 },
                    { "username", username },
                    { "twofactorcode", Authenticator.CurrentCode },
                    //data.Add("emailauth", string.Empty);
                    { "loginfriendlyname", "#login_emailauth_friendlyname_mobile" },
                    { "captchagid", (string.IsNullOrEmpty(captchaId) == false ? captchaId : "-1") },
                    { "captcha_text", (string.IsNullOrEmpty(captchaText) == false ? captchaText : "enter above characters") },
                    //data.Add("emailsteamid", (string.IsNullOrEmpty(emailcode) == false ? this.SteamId ?? string.Empty : string.Empty));
                    { "rsatimestamp", SelectTokenValueStr(rsaresponse,"timestamp") },
                    { "remember_login", "false" },
                    { "oauth_client_id", "DE45CD61" },
                    { "oauth_scope", "read_profile write_profile read_client write_client" },
                    { "donotache", new DateTime().ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds.ToString() }
                };
                var loginresponse = await SendAsync<Dictionary<string, object>>(COMMUNITY_BASE + "/mobilelogin/dologin/", HttpMethod.Post, data, null, cancellationToken);

                loginresponse = loginresponse.ThrowIsNull(nameof(loginresponse));

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
                Session.OAuthToken = SelectTokenValueStr(oauthjson, "oauth_token");
                if (oauthjson.SelectToken("steamid") != null)
                {
                    Session.SteamId = SelectTokenValueStr(oauthjson, "steamid");
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(Session.OAuthToken) == false)
            {
                PollConfirmationsStop();

                if (string.IsNullOrEmpty(Session.UmqId) == false)
                {
                    var data = new Dictionary<string, string?>
                    {
                        { "access_token", Session.OAuthToken },
                        { "umqid", Session.UmqId }
                    };
                    await SendAsync<string>(API_LOGOFF, HttpMethod.Post, data, null, cancellationToken);
                }
            }

            Clear();
        }

        /// <summary>
        /// Refresh the login session cookies from the OAuth code
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>true if successful</returns>
        public async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var data = new Dictionary<string, string?>
                {
                    { "access_token", Session.OAuthToken }
                };
                var response = await SendAsync<string>(API_GETWGTOKEN, HttpMethod.Post, data, null, cancellationToken);
                if (string.IsNullOrEmpty(response))
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<bool> UmqLoginAsync(CancellationToken cancellationToken = default)
        {
            if (IsLoggedIn() == false)
            {
                return false;
            }

            var data = new Dictionary<string, string?>
            {
                { "access_token", Session.OAuthToken }
            };
            var loginresponse = await SendAsync<Dictionary<string, object>>(API_LOGON, HttpMethod.Post, data, null, cancellationToken);
            loginresponse = loginresponse.ThrowIsNull(nameof(loginresponse));
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
        /// Get the current trade Confirmations
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>list of Confirmation objects</returns>
        public async Task<List<Confirmation>> GetConfirmationsAsync(CancellationToken cancellationToken = default)
        {
            long servertime = (CurrentTime + Authenticator.ServerTimeDiff) / 1000L;

            var authSteamData = Authenticator.SteamData;
            authSteamData = authSteamData.ThrowIsNull(nameof(authSteamData));

            var jids = JObject.Parse(authSteamData).SelectToken("identity_secret");
            var ids = jids?.Value<string>() ?? string.Empty;

            var timehash = CreateTimeHash(servertime, "conf", ids);

            var data = new Dictionary<string, string?>
            {
                { "p", Authenticator.DeviceId },
                { "a", Session.SteamId },
                { "k", timehash },
                { "t", servertime.ToString() },
                { "m", "android" },
                { "tag", "conf" }
            };

            var html = await SendAsync<string>(COMMUNITY_BASE + "/mobileconf/conf", HttpMethod.Get, data, null, cancellationToken);

            html = html.ThrowIsNull(nameof(html));

            // save last html for confirmations details
            ConfirmationsHtml = html;
            ConfirmationsQuery = string.Join("&", data.Select(x => string.Format("{0}={1}", HttpUtility.UrlEncode(x.Key), HttpUtility.UrlEncode(x.Value))));

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
                    foreach (var confId in Session.Confirmations.Ids)
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
        /// <param name="cancellationToken"></param>
        /// <returns>html string of details</returns>
        public async Task<string> GetConfirmationDetailsAsync(Confirmation trade, CancellationToken cancellationToken = default)
        {
            // build details URL
            string url = COMMUNITY_BASE + "/mobileconf/details/" + trade.Id + "?" + ConfirmationsQuery;

            var response = await SendAsync<string>(url, HttpMethod.Get, null, null, cancellationToken);
            response = response.ThrowIsNull(nameof(response));
            if (!response.Contains("success", StringComparison.CurrentCulture))
            {
                throw new WinAuthInvalidSteamRequestException("Invalid request from steam: " + response);
            }

            var jObj = JObject.Parse(response);
            if (jObj?.SelectToken("success")?.Value<bool>() == true)
            {
                var html = jObj?.SelectToken("html")?.Value<string>();
                html = html.ThrowIsNull(nameof(html));

                Regex detailsRegex = new Regex(@"(.*<body[^>]*>\s*<div\s+class=""[^""]+"">).*(</div>.*?</body>\s*</html>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var match = detailsRegex.Match(ConfirmationsHtml.ThrowIsNull(nameof(ConfirmationsHtml)));
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
        /// <param name="cancellationToken"></param>
        /// <returns>true if successful</returns>
        public async Task<bool> ConfirmTradeAsync(string id, string key, bool accept, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(Session.OAuthToken))
            {
                return false;
            }

            long servertime = (CurrentTime + Authenticator.ServerTimeDiff) / 1000L;

            var authSteamData = Authenticator.SteamData;
            authSteamData = authSteamData.ThrowIsNull(nameof(authSteamData));

            var jids = JObject.Parse(authSteamData).SelectToken("identity_secret");
            var ids = jids?.Value<string>() ?? string.Empty;

            var timehash = CreateTimeHash(servertime, "conf", ids);

            var data = new Dictionary<string, string?>
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
                var response = await SendAsync<string>(COMMUNITY_BASE + "/mobileconf/ajaxop", HttpMethod.Get, data, null, cancellationToken);
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
                        if (Session.Confirmations.Ids?.Contains(id) == true)
                        {
                            Session.Confirmations.Ids.Remove(id);
                        }
                    }
                }

                return true;
            }
            catch (WinAuthInvalidSteamRequestException ex)
            {
                Error = ex.Message
#if DEBUG
                    + Environment.NewLine + ex.StackTrace
#endif
                    ;
                return false;
            }
        }
    }
}