using static BD.WTTS.Services.IArchiSteamFarmWebApiService;

namespace BD.WTTS.Services.Implementation;

partial class ArchiSteamFarmWebApiServiceImpl // ApiControllers
    : IASFClient, IBotClient, ICommandClient
{
    public IASFClient ASF => this;

    public async Task<IApiRsp<GenericResponse>> Encrypt(
        ASFEncryptRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<ASFEncryptRequest, GenericResponse>(
            "Api/ASF/Encrypt",
            HttpMethod.Post,
            request,
            cancellationToken: cancellationToken);
        return result!;
    }

    public IBotClient Bot => this;

    public ICommandClient Command => this;
}