// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

internal sealed class ScriptRepository : Repository<Script, int>, IScriptRepository
{
    public async Task<bool> ExistsScriptAsync(string md5, string sha512, CancellationToken cancellationToken)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            return (await dbConnection.Table<Script>().CountAsync(x => x.MD5 == md5 && x.SHA512 == sha512)) > 0;
        }, cancellationToken: cancellationToken);
    }

    public async Task SaveScriptEnableAsync(ScriptDTO item, CancellationToken cancellationToken)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            const string sql_ =
            $"{SQLStrings.Update}[{Script.TableName}] " +
                $"set [{Script.ColumnName_Enable}] = {{0}} " +
                $"where [{Script.ColumnName_Id}] = {{1}}";
            var sql = string.Format(sql_, !item.Disable, item.LocalId);
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
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveScriptCachePathAsync(ScriptDTO item, CancellationToken cancellationToken)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            const string sql_ =
            $"{SQLStrings.Update}[{Script.TableName}] " +
                $"set [{Script.ColumnName_CachePath}] = '{{0}}' " +
                $"where [{Script.ColumnName_Id}] = {{1}}";
            var sql = string.Format(sql_, item.CachePath, item.LocalId);
            var r = await dbConnection.ExecuteAsync(sql);
            return r;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<Script>> GetAllAsync(CancellationToken cancellationToken)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            return await dbConnection
                         .Table<Script>()
                         .OrderBy(x => x.Order)
                         .Take(IScriptRepository.MaxValue)
                         .ToArrayAsync();
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<Script> ExistsScriptNameAsync(string name, CancellationToken cancellationToken)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            return await dbConnection
                         .Table<Script>()
                         .FirstOrDefaultAsync(x => x.Name == name);
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}