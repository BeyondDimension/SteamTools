using AutoMapper;
using System.Application.Entities;
using System.Application.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Repositories.Implementation
{
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
}
