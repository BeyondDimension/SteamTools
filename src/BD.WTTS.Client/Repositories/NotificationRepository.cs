// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

internal sealed class NotificationRepository : Repository<Notification, Guid>, INotificationRepository
{
    public async Task<IList<Notification>> GetAllAsync(Expression<Func<Notification, bool>>? expression = null)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        return await AttemptAndRetry(async () =>
        {
            var r = dbConnection.Table<Notification>();
            if (expression != null)
                r = r.Where(expression);
            return await r.ToArrayAsync();
        }).ConfigureAwait(false);
    }
}