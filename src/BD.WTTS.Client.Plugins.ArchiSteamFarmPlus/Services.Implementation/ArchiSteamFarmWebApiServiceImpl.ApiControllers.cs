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

    public async Task<IApiRsp> Hash(
        ASFHashRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ASFHashRequest, Void>(
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
        UpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<UpdateRequest, string>(
            "Api/ASF/Update",
            HttpMethod.Post,
            reqBody: request,
            cancellationToken: cancellationToken);
        return result!;
    }

    #endregion

    public IBotClient Bot => this;

    #region BotController

    #endregion

    public ICommandClient Command => this;

    #region CommandController

    #endregion
}