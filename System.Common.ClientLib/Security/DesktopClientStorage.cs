using System.Application.Repositories;
using System.Threading.Tasks;
using KeyValuePair = System.Application.Entities.KeyValuePair;

namespace System.Security
{
    internal sealed class DesktopClientStorage : Repository<KeyValuePair, string>, IStorage
    {
        bool IStorage.IsNativeSupportedBytes => true;

        async Task<byte[]?> IStorage.GetBytesAsync(string key)
        {
            var item = await FirstOrDefaultAsync(x => x.Id == key);
            var value = item?.Value;
            return value;
        }

        async Task InsertOrUpdateAsync(string key, byte[] value)
        {
            await InsertOrUpdateAsync(new KeyValuePair
            {
                Id = key,
                Value = value,
            });
        }

        Task IStorage.SetAsync(string key, byte[]? value)
        {
            if (value == null || value.Length <= 0)
            {
                return DeleteAsync(key);
            }
            else
            {
                return InsertOrUpdateAsync(key, value);
            }
        }

        async Task<bool> IStorage.RemoveAsync(string key)
        {
            var result = await DeleteAsync(key);
            return result > 0;
        }
    }
}