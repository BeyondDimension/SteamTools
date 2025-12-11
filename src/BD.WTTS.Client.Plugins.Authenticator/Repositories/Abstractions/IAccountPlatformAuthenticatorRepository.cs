using static BD.Common.Services.ISecurityService;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories.Abstractions;

public interface IAccountPlatformAuthenticatorRepository
{
    /// <summary>
    /// 游戏账号平台令牌可导入数量最大值
    /// </summary>
    const int MaxValue = 1000;

    /// <summary>
    /// 从本地获取所有
    /// </summary>
    /// <param name="secondaryPassword"></param>
    /// <returns></returns>
    [Obsolete("use GetAllSourceAsync()")]
    Task<List<IAuthenticatorDTO>> GetAllAsync(string? secondaryPassword = null);

    /// <summary>
    /// 从本地获取所有源数据
    /// </summary>
    /// <returns></returns>
    Task<AccountPlatformAuthenticator[]> GetAllSourceAsync();

    /// <summary>
    /// 从本地获取第一条源数据
    /// </summary>
    /// <returns></returns>
    Task<AccountPlatformAuthenticator?> GetFirstOrDefaultSourceAsync();

    /// <summary>
    /// 源数据中是否含有本地加密的数据
    /// </summary>
    /// <param name="sources"></param>
    /// <returns></returns>
    bool HasLocal(IEnumerable<AccountPlatformAuthenticator> sources)
        => sources.Any() && sources.Any(x => !x.IsNotLocal);

    /// <inheritdoc cref="HasLocal(IEnumerable{AccountPlatformAuthenticator})"/>
    bool HasLocal(params AccountPlatformAuthenticator[] sources)
    {
        IEnumerable<AccountPlatformAuthenticator> sources_ = sources;
        return HasLocal(sources_);
    }

    /// <inheritdoc cref="HasLocal(IEnumerable{AccountPlatformAuthenticator})"/>
    Task<bool> HasLocalAsync();

    /// <summary>
    /// 源数据中是否含有需要二级密码的数据
    /// </summary>
    /// <param name="sources"></param>
    /// <returns></returns>
    bool HasSecondaryPassword(IEnumerable<AccountPlatformAuthenticator> sources)
        => sources.Any() && sources.Any(x => x.IsNeedSecondaryPassword);

    /// <inheritdoc cref="HasSecondaryPassword(IEnumerable{AccountPlatformAuthenticator})"/>
    bool HasSecondaryPassword(params AccountPlatformAuthenticator[] sources)
    {
        IEnumerable<AccountPlatformAuthenticator> sources_ = sources;
        return HasSecondaryPassword(sources_);
    }

    /// <inheritdoc cref="HasSecondaryPassword(IEnumerable{AccountPlatformAuthenticator})"/>
    Task<bool> HasSecondaryPasswordAsync();

    /// <summary>
    /// 将源数据转换为传输模型
    /// </summary>
    /// <param name="sources"></param>
    /// <param name="secondaryPassword"></param>
    /// <returns></returns>
    Task<List<IAuthenticatorDTO>> ConvertToListAsync(IEnumerable<AccountPlatformAuthenticator> sources, string? secondaryPassword = null);

    /// <summary>
    /// 插入或更新一条到本地，插入前需要判断当前值是否超过了 <see cref="MaxValue"/>
    /// </summary>
    /// <param name="item"></param>
    /// <param name="isLocal"></param>
    /// <param name="secondaryPassword"></param>
    /// <returns></returns>
    Task<(bool isSuccess, bool isUpdate)> InsertOrUpdateAsync(IAuthenticatorDTO item, bool isLocal, string? secondaryPassword = null);

    /// <inheritdoc cref="InsertOrUpdateAsync(IAuthenticatorDTO, bool, string?)"/>
    async Task InsertOrUpdateAsync(IEnumerable<IAuthenticatorDTO> items, bool isLocal, string? secondaryPassword = null)
    {
        foreach (var item in items)
        {
            await InsertOrUpdateAsync(item, isLocal, secondaryPassword);
        }
    }

    /// <summary>
    /// 验证源数据内令牌是否已存在
    /// </summary>
    /// <param name="sourceList"></param>
    /// <param name="item"></param>
    /// <param name="isLocal"></param>
    /// <param name="secondaryPassword"></param>
    /// <returns></returns>
    Task<bool> Exists(IEnumerable<AccountPlatformAuthenticator> sourceList, IAuthenticatorDTO item,
        bool isLocal, string? secondaryPassword = null);

    /// <summary>
    /// 根据本地 Id 删除一条
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteAsync(ushort id);

    /// <summary>
    /// 根据云同步 Id 删除
    /// </summary>
    /// <param name="serverId"></param>
    /// <returns></returns>
    Task DeleteAsync(Guid serverId);

