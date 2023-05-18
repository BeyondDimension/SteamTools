using Fusillade;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories.Abstractions;

public interface IRequestCacheRepository : IRepository<RequestCache, string>, IRequestCache
{
}
