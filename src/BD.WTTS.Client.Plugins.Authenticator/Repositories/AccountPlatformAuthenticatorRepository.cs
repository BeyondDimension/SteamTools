using System.Security.Cryptography;
using System.Text;
using BD.Common.Enums;
using BD.WTTS.Repositories.Abstractions;
using BD.WTTS.Serialization;
using static BD.WTTS.Repositories.AuthenticatorEnvelope;
using static BD.Common.Services.ISecurityService;
using static BD.WTTS.Repositories.Abstractions.IAccountPlatformAuthenticatorRepository;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

internal sealed class AccountPlatformAuthenticatorRepository : Repository<AccountPlatformAuthenticator, ushort>, IAccountPlatformAuthenticatorRepository
{
    readonly ISecurityService ss;
    readonly SQLiteAsyncConnection? testConnection;
    static readonly UTF8Encoding StrictUtf8 = new(false, true);

    const string TableSql = $"[{AccountPlatformAuthenticator.TableName}]";
    const string IdSql = $"[{AccountPlatformAuthenticator.ColumnName_Id}]";
    const string ProtectionUpdateSql = $"UPDATE {TableSql} SET [D4117D89] = ?, [60FAF486] = ?, [44FF3988] = ?, [4AF8A895] = ? WHERE {IdSql} = ?";

    public AccountPlatformAuthenticatorRepository(ISecurityService ss)
    {
        this.ss = ss;
    }

    internal AccountPlatformAuthenticatorRepository(ISecurityService ss, SQLiteAsyncConnection connection) : this(ss)
    {
        testConnection = connection;
    }

    protected override ValueTask<SQLiteAsyncConnection> GetDbConnection() =>
        testConnection is { } connection ? new(connection) : base.GetDbConnection();

    public async Task<AccountPlatformAuthenticator[]> GetAllSourceAsync()
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(t =>
        {
            t.ThrowIfCancellationRequested();
            return dbConnection.Table<AccountPlatformAuthenticator>().OrderBy(x => x.Id).ToArrayAsync();
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
        return await AttemptAndRetry(t =>
        {
            t.ThrowIfCancellationRequested();
            return dbConnection.Table<AccountPlatformAuthenticator>().CountAsync(x => !x.IsNotLocal);
        }, cancellationToken: CancellationToken.None).ConfigureAwait(false) != 0;
    }

    public async Task<bool> HasSecondaryPasswordAsync()
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(t =>
        {
            t.ThrowIfCancellationRequested();
            return dbConnection.Table<AccountPlatformAuthenticator>().CountAsync(x => x.IsNeedSecondaryPassword);
        }, cancellationToken: CancellationToken.None).ConfigureAwait(false) != 0;
    }

    static int GetOrder(IOrderAuthenticator item)
    {
        var index = item.Index == default ? item.Id : item.Index;
        return index;
    }

    static EncryptionMode? LegacyMode(byte[]? blob)
    {
        if (blob == null || blob.Length < sizeof(int))
            return null;
        var mode = BitConverter.ToInt32(blob, 0);
        return mode is >= 1 and <= 4 ? (EncryptionMode)mode : null;
    }

    static bool HasLegacyLocal(byte[]? blob) => LegacyMode(blob) is
        EncryptionMode.EmbeddedAesWithLocal or EncryptionMode.EmbeddedAesWithSecondaryPasswordWithLocal;

    static bool HasLegacyPassword(byte[]? blob) => LegacyMode(blob) is
        EncryptionMode.EmbeddedAesWithSecondaryPassword or EncryptionMode.EmbeddedAesWithSecondaryPasswordWithLocal;

    internal static bool NeedsProtectionUpgrade(AccountPlatformAuthenticator row) =>
        !IsEnvelope(row.Value) && (row.IsNeedSecondaryPassword || HasLegacyPassword(row.Value) ||
            (OperatingSystem.IsWindows() && (!row.IsNotLocal || HasLegacyLocal(row.Value))));

    internal static bool RequiresWindowsHello(AccountPlatformAuthenticator row) =>
        AuthenticatorEnvelope.RequiresWindowsHello(row.Name) || AuthenticatorEnvelope.RequiresWindowsHello(row.Value);