    /// <summary>
    /// 根据本地 Id 获取源
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<AccountPlatformAuthenticator?> FindAsync(ushort id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据本地源修改显示名称
    /// </summary>
    /// <param name="source"></param>
    /// <param name="name"></param>
    /// <param name="isLocal"></param>
    /// <returns></returns>
    Task RenameAsync(AccountPlatformAuthenticator source, string name, bool isLocal);

    /// <summary>
    /// 根据本地 Id 修改显示名称
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="isLocal"></param>
    /// <returns></returns>
    Task RenameAsync(ushort id, string name, bool isLocal);

    /// <summary>
    /// 根据本地 Id 设置云同步 Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="serverId"></param>
    /// <returns></returns>
    Task SetServerIdAsync(ushort id, Guid serverId);

    /// <inheritdoc cref="SetServerIdAsync(ushort, Guid)"/>
    Task SetServerIdAsync(AccountPlatformAuthenticator source, Guid serverId);

    /// <summary>
    /// 切换加密模式，对已有的整个数据重新加密，调用前必须判断值不一样才能调用，如果耗时太长要搞一个转圈或进度条
    /// </summary>
    /// <param name="isLocal"></param>
    /// <param name="secondaryPassword"></param>
    /// <param name="items">可传递当前在视图模型上的数据，也可以传递 <see langword="null"/> 重新从数据库中查询</param>
    /// <returns></returns>
    Task SwitchEncryptionModeAsync(bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO>? items = null);

    /// <summary>
    /// 导出一组 DTO 模型
    /// </summary>
    /// <param name="isLocal"></param>
    /// <param name="secondaryPassword"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    Task<byte[]> ExportAsync(bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO> items);

    /// <inheritdoc cref="ExportAsync(bool, string?, IEnumerable{IAuthenticatorDTO})"/>
    Task<byte[]> ExportAsync(bool isLocal, string? secondaryPassword, params IAuthenticatorDTO[] items)
    {
        IEnumerable<IAuthenticatorDTO> sources_ = items;
        return ExportAsync(isLocal, secondaryPassword, sources_);
    }

    /// <inheritdoc cref="ExportAsync(bool, string?, IEnumerable{IAuthenticatorDTO})"/>
    Task ExportAsync(Stream stream, bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO> items);

    /// <inheritdoc cref="ExportAsync(bool, string?, IEnumerable{IAuthenticatorDTO})"/>
    Task ExportAsync(Stream stream, bool isLocal, string? secondaryPassword, params IAuthenticatorDTO[] items)
    {
        IEnumerable<IAuthenticatorDTO> sources_ = items;
        return ExportAsync(stream, isLocal, secondaryPassword, sources_);
    }

    /// <summary>
    /// 导入一组数据，返回对应的DTO模型组，之后可调用 <see cref="InsertOrUpdateAsync(IEnumerable{IAuthenticatorDTO}, bool, string?)"/> 插入数据库
    /// </summary>
    /// <param name="secondaryPassword"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<(ImportResultCode resultCode, IReadOnlyList<IAuthenticatorDTO> result, int sourcesCount)> ImportAsync(string? secondaryPassword, byte[] content);

    /// <summary>
    /// 移动排序值，上移或下移，返回受影响的行数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="convert"></param>
    /// <param name="items"></param>
    /// <param name="index"></param>
    /// <param name="upOrDown"></param>
    /// <returns></returns>
    Task<int> MoveOrderByIndexAsync<T>(Func<T, IAuthenticatorDTO> convert, IReadOnlyList<T> items, int index, bool upOrDown);

    /// <inheritdoc cref="MoveOrderByIndexAsync{T}(Func{T, IAuthenticatorDTO}, IReadOnlyList{T}, int, bool)"/>
    async Task<int> MoveOrderByItemAsync<T>(Func<T, IAuthenticatorDTO> convert, IReadOnlyList<T> items, T item, bool upOrDown)
    {
        var index = items.IndexOf(item);
        if (index > -1)
        {
            return await MoveOrderByIndexAsync<T>(convert, items, index, upOrDown);
        }
        return 0;
    }

    /// <summary>
    /// 指定改变IAuthenticatorDTO的排序指
    /// </summary>
    /// <param name="item"></param>
    /// <returns>受影响的行数</returns>
    Task<int> UpdateIndexByItemAsync(IAuthenticatorDTO item);

    public enum ImportResultCode
    {
        /// <summary>
        /// 导入成功
        /// </summary>
        Success = 200,

        /// <summary>
        /// 部分数据导入成功
        /// </summary>
        PartSuccess,

        /// <summary>
        /// 格式不正确
        /// </summary>
        IncorrectFormat = 400,

        /// <inheritdoc cref="DResultCode.EmbeddedAesFail"/>
        EmbeddedAesFail = DResultCode.EmbeddedAesFail,

        /// <inheritdoc cref="DResultCode.LocalFail"/>
        LocalFail = DResultCode.LocalFail,

        /// <inheritdoc cref="DResultCode.SecondaryPasswordFail"/>
        SecondaryPasswordFail = DResultCode.SecondaryPasswordFail,

        /// <inheritdoc cref="DResultCode.IncorrectValueFail"/>
        IncorrectValueFail = DResultCode.IncorrectValueFail,

        /// <inheritdoc cref="DResultCode.UTF8GetStringFail"/>
        UTF8GetStringFail = DResultCode.UTF8GetStringFail,
    }
}