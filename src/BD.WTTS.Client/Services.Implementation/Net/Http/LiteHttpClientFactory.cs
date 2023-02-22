// https://github.com/dotnet/runtime/blob/v7.0.3/src/libraries/Microsoft.Extensions.Http/src/DefaultHttpClientFactory.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

public sealed class LiteHttpClientFactory : IHttpClientFactory, IDisposable
{
    bool disposedValue;
    readonly ConcurrentDictionary<string, HttpClient> pairs = new();

    public HttpClient CreateClient(string name)
    {
        if (pairs.TryGetValue(name, out var result))
            return result;
        return pairs[name] = new HttpClient();
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                foreach (var item in pairs.Values)
                {
                    item.Dispose();
                }
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
