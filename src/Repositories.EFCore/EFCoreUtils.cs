using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Application.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// EFCore 工具类
    /// </summary>
    public static class EFCoreUtils
    {
        /// <summary>
        /// 根据类型获取表名称，仅支持 SqlServer
        /// </summary>
        /// <param name="database"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static string GetTableNameByClrType(DatabaseFacade database, IEntityType entityType)
        {
            var tableName = entityType.GetTableName();
            if (tableName == null) throw new NullReferenceException(nameof(tableName));
            var schema = entityType.GetSchema();
            var databaseProviderName = database.ProviderName;
            return databaseProviderName switch
            {
                SQLStrings.SqlServer => $"[{(string.IsNullOrEmpty(schema) ? "dbo" : schema)}].[{tableName}]",
                _ => throw new ArgumentOutOfRangeException(nameof(databaseProviderName), databaseProviderName, "Currently only sqlserver is supported."),
            };
        }

        /// <summary>
        /// 根据类型获取表名称，仅支持 SqlServer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static string GetTableNameByClrType(DbContext context, Type entityType)
        {
            var entityType_ = context.Model.FindEntityType(entityType);
            return GetTableNameByClrType(context.Database, entityType_);
        }

        /// <summary>
        /// 获取SQL字符串 SET IDENTITY_INSERT 允许将显式值插入到表的标识列中，仅支持 SqlServer
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="enable"></param>
        public static string GetSqlSetIdentityInsert(IEFRepository repository, bool enable)
        {
            var tableName = repository.TableName;
            return SQLStrings.IDENTITY_INSERT(tableName, enable);
        }

        /// <summary>
        /// 执行SQL字符串 SET IDENTITY_INSERT 允许将显式值插入到表的标识列中，仅支持 SqlServer
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="enable"></param>
        public static int SetIdentityInsert(IEFRepository repository, bool enable)
        {
            var sql = GetSqlSetIdentityInsert(repository, enable);
            return repository.DbContext.Database.ExecuteSqlRaw(sql);
        }

        /// <summary>
        /// 执行SQL字符串 SET IDENTITY_INSERT 允许将显式值插入到表的标识列中，仅支持 SqlServer
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="enable"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<int> SetIdentityInsertAsync(IEFRepository repository, bool enable, CancellationToken cancellationToken = default)
        {
            var sql = GetSqlSetIdentityInsert(repository, enable);
            return repository.DbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        /// <summary>
        /// 允许将显式值插入到表的标识列中的 <see cref="DbContext.SaveChanges"/>，仅支持 SqlServer
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static int IdentityInsertSaveChanges(IEFRepository repository)
        {
            int result;
            repository.DbContext.Database.OpenConnection();
            try
            {
                SetIdentityInsert(repository, true);
                result = repository.DbContext.SaveChanges();
                SetIdentityInsert(repository, false);
            }
            finally
            {
                repository.DbContext.Database.CloseConnection();
            }
            return result;
        }

        /// <summary>
        /// 允许将显式值插入到表的标识列中的 <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>，仅支持 SqlServer
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<int> IdentityInsertSaveChangesAsync(IEFRepository repository, CancellationToken cancellationToken = default)
        {
            int result;
            await repository.DbContext.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                await SetIdentityInsertAsync(repository, true, cancellationToken);
                result = await repository.DbContext.SaveChangesAsync(cancellationToken);
                await SetIdentityInsertAsync(repository, false, cancellationToken);
            }
            finally
            {
                await repository.DbContext.Database.CloseConnectionAsync();
            }
            return result;
        }

        /// <summary>
        /// 允许将显式值插入到表的标识列中的 多个实体插入到数据库中，仅支持 SqlServer
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="repository"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static int IdentityInsertRange<TEntity>(
            IEFRepository<TEntity> repository,
            IEnumerable<TEntity> entities) where TEntity : class
        {
            repository.Entity.AddRange(entities);
            var result = IdentityInsertSaveChanges(repository);
            return result;
        }

        /// <summary>
        /// 允许将显式值插入到表的标识列中的 多个实体插入到数据库中，仅支持 SqlServer
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="repository"></param>
        /// <param name="entities"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<int> IdentityInsertRangeAsync<TEntity>(
            IEFRepository<TEntity> repository,
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            await repository.Entity.AddRangeAsync(entities, cancellationToken);
            var result = await IdentityInsertSaveChangesAsync(repository, cancellationToken);
            return result;
        }
    }
}