    public async Task<bool> Exists(IEnumerable<AccountPlatformAuthenticator> sourceList, IAuthenticatorDTO item,
        bool isLocal, string? secondaryPassword = null)
    {
        var secret = item.Value?.SecretKey;
        if (secret == null)
            return false;
        var rows = sourceList as AccountPlatformAuthenticator[] ?? sourceList.ToArray();
        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        using var keys = new Keys(secondaryPassword);
        var found = false;
        foreach (var row in rows)
        {
            var (name, value) = await ReadPayloadAsync(row, secondaryPassword, keys).ConfigureAwait(false);
            try
            {
                var decoded = DecodeValue(value);
                if (decoded.SecretKey is { } candidate)
                {
                    found |= CryptographicOperations.FixedTimeEquals(secret, candidate);
                    CryptographicOperations.ZeroMemory(candidate);
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(name);
                CryptographicOperations.ZeroMemory(value);
            }
        }
        return found;
    }

    async ValueTask<byte[]> ReadLegacyAsync(byte[]? blob, string? password, bool allowPlain)
    {
        if (blob == null || blob.Length == 0)
            return Array.Empty<byte>();
        if (LegacyMode(blob) == null)
        {
            if (!allowPlain)
                throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
            // Some old unprotected rows stored plain UTF-8 / MessagePack. Never wipe the caller's row.
            return blob.ToArray();
        }
        var (bytes, code) = await ss.DB2(blob, password).ConfigureAwait(false);
        if (code != DResultCode.Success)
            throw new AuthenticatorReadException((ImportResultCode)(int)code);
        if (bytes == null)
            throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
        return ReferenceEquals(bytes, blob) ? bytes.ToArray() : bytes;
    }

    async ValueTask<(byte[] name, byte[] value)> ReadPayloadAsync(AccountPlatformAuthenticator row, string? password, Keys keys)
    {
        if (IsEnvelope(row.Value) || IsEnvelope(row.Name))
        {
            if (AuthenticatorEnvelope.RequiresWindowsHello(row.Value) != AuthenticatorEnvelope.RequiresWindowsHello(row.Name))
                throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
            if (row.IsNeedSecondaryPassword && !SharePasswordSalt(row.Value, row.Name))
                throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
            var value = await OpenAsync(ss, row.Value, Purpose.Value, !row.IsNotLocal, row.IsNeedSecondaryPassword, keys).ConfigureAwait(false);
            try
            {
                var name = await OpenAsync(ss, row.Name, Purpose.Name, !row.IsNotLocal, row.IsNeedSecondaryPassword, keys).ConfigureAwait(false);
                return (name, value);
            }
            catch (AuthenticatorReadException ex) when (ex.ResultCode == ImportResultCode.SecondaryPasswordFail)
            {
                CryptographicOperations.ZeroMemory(value);
                // The value already authenticated this password and salt.
                throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
            }
            catch
            {
                CryptographicOperations.ZeroMemory(value);
                throw;
            }
        }

        // Legacy labels were never password-encrypted, even on a password-protected row.
        // Legacy encryption is self-describing; flags may be mixed across the batch.
        if ((row.IsNeedSecondaryPassword && !HasLegacyPassword(row.Value)) ||
            (!row.IsNotLocal && !HasLegacyLocal(row.Value)))
            throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
        var legacyValue = await ReadLegacyAsync(row.Value, password, row.IsNotLocal && !row.IsNeedSecondaryPassword).ConfigureAwait(false);
        try
        {
            var name = await ReadLegacyAsync(row.Name, password, row.IsNotLocal).ConfigureAwait(false);
            return (name, legacyValue);
        }
        catch
        {
            CryptographicOperations.ZeroMemory(legacyValue);
            throw;
        }
    }

    static string DecodeName(byte[] name)
    {
        try
        {
            return StrictUtf8.GetString(name);
        }
        catch (DecoderFallbackException)
        {
            throw new AuthenticatorReadException(ImportResultCode.UTF8GetStringFail);
        }
    }

    static IAuthenticatorValueDTO DecodeValue(byte[] value)
    {
        try
        {
            return AuthenticatorPayloadSerializer.ReadValue(value)
                ?? throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
        }
        catch (Exception ex) when (ex is not AuthenticatorReadException and not OutOfMemoryException)
        {
            throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);
        }
    }

