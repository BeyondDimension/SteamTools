using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using BD.SteamClient.Models;
using BD.SteamClient.Services;
using System.Security.Cryptography;
using WinAuth;
using JsonException = System.Text.Json.JsonException;

namespace BD.WTTS.UI.ViewModels;

/// <summary>Reads the authenticator repository only after its existing protection is unlocked.</summary>
internal sealed class AuthenticatorSavedSteamAccounts : ISteamSavedAccountProvider
{
    static readonly Dictionary<ushort, WeakReference<SteamAuthenticator>> liveAuthenticators = new();
    static readonly Uri community = new("https://steamcommunity.com/");
    static readonly Uri communityTransfer = new(community, "login/settoken");
    static readonly Uri communityProfile = new(community, "my/");

    enum TokenPlatform { Web, App }

    internal static void TrackLiveAuthenticator(ushort id, SteamAuthenticator authenticator)
    {
        lock (liveAuthenticators)
            liveAuthenticators[id] = new(authenticator);
    }

    internal static void UntrackLiveAuthenticator(ushort id, SteamAuthenticator authenticator)
    {
        lock (liveAuthenticators)
        {
            if (liveAuthenticators.TryGetValue(id, out var reference) &&
                reference.TryGetTarget(out var current) && ReferenceEquals(current, authenticator))
                liveAuthenticators.Remove(id);
        }
    }

    static SteamAuthenticator? FindLiveAuthenticator(ushort id, ulong steamId)
    {
        lock (liveAuthenticators)
        {
            if (!liveAuthenticators.TryGetValue(id, out var reference)) return null;
            if (!reference.TryGetTarget(out var authenticator))
            {
                liveAuthenticators.Remove(id);
                return null;
            }
            return authenticator.SteamId64 is > 0 && (ulong)authenticator.SteamId64.Value == steamId ? authenticator : null;
        }
    }

    internal static WinAuth.SteamClient.SteamSession? FindUsableSession(ushort id, SteamAuthenticator stored,
        out SteamAuthenticator? liveAuthenticator)
    {
        liveAuthenticator = null;
        if (stored.SteamId64 is not > 0) return null;
        var steamId = (ulong)stored.SteamId64.Value;
        var live = FindLiveAuthenticator(id, steamId);
        if (live != null)
        {
            var session = live.Client.Session;
            if (!IsSessionUsable(session, steamId) && !string.IsNullOrWhiteSpace(live.SessionData))
                session = live.GetClient().Session;
            if (!IsSessionUsable(session, steamId)) return null;
            liveAuthenticator = live;
            return session;
        }
        if (string.IsNullOrWhiteSpace(stored.SessionData)) return null;
        var saved = new WinAuth.SteamClient.SteamSession(stored.SessionData);
        return IsSessionUsable(saved, steamId) ? saved : null;
    }

    internal static bool IsLiveSessionCurrent(ushort id, SteamAuthenticator authenticator, WinAuth.SteamClient.SteamSession session,
        string? access, string? refresh)
        => ReferenceEquals(FindLiveAuthenticator(id, session.SteamID), authenticator) &&
           ReferenceEquals(authenticator.Client.Session, session) && session.AccessToken == access && session.RefreshToken == refresh;

    static bool IsSessionUsable(WinAuth.SteamClient.SteamSession? session, ulong steamId)
        => session != null && session.SteamID == steamId &&
            (GetUsableTokenPlatform(session.AccessToken, steamId, refresh: false) != null ||
             GetUsableTokenPlatform(session.RefreshToken, steamId, refresh: true) != null);

    internal static bool IsBrowserRefreshToken(string? token, ulong steamId)
        => GetUsableTokenPlatform(token, steamId, refresh: true) == TokenPlatform.Web;

    public async Task<IReadOnlyList<SteamSavedLoginAccount>> LoadAsync(CancellationToken cancellationToken)
    {
        var sources = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
        cancellationToken.ThrowIfCancellationRequested();
        var vault = await AuthenticatorHelper.ReadAuthorizedVaultAsync(sources, cancellationToken);
        if (vault == null)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text073);
        var entries = vault.Value.items;
        cancellationToken.ThrowIfCancellationRequested();
        var result = new List<SteamSavedLoginAccount>();
        foreach (var entry in entries)
        {
            if (entry.Value is not SteamAuthenticator auth || auth.SteamId64 is not > 0) continue;
            var id = (ulong)auth.SteamId64.Value;
            var saved = FindUsableSession(entry.Id, auth, out var liveAuthenticator);
            if (saved == null) continue;
            var source = sources.FirstOrDefault(x => x.Id == entry.Id);
            if (source?.Value == null) continue;
            var sourceId = source.Id;
            var encryptedValue = source.Value;
            var access = saved.AccessToken;
            var refresh = saved.RefreshToken;
            var name = string.IsNullOrWhiteSpace(auth.AccountName) ? entry.Name : auth.AccountName;
            result.Add(new SteamSavedLoginAccount(name ?? "Steam", id, async token =>
            {
                var current = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
                token.ThrowIfCancellationRequested();
                var item = current.FirstOrDefault(x => x.Id == sourceId);
                if (item?.Value == null || !item.Value.SequenceEqual(encryptedValue))
                    throw new InvalidOperationException(Strings.L10nAuthIdle_Text075);
                if (liveAuthenticator != null && !IsLiveSessionCurrent(sourceId, liveAuthenticator, saved, access, refresh))
                    throw new InvalidOperationException(Strings.L10nAuthIdle_Text076);
                var webSession = await CreateWebSessionAsync(id, access, refresh, Ioc.Get<ISteamAccountService>(), token);
                current = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
                token.ThrowIfCancellationRequested();
                item = current.FirstOrDefault(x => x.Id == sourceId);
                if (item?.Value == null || !item.Value.SequenceEqual(encryptedValue))
                    throw new InvalidOperationException(Strings.L10nAuthIdle_Text075);
                if (liveAuthenticator != null && !IsLiveSessionCurrent(sourceId, liveAuthenticator, saved, access, refresh))
                    throw new InvalidOperationException(Strings.L10nAuthIdle_Text076);
                return webSession;
            }));
        }
        return result;
    }

    static TokenPlatform? GetUsableTokenPlatform(string? token, ulong steamId, bool refresh)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3 || parts[0].Length == 0 || parts[2].Length == 0) return null;
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = doc.RootElement;
            if (root.GetProperty("sub").GetString() != steamId.ToString(CultureInfo.InvariantCulture) ||
                root.GetProperty("exp").GetInt64() <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;
            var aud = root.GetProperty("aud");
            if (aud.ValueKind != JsonValueKind.Array) return null;
            var mobile = false;
            var web = false;
            var derive = false;
            foreach (var audience in aud.EnumerateArray())
            {
                mobile |= audience.GetString() == "mobile";
                web |= audience.GetString() == "web";
                derive |= audience.GetString() == "derive";
            }
            // Local suitability check only; the community request authenticates the token with Steam.
            if (derive != refresh || (!web && !(refresh && mobile))) return null;
            return mobile ? TokenPlatform.App : TokenPlatform.Web;
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException or KeyNotFoundException or OverflowException) { return null; }
    }

    internal static async Task<SteamSession> CreateWebSessionAsync(ulong steamId, string? accessToken, string? refreshToken,
        ISteamAccountService accountService, CancellationToken cancellationToken, Func<CookieContainer, HttpMessageHandler>? handlerFactory = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var accessPlatform = GetUsableTokenPlatform(accessToken, steamId, refresh: false);
        var refreshPlatform = GetUsableTokenPlatform(refreshToken, steamId, refresh: true);
        if (accessPlatform == null && refreshPlatform == null)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text077);

        if (accessPlatform == null && refreshPlatform == TokenPlatform.App)
        {
            accessToken = await accountService.RefreshAccessToken(steamId, refreshToken!).WaitAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            accessPlatform = GetUsableTokenPlatform(accessToken, steamId, refresh: false);
            if (accessPlatform != TokenPlatform.App)
                throw new InvalidOperationException(Strings.L10nAuthIdle_Text078);
        }

        var expectedId = steamId.ToString(CultureInfo.InvariantCulture);
        var sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
        if (accessPlatform != null)
        {
            var cookies = CreateAccessCookies(expectedId, accessToken!, sessionId);
            using var client = CreateWebClient(cookies, handlerFactory);
            if (await VerifyCommunityAsync(client, expectedId, cancellationToken))
                return new SteamSession { SteamId = expectedId, AccessToken = accessToken!, CookieContainer = cookies };
        }

        // Browser refresh tokens use the website's cookie transfer. The app endpoint rejects them.
        if (refreshPlatform != TokenPlatform.Web || accessPlatform == TokenPlatform.App)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text079);

        var browserCookies = new CookieContainer();
        browserCookies.Add(new Cookie("sessionid", sessionId, "/", community.Host) { Secure = true });
        using var browser = CreateWebClient(browserCookies, handlerFactory);
        accessToken = await RenewBrowserSessionAsync(browser, browserCookies, steamId, expectedId, refreshToken!, sessionId, cancellationToken);
        if (!await VerifyCommunityAsync(browser, expectedId, cancellationToken))
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text079);

        // Keep only the derived access cookies. Finalization can set a long-lived login-site refresh cookie.
        return new SteamSession
        {
            SteamId = expectedId,
            AccessToken = accessToken,
            CookieContainer = CreateAccessCookies(expectedId, accessToken, sessionId),
        };
    }

    static CookieContainer CreateAccessCookies(string steamId, string accessToken, string sessionId)
    {
        var secure = Uri.EscapeDataString($"{steamId}||{accessToken}");
        var cookies = new CookieContainer();
        foreach (var domain in new[] { community.Host, "steampowered.com" })
        {
            cookies.Add(new Cookie("steamLoginSecure", secure, "/", domain) { Secure = true, HttpOnly = true });
            cookies.Add(new Cookie("sessionid", sessionId, "/", domain) { Secure = true });
        }
        return cookies;
    }

    static HttpClient CreateWebClient(CookieContainer cookies, Func<CookieContainer, HttpMessageHandler>? handlerFactory)
        => new(handlerFactory?.Invoke(cookies) ?? new HttpClientHandler
        {
            CookieContainer = cookies, Proxy = HttpClient.DefaultProxy, AllowAutoRedirect = false,
        });

    static async Task<bool> VerifyCommunityAsync(HttpClient client, string expectedId, CancellationToken cancellationToken)
    {
        using var check = await client.GetAsync(communityProfile, cancellationToken);
        var location = check.Headers.Location;
        var path = location?.IsAbsoluteUri == true ? location.AbsolutePath : location?.OriginalString;
        cancellationToken.ThrowIfCancellationRequested();
        return (int)check.StatusCode is >= 300 and < 400 && path != null &&
            (!location!.IsAbsoluteUri || (location.Scheme == Uri.UriSchemeHttps && location.Host == community.Host && location.IsDefaultPort)) &&
            (path.StartsWith("/id/", StringComparison.OrdinalIgnoreCase) ||
             path == "/profiles/" + expectedId || path.StartsWith("/profiles/" + expectedId + "/", StringComparison.Ordinal));
    }

    static async Task<string> RenewBrowserSessionAsync(HttpClient client, CookieContainer cookies, ulong steamId,
        string expectedId, string refreshToken, string sessionId, CancellationToken cancellationToken)
    {
        using var finalize = new HttpRequestMessage(HttpMethod.Post, "https://login.steampowered.com/jwt/finalizelogin")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["nonce"] = refreshToken,
                ["sessionid"] = sessionId,
                ["redir"] = "https://steamcommunity.com/login/home/?goto=",
            }),
        };
        finalize.Headers.TryAddWithoutValidation("Origin", community.GetLeftPart(UriPartial.Authority));
        finalize.Headers.Referrer = community;
        using var response = await client.SendAsync(finalize, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text078);

        var status = await response.Content.ReadFromJsonAsync(FinalizeLoginStatus_.Default.FinalizeLoginStatus, cancellationToken);
        if (status == null || status.SteamId != expectedId)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text079);

        // Send the transfer details to the fixed community endpoint only; ignore other Steam products.
        var transfer = status.TransferInfo?.FirstOrDefault(item =>
            Uri.TryCreate(item?.Url, UriKind.Absolute, out var url) && url == communityTransfer &&
            url.UserInfo.Length == 0 && url.Fragment.Length == 0)?.Params;
        if (transfer == null || string.IsNullOrWhiteSpace(transfer.Nonce) || string.IsNullOrWhiteSpace(transfer.Auth))
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text079);

        using var transferForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["nonce"] = transfer.Nonce,
            ["auth"] = transfer.Auth,
            ["steamID"] = expectedId,
        });
        using var transferred = await client.PostAsync(communityTransfer, transferForm, cancellationToken);
        var signedCookie = cookies.GetCookies(community)["steamLoginSecure"]?.Value;
        var signedValue = signedCookie == null ? null : Uri.UnescapeDataString(signedCookie);
        var prefix = expectedId + "||";
        if (!transferred.IsSuccessStatusCode || signedValue == null || !signedValue.StartsWith(prefix, StringComparison.Ordinal))
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text079);

        var accessToken = signedValue[prefix.Length..];
        if (GetUsableTokenPlatform(accessToken, steamId, refresh: false) == null)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text078);
        return accessToken;
    }
}
