// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

internal sealed class ScriptRepository : Repository<Script, int>, IScriptRepository
{
    public async Task<bool> ExistsScriptAsync(string md5, string sha512)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async () =>
        {
            return (await dbConnection.Table<Script>().CountAsync(x => x.MD5 == md5 && x.SHA512 == sha512)) > 0;
        });
    }

    public async Task SaveScriptEnableAsync(ScriptDTO item)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        await AttemptAndRetry(async () =>
        {
            const string sql_ =
            $"{SQLStrings.Update}[{Script.TableName}] " +
                $"set [{Script.ColumnName_Enable}] = {{0}} " +
                $"where [{Script.ColumnName_Id}] = {{1}}";
            var sql = string.Format(sql_, item.Enable, item.LocalId);
            //var sql =
            //    SQLStrings.Update +
            //    "[" + Script.TableName + "]" +
            //    " set " +
            //    "[" + Script.ColumnName_Enable + "]" +
            //    $" = {item.Enable}" +
            //    " where " +
            //    "[" + Script.ColumnName_Id + "]" +
            //    $" = {item.LocalId}";
            var r = await dbConnection.ExecuteAsync(sql);
            return r;
        }).ConfigureAwait(false);
    }

    public async Task<IList<Script>> GetAllAsync()
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async () =>
        {
            return await dbConnection
                         .Table<Script>()
                         .OrderBy(x => x.Order)
                         .Take(IScriptRepository.MaxValue)
                         .ToArrayAsync();
        }).ConfigureAwait(false);
    }

    public async Task<Script> ExistsScriptNameAsync(string name)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async () =>
        {
            return await dbConnection
                         .Table<Script>()
                         .FirstOrDefaultAsync(x => x.Name == name);
        }).ConfigureAwait(false);
    }
}