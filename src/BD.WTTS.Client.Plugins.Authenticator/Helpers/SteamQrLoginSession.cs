using JsonException = System.Text.Json.JsonException;
using BD.SteamClient.Services;
using WinAuth;

namespace BD.WTTS.Helpers;

internal static class SteamQrLoginSession
{
    public const string LoginRequiredMessage = "仅已在“交易确认”中登录并保存登录状态的 Steam 令牌可使用扫码登录。请先打开此令牌的“确认交易”，登录并勾选记住登录后重试。";
    public const string SessionExpiredMessage = "交易确认保存的登录状态已失效或不支持移动端确认。请先到此令牌的“确认交易”重新登录并保存状态，再使用扫码。";

    public static bool CanUse(SteamAuthenticator authenticator)
        => TryGetSession(authenticator, out _, out _);

    public static async Task<string?> GetAccessTokenAsync(SteamAuthenticator authenticator,
        ISteamAccountService accountService, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryGetSession(authenticator, out var session, out var steamId))
            return null;
        if (IsTokenUsable(session!.AccessToken, steamId, refresh: false))
            return session.AccessToken;

        var savedData = authenticator.SessionData;
        var accessToken = await accountService.RefreshAccessToken(steamId, session.RefreshToken!).WaitAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        // A logout or a replacement session must win over an in-flight refresh.
        if (savedData != authenticator.SessionData || !IsTokenUsable(accessToken, steamId, refresh: false))
            return null;
        session.AccessToken = accessToken;
        authenticator.SessionData = session.ToString();
        return accessToken;
    }

    static bool TryGetSession(SteamAuthenticator authenticator, out WinAuth.SteamClient.SteamSession? session, out ulong steamId)
    {
        session = null;
        steamId = 0;
        try
        {
            if (authenticator.SteamId64 is not > 0 || string.IsNullOrWhiteSpace(authenticator.SessionData))
                return false;
            steamId = (ulong)authenticator.SteamId64.Value;
            session = new WinAuth.SteamClient.SteamSession(authenticator.SessionData);
            return session.SteamID == steamId &&
                (IsTokenUsable(session.AccessToken, steamId, refresh: false) ||
                 IsTokenUsable(session.RefreshToken, steamId, refresh: true));
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException or OverflowException)
        {
            return false;
        }
    }

    static bool IsTokenUsable(string? token, ulong expectedSteamId, bool refresh)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3 || parts[0].Length == 0 || parts[2].Length == 0)
                return false;
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = document.RootElement;
            if (root.GetProperty("sub").GetString() != expectedSteamId.ToString(CultureInfo.InvariantCulture) ||
                root.GetProperty("exp").GetInt64() <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return false;
            var audiences = root.GetProperty("aud");
            if (audiences.ValueKind != JsonValueKind.Array)
                return false;
            var mobile = false;
            var derive = false;
            foreach (var audience in audiences.EnumerateArray())
            {
                mobile |= audience.GetString() == "mobile";
                derive |= audience.GetString() == "derive";
            }
            // Suitability check only; Steam verifies token signatures server-side.
            return mobile && derive == refresh;
        }
        catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException or KeyNotFoundException or OverflowException)
        {
            return false;
        }
    }
}