    async ValueTask<IAuthenticatorDTO> DecodeAsync(AccountPlatformAuthenticator row, string? password, Keys keys)
    {
        var (name, value) = await ReadPayloadAsync(row, password, keys).ConfigureAwait(false);
        try
        {
            return new AuthenticatorDTO
            {
                Id = row.Id,
                Name = DecodeName(name),
                ServerId = row.ServerId,
                Value = DecodeValue(value),
                Index = GetOrder(row),
                Created = row.Created,
                LastUpdate = row.LastUpdate,
            };
        }
        finally
        {
            CryptographicOperations.ZeroMemory(name);
            CryptographicOperations.ZeroMemory(value);
        }
    }

    static void ClearDecodedSecrets(IEnumerable<IAuthenticatorDTO> items)
    {
        foreach (var item in items)
        {
            if (item.Value?.SecretKey is { } secret)
                CryptographicOperations.ZeroMemory(secret);
        }
    }

    public async Task<List<IAuthenticatorDTO>> ConvertToListAsync(IEnumerable<AccountPlatformAuthenticator> sources,
        string? secondaryPassword = null, bool verifiedWithHello = false)
    {
        var rows = sources as AccountPlatformAuthenticator[] ?? sources.ToArray();
        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        using var keys = new Keys(secondaryPassword, verifiedWithHello);
        var list = new List<IAuthenticatorDTO>();
        try
        {
            foreach (var row in rows)
                list.Add(await DecodeAsync(row, secondaryPassword, keys).ConfigureAwait(false));
            return list;
        }
        catch
        {
            ClearDecodedSecrets(list);
            throw;
        }
    }

    public async Task<List<IAuthenticatorDTO>> GetAllAsync(string? secondaryPassword = null)
    {
        var sources = await GetAllSourceAsync().ConfigureAwait(false);
        return await ConvertToListAsync(sources, secondaryPassword).ConfigureAwait(false);
    }

    async ValueTask SetPayloadAsync(AccountPlatformAuthenticator row, byte[] name, byte[] value,
        bool isLocal, byte[]? salt, Keys keys, bool windowsHello)
    {
        if (isLocal || salt != null)
        {
            row.Name = await SealAsync(ss, name, Purpose.Name, isLocal, salt, keys, windowsHello).ConfigureAwait(false);
            row.Value = await SealAsync(ss, value, Purpose.Value, isLocal, salt, keys, windowsHello).ConfigureAwait(false);
        }
        else
        {
            // Keep the old portable, no-password wire format for older clients.
            row.Name = await ss.EB(name, EncryptionMode.EmbeddedAes).ConfigureAwait(false);
            row.Value = await ss.EB(value, EncryptionMode.EmbeddedAes).ConfigureAwait(false)
                ?? throw new CryptographicException("Authenticator encoding failed.");
            if (ReferenceEquals(row.Name, name))
                row.Name = name.ToArray();
            if (ReferenceEquals(row.Value, value))
                row.Value = value.ToArray();
        }
        row.IsNotLocal = !isLocal;
        row.IsNeedSecondaryPassword = salt != null;
    }

