using Microsoft.EntityFrameworkCore;
using System.Application.Columns;
using System.Application.Entities;
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
        /// 根据页码进行分页查询，调用此方法前 必须 进行排序
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

        /// <summary>
        /// 根据偏移量进行分页查询，调用此方法前 必须 进行排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="offset"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<PagedModel<TEntity>> PagingOffsetAsync<TEntity>(
            this IQueryable<TEntity> source,
            int offset = 0,
            int pageSize = IPagedModel.DefaultPageSize,
            CancellationToken cancellationToken = default)
        {
            if (offset > 0) source = source.Skip(offset);
            var dataSource = await source.Take(pageSize).ToListAsync(cancellationToken);
            var pagedModel = new PagedModel<TEntity>
            {
                PageSize = pageSize,
                DataSource = dataSource,
            };
            return pagedModel;
        }

        /// <summary>
        /// 根据页码进行分页查询，此方法 已使用 创建时间倒序排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="current"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<PagedModel<TEntity>> PagingSortAsync<TEntity>(
          this IQueryable<TEntity> source,
          int current = IPagedModel.DefaultCurrent,
          int pageSize = IPagedModel.DefaultPageSize,
          CancellationToken cancellationToken = default) where TEntity : class, ICreationTime
        {
            source = source.OrderByDescending(x => x.CreationTime);
            return source.PagingAsync(current, pageSize, cancellationToken);
        }

        /// <summary>
        /// 根据创建时间描点进行分页查询，此方法 已使用 创建时间倒序排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="anchor"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<PagedModel<TEntity>> PagingSortAsync<TEntity>(
            this IQueryable<TEntity> source,
            ICreationTime? anchor = null,
            int pageSize = IPagedModel.DefaultPageSize,
            CancellationToken cancellationToken = default) where TEntity : class, ICreationTime
        {
            var anchorCreationTime = anchor == null ? default : anchor.CreationTime;
            return source.PagingSortAsync(anchorCreationTime, pageSize, cancellationToken);
        }

        /// <summary>
        /// 根据创建时间描点进行分页查询，此方法 已使用 创建时间倒序排序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="anchor"></param>
        /// <param name="pageSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<PagedModel<TEntity>> PagingSortAsync<TEntity>(
           this IQueryable<TEntity> source,
           DateTimeOffset anchor = default,
           int pageSize = IPagedModel.DefaultPageSize,
           CancellationToken cancellationToken = default)
            where TEntity : class, ICreationTime
        {
            if (anchor != default) source = source.Where(x => x.CreationTime > anchor);
            source = source.OrderByDescending(x => x.CreationTime);
            return await source.PagingOffsetAsync(0, pageSize, cancellationToken);
        }

        #endregion
    }
}