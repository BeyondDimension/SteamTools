// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

internal sealed class NotificationRepository : Repository<Notification, Guid>, INotificationRepository
{
    public async Task<IList<Notification>> GetAllAsync(Expression<Func<Notification, bool>>? expression = null, CancellationToken cancellationToken = default)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            var r = dbConnection.Table<Notification>();
            if (expression != null)
                r = r.Where(expression);
            return await r.ToArrayAsync();
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}