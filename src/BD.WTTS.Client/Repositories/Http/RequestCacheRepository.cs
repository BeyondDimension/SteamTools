using Fusillade;
using LiteDB;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

internal sealed class RequestCacheRepository : IRequestCacheRepository, IDisposable
{
    const string DefaultRequestUri = "/";
    const string DefaultRequestHost = "_";
    const string HTTP = "HTTP";
    const string TableName = "RequestCache";
    const string DbFileName = "RequestCache.LiteDB";

    readonly LiteDatabase db;
    readonly ILiteCollection<RequestCache> table;
    bool disposedValue;

    public RequestCacheRepository()
    {
        var dbPath = Path.Combine(IOPath.CacheDirectory, HTTP);
        IOPath.DirCreateByNotExists(dbPath);
        dbPath = Path.Combine(dbPath, DbFileName);
        db = new LiteDatabase(dbPath);
        BsonMapper.Global.Entity<RequestCache>().Id(x => x.Id, autoId: false);
        table = db.GetCollection<RequestCache>(TableName);
    }

    readonly record struct AnalyzeFileTypeResult
    {
        AnalyzeFileTypeResult(string fileEx)
        {
            FileEx = fileEx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AnalyzeFileTypeResult(string fileEx)
            => new(fileEx);

        AnalyzeFileTypeResult(ImageFormat imageFormat)
        {
            ImageFormat = imageFormat;
            FileEx = imageFormat.GetExtension();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AnalyzeFileTypeResult(ImageFormat imageFormat)
            => new(imageFormat);

        public string FileEx { get; init; }

        public ImageFormat? ImageFormat { get; init; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static AnalyzeFileTypeResult AnalyzeFileType(byte[] buffer)
    {
        if (FileFormat.IsImage(buffer, out var imageFormat))
        {
            return imageFormat;
        }

        var buffer_ = buffer.AsSpan();
        Utf8StringComparerOrdinalIgnoreCase comparer = new();

        // 根据文件头识别一些文件类型使用正确的文件扩展名
        var magicNumber = "<html"u8;
        if (magicNumber.SequenceEqual(buffer_[..magicNumber.Length], comparer))
        {
            return FileEx.HTML;
        }
        magicNumber = "<body"u8;
        if (magicNumber.SequenceEqual(buffer_[..magicNumber.Length], comparer))
        {
            return FileEx.HTML;
        }
        magicNumber = "<!DOCTYPE"u8;
        if (magicNumber.SequenceEqual(buffer_[..magicNumber.Length], comparer))
        {
            return FileEx.HTML;
        }
        magicNumber = "<?xml"u8;
        if (magicNumber.SequenceEqual(buffer_[..magicNumber.Length], comparer))
        {
            return FileEx.XML;
        }
        magicNumber = "{"u8;
        if (magicNumber.SequenceEqual(buffer_[..magicNumber.Length], comparer))
        {
            return FileEx.JSON;
        }

        return FileEx.BIN;
    }

    sealed class Utf8StringComparerOrdinalIgnoreCase : IEqualityComparer<byte>
    {
        public Utf8StringComparerOrdinalIgnoreCase() { }

        // https://www.geeksforgeeks.org/lower-case-upper-case-interesting-fact/

        static byte Convert(byte b)
        {
            int i = b;
            i &= ~32;
            return (byte)i;
        }

        bool IEqualityComparer<byte>.Equals(byte x, byte y)
            => Convert(x) == Convert(y);

        int IEqualityComparer<byte>.GetHashCode(byte obj)
            => EqualityComparer<byte>.Default.GetHashCode(Convert(obj));
    }

    async Task IRequestCache.Save(
        HttpRequestMessage request,
        HttpResponseMessage response,
        string key,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            return; // 仅缓存成功的状态码

        if (request.Headers.TryGetValues(ApiConstants.Headers.Request.SecurityKey, out var _) ||
            request.Headers.TryGetValues(ApiConstants.Headers.Request.SecurityKeyHex, out var _))
        {
            // 加密的数据不进行缓存
            return;
        }

        var rspContent = response.Content;
        if (rspContent == null)
            return;

        var bytes = await rspContent.ReadAsByteArrayAsync(cancellationToken);
        if (!bytes.Any_Nullable())
            return;

        var requestUri = response.RequestMessage?.RequestUri ?? request.RequestUri;
        string requestHost, requestUriString;
        if (requestUri == null)
        {
            requestUriString = DefaultRequestUri;
            requestHost = DefaultRequestHost;
        }
        else
        {
            requestHost = requestUri.Host;
            if (string.IsNullOrWhiteSpace(requestHost))
                requestHost = DefaultRequestHost;
            requestUriString = requestUri.ToString();
        }

        var fileTypeResult = AnalyzeFileType(bytes);
        var hashKey = Hashs.String.SHA384(bytes, false);
        var fileEx = fileTypeResult.FileEx;
        var fileName = hashKey + fileEx;
        var relativePath = Path.Combine(HTTP, requestHost, fileName);
        var baseDirPath = Path.Combine(IOPath.CacheDirectory, HTTP, requestHost);
        IOPath.DirCreateByNotExists(baseDirPath);
        var filePath = Path.Combine(IOPath.CacheDirectory, HTTP, requestHost, fileName);
        var isWriteFile = true;
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                if (isWriteFile = fileInfo.Length != bytes.Length)
                {
                    fileInfo.Delete();
                }
            }
        }
        catch
        {

        }
        if (isWriteFile)
        {
            try
            {
                await File.WriteAllBytesAsync(filePath, bytes, cancellationToken); // 写入文件
            }
            catch
            {
                return;
            }
            finally
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        IOPath.FileIfExistsItDelete(filePath);
                    }
                    catch
                    {

                    }
                }
            }
        }

