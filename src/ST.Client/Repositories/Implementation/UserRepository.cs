using System.Application.Entities;

namespace System.Application.Repositories.Implementation
{
    internal sealed class UserRepository : Repository<User, Guid>, IUserRepository
    {
    }
}