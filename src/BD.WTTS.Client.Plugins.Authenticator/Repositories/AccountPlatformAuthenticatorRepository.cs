using static BD.Common.Services.ISecurityService;
using static BD.WTTS.Repositories.Abstractions.IAccountPlatformAuthenticatorRepository;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

internal sealed class AccountPlatformAuthenticatorRepository : Repository<AccountPlatformAuthenticator, ushort>, IAccountPlatformAuthenticatorRepository
{
    readonly ISecurityService ss;

    public AccountPlatformAuthenticatorRepository(ISecurityService ss)
    {
        this.ss = ss;
    }

    public async Task<AccountPlatformAuthenticator[]> GetAllSourceAsync()
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(t =>
        {
            t.ThrowIfCancellationRequested();
            return dbConnection.Table<AccountPlatformAuthenticator>().Take(MaxValue).ToArrayAsync();
        }, cancellationToken: CancellationToken.None).ConfigureAwait(false);
    }

    public async Task<AccountPlatformAuthenticator?> GetFirstOrDefaultSourceAsync()
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(t =>
        {
            t.ThrowIfCancellationRequested();
            return dbConnection.Table<AccountPlatformAuthenticator>().FirstOrDefaultAsync();
        }, cancellationToken: CancellationToken.None).ConfigureAwait(false);
    }

    public async Task<bool> HasLocalAsync()
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            var item = await dbConnection.Table<AccountPlatformAuthenticator>().FirstOrDefaultAsync();
            if (item != null)
            {
                item = await dbConnection.Table<AccountPlatformAuthenticator>().FirstOrDefaultAsync(x => !x.IsNotLocal);
                return item != null;
            }
            return false;
        }, cancellationToken: CancellationToken.None).ConfigureAwait(false);
    }

    public async Task<bool> HasSecondaryPasswordAsync()
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            var item = await dbConnection.Table<AccountPlatformAuthenticator>().FirstOrDefaultAsync();
            if (item != null)
            {
                item = await dbConnection.Table<AccountPlatformAuthenticator>().FirstOrDefaultAsync(x => !x.IsNeedSecondaryPassword);
                return item != null;
            }
            return false;
        }, cancellationToken: CancellationToken.None).ConfigureAwait(false);
    }

    static int GetOrder(IOrderAuthenticator item)
    {
        var index = item.Index == default ? item.Id : item.Index;
        return index;
    }

    async Task<IAuthenticatorDTO?> ConvertAsync(AccountPlatformAuthenticator item, string? secondaryPassword)
    {
        (var value, var _) = await Convert2Async(item, secondaryPassword);
        return value;
    }

    static ImportResultCode Convert(DResultCode resultCode)
    {
        var resultCode_ = (int)resultCode;
        var resultCode__ = (ImportResultCode)resultCode_;
        return resultCode__;
    }

    public async Task<bool> Exists(IEnumerable<AccountPlatformAuthenticator> sourceList, IAuthenticatorDTO item,
        bool isLocal, string? secondaryPassword = null)
    {
        // var value = Serializable.SMP(item.Value);
        // (var notSecondaryPassword, var encryptionMode) = GetEncryptionMode2(isLocal, secondaryPassword);
        // var value_bytes = await ss.EB(value, encryptionMode, secondaryPassword);

        //return sourceList.Any(i => i.Value == value_bytes);
        if (item.Value?.SecretKey == null) return false;

        var auths = ConvertAsync(sourceList, secondaryPassword);
        return await auths.AnyAsync(i =>
            i?.Value?.SecretKey != null && i.Value.SecretKey.SequenceEqual(item.Value.SecretKey));
    }

    async Task<(IAuthenticatorDTO? value, ImportResultCode resultCode)> Convert2Async(AccountPlatformAuthenticator item, string? secondaryPassword)
    {
        var (value_bytes, result_code) = await ss.DB2(item.Value, secondaryPassword);
        if (result_code != DResultCode.Success) return (null, Convert(result_code));

        var (name_str, name_result_code) = await ss.D2(item.Name, secondaryPassword);
        if (name_result_code != DResultCode.Success) return (null, Convert(name_result_code));

        IAuthenticatorValueDTO? value;
        try
        {
            //TODO 谷歌云令牌的解码有问题
            value = Serializable.DMP<IAuthenticatorValueDTO>(value_bytes!);
            if (value == null) return (null, ImportResultCode.Success);
        }
        catch
        {
            return (null, ImportResultCode.IncorrectFormat);
        }

        var index = GetOrder(item);
        var result = new AuthenticatorDTO
        {
            Id = item.Id,
            Name = name_str ?? string.Empty,
            ServerId = item.ServerId,
            Value = value,
            Index = index,
            Created = item.Created,
            LastUpdate = item.LastUpdate,
        };
        return (result, ImportResultCode.Success);
    }

    async IAsyncEnumerable<IAuthenticatorDTO?> ConvertAsync(IEnumerable<AccountPlatformAuthenticator> sources, string? secondaryPassword)
    {
        foreach (var item in sources)
        {
            var value = await ConvertAsync(item, secondaryPassword);
            yield return value;
        }
    }

    public async Task<List<IAuthenticatorDTO>> ConvertToListAsync(IEnumerable<AccountPlatformAuthenticator> sources, string? secondaryPassword = null)
    {
        var query = ConvertAsync(sources, secondaryPassword);

        var list = new List<IAuthenticatorDTO>();

        await foreach (var item in query)
        {
            if (item != null)
            {
                list.Add(item);
            }
        }

        return list;
    }

    public async Task<List<IAuthenticatorDTO>> GetAllAsync(string? secondaryPassword = null)
    {
        var sources = await GetAllSourceAsync();
        return await ConvertToListAsync(sources);
    }

    static EncryptionMode GetEncryptionMode(bool isLocal, string? secondaryPassword)
    {
        (bool _, EncryptionMode mode) = GetEncryptionMode2(isLocal, secondaryPassword);
        return mode;
    }

    static (bool notSecondaryPassword, EncryptionMode mode) GetEncryptionMode2(bool isLocal, string? secondaryPassword)
    {
        var notSecondaryPassword = string.IsNullOrEmpty(secondaryPassword);
        var encryptionMode =
            isLocal ?
                (notSecondaryPassword ?
                    EncryptionMode.EmbeddedAesWithLocal :
                    EncryptionMode.EmbeddedAesWithSecondaryPasswordWithLocal) :
                (notSecondaryPassword ?
                    EncryptionMode.EmbeddedAes :
                    EncryptionMode.EmbeddedAesWithSecondaryPassword);
        return (notSecondaryPassword, encryptionMode);
    }

    async Task<AccountPlatformAuthenticator> ConvertAsync(IAuthenticatorDTO item, bool isLocal,
       string? secondaryPassword = null)
    {
        var value = Serializable.SMP(item.Value);

        (var notSecondaryPassword, var encryptionMode) = GetEncryptionMode2(isLocal, secondaryPassword);

        var name_encryptionMode = GetEncryptionMode(isLocal, null);
        var name_bytes = await ss.E(item.Name ?? string.Empty, name_encryptionMode, null);

        var value_bytes = await ss.EB(value, encryptionMode, secondaryPassword);
        value_bytes = value_bytes.ThrowIsNull(nameof(value_bytes));

        var entity = new AccountPlatformAuthenticator
        {
            Id = item.Id,
            Name = name_bytes,
            ServerId = item.ServerId,
            Value = value_bytes,
            IsNotLocal = !isLocal,
            IsNeedSecondaryPassword = !notSecondaryPassword,
            Index = item.Index,
            Created = item.Created == default ? DateTimeOffset.Now : item.Created,
            LastUpdate = DateTimeOffset.Now,
        };
        return entity;
    }

    public async Task<(bool isSuccess, bool isUpdate)> InsertOrUpdateAsync(IAuthenticatorDTO item, bool isLocal,
        string? secondaryPassword = null)
    {

        var entity = await ConvertAsync(item, isLocal, secondaryPassword);

        var r = await InsertOrUpdateAsync(entity);

        item.Id = entity.Id;

        return (r.rowCount > 0, r.result == DbRowExecResult.Update);
    }

    public async Task DeleteAsync(ushort id) => await base.DeleteAsync(id);

    public async Task<int> DeleteAsync(Guid serverId)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(t =>
        {
            t.ThrowIfCancellationRequested();
            const string sql =
                SQLStrings.DeleteFrom +
                $"[{AccountPlatformAuthenticator.TableName}]" +
                " where " +
                $"[{AccountPlatformAuthenticator.ColumnName_ServerId}]"
                + " = ?";
            return dbConnection.ExecuteAsync(sql,
                serverId);
        }, cancellationToken: CancellationToken.None).ConfigureAwait(false);
    }

    async Task IAccountPlatformAuthenticatorRepository.DeleteAsync(Guid serverId)
        => await DeleteAsync(serverId);

    public async Task RenameAsync(AccountPlatformAuthenticator source, string name, bool isLocal)
    {
        var encryptionMode = GetEncryptionMode(isLocal, null);

        var name_bytes = await ss.E(name, encryptionMode, null);

        source.Name = name_bytes;
        await UpdateAsync(source);
    }

    public async Task RenameAsync(ushort id, string name, bool isLocal)
    {
        var source = await FindAsync(id);
        if (source != null)
        {
            await RenameAsync(source, name, isLocal);
        }
    }

    public async Task SetServerIdAsync(AccountPlatformAuthenticator source, Guid serverId)
    {
        source.ServerId = serverId;
        await UpdateAsync(source);
    }

    public async Task SetServerIdAsync(ushort id, Guid serverId)
    {
        var source = await FindAsync(id);
        if (source != null)
        {
            await SetServerIdAsync(source, serverId);
        }
    }

    public async Task SwitchEncryptionModeAsync(bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO>? items)
    {
        if (items == null)
        {
            items = await GetAllAsync(secondaryPassword);
        }

        foreach (var item in items)
        {
            await InsertOrUpdateAsync(item, isLocal, secondaryPassword);
        }
    }

    IEnumerable<AccountPlatformAuthenticator>? Import(byte[] content)
    {
        try
        {
            var result = Serializable.DMP<IEnumerable<AccountPlatformAuthenticator>>(content);
            return result;
        }
        catch
        {
            return null;
        }
    }

    async Task<List<AccountPlatformAuthenticator>> ExportCoreAsync(bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO> items)
    {
        var list = new List<AccountPlatformAuthenticator>();
        foreach (var item in items)
        {
            var entity = await ConvertAsync(item, isLocal, secondaryPassword);
            list.Add(entity);
        }
        return list;
    }

    public async Task<byte[]> ExportAsync(bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO> items)
    {
        var list = await ExportCoreAsync(isLocal, secondaryPassword, items);
        var result = Serializable.SMP(list);
        return result;
    }

    public async Task ExportAsync(Stream stream, bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO> items)
    {
        var list = await ExportCoreAsync(isLocal, secondaryPassword, items);
        await Serializable.SMPAsync(stream, list);
    }

    public async Task<(ImportResultCode resultCode, IReadOnlyList<IAuthenticatorDTO> result, int sourcesCount)> ImportAsync(string? secondaryPassword, byte[] content)
    {
        int sourcesCount = 0;
        var resultCode = ImportResultCode.Success;
        IReadOnlyList<IAuthenticatorDTO>? result = null;
        var sources = Import(content);
        if (sources == null)
        {
            resultCode = ImportResultCode.IncorrectFormat;
        }
        else
        {
            sourcesCount = sources.Count();
            var list = new List<IAuthenticatorDTO>();
            foreach (var source in sources)
            {
                (var item, var item_result_code) = await Convert2Async(source, secondaryPassword);
                if (item_result_code != ImportResultCode.Success)
                {
                    resultCode = item_result_code;
                    break;
                }
                if (item != null)
                {
                    //导入数据应重新排序
                    item.Index = 0;
                    list.Add(item);
                }
                else
                {
                    resultCode = ImportResultCode.PartSuccess;
                }
            }
            result = list;
        }
        result ??= Array.Empty<IAuthenticatorDTO>();
        if (!result.Any() && resultCode == ImportResultCode.Success)
            resultCode = ImportResultCode.IncorrectFormat;
        return (resultCode, result, sourcesCount);
    }

    public async Task<int> UpdateIndexByItemAsync(IAuthenticatorDTO item)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            const string sql_ =
                SQLStrings.Update +
                "[" + AccountPlatformAuthenticator.TableName + "]" +
                " set " +
                "[" + AccountPlatformAuthenticator.ColumnName_Index + "]" +
                $" = {{0}}" +
                " where " +
                "[" + AccountPlatformAuthenticator.ColumnName_Id + "]" +
                $" = {{1}}";
            var sql = string.Format(sql_, item.Index, item.Id);
            var r = await dbConnection.ExecuteAsync(sql);
            return r;
        }, cancellationToken: CancellationToken.None).ConfigureAwait(false);

        //var source = await FindAsync(item.Id);
        //if (source != null)
        //{
        //    source.Index = item.Index;
        //    var r = await UpdateAsync(source);
        //    return r;
        //}
        //return 0;
    }

    public async Task<int> MoveOrderByIndexAsync<T>(Func<T, IAuthenticatorDTO> convert, IReadOnlyList<T> items, int index, bool upOrDown)
    {
        var item = items[index];
        var item2Index = upOrDown ? index - 1 : index + 1;
        if (item2Index > -1 && item2Index < items.Count)
        {
            var item2 = items[item2Index];
            var itemC = convert(item);
            var itemC2 = convert(item2);
            var orderIndex = GetOrder(itemC);
            var orderIndex2 = GetOrder(itemC2);
            itemC.Index = orderIndex2;
            itemC2.Index = orderIndex;
            var r = await Task.WhenAll(
                UpdateIndexByItemAsync(itemC),
                UpdateIndexByItemAsync(itemC2));
            return r.Sum();
        }
        return 0;
    }
}