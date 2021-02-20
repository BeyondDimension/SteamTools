using System.Application.Entities;

namespace System.Application.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
    }
}