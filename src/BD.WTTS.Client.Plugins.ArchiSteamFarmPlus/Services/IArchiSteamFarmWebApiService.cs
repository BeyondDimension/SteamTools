namespace BD.WTTS.Services;

public interface IArchiSteamFarmWebApiService
{
    static IArchiSteamFarmWebApiService Instance => Ioc.Get<IArchiSteamFarmWebApiService>();

    // http://localhost:1242/swagger/index.html
    // IApiRsp = GenericResponse
    // IApiRsp<T> = GenericResponse<T>
    // T maybe null !!!

    IASFClient ASF { get; }

    interface IASFClient
    {
        /// <summary>
        /// Encrypts data with ASF encryption mechanisms using provided details.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IApiRsp> Encrypt(
            ASFEncryptRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches common info related to ASF as a whole.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IApiRsp<ASFResponse>> Get(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Hashes data with ASF hashing mechanisms using provided details.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IApiRsp> Hash(
            ASFHashRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates ASF's global config.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IApiRsp> Post(
            ASFRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes ASF shutdown itself.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IApiRsp> Exit(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes ASF restart itself.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IApiRsp> Restart(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes ASF update itself.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IApiRsp<string?>> Update(
            UpdateRequest request,
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
