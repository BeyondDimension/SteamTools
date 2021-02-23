using System.Application.Repositories;
using System.Threading.Tasks;
using KeyValuePair = System.Application.Entities.KeyValuePair;

namespace System.Security
{
    internal sealed class DesktopClientStorage : Repository<KeyValuePair, string>, IStorage
    {
        public async Task<string?> GetAsync(string key)
        {
            var item = await FirstOrDefaultAsync(x => x.Id == key);
            return item?.Value;
        }

        string? IStorage.Get(string key)
        {
            Func<Task<string?>> func = () => GetAsync(key);
            return func.RunSync();
        }

        void IStorage.Set(string key, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                DeleteByKey(key);
            }
            else
            {
                Func<Task> func = () => InsertOrUpdateAsync(key, value);
                func.RunSync();
            }
        }

        async Task InsertOrUpdateAsync(string key, string value)
        {
            await InsertOrUpdateAsync(new KeyValuePair
            {
                Id = key,
                Value = value,
            });
        }

        async Task IStorage.SetAsync(string key, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                await DeleteAsync(key);
            }
            else
            {
                await InsertOrUpdateAsync(key, value);
            }
        }

        int DeleteByKey(string key)
        {
            Func<Task<int>> func = () => DeleteAsync(key);
            return func.RunSync();
        }

        bool IStorage.Remove(string key)
        {
            var result = DeleteByKey(key);
            return result > 0;
        }

        async Task<bool> IStorage.RemoveAsync(string key)
        {
            var result = await DeleteAsync(key);
            return result > 0;
        }
    }
}