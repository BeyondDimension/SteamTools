// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories.Abstractions;

public interface INotificationRepository : IRepository<Notification, Guid>
{
    Task<IList<Notification>> GetAllAsync(Expression<Func<Notification, bool>>? expression = null, CancellationToken cancellationToken = default);
}