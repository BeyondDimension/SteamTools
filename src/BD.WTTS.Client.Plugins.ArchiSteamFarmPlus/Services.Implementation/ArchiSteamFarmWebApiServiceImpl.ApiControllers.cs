using ArchiSteamFarm.Steam;
using static BD.WTTS.Services.IArchiSteamFarmWebApiService;

namespace BD.WTTS.Services.Implementation;

partial class ArchiSteamFarmWebApiServiceImpl // ApiControllers
    : IASFClient, IBotClient, ICommandClient
{
    public IASFClient ASF => this;

    #region ASFController

    public async Task<IApiRsp> Encrypt(
        ASFEncryptRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ASFEncryptRequest, Void>(
            "Api/ASF/Encrypt",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp<ASFResponse>> Get(
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, ASFResponse>(
            "Api/ASF",
            HttpMethod.Get,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp<string>> Hash(
        ASFHashRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ASFHashRequest, string>(
            "Api/ASF/Hash",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> Post(
        ASFRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ASFRequest, Void>(
            "Api/ASF",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> Exit(
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, Void>(
            "Api/ASF/Exit",
            HttpMethod.Post,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> Restart(
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, Void>(
            "Api/ASF/Restart",
            HttpMethod.Post,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp<string?>> Update(
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, string>(
            "Api/ASF/Update",
            HttpMethod.Post,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    #endregion

    public IBotClient Bot => this;

    #region BotController

    public async Task<IApiRsp> BotDelete(
        string botNames,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, Void>(
            $"Api/Bot/{botNames}",
            HttpMethod.Delete,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp<IReadOnlyDictionary<string, Bot>>> BotGet(
        string botNames,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, IReadOnlyDictionary<string, Bot>>(
            $"Api/Bot/{botNames}",
            HttpMethod.Get,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> BotPost(
        string botNames,
        BotRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<BotRequest, Void>(
            $"Api/Bot/{botNames}",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> GamesToRedeemInBackgroundDelete(
        string botNames,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, Void>(
            $"Api/Bot/{botNames}/GamesToRedeemInBackground",
            HttpMethod.Delete,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp<IReadOnlyDictionary<string, GamesToRedeemInBackgroundResponse>>> GamesToRedeemInBackgroundGet(
        string botNames,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, IReadOnlyDictionary<string, GamesToRedeemInBackgroundResponse>>(
            $"Api/Bot/{botNames}/GamesToRedeemInBackground",
            HttpMethod.Get,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> GamesToRedeemInBackgroundPost(
        string botNames,
        BotGamesToRedeemInBackgroundRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<BotGamesToRedeemInBackgroundRequest, Void>(
            $"Api/Bot/{botNames}/GamesToRedeemInBackground",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> InputPost(
        string botNames,
        BotInputRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<BotInputRequest, Void>(
            $"Api/Bot/{botNames}/Input",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> PausePost(
        string botNames,
        BotPauseRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<BotPauseRequest, Void>(
            $"Api/Bot/{botNames}/Pause",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> RedeemPost(
        string botNames,
        BotRedeemRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<BotRedeemRequest, Void>(
            $"Api/Bot/{botNames}/Redeem",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> RenamePost(
        string botNames,
        BotRenameRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<BotRenameRequest, Void>(
            $"Api/Bot/{botNames}/Rename",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> ResumePost(
        string botNames,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, Void>(
            $"Api/Bot/{botNames}/Resume",
            HttpMethod.Post,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> StartPost(
        string botNames,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, Void>(
            $"Api/Bot/{botNames}/Start",
            HttpMethod.Post,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    public async Task<IApiRsp> StopPost(
        string botNames,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<Void, Void>(
            $"Api/Bot/{botNames}/Stop",
            HttpMethod.Post,
            reqBody: default,
            cancellationToken: cancellationToken);
        return result!;
    }

    #endregion

    public ICommandClient Command => this;

    #region CommandController

    public async Task<IApiRsp<string>> CommandPost(
        CommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<CommandRequest, string>(
            $"Api/Command",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    #endregion
}