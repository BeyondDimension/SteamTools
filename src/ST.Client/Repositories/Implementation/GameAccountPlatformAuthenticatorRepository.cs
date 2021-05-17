using System.Application.Entities;
using System.Application.Models;
using System.Application.Services;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

namespace System.Application.Repositories.Implementation
{
    internal sealed class GameAccountPlatformAuthenticatorRepository : Repository<GameAccountPlatformAuthenticator, ushort>, IGameAccountPlatformAuthenticatorRepository
    {
        readonly ISecurityService ss;

        public GameAccountPlatformAuthenticatorRepository(ISecurityService ss)
        {
            this.ss = ss;
        }

        public async Task<GameAccountPlatformAuthenticator[]> GetAllSourceAsync()
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() =>
            {
                return dbConnection.Table<GameAccountPlatformAuthenticator>().Take(IGameAccountPlatformAuthenticatorRepository.MaxValue).ToArrayAsync();
            }).ConfigureAwait(false);
        }

        async Task<IGAPAuthenticatorDTO?> Convert(GameAccountPlatformAuthenticator item, string? secondaryPassword)
        {
            var value_bytes = await ss.DB(item.Value, secondaryPassword);
            if (value_bytes == null) return null;

            var name_str = await ss.D(item.Name, secondaryPassword);
            if (name_str == null) return null;

            IGAPAuthenticatorValueDTO? value;
            try
            {
                value = Serializable.DMP<IGAPAuthenticatorValueDTO>(value_bytes);
                if (value == null) return null;
            }
            catch
            {
                return null;
            }

            return new GAPAuthenticatorDTO
            {
                Id = item.Id,
                Name = name_str,
                ServerId = item.ServerId,
                Value = value,
            };
        }

        async IAsyncEnumerable<IGAPAuthenticatorDTO?> Convert(IEnumerable<GameAccountPlatformAuthenticator> items, string? secondaryPassword)
        {
            foreach (var item in items)
            {
                var value = await Convert(item, secondaryPassword);
                yield return value;
            }
        }

        public async Task<List<IGAPAuthenticatorDTO>> ConvertToList(IEnumerable<GameAccountPlatformAuthenticator> sources, string? secondaryPassword = null)
        {
            var query = Convert(sources, secondaryPassword);

            var list = new List<IGAPAuthenticatorDTO>();

            await foreach (var item in query)
            {
                if (item != null)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public async Task<List<IGAPAuthenticatorDTO>> GetAllAsync(string? secondaryPassword = null)
        {
            var sources = await GetAllSourceAsync();
            return await ConvertToList(sources);
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

        public async Task InsertOrUpdateAsync(IGAPAuthenticatorDTO item, bool isLocal,
            string? secondaryPassword = null)
        {
            var value = Serializable.SMP(item.Value);

            (var notSecondaryPassword, var encryptionMode) = GetEncryptionMode2(isLocal, secondaryPassword);

            var name_bytes = await ss.E(item.Name ?? string.Empty, encryptionMode, secondaryPassword);
            name_bytes = name_bytes.ThrowIsNull(nameof(name_bytes));

            var value_bytes = await ss.EB(value, encryptionMode, secondaryPassword);
            value_bytes = value_bytes.ThrowIsNull(nameof(value_bytes));

            var entity = new GameAccountPlatformAuthenticator
            {
                Id = item.Id,
                Name = name_bytes,
                ServerId = item.ServerId,
                Value = value_bytes,
                IsNotLocal = !isLocal,
                IsNeedSecondaryPassword = !notSecondaryPassword,
            };

            await InsertOrUpdateAsync(entity);

            item.Id = entity.Id;
        }

        //public override async Task<(int rowCount, DbRowExecResult result)> InsertOrUpdateAsync(GameAccountPlatformAuthenticator entity)
        //{
        //    var dbConnection = await GetDbConnection().ConfigureAwait(false);
        //    var rowCount = await AttemptAndRetry(() =>
        //    {
        //        if (entity.Id == default) return dbConnection.InsertAsync(entity);
        //        return dbConnection.InsertOrReplaceAsync(entity);
        //    }).ConfigureAwait(false);
        //    return (rowCount, DbRowExecResult.InsertOrUpdate);
        //}

        public new async Task DeleteAsync(ushort id) => await base.DeleteAsync(id);

        public async Task<int> DeleteAsync(Guid serverId)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() =>
            {
                return dbConnection.ExecuteAsync(
                    SQLStrings.DeleteFrom +
                    GameAccountPlatformAuthenticator.TableName +
                    " where " +
                    GameAccountPlatformAuthenticator.ColumnName_ServerId
                    + " = ?",
                    serverId);
            }).ConfigureAwait(false);
        }

        async Task IGameAccountPlatformAuthenticatorRepository.DeleteAsync(Guid serverId)
            => await DeleteAsync(serverId);

        public async Task RenameAsync(GameAccountPlatformAuthenticator source, string name, bool isLocal, string? secondaryPassword)
        {
            var encryptionMode = GetEncryptionMode(isLocal, secondaryPassword);

            var name_bytes = await ss.E(name, encryptionMode, secondaryPassword);
            name_bytes = name_bytes.ThrowIsNull(nameof(name_bytes));

            source.Name = name_bytes;
            await UpdateAsync(source);
        }

        public async Task RenameAsync(ushort id, string name, bool isLocal, string? secondaryPassword)
        {
            var source = await FindAsync(id);
            if (source != null)
            {
                await RenameAsync(source, name, isLocal, secondaryPassword);
            }
        }

        public async Task SetServerIdAsync(GameAccountPlatformAuthenticator source, Guid serverId)
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

        public async Task SwitchEncryptionModeAsync(bool isLocal, string? secondaryPassword, IEnumerable<IGAPAuthenticatorDTO>? items)
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
    }
}