using System.Text.Encodings.Web;
using BD.SteamClient.Services.Implementation;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.Services.Implementation;

internal sealed class SteamSessionService(IServiceProvider services) : SteamSessionServiceImpl, ISteamSessionService
{
    static readonly SteamSessionJsonContext jsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    async Task<bool> ISteamSessionService.SaveSession(SteamSession steamSession)
    {
        try
        {
            var json = JsonSerializer.Serialize(steamSession, jsonContext.SteamSession);
            await services.GetRequiredService<ISecureStorage>().SetAsync(ISteamSessionService.CurrentSteamUserKey, json);
            return true;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            return false;
        }
    }

    async Task<SteamSession?> ISteamSessionService.LoadSession()
    {
        try
        {
            var json = await services.GetRequiredService<ISecureStorage>().GetAsync(ISteamSessionService.CurrentSteamUserKey);
            if (json is null)
                return null;

            var session = JsonSerializer.Deserialize(json, jsonContext.SteamSession);
            if (session is null || !services.GetRequiredService<ISteamAccountService>().IsAccessTokenValid(session.AccessToken))
                return null;

            session.GenerateSetCookie();
            AddOrSetSeesion(session);
            await ((ISteamSessionService)this).SaveSession(session);
            return session;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            return null;
        }
    }
}

[JsonSerializable(typeof(SteamSession))]
internal sealed partial class SteamSessionJsonContext : JsonSerializerContext;