    async ValueTask<AccountPlatformAuthenticator> EncodeAsync(IAuthenticatorDTO item, bool isLocal, byte[]? salt, Keys keys,
        bool windowsHello = false)
    {
        var row = new AccountPlatformAuthenticator
        {
            Id = item.Id,
            ServerId = item.ServerId,
            Index = item.Index,
            Created = item.Created == default ? DateTimeOffset.Now : item.Created,
            LastUpdate = DateTimeOffset.Now,
        };
        var name = StrictUtf8.GetBytes(item.Name ?? string.Empty);
        try
        {
            var value = AuthenticatorPayloadSerializer.WriteValue(item.Value ?? throw new ArgumentException("Authenticator value is required.", nameof(item)));
            try
            {
                await SetPayloadAsync(row, name, value, isLocal, salt, keys, windowsHello).ConfigureAwait(false);
                return row;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(value);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(name);
        }
    }

    static void VerifySnapshot(SQLiteConnection connection, AccountPlatformAuthenticator[] snapshot)
    {
        var current = connection.Table<AccountPlatformAuthenticator>().OrderBy(x => x.Id).ToArray();
        if (current.Length == snapshot.Length)
        {
            var equal = true;
            for (var i = 0; i < current.Length && equal; i++)
            {
                var left = current[i];
                var right = snapshot[i];
                equal = left.Id == right.Id && left.IsNotLocal == right.IsNotLocal &&
                    left.IsNeedSecondaryPassword == right.IsNeedSecondaryPassword && left.Index == right.Index &&
                    left.ServerId == right.ServerId && left.Created == right.Created && left.LastUpdate == right.LastUpdate &&
                    SameBlob(left.Name, right.Name) && SameBlob(left.Value, right.Value);
            }
            if (equal)
                return;
        }
        throw new InvalidOperationException("The authenticator vault changed while preparing the write.");

        static bool SameBlob(byte[]? left, byte[]? right) =>
            (left == null) == (right == null) && left.AsSpan().SequenceEqual(right);
    }

    public async Task<(bool isSuccess, bool isUpdate)> InsertOrUpdateAsync(IAuthenticatorDTO item, bool isLocal,
        string? secondaryPassword = null, bool verifiedWithHello = false)
    {
        var isUpdate = item.Id != 0;
        await InsertOrUpdateAsync(new[] { item }, isLocal, secondaryPassword, verifiedWithHello).ConfigureAwait(false);
        return (true, isUpdate);
    }

    public async Task InsertOrUpdateAsync(IEnumerable<IAuthenticatorDTO> items, bool isLocal, string? secondaryPassword = null,
        bool verifiedWithHello = false)
    {
        var batch = items.ToArray();
        if (batch.Length == 0)
            return;
        var snapshot = await GetAllSourceAsync().ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        var hasPassword = !string.IsNullOrEmpty(secondaryPassword);
        var windowsHello = snapshot.Any(RequiresWindowsHello);
        if (snapshot.Length != 0 &&
            (isLocal != snapshot.Any(x => !x.IsNotLocal || HasLegacyLocal(x.Value) || HasLegacyLocal(x.Name)) ||
             hasPassword != snapshot.Any(x => x.IsNeedSecondaryPassword || HasLegacyPassword(x.Value) || HasLegacyPassword(x.Name))))
            throw new InvalidOperationException("Use the vault protection switch to change the configured mode.");

        using var keys = new Keys(secondaryPassword, verifiedWithHello);
        byte[]? salt = null;
        foreach (var row in snapshot)
        {
            // Check the actual credentials, not only the flags, before replacing or adding rows.
            var (name, value) = await ReadPayloadAsync(row, secondaryPassword, keys).ConfigureAwait(false);
            CryptographicOperations.ZeroMemory(name);
            CryptographicOperations.ZeroMemory(value);
            if (hasPassword)
                salt ??= PasswordSalt(row.Value);
        }
        if (hasPassword)
            salt ??= NewSalt();

        var encoded = new AccountPlatformAuthenticator[batch.Length];
        var knownIds = new HashSet<ushort>(snapshot.Select(x => x.Id));
        var seenIds = new HashSet<ushort>();
        for (var i = 0; i < batch.Length; i++)
        {
            var id = batch[i].Id;
            if (id != 0 && (!knownIds.Contains(id) || !seenIds.Add(id)))
                throw new InvalidOperationException("The authenticator update does not identify a unique existing row.");
            encoded[i] = await EncodeAsync(batch[i], isLocal, salt, keys, windowsHello).ConfigureAwait(false);
        }

        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        await dbConnection.RunInTransactionAsync(connection =>
        {
            VerifySnapshot(connection, snapshot);
            foreach (var row in encoded)
            {
                if ((row.Id == 0 ? connection.Insert(row) : connection.Update(row)) != 1)
                    throw new InvalidOperationException("An authenticator could not be saved.");
            }
        }).ConfigureAwait(false);
        for (var i = 0; i < batch.Length; i++)
            batch[i].Id = encoded[i].Id;
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

    public Task RenameAsync(AccountPlatformAuthenticator source, string name, bool isLocal, string? secondaryPassword = null,
        bool verifiedWithHello = false)
        => RenameAsync(source.Id, name, isLocal, secondaryPassword, verifiedWithHello);

    public async Task RenameAsync(ushort id, string name, bool isLocal, string? secondaryPassword = null,
        bool verifiedWithHello = false)
    {
        var snapshot = await GetAllSourceAsync().ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        var row = snapshot.FirstOrDefault(x => x.Id == id);
        if (row == null)
            return;

        var local = !row.IsNotLocal;
        var hasPassword = row.IsNeedSecondaryPassword;
        if ((!local && (HasLegacyLocal(row.Name) || HasLegacyLocal(row.Value))) ||
            (!hasPassword && (HasLegacyPassword(row.Name) || HasLegacyPassword(row.Value))))
            throw new AuthenticatorReadException(ImportResultCode.IncorrectFormat);

        using var keys = new Keys(secondaryPassword, verifiedWithHello);
        var salt = hasPassword ? PasswordSalt(row.Value) ?? snapshot.Select(x => PasswordSalt(x.Value)).FirstOrDefault(x => x != null) ?? NewSalt() : null;
        var (oldName, value) = await ReadPayloadAsync(row, secondaryPassword, keys).ConfigureAwait(false);
        var replacement = new AccountPlatformAuthenticator { Id = id };
        try
        {
            var newName = StrictUtf8.GetBytes(name);
            try
            {
                await SetPayloadAsync(replacement, newName, value, local, salt, keys, RequiresWindowsHello(row)).ConfigureAwait(false);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(newName);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(oldName);
            CryptographicOperations.ZeroMemory(value);
        }
        await ReplaceProtectionAsync(snapshot, new[] { replacement }).ConfigureAwait(false);
    }

    public Task SetServerIdAsync(AccountPlatformAuthenticator source, Guid serverId) => SetServerIdAsync(source.Id, serverId);

    public async Task SetServerIdAsync(ushort id, Guid serverId)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        await AttemptAndRetry(t =>
        {
            t.ThrowIfCancellationRequested();
            return dbConnection.ExecuteAsync(
                $"UPDATE {TableSql} SET [{AccountPlatformAuthenticator.ColumnName_ServerId}] = ? WHERE {IdSql} = ?", serverId, id);
        }, cancellationToken: CancellationToken.None).ConfigureAwait(false);
    }

    public async Task SwitchEncryptionModeAsync(bool isLocal, string? currentPassword, string? newPassword,
        bool windowsHello = false, bool verifiedWithHello = false)
    {
        if (windowsHello && (!OperatingSystem.IsWindows() || !isLocal || !string.IsNullOrEmpty(newPassword) || !verifiedWithHello))
            throw new InvalidOperationException("Windows Hello requires verified current-user protection without a password.");

        var snapshot = await GetAllSourceAsync().ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        if (snapshot.Length == 0)
            return;

        using var currentKeys = new Keys(currentPassword, verifiedWithHello);
        using var newKeys = new Keys(newPassword, verifiedWithHello);
        var salt = string.IsNullOrEmpty(newPassword) ? null : NewSalt();
        var replacements = new AccountPlatformAuthenticator[snapshot.Length];
        for (var i = 0; i < snapshot.Length; i++)
        {
            var row = snapshot[i];
            var (name, value) = await ReadPayloadAsync(row, currentPassword, currentKeys).ConfigureAwait(false);
            try
            {
                // Validate the actual payload without reserializing it or creating a label string.
                try
                {
                    _ = StrictUtf8.GetCharCount(name);
                }
                catch (DecoderFallbackException)
                {
                    throw new AuthenticatorReadException(ImportResultCode.UTF8GetStringFail);
                }
                if (DecodeValue(value).SecretKey is { } secret)
                    CryptographicOperations.ZeroMemory(secret);
                var replacement = new AccountPlatformAuthenticator { Id = row.Id };
                await SetPayloadAsync(replacement, name, value, isLocal, salt, newKeys, windowsHello).ConfigureAwait(false);
                replacements[i] = replacement;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(name);
                CryptographicOperations.ZeroMemory(value);
            }
        }
        await ReplaceProtectionAsync(snapshot, replacements).ConfigureAwait(false);
    }

    async Task ReplaceProtectionAsync(AccountPlatformAuthenticator[] snapshot, AccountPlatformAuthenticator[] replacements)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        await dbConnection.RunInTransactionAsync(connection =>
        {
            VerifySnapshot(connection, snapshot);
            foreach (var row in replacements)
            {
                if (connection.Execute(ProtectionUpdateSql, row.Name, row.Value, row.IsNotLocal, row.IsNeedSecondaryPassword, row.Id) != 1)
                    throw new InvalidOperationException("Authenticator protection could not be saved.");
            }
        }).ConfigureAwait(false);
    }

    static AccountPlatformAuthenticator[]? Import(byte[] content)
    {
        try
        {
            return AuthenticatorPayloadSerializer.ReadBackup(content);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return null;
        }
    }

    async Task<List<AccountPlatformAuthenticator>> ExportCoreAsync(bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO> items)
    {
        var batch = items as IAuthenticatorDTO[] ?? items.ToArray();
        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        using var keys = new Keys(secondaryPassword);
        var salt = string.IsNullOrEmpty(secondaryPassword) ? null : NewSalt();
        var list = new List<AccountPlatformAuthenticator>();
        foreach (var item in batch)
        {
            var entity = await EncodeAsync(item, isLocal, salt, keys).ConfigureAwait(false);
            list.Add(entity);
        }
        return list;
    }

    public async Task<byte[]> ExportAsync(bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO> items)
    {
        var list = await ExportCoreAsync(isLocal, secondaryPassword, items).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        return AuthenticatorPayloadSerializer.WriteBackup(list);
    }

    public async Task ExportAsync(Stream stream, bool isLocal, string? secondaryPassword, IEnumerable<IAuthenticatorDTO> items)
    {
        var list = await ExportCoreAsync(isLocal, secondaryPassword, items).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        await AuthenticatorPayloadSerializer.WriteBackupAsync(stream, list).ConfigureAwait(false);
    }

    public async Task<(ImportResultCode resultCode, IReadOnlyList<IAuthenticatorDTO> result, int sourcesCount)> ImportAsync(string? secondaryPassword, byte[] content)
    {
        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        var sources = Import(content);
        if (sources == null || sources.Length == 0 || sources.Any(x => x == null))
            return (ImportResultCode.IncorrectFormat, Array.Empty<IAuthenticatorDTO>(), sources?.Length ?? 0);

        try
        {
            var result = await ConvertToListAsync(sources, secondaryPassword).ConfigureAwait(false);
            foreach (var item in result)
                item.Index = 0;
            return (ImportResultCode.Success, result, sources.Length);
        }
        catch (AuthenticatorReadException ex)
        {
            return (ex.ResultCode, Array.Empty<IAuthenticatorDTO>(), sources.Length);
        }
    }

    public async Task UpdateOrderAsync(IReadOnlyList<(ushort Id, int Index)> order)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        await dbConnection.RunInTransactionAsync(connection =>
        {
            var count = connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM [{AccountPlatformAuthenticator.TableName}]");
            if (count != order.Count)
                throw new InvalidOperationException("Authenticator list changed while saving its order.");
            foreach (var (id, index) in order)
            {
                var changed = connection.Execute(
                    $"UPDATE [{AccountPlatformAuthenticator.TableName}] SET [{AccountPlatformAuthenticator.ColumnName_Index}] = ? WHERE [{AccountPlatformAuthenticator.ColumnName_Id}] = ?",
                    index, id);
                if (changed != 1)
                    throw new InvalidOperationException("Authenticator order could not be saved.");
            }
        }).ConfigureAwait(false);
    }

}