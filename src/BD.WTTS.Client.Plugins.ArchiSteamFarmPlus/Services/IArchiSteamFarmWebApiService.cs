namespace BD.WTTS.Services;

public interface IArchiSteamFarmWebApiService
{
    IASFClient ASF { get; }

    interface IASFClient
    {
        /// <summary>
        /// Encrypts data with ASF encryption mechanisms using provided details.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IApiRsp<GenericResponse>> Encrypt(
            ASFEncryptRequest request,
            CancellationToken cancellationToken = default);
    }

    IBotClient Bot { get; }

    interface IBotClient
    {

    }

    ICommandClient Command { get; }

    interface ICommandClient
    {

    }
}
