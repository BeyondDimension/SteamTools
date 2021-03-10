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
                Name = item.Name,
                ServerId = item.ServerId,
                Platform = value.Platform,
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

        public async Task InsertOrUpdateAsync(IGameAccountPlatformAuthenticatorDTO item, bool isLocal, string? secondaryPassword = null)
        {
            var value = Serializable.SMP(item.Value);

            var encryptionMode = GetEncryptionMode(isLocal, secondaryPassword);

            var value_bytes = await ss.EB(value, encryptionMode, secondaryPassword);

            var entity = new GameAccountPlatformAuthenticator
            {
                Id = item.Id,
                Name = item.Name,
                ServerId = item.ServerId,
                Value = value_bytes.ThrowIsNull(nameof(value_bytes)),
            };

            await InsertOrUpdateAsync(entity);
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
                    + " = {0}",
                    serverId);
            }).ConfigureAwait(false);
        }

        async Task IGameAccountPlatformAuthenticatorRepository.DeleteAsync(Guid serverId)
            => await DeleteAsync(serverId);

        public async Task RenameAsync(ushort id, string name)
        {
            var item = await FindAsync(id);
            if (item != null)
            {
                item.Name = name;
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