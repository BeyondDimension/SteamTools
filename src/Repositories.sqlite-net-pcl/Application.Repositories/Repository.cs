using Polly;
using SQLite;
using System.Application.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Repositories
{
    /// <summary>
    /// 由 sqlite-net-pcl 实现的仓储层
    /// </summary>
    public abstract class Repository : IRepository
    {
        #region https://codetraveler.io/2019/11/26/efficiently-initializing-sqlite-database/

        static string? mDataBaseDirectory;

        /// <summary>
        /// 获取或设定数据库目录，默认值为 <see cref="IOPath.AppDataDirectory"/>
        /// </summary>
        public static string DataBaseDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mDataBaseDirectory)) mDataBaseDirectory = IOPath.AppDataDirectory;
                return mDataBaseDirectory;
            }
            set
            {
                mDataBaseDirectory = value;
            }
        }

        static SQLiteAsyncConnection GetConnection()
        {
            var dbPath = DataBaseDirectory;
            IOPath.DirCreateByNotExists(dbPath);
            dbPath = Path.Combine(dbPath, "application.dbf");
            return new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        }

        static readonly Lazy<SQLiteAsyncConnection> dbConnection = new Lazy<SQLiteAsyncConnection>(GetConnection);

        static SQLiteAsyncConnection DbConnection => dbConnection.Value;

        protected static async ValueTask<SQLiteAsyncConnection> GetDbConnection<T>()
        {
            if (!DbConnection.TableMappings.Any(x => x.MappedType == typeof(T)))
            {
                // On sqlite-net v1.6.0+, enabling write-ahead logging allows for faster database execution
                await DbConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
                await DbConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
            }
            return DbConnection;
        }

        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, int numRetries = 10)
        {
            return Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).ExecuteAsync(action);
            static TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromMilliseconds(Math.Pow(2, attemptNumber));
        }

        #endregion
    }

    /// <inheritdoc cref="Repository"/>
    public abstract class Repository<TEntity> : Repository, IRepository<TEntity> where TEntity : class, new()
    {
        protected static ValueTask<SQLiteAsyncConnection> GetDbConnection() => GetDbConnection<TEntity>();

        #region 增(Insert Funs) 立即执行并返回受影响的行数

        public virtual async Task<int> InsertAsync(TEntity entity)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() => dbConnection.InsertAsync(entity)).ConfigureAwait(false);
        }

        public virtual async Task<int> InsertRangeAsync(IEnumerable<TEntity> entities)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() => dbConnection.InsertAllAsync(entities)).ConfigureAwait(false);
        }

        #endregion

        #region 删(Delete Funs) 立即执行并返回受影响的行数

        public virtual async Task<int> DeleteAsync(TEntity entity)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() => dbConnection.DeleteAsync(entity)).ConfigureAwait(false);
        }

        #endregion

        #region 改(Update Funs) 立即执行并返回受影响的行数

        public virtual async Task<int> UpdateAsync(TEntity entity)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() => dbConnection.UpdateAsync(entity)).ConfigureAwait(false);
        }

        public virtual async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() => dbConnection.UpdateAllAsync(entities)).ConfigureAwait(false);
        }

        #endregion

        #region 查(通用查询)

        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() => dbConnection.FindAsync(predicate)).ConfigureAwait(false);
        }

        #endregion
    }

    /// <inheritdoc cref="IRepository"/>
    public abstract class Repository<TEntity, TPrimaryKey> : Repository<TEntity>, IRepository<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>, new()
        where TPrimaryKey : IEquatable<TPrimaryKey>
    {
        object IGetPrimaryKey<TEntity>.GetPrimaryKey(TEntity entity) => GetPrimaryKey(entity);

        protected TPrimaryKey GetPrimaryKey(TEntity entity)
        {
            IGetPrimaryKey<TEntity, TPrimaryKey> getPrimaryKey = this;
            return getPrimaryKey.GetPrimaryKey(entity);
        }

        #region 删(Delete Funs) 立即执行并返回受影响的行数

        public override Task<int> DeleteAsync(TEntity entity)
        {
            var primaryKey = GetPrimaryKey(entity);
            return DeleteAsync(primaryKey);
        }

        public virtual async Task<int> DeleteAsync(TPrimaryKey primaryKey)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() => dbConnection.DeleteAsync<TEntity>(primaryKey)).ConfigureAwait(false);
        }

        #endregion

        #region 查(通用查询)

        public virtual async ValueTask<TEntity?> FindAsync(TPrimaryKey primaryKey, CancellationToken cancellationToken = default)
        {
            if (IRepository<TEntity, TPrimaryKey>.IsDefault(primaryKey)) return default;
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            return await AttemptAndRetry(() => dbConnection.FindAsync<TEntity>(primaryKey)).ConfigureAwait(false);
        }

        public virtual async ValueTask<bool> ExistAsync(TPrimaryKey primaryKey, CancellationToken cancellationToken = default)
        {
            if (IRepository<TEntity, TPrimaryKey>.IsDefault(primaryKey)) return false;
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            var count = await AttemptAndRetry(CountAsync).ConfigureAwait(false);
            return count > 0;
            Task<int> CountAsync() => dbConnection.Table<TEntity>().CountAsync(IRepository<TEntity, TPrimaryKey>.LambdaEqualId(primaryKey));
        }

        #endregion

        #region 增或改(InsertOrUpdate Funs) 立即执行并返回受影响的行数

        public virtual async Task<(int rowCount, DbRowExecResult result)> InsertOrUpdateAsync(TEntity entity)
        {
            var dbConnection = await GetDbConnection().ConfigureAwait(false);
            var rowCount = await AttemptAndRetry(() =>
            {
                var primaryKey = GetPrimaryKey(entity);
                if (IRepository<TEntity, TPrimaryKey>.IsDefault(primaryKey))
                {
                    return dbConnection.InsertAsync(entity);
                }
                else
                {
                    return dbConnection.InsertOrReplaceAsync(entity);
                }
            }).ConfigureAwait(false);
            return (rowCount, DbRowExecResult.InsertOrUpdate);
        }

        #endregion
    }
}