namespace BD.WTTS.Repositories;

public abstract class CacheRepository<[DynamicallyAccessedMembers(IEntity.DynamicallyAccessedMemberTypes)] TEntity, TPrimaryKey> : Repository<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>, new()
    where TPrimaryKey : IEquatable<TPrimaryKey>
{
    const string fileName = "cache.dbf";

    static SQLiteAsyncConnection GetConnection()
    {
        var dbPath = Path.Combine(IOPath.CacheDirectory, fileName);
        return GetConnection(dbPath);
    }

    static readonly Lazy<SQLiteAsyncConnection> dbConnection = new(GetConnection);

    protected sealed override async ValueTask<SQLiteAsyncConnection> GetDbConnection()
    {
        var connection = dbConnection.Value;
        await GetDbConnection<TEntity>(connection);
        return connection;
    }
}
