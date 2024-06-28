// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

[Mobius(
"""
Mobius.Repositories.Identity.UserRepository
""")]
internal sealed class UserRepository : Repository<User, Guid>, IUserRepository
{
}