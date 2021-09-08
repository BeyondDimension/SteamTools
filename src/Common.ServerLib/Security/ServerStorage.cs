using Microsoft.EntityFrameworkCore;
using System.Application.Repositories;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;
using KeyValuePair = System.Application.Entities.KeyValuePair;

namespace System.Security
{
    internal sealed class ServerStorage<TDbContext> : Repository<TDbContext, KeyValuePair, string>, IStorage
        where TDbContext : DbContext
    {
        bool IStorage.IsNativeSupportedBytes => false;

        public ServerStorage(TDbContext dbContext) : base(dbContext)
        {
        }

        IQueryable<string> Get(string key)
            => Entity.Where(x => !x.SoftDeleted && x.Id == key).Select(x => x.Value);

        async Task<string?> IStorage.GetAsync(string key)
        {
            var value = await Get(key).FirstOrDefaultAsync();
            return value;
        }

        IQueryable<KeyValuePair> GetSetQuery(string key)
            => Entity.IgnoreQueryFilters().Where(x => x.Id == key);

        static Expression<Func<KeyValuePair, KeyValuePair>> GetSetValueUpdateExpression(string value)
               => x => new KeyValuePair { SoftDeleted = false, Value = value };

        Task IStorage.SetAsync(string key, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DeleteAsync(key);
            }
            else
            {
                return GetSetQuery(key).UpdateAsync(GetSetValueUpdateExpression(value));
            }
        }

        async Task<bool> IStorage.RemoveAsync(string key)
        {
            var result = await DeleteAsync(key);
            return result > 0;
        }

        async Task<bool> IStorage.ContainsKeyAsync(string key)
        {
            var result = await Get(key).AnyAsync();
            return result;
        }
    }
}