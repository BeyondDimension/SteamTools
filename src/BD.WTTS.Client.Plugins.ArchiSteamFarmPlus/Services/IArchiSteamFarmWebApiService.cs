using ArchiSteamFarm.Steam;

namespace BD.WTTS.Services;

public interface IArchiSteamFarmWebApiService
{
    static IArchiSteamFarmWebApiService Instance => Ioc.Get<IArchiSteamFarmWebApiService>();

    // http://localhost:1242/swagger/index.html
    // IApiRsp = GenericResponse
    // IApiRsp<T> = GenericResponse<T>
    // T maybe null !!!

    void SetIPCUrl(string ipcUrl);

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
        Task<IApiRsp<string>> Hash(
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
            CancellationToken cancellationToken = default);
    }

    IBotClient Bot { get; }

    interface IBotClient
    {
        /// <summary>
        ///     Deletes all files related to given bots.
        /// </summary>
        Task<IApiRsp> BotDelete(
            string botNames,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Fetches common info related to given bots.
        /// </summary>
        Task<IApiRsp<IReadOnlyDictionary<string, Bot>>> BotGet(
            string botNames,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Updates bot config of given bot.
        /// </summary>
        Task<IApiRsp> BotPost(
            string botNames,
            BotRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Removes BGR output files of given bots.
        /// </summary>
        Task<IApiRsp> GamesToRedeemInBackgroundDelete(
            string botNames,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Fetches BGR output files of given bots.
        /// </summary>
        Task<IApiRsp<IReadOnlyDictionary<string, GamesToRedeemInBackgroundResponse>>> GamesToRedeemInBackgroundGet(
            string botNames,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Adds keys to redeem using BGR to given bot.
        /// </summary>
        Task<IApiRsp> GamesToRedeemInBackgroundPost(
            string botNames,
            BotGamesToRedeemInBackgroundRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Provides input value to given bot for next usage.
        /// </summary>
        Task<IApiRsp> InputPost(
            string botNames,
            BotInputRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Pauses given bots.
        /// </summary>
        Task<IApiRsp> PausePost(
            string botNames,
            BotPauseRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Redeems cd-keys on given bot.
        /// </summary>
        /// <remarks>
        ///     Response contains a map that maps each provided cd-key to its redeem result.
        ///     Redeem result can be a null value, this means that ASF didn't even attempt to send a request (e.g. because of bot not being connected to Steam network).
        /// </remarks>
        Task<IApiRsp> RedeemPost(
            string botNames,
            BotRedeemRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Renames given bot along with all its related files.
        /// </summary>
        Task<IApiRsp> RenamePost(
            string botNames,
            BotRenameRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Resumes given bots.
        /// </summary>
        Task<IApiRsp> ResumePost(
            string botNames,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Starts given bots.
        /// </summary>
        Task<IApiRsp> StartPost(
            string botNames,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Stops given bots.
        /// </summary>
        Task<IApiRsp> StopPost(
            string botNames,
            CancellationToken cancellationToken = default);
    }

    ICommandClient Command { get; }

    interface ICommandClient
    {
        /// <summary>
        ///     Executes a command.
        /// </summary>
        /// <remarks>
        ///     This API endpoint is supposed to be entirely replaced by ASF actions available under /Api/ASF/{action} and /Api/Bot/{bot}/{action}.
        ///     You should use "given bot" commands when executing this endpoint, omitting targets of the command will cause the command to be executed on first defined bot
        /// </remarks>
        Task<IApiRsp<string>> CommandPost(
            CommandRequest request,
            CancellationToken cancellationToken = default);
    }
}
