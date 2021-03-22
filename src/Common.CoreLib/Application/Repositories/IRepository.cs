using System.Application.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace System.Application.Repositories
{
    /// <summary>
    /// 仓储接口定义
    /// </summary>
    public interface IRepository
    {
        protected static async Task<int> OperateRangeAsync<T>(
          IEnumerable<T> entities,
          Func<T, Task<int>> operate)
          => (await Task.WhenAll(entities.Select(operate))).Sum();

        protected static async IAsyncEnumerable<(int rowCount, T entity)> OperateRangeAsyncEnumerable<T>(
          IEnumerable<T> entities,
          Func<T, Task<int>> operate)
        {
            foreach (var entity in entities)
            {
                var rowCount = await operate(entity);
                yield return (rowCount, entity);
            }
        }

        protected static async IAsyncEnumerable<(int rowCount, DbRowExecResult result, T entity)> OperateRangeAsyncEnumerable<T>(
            IEnumerable<T> entities,
            Func<T, Task<(int rowCount, DbRowExecResult result)>> operate)
        {
            foreach (var entity in entities)
            {
                var (rowCount, logic) = await operate(entity);
                yield return (rowCount, logic, entity);
            }
        }
    }

    /// <inheritdoc cref="IRepository"/>
    public interface IRepository<TEntity> : IRepository where TEntity : class
    {
        #region 增(Insert Funs) 立即执行并返回受影响的行数

        /// <summary>
        /// 将实体插入到数据库中
        /// </summary>
        /// <param name="entity">要添加的实体</param>
        /// <returns>受影响的行数</returns>
        Task<int> InsertAsync(TEntity entity);

        /// <summary>
        /// 将多个实体插入到数据库中
        /// </summary>
        /// <param name="entities">要添加的多个实体</param>
        /// <returns>受影响的行数</returns>
        public Task<int> InsertRangeAsync(IEnumerable<TEntity> entities) => OperateRangeAsync(entities, InsertAsync);

        /// <inheritdoc cref="InsertRangeAsync(IEnumerable{TEntity})"/>
        public Task<int> InsertRangeAsync(params TEntity[] entities) => InsertRangeAsync(entities.AsEnumerable());

        #endregion

        #region 删(Delete Funs) 立即执行并返回受影响的行数

        /// <summary>
        /// 将实体从数据库中删除
        /// </summary>
        /// <param name="entity">要删除的实体</param>
        /// <returns>受影响的行数</returns>
        Task<int> DeleteAsync(TEntity entity);

        /// <summary>
        /// 将多个实体从数据库中删除
        /// </summary>
        /// <param name="entities">要删除的多个实体</param>
        /// <returns>受影响的行数</returns>
        public Task<int> DeleteRangeAsync(IEnumerable<TEntity> entities) => OperateRangeAsync(entities, DeleteAsync);

        /// <inheritdoc cref="DeleteRangeAsync(IEnumerable{TEntity})"/>
        public Task<int> DeleteRangeAsync(params TEntity[] entities) => DeleteRangeAsync(entities.AsEnumerable());

        #endregion

        #region 改(Update Funs) 立即执行并返回受影响的行数

        /// <summary>
        /// 将实体更新到数据库中
        /// </summary>
        /// <param name="entity">要更新的实体</param>
        /// <returns>受影响的行数</returns>
        Task<int> UpdateAsync(TEntity entity);

        /// <summary>
        /// 将多个实体更新到数据库中
        /// </summary>
        /// <param name="entities">要更新的多个实体</param>
        /// <returns>受影响的行数</returns>
        public Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities) => OperateRangeAsync(entities, UpdateAsync);

        /// <inheritdoc cref="UpdateRangeAsync(IEnumerable{TEntity})"/>
        public Task<int> UpdateRangeAsync(params TEntity[] entities) => UpdateRangeAsync(entities.AsEnumerable());

        #endregion

        #region 查(通用查询)

        /// <summary>
        /// 返回序列中满足指定的条件或默认值，如果找到这样的元素的第一个元素
        /// </summary>
        /// <param name="predicate">用于测试每个元素是否满足条件的函数</param>
        /// <returns>
        /// default(TEntity) 如果 集合 为空，或者如果没有元素通过由指定的测试 <paramref name="predicate" />;
        /// 否则为中的第一个元素 集合 通过由指定的测试 <paramref name="predicate" />
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 集合 或 <paramref name="predicate" /> 为 <see langword="null" />
        /// </exception>
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        #endregion
    }

    /// <inheritdoc cref="IRepository"/>
    public interface IRepository<TEntity, TPrimaryKey> : IRepository<TEntity>, IGetPrimaryKey<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
        where TPrimaryKey : IEquatable<TPrimaryKey>
    {
        protected static bool IsDefault(TPrimaryKey primaryKey)
        {
            if (primaryKey == null) return true; // null is default
            TPrimaryKey defPrimaryKey = default;
            if (defPrimaryKey == null) return false; // primaryKey not null
            return EqualityComparer<TPrimaryKey>.Default.Equals(primaryKey, defPrimaryKey);
        }

        protected static Expression<Func<TEntity, bool>> LambdaEqualId(TPrimaryKey primaryKey)
        {
            var parameter = Expression.Parameter(typeof(TEntity));
            var left = Expression.PropertyOrField(parameter, nameof(IEntity<TPrimaryKey>.Id));
            var right = Expression.Constant(primaryKey, typeof(TPrimaryKey));
            var body = Expression.Equal(left, right);
            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }

        #region 删(Delete Funs) 立即执行并返回受影响的行数

        /// <summary>
        /// 根据主键将实体从数据库中删除
        /// </summary>
        /// <param name="primaryKey">要删除的实体主键</param>
        /// <returns>受影响的行数</returns>
        Task<int> DeleteAsync(TPrimaryKey primaryKey);

        /// <summary>
        /// 根据主键将多个实体从数据库中删除
        /// </summary>
        /// <param name="primaryKeys">要删除的多个实体主键</param>
        /// <returns>受影响的行数</returns>
        public Task<int> DeleteRangeAsync(IEnumerable<TPrimaryKey> primaryKeys) => OperateRangeAsync(primaryKeys, DeleteAsync);

        /// <inheritdoc cref="DeleteRangeAsync(IEnumerable{TPrimaryKey})"/>
        public Task<int> DeleteRangeAsync(params TPrimaryKey[] primaryKeys) => DeleteRangeAsync(primaryKeys.AsEnumerable());

        /// <inheritdoc cref="DeleteRangeAsync(IEnumerable{TPrimaryKey})"/>
        public IAsyncEnumerable<(int rowCount, TPrimaryKey entity)> DeleteRangeAsyncEnumerable(IEnumerable<TPrimaryKey> primaryKeys)
            => OperateRangeAsyncEnumerable(primaryKeys, DeleteAsync);

        /// <inheritdoc cref="DeleteRangeAsync(IEnumerable{TPrimaryKey})"/>
        public IAsyncEnumerable<(int rowCount, TPrimaryKey entity)> DeleteRangeAsyncEnumerable(params TPrimaryKey[] primaryKeys)
            => DeleteRangeAsyncEnumerable(primaryKeys.AsEnumerable());

        #endregion

        #region 查(通用查询)

        /// <summary>
        /// 根据主键查询实体
        /// </summary>
        /// <param name="primaryKey">要查询的实体主键</param>
        /// <returns>查询到的实体</returns>
        ValueTask<TEntity?> FindAsync(TPrimaryKey primaryKey);

        /// <summary>
        /// 判断实体是否已经存在
        /// </summary>
        /// <param name="entity">要查询的实体</param>
        /// <param name="primaryKey">要查询的实体主键</param>
        /// <returns>实体是否存在数据库中</returns>
        public async ValueTask<bool> ExistAsync(TPrimaryKey primaryKey)
        {
            var entity = await FindAsync(primaryKey);
            return entity != null;
        }

        /// <inheritdoc cref="ExistAsync(TPrimaryKey)"/>
        public ValueTask<bool> ExistAsync(TEntity entity)
        {
            var primaryKey = GetPrimaryKey(entity);
            return ExistAsync(primaryKey);
        }

        #endregion

        #region 增或改(InsertOrUpdate Funs) 立即执行并返回受影响的行数

        /// <summary>
        /// 新增或更新实体
        /// </summary>
        /// <param name="entity">要新增或更新的实体</param>
        /// <returns>受影响的行数与数据库逻辑</returns>
        public async Task<(int rowCount, DbRowExecResult result)> InsertOrUpdateAsync(TEntity entity)
        {
            var exist = await ExistAsync(entity);
            int rowCount;
            DbRowExecResult result;
            if (exist)
            {
                rowCount = await UpdateAsync(entity);
                result = DbRowExecResult.Update;
            }
            else
            {
                rowCount = await InsertAsync(entity);
                result = DbRowExecResult.Insert;
            }
            return (rowCount, result);
        }

        /// <summary>
        /// 批量新增或更新实体
        /// </summary>
        /// <param name="entities">要新增或更新的多个实体</param>
        /// <returns></returns>
        public IAsyncEnumerable<(int rowCount, DbRowExecResult result, TEntity entity)> InsertOrUpdateAsync(IEnumerable<TEntity> entities)
            => OperateRangeAsyncEnumerable(entities, InsertOrUpdateAsync);

        /// <inheritdoc cref="InsertOrUpdateAsync(IEnumerable{TEntity})"/>
        public IAsyncEnumerable<(int rowCount, DbRowExecResult result, TEntity entity)> InsertOrUpdateAsync(params TEntity[] entities)
            => InsertOrUpdateAsync(entities.AsEnumerable());

        #endregion
    }
}