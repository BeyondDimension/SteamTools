using System.Application.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace System
{
    public static class QueryableExtensions
    {
        #region WhereOr

        /// <inheritdoc cref="QueryableExtensions.WhereOr{T}(IReadOnlyList{Expression{Func{T, bool}}})"/>
        public static IQueryable<T> WhereOr<T>(this IQueryable<T> source, IReadOnlyList<Expression<Func<T, bool>>> predicates)
        {
            var predicate = ExpressionHelper.WhereOr(predicates);
            return source.Where(predicate);
        }

        /// <inheritdoc cref="QueryableExtensions.WhereOr{T}(IReadOnlyList{Expression{Func{T, bool}}})"/>
        public static IQueryable<T> WhereOr<T>(this IQueryable<T> source, IEnumerable<Expression<Func<T, bool>>> predicates)
            => source.WhereOr(predicates.ToArray());

        /// <inheritdoc cref="QueryableExtensions.WhereOr{T}(IReadOnlyList{Expression{Func{T, bool}}})"/>
        public static IQueryable<T> WhereOr<T>(this IQueryable<T> source, params Expression<Func<T, bool>>[] predicates)
        {
            IReadOnlyList<Expression<Func<T, bool>>> _predicates = predicates;
            return source.WhereOr(_predicates);
        }

        #endregion

        #region Paging

        /// <summary>
        /// 分页查询，调用此方法前必须进行排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<PagedModel<TEntity>> PagingAsync<TEntity>(
            this IQueryable<TEntity> source,
            int current = IPagedModel.DefaultCurrent,
            int pageSize = IPagedModel.DefaultPageSize,
            CancellationToken cancellationToken = default)
        {
            var skipCount = (current - 1) * pageSize;
            var futureTotal = source.DeferredCount().FutureValue();
            var futureDataSource = source.Skip(skipCount).Take(pageSize).Future();
            var total = await futureTotal.ValueAsync(cancellationToken);
            var dataSource = await futureDataSource.ToListAsync(cancellationToken);
            var pagedModel = new PagedModel<TEntity>
            {
                Current = current,
                PageSize = pageSize,
                Total = total,
                DataSource = dataSource,
            };
            return pagedModel;
        }

        #endregion
    }
}