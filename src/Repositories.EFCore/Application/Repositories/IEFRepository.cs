using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Repositories
{
    public interface IEFRepository
    {
        DbContext DbContext { get; }

        string TableName { get; }

        /// <inheritdoc cref="DbContext.SaveChangesAsync(CancellationToken)"/>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => DbContext.SaveChangesAsync(cancellationToken);

        /// <inheritdoc cref="DbContext.SaveChangesAsync(bool, CancellationToken)"/>
        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            => DbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public interface IEFRepository<TEntity> : IEFRepository where TEntity : class
    {
        DbSet<TEntity> Entity { get; }
    }
}