using System.Net;
using System.Net.Http;
using System.Text.Json;
using BD.SteamClient.Models;
using BD.SteamClient.Services;
using System.Security.Cryptography;
using WinAuth;
using AppResources = BD.WTTS.Client.Resources.Strings;
using JsonException = System.Text.Json.JsonException;

namespace BD.WTTS.UI.ViewModels;

/// <summary>Reads the authenticator repository only after its existing protection is unlocked.</summary>
internal sealed class AuthenticatorSavedSteamAccounts : ISteamSavedAccountProvider
{
    static readonly Dictionary<ushort, WeakReference<SteamAuthenticator>> liveAuthenticators = new();

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
            (IsTokenUsable(session.AccessToken, steamId, refresh: false) || IsTokenUsable(session.RefreshToken, steamId, refresh: true));

    public async Task<IReadOnlyList<SteamSavedLoginAccount>> LoadAsync(CancellationToken cancellationToken)
    {
        var sources = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
        cancellationToken.ThrowIfCancellationRequested();
        var protectedSource = sources.FirstOrDefault(x => x.IsNeedSecondaryPassword);
        string? password = null;
        if (protectedSource != null)
        {
            var prompt = new TextBoxWindowViewModel { InputType = TextBoxWindowViewModel.TextBoxInputType.Password };
            if (!await IWindowManager.Instance.ShowTaskDialogAsync(prompt, AppResources.Title_InputAuthPassword, isDialog: false, isCancelButton: true))
                throw new InvalidOperationException("已取消解锁令牌；可以返回其他登录方式或重新读取令牌。");
            cancellationToken.ThrowIfCancellationRequested();
            password = prompt.Value;
            prompt.Value = null;
            if (string.IsNullOrEmpty(password) || !await AuthenticatorHelper.ValidatePassword(protectedSource, password))
                throw new InvalidOperationException("令牌二级密码不正确，请重新读取并解锁。");
        }
        var entries = await AuthenticatorHelper.GetAllAuthenticatorsAsync(sources, password);
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
                    throw new InvalidOperationException("令牌登录状态已更改，请重新读取令牌后重试。");
                if (liveAuthenticator != null && !IsLiveSessionCurrent(sourceId, liveAuthenticator, saved, access, refresh))
                    throw new InvalidOperationException("交易确认登录状态已更改，请重新读取令牌后重试。");
                var webSession = await CreateWebSessionAsync(id, access, refresh, Ioc.Get<ISteamAccountService>(), token);
                current = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
                token.ThrowIfCancellationRequested();
                item = current.FirstOrDefault(x => x.Id == sourceId);
                if (item?.Value == null || !item.Value.SequenceEqual(encryptedValue))
                    throw new InvalidOperationException("令牌登录状态已更改，请重新读取令牌后重试。");
                if (liveAuthenticator != null && !IsLiveSessionCurrent(sourceId, liveAuthenticator, saved, access, refresh))
                    throw new InvalidOperationException("交易确认登录状态已更改，请重新读取令牌后重试。");
                return webSession;
            }));
        }
        return result;
    }

    static bool IsTokenUsable(string? token, ulong steamId, bool refresh)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3 || parts[0].Length == 0 || parts[2].Length == 0) return false;
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = doc.RootElement;
            if (root.GetProperty("sub").GetString() != steamId.ToString(CultureInfo.InvariantCulture) ||
                root.GetProperty("exp").GetInt64() <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return false;
            var aud = root.GetProperty("aud");
            if (aud.ValueKind != JsonValueKind.Array) return false;
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
            return mobile && derive == refresh && (refresh || web);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException or KeyNotFoundException or OverflowException) { return false; }
    }

    internal static async Task<SteamSession> CreateWebSessionAsync(ulong steamId, string? accessToken, string? refreshToken,
        ISteamAccountService accountService, CancellationToken cancellationToken, Func<CookieContainer, HttpMessageHandler>? handlerFactory = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsTokenUsable(accessToken, steamId, refresh: false))
        {
            if (!IsTokenUsable(refreshToken, steamId, refresh: true))
                throw new InvalidOperationException("交易确认登录状态已过期，请在令牌插件中重新登录交易确认。");
            accessToken = await accountService.RefreshAccessToken(steamId, refreshToken!).WaitAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsTokenUsable(accessToken, steamId, refresh: false))
                throw new InvalidOperationException("交易确认登录状态已过期或不支持网页登录，请重新登录交易确认。");
        }

        var expectedId = steamId.ToString(CultureInfo.InvariantCulture);
        var sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
        var secure = Uri.EscapeDataString($"{expectedId}||{accessToken}");
        var cookies = new CookieContainer();
        foreach (var domain in new[] { "steamcommunity.com", "steampowered.com" })
        {
            cookies.Add(new Cookie("steamLoginSecure", secure, "/", domain) { Secure = true, HttpOnly = true });
            cookies.Add(new Cookie("sessionid", sessionId, "/", domain) { Secure = true });
        }
        using var handler = handlerFactory?.Invoke(cookies) ?? new HttpClientHandler { CookieContainer = cookies, Proxy = HttpClient.DefaultProxy, AllowAutoRedirect = false };
        using var client = new HttpClient(handler);
        using var check = await client.GetAsync("https://steamcommunity.com/my/", cancellationToken);
        var location = check.Headers.Location;
        var path = location?.IsAbsoluteUri == true ? location.AbsolutePath : location?.OriginalString;
        if ((int)check.StatusCode is < 300 or >= 400 || path == null ||
            (location!.IsAbsoluteUri && (location.Scheme != Uri.UriSchemeHttps || location.Host != "steamcommunity.com" || location.Port != 443)) ||
            !(path.StartsWith("/id/", StringComparison.OrdinalIgnoreCase) ||
              path == "/profiles/" + expectedId || path.StartsWith("/profiles/" + expectedId + "/", StringComparison.Ordinal)))
            throw new InvalidOperationException("Steam 未接受该网页登录状态，请在令牌插件中重新登录交易确认。");
        cancellationToken.ThrowIfCancellationRequested();
        return new SteamSession { SteamId = expectedId, AccessToken = accessToken!, RefreshToken = string.Empty, CookieContainer = cookies };
    }
}
