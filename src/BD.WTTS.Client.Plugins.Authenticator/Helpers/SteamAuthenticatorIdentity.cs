using System.Text.Json.Nodes;
using BD.SteamClient.Models;
using WinAuth;

namespace BD.WTTS.Helpers;

internal static class SteamAuthenticatorIdentity
{
    public static void ValidateCredentialAccount(SteamAuthenticator authenticator, SteamLoginState state)
    {
        var tokenId = authenticator.SteamId64;
        if (state.SteamId == 0)
            throw new InvalidOperationException("Steam 未返回有效的账号 ID，请重试登录。");
        if (tokenId is > 0 && state.SteamId != (ulong)tokenId.Value)
            throw new InvalidOperationException("登录账号与此令牌的 SteamID 不匹配，请使用令牌对应的 Steam 账号。");

        var accountName = authenticator.AccountName;
        if (!string.IsNullOrWhiteSpace(accountName) &&
            !string.Equals(state.Username?.Trim(), accountName.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("登录账号与此令牌的账号名称不匹配，请使用令牌对应的 Steam 账号。");
        if (tokenId is not > 0 && string.IsNullOrWhiteSpace(accountName))
            throw new InvalidOperationException("此令牌缺少 SteamID 和账号名称，无法核对登录账号。");
    }

    public static void ConfirmAuthenticatedAccount(SteamAuthenticator authenticator, SteamLoginState state, bool automaticCodeAccepted)
    {
        var tokenId = authenticator.SteamId64;
        if (!state.Success || state.SteamId == 0 || state.SteamId > long.MaxValue ||
            (tokenId is > 0 && state.SteamId != (ulong)tokenId.Value))
            throw new InvalidOperationException("登录账号与此令牌不匹配，未保存任何登录状态。");
        if (tokenId is > 0) return;

        if (!automaticCodeAccepted)
            throw new InvalidOperationException("此令牌缺少 SteamID，请使用账号密码登录并自动验证当前令牌后重试。");
        ValidateCredentialAccount(authenticator, state);
        if (JsonNode.Parse(authenticator.SteamData ?? "{}") is not JsonObject data)
            throw new InvalidOperationException("此令牌的数据格式无效，无法保存 SteamID。");
        data["steamid"] = (long)state.SteamId;
        authenticator.SteamData = data.ToJsonString();
    }
}
