using System.Application.Columns;
using System.Application.Entities;
using System.Application.Models;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using static System.Application.Repositories.IGameAccountPlatformAuthenticatorRepository;
using static System.Application.Services.ISecurityService;

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
                return dbConnection.Table<GameAccountPlatformAuthenticator>().Take(MaxValue).ToArrayAsync();
            }).ConfigureAwait(false);
        }

        public async Task<GameAccountPlatformAuthenticator?> GetFirstOrDefaultSourceAsync()
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() =>
            {
                return dbConnection.Table<GameAccountPlatformAuthenticator>().FirstOrDefaultAsync();
            }).ConfigureAwait(false);
        }

        public async Task<bool> HasLocalAsync()
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(async () =>
            {
                var item = await dbConnection.Table<GameAccountPlatformAuthenticator>().FirstOrDefaultAsync();
                if (item != null)
                {
                    item = await dbConnection.Table<GameAccountPlatformAuthenticator>().FirstOrDefaultAsync(x => !x.IsNotLocal);
                    return item != null;
                }
                return false;
            }).ConfigureAwait(false);
        }

        public async Task<bool> HasSecondaryPasswordAsync()
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(async () =>
            {
                var item = await dbConnection.Table<GameAccountPlatformAuthenticator>().FirstOrDefaultAsync();
                if (item != null)
                {
                    item = await dbConnection.Table<GameAccountPlatformAuthenticator>().FirstOrDefaultAsync(x => !x.IsNeedSecondaryPassword);
                    return item != null;
                }
                return false;
            }).ConfigureAwait(false);
        }

        static int GetOrder(IOrderGAPAuthenticator item)
        {
            var index = item.Index == default ? item.Id : item.Index;
            return index;
        }

        async Task<IGAPAuthenticatorDTO?> Convert(GameAccountPlatformAuthenticator item, string? secondaryPassword)
        {
            (var value, var _) = await Convert2(item, secondaryPassword);
            return value;
        }

        static ImportResultCode Convert(DResultCode resultCode)
        {
            var resultCode_ = (int)resultCode;
            var resultCode__ = (ImportResultCode)resultCode_;
            return resultCode__;
        }

        async Task<(IGAPAuthenticatorDTO? value, ImportResultCode resultCode)> Convert2(GameAccountPlatformAuthenticator item, string? secondaryPassword)
        {
            var (value_bytes, result_code) = await ss.DB2(item.Value, secondaryPassword);
            if (result_code != DResultCode.Success) return (null, Convert(result_code));

            var (name_str, name_result_code) = await ss.D2(item.Name, secondaryPassword);
            if (name_result_code != DResultCode.Success) return (null, Convert(name_result_code));

            IGAPAuthenticatorValueDTO? value;
            try
            {
                value = Serializable.DMP<IGAPAuthenticatorValueDTO>(value_bytes!);
                if (value == null) return (null, ImportResultCode.Success);
            }
            catch
            {
                return (null, ImportResultCode.IncorrectFormat);
            }

            var index = GetOrder(item);
            var result = new GAPAuthenticatorDTO
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

        async IAsyncEnumerable<IGAPAuthenticatorDTO?> Convert(IEnumerable<GameAccountPlatformAuthenticator> sources, string? secondaryPassword)
        {
            foreach (var item in sources)
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

        async Task<GameAccountPlatformAuthenticator> Convert(IGAPAuthenticatorDTO item, bool isLocal,
           string? secondaryPassword = null)
        {
            var value = Serializable.SMP(item.Value);

            (var notSecondaryPassword, var encryptionMode) = GetEncryptionMode2(isLocal, secondaryPassword);

            var name_encryptionMode = GetEncryptionMode(isLocal, null);
            var name_bytes = await ss.E(item.Name ?? string.Empty, name_encryptionMode, null);

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
                Index = item.Index,
                Created = item.Created,
                LastUpdate = item.LastUpdate,
            };
            return entity;
        }

        public async Task InsertOrUpdateAsync(IGAPAuthenticatorDTO item, bool isLocal,
            string? secondaryPassword = null)
        {
            var entity = await Convert(item, isLocal, secondaryPassword);

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

        public async Task RenameAsync(GameAccountPlatformAuthenticator source, string name, bool isLocal)
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

        byte[] Export(IEnumerable<GameAccountPlatformAuthenticator> sources)
        {
            var result = Serializable.SMP(sources);
            return result;
        }

        IEnumerable<GameAccountPlatformAuthenticator>? Import(byte[] content)
        {
            try
            {
                var result = Serializable.DMP<IEnumerable<GameAccountPlatformAuthenticator>>(content);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<byte[]> ExportAsync(bool isLocal, string? secondaryPassword, IEnumerable<IGAPAuthenticatorDTO> items)
        {
            var list = new List<GameAccountPlatformAuthenticator>();
            foreach (var item in items)
            {
                var entity = await Convert(item, isLocal, secondaryPassword);
                list.Add(entity);
            }
            var result = Export(list);
            return result;
        }

        public async Task<(ImportResultCode resultCode, IReadOnlyList<IGAPAuthenticatorDTO> result, int sourcesCount)> ImportAsync(string? secondaryPassword, byte[] content)
        {
            int sourcesCount = 0;
            var resultCode = ImportResultCode.Success;
            IReadOnlyList<IGAPAuthenticatorDTO>? result = null;
            var sources = Import(content);
            if (sources == null)
            {
                resultCode = ImportResultCode.IncorrectFormat;
            }
            else
            {
                sourcesCount = sources.Count();
                var list = new List<IGAPAuthenticatorDTO>();
                foreach (var source in sources)
                {
                    (var item, var item_result_code) = await Convert2(source, secondaryPassword);
                    if (item_result_code != ImportResultCode.Success)
                    {
                        resultCode = item_result_code;
                        break;
                    }
                    if (item != null)
                    {
                        list.Add(item);
                    }
                    else
                    {
                        resultCode = ImportResultCode.PartSuccess;
                    }
                }
                result = list;
            }
            result ??= Array.Empty<IGAPAuthenticatorDTO>();
            if (!result.Any() && resultCode == ImportResultCode.Success)
                resultCode = ImportResultCode.IncorrectFormat;
            return (resultCode, result, sourcesCount);
        }

        async Task<int> UpdateIndexByItemAsync(IGAPAuthenticatorDTO item)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(async () =>
            {
                var sql =
                    SQLStrings.Update +
                    "[" + GameAccountPlatformAuthenticator.TableName + "]" +
                    " set " +
                    "[" + GameAccountPlatformAuthenticator.ColumnName_Index + "]" +
                    $" = {item.Index}" +
                    " where " +
                    "[" + GameAccountPlatformAuthenticator.ColumnName_Id + "]" +
                    $" = {item.Id}";
                var r = await dbConnection.ExecuteAsync(sql);
                return r;
            }).ConfigureAwait(false);

            //var source = await FindAsync(item.Id);
            //if (source != null)
            //{
            //    source.Index = item.Index;
            //    var r = await UpdateAsync(source);
            //    return r;
            //}
            //return 0;
        }

        public async Task<int> MoveOrderByIndexAsync(IList<IGAPAuthenticatorDTO> items, int index, bool upOrDown)
        {
            var item = items[index];
            var item2Index = upOrDown ? index - 1 : index + 1;
            if (item2Index > -1 && item2Index < items.Count)
            {
                var item2 = items[item2Index];
                var orderIndex = GetOrder(item);
                var orderIndex2 = GetOrder(item2);
                item.Index = orderIndex2;
                item2.Index = orderIndex;
                //var r = await UpdateIndexByItemAsync(item);
                //r += await UpdateIndexByItemAsync(item2);
                //return r;
                var r = await Task.WhenAll(
                    UpdateIndexByItemAsync(item),
                    UpdateIndexByItemAsync(item2));
                return r.Sum();
            }
            return 0;
        }
    }
}