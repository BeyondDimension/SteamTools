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
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text001);
        if (tokenId is > 0 && state.SteamId != (ulong)tokenId.Value)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text002);

        var accountName = authenticator.AccountName;
        if (!string.IsNullOrWhiteSpace(accountName) &&
            !string.Equals(state.Username?.Trim(), accountName.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text003);
        if (tokenId is not > 0 && string.IsNullOrWhiteSpace(accountName))
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text004);
    }

    public static void ConfirmAuthenticatedAccount(SteamAuthenticator authenticator, SteamLoginState state, bool automaticCodeAccepted)
    {
        var tokenId = authenticator.SteamId64;
        if (!state.Success || state.SteamId == 0 || state.SteamId > long.MaxValue ||
            (tokenId is > 0 && state.SteamId != (ulong)tokenId.Value))
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text005);
        if (tokenId is > 0) return;

        if (!automaticCodeAccepted)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text006);
        ValidateCredentialAccount(authenticator, state);
        if (JsonNode.Parse(authenticator.SteamData ?? "{}") is not JsonObject data)
            throw new InvalidOperationException(Strings.L10nAuthIdle_Text007);
        data["steamid"] = (long)state.SteamId;
        authenticator.SteamData = data.ToJsonString();
    }
}
