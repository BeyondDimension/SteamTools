using System.Application.Entities;
using System.Application.Models;
using System.Application.Security;
using System.Application.Services;
using System.Collections.Generic;
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

        async Task<GameAccountPlatformAuthenticator[]> GetAllAsync()
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() =>
            {
                return dbConnection.Table<GameAccountPlatformAuthenticator>().Take(IGameAccountPlatformAuthenticatorRepository.MaxValue).ToArrayAsync();
            }).ConfigureAwait(false);
        }

        async Task<IGameAccountPlatformAuthenticatorDTO?> Convert(GameAccountPlatformAuthenticator item, string? secondaryPassword)
        {
            var value_bytes = await ss.DB(item.Value, secondaryPassword);
            if (value_bytes == null) return null;

            var name_str = await ss.D(item.Name, secondaryPassword);
            if (name_str == null) return null;

            IGameAccountPlatformAuthenticatorValueDTO? value;
            try
            {
                value = Serializable.DMP<IGameAccountPlatformAuthenticatorValueDTO>(value_bytes);
                if (value == null) return null;
            }
            catch
            {
                return null;
            }

            return new GameAccountPlatformAuthenticatorDTO
            {
                Id = item.Id,
                Name = name_str,
                ServerId = item.ServerId,
                Value = value,
            };
        }

        async IAsyncEnumerable<IGameAccountPlatformAuthenticatorDTO?> Convert(IEnumerable<GameAccountPlatformAuthenticator> items, string? secondaryPassword)
        {
            foreach (var item in items)
            {
                var value = await Convert(item, secondaryPassword);
                yield return value;
            }
        }

        public async Task<List<IGameAccountPlatformAuthenticatorDTO>> GetAllAsync(string? secondaryPassword)
        {
            var items = await GetAllAsync();

            var query = Convert(items, secondaryPassword);

            var list = new List<IGameAccountPlatformAuthenticatorDTO>();

            await foreach (var item in query)
            {
                if (item != null)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        static EncryptionMode GetEncryptionMode(bool isLocal, string? secondaryPassword)
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
            return encryptionMode;
        }

        public async Task InsertOrUpdateAsync(IGameAccountPlatformAuthenticatorDTO item, bool isLocal, string? secondaryPassword)
        {
            var value = Serializable.SMP(item.Value);

            var encryptionMode = GetEncryptionMode(isLocal, secondaryPassword);

            var name_bytes = await ss.E(item.Name, encryptionMode, secondaryPassword);
            name_bytes = name_bytes.ThrowIsNull(nameof(name_bytes));

            var value_bytes = await ss.EB(value, encryptionMode, secondaryPassword);
            value_bytes = value_bytes.ThrowIsNull(nameof(value_bytes));

            var entity = new GameAccountPlatformAuthenticator
            {
                Id = item.Id,
                Name = name_bytes,
                ServerId = item.ServerId,
                Value = value_bytes,
            };

            await InsertOrUpdateAsync(entity);

            item.Id = entity.Id;
        }

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

        public async Task RenameAsync(ushort id, string name, bool isLocal, string? secondaryPassword)
        {
            var item = await FindAsync(id);
            if (item != null)
            {
                var encryptionMode = GetEncryptionMode(isLocal, secondaryPassword);

                var name_bytes = await ss.E(name, encryptionMode, secondaryPassword);
                name_bytes = name_bytes.ThrowIsNull(nameof(name_bytes));

                item.Name = name_bytes;
                await UpdateAsync(item);
            }
        }

        public async Task SetServerIdAsync(ushort id, Guid serverId)
        {
            var item = await FindAsync(id);
            if (item != null)
            {
                item.ServerId = serverId;
                await UpdateAsync(item);
            }
        }

        public async Task SwitchEncryptionModeAsync(bool isLocal, string? secondaryPassword, IEnumerable<IGameAccountPlatformAuthenticatorDTO>? items)
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