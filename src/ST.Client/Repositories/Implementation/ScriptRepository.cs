using AutoMapper;
using System;
using System.Application.Entities;
using System.Application.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Repositories.Implementation
{
    internal sealed class ScriptRepository : Repository<Script, int>, IScriptRepository
    {
        public async Task<bool> ExistsScript(string md5, string sha512)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(async () =>
            {
                return (await dbConnection.Table<Script>().CountAsync(x => x.MD5 == md5 && x.SHA512 == sha512)) > 0;
            });
        }
        public async Task SaveScriptEnable(ScriptDTO item) {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            await AttemptAndRetry(async () =>
            {
                var sql =
                    SQLStrings.Update +
                    "[" + Script.TableName + "]" +
                    " set " +
                    "[" + Script.ColumnName_Enable + "]" +
                    $" = {item.Enable}" +
                    " where " +
                    "[" + Script.ColumnName_Id + "]" +
                    $" = {item.LocalId}";
                var r = await dbConnection.ExecuteAsync(sql);
                return r;
            }).ConfigureAwait(false);
        }
        public async Task<IList<Script>> GetAllAsync()
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(async () =>
            {
                return await dbConnection.Table<Script>().OrderBy(x => x.Order).Take(IScriptRepository.MaxValue).ToArrayAsync();
            }).ConfigureAwait(false);
        }

        public async Task<Script> ExistsScriptName(string name)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(async () =>
            {
                return await dbConnection.Table<Script>().FirstOrDefaultAsync(x => x.Name == name);
            }).ConfigureAwait(false);
        }
    }
}
