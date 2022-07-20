using System.Application.Entities;
using System.Application.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Repositories
{
    public interface INotificationRepository : IRepository<Notification, Guid>
    {

        Task<IList<Notification>> GetAllAsync(Expression<Func<Notification, bool>>? expression = null);
    }
}