        if (cancellationToken.IsCancellationRequested)
            return;

        var entity = table.FindById(key);
        bool isInsertOrUpdate;
        if (entity == null)
        {
            isInsertOrUpdate = true;
            entity = new()
            {
                Id = key,
            };
        }
        else
        {
            isInsertOrUpdate = false;
        }
        entity.HttpMethod = request.Method.Method;
        entity.RequestUri = requestUriString;
        entity.RelativePath = relativePath;

        if (cancellationToken.IsCancellationRequested)
            return;

        if (isInsertOrUpdate)
            table.Insert(entity);
        else
            table.Update(entity);
    }

    public Task UpdateUsageTimeByIdAsync(string id, CancellationToken cancellationToken) => Task.Run(() =>
    {
        var entity = table.FindById(id);
        if (!cancellationToken.IsCancellationRequested && entity != null)
        {
            var now = DateTimeOffset.Now.Ticks;
            table.UpdateMany(x => new() { UsageTime = now, }, x => x.Id == id);
        }
    }, cancellationToken);

    async Task<byte[]> IRequestCache.Fetch(
        HttpRequestMessage request,
        string key,
        CancellationToken cancellationToken)
    {
        var requestUri = request.RequestUri;
        string requestUriString;
        if (requestUri == null)
        {
            requestUriString = DefaultRequestUri;
        }
        else
        {
            requestUriString = requestUri.ToString();
        }

        var entity = table.Query().Where(x => x.Id == key &&
            x.HttpMethod == request.Method.Method &&
            x.RequestUri == requestUriString).FirstOrDefault();

        if (!cancellationToken.IsCancellationRequested && entity != null)
        {
            try
            {
                var filePath = Path.Combine(IOPath.CacheDirectory, entity.RelativePath);
                if (File.Exists(filePath))
                {
                    var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                    return bytes;
                }
            }
            catch
            {
                goto reZero;
            }
            await UpdateUsageTimeByIdAsync(entity.Id, cancellationToken);
        }

    reZero: return Array.Empty<byte>();
    }

    public Task<int> DeleteAllAsync() => Task.Run(() =>
    {
        var r = table.DeleteAll();
        var dirPath = Path.Combine(IOPath.CacheDirectory, HTTP);
        var items = Directory.EnumerateDirectories(dirPath);
        foreach (var item in items)
        {
            IOPath.DirTryDelete(item, true);
        }
        return r;
    });

    public Task<int> DeleteAllAsync(DateTimeOffset usageTime)
    {
        var usageTime_ = usageTime.Ticks;
        return Task.Run(() =>
        {
            Expression<Func<RequestCache, bool>> predicate = x => x.UsageTime < usageTime_;
            var r = table.DeleteMany(predicate);
            if (r > 0)
            {
                var items = table.Query().Where(predicate).ToArray();
                foreach (var item in items)
                {
                    var filePath = Path.Combine(IOPath.CacheDirectory, item.RelativePath);
                    IOPath.FileTryDelete(filePath);
                }
            }
            return r;
        });
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                db.Dispose();
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
