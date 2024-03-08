using Fusillade;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

internal sealed class RequestCacheRepository : CacheRepository<RequestCache, string>, IRequestCacheRepository
{
    const string CacheDirName = "Http";

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

        var requestUriString = IRequestCacheRepository.GetOriginalRequestUri(request);
        var fileTypeResult = FileFormat.AnalyzeFileType(bytes);
        var hashKey = Hashs.String.SHA384(bytes, false);
        var fileEx = fileTypeResult.FileEx;
        var fileName = hashKey + fileEx;
        var subDirName = fileTypeResult.ImageFormat.HasValue ? "Images" : "Binaries";
        var hash_0 = hashKey[..2];
        var hash_1 = hashKey[2..4]; // 通过两级目录来避免文件夹内文件过多造成的性能下降
        var relativePath = Path.Combine(CacheDirName, subDirName, hash_0, hash_1, fileName);
        var baseDirPath = Path.Combine(IOPath.CacheDirectory, CacheDirName, subDirName, hash_0, hash_1);
        IOPath.DirCreateByNotExists(baseDirPath);
        var filePath = Path.Combine(baseDirPath, fileName);
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

        key = UniqueKeyForRequest(requestUriString, request);

        var entity = new RequestCache
        {
            Id = key,
            HttpMethod = request.Method.Method,
            RequestUri = requestUriString,
            RelativePath = relativePath,
        };
        await InsertOrUpdateAsync(entity, cancellationToken);
    }

    public async Task UpdateUsageTimeByIdAsync(string id, CancellationToken cancellationToken)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        await AttemptAndRetry(async t =>
        {
            t.ThrowIfCancellationRequested();
            const string sql_ = $"{SQLStrings.Update}[{RequestCache.TableName}] " +
               $"set [{RequestCache.ColumnName_UsageTime}] = {{0}} " +
               $"where [{RequestCache.ColumnName_Id}] = '{{1}}'";
            var sql = string.Format(sql_, DateTimeOffset.Now.Ticks, id);
            var r = await dbConnection.ExecuteAsync(sql);
            return r;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    async Task<byte[]> IRequestCache.Fetch(
        HttpRequestMessage request,
        string key,
        CancellationToken cancellationToken)
    {
        var requestUriString = IRequestCacheRepository.GetOriginalRequestUri(request);
        key = UniqueKeyForRequest(requestUriString, request);

        var entity = await FirstOrDefaultAsync(x => x.Id == key &&
            x.HttpMethod == request.Method.Method &&
            x.RequestUri == requestUriString,
            cancellationToken);

        if (!cancellationToken.IsCancellationRequested && entity != null)
        {
            Task2.InBackground(async () =>
            {
                try
                {
                    await UpdateUsageTimeByIdAsync(entity.Id, cancellationToken);
                }
                catch
                {

                }
            });
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
        }

    reZero: return Array.Empty<byte>();
    }

    public async Task<int> DeleteAllAsync()
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        var r = await dbConnection.DeleteAllAsync<RequestCache>();
        return r;
    }

    public async Task<int> DeleteAllAsync(DateTimeOffset dateTimeOffset)
    {
        var dbConnection = await GetDbConnection().ConfigureAwait(false);
        const string sql = $"{SQLStrings.DeleteFrom}[{RequestCache.TableName}] " +
               $"where [{RequestCache.ColumnName_UsageTime}] < ?";
        var r = await dbConnection.ExecuteAsync(sql, dateTimeOffset.UtcTicks);
        return r;
    }

    static string UniqueKeyForRequest(string originalRequestUri, HttpRequestMessage request)
    {
        // https://github.com/reactiveui/Fusillade/blob/2.4.67/src/Fusillade/RateLimitedHttpMessageHandler.cs#L54-L89

        using var s = new MemoryStream();
        s.Write(Encoding.UTF8.GetBytes(originalRequestUri));
        s.Write("\r\n"u8);
        s.Write(Encoding.UTF8.GetBytes(request.Method.Method));
        s.Write("\r\n"u8);
        static void Write(Stream s, IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                var str = item.ToString();
                if (!string.IsNullOrEmpty(str))
                    s.Write(Encoding.UTF8.GetBytes(str));
                s.Write("|"u8);
            }
        }
        Write(s, request.Headers.Accept);
        s.Write("\r\n"u8);
        //Write(s, request.Headers.AcceptEncoding); // AcceptEncoding 在 Fetch 中为空，Save 时有值😅
        s.Write("gzip|deflate|br"u8);
        s.Write("\r\n"u8);
        var referrer = request.Headers.Referrer;
        if (referrer == default)
            s.Write("http://example"u8);
        else
            s.Write(Encoding.UTF8.GetBytes(referrer.ToString()));
        s.Write("\r\n"u8);
        Write(s, request.Headers.UserAgent);
        s.Write("\r\n"u8);
        if (request.Headers.Authorization != null)
        {
            var parameter = request.Headers.Authorization.Parameter;
            if (!string.IsNullOrEmpty(parameter))
                s.Write(Encoding.UTF8.GetBytes(parameter));
            s.Write(Encoding.UTF8.GetBytes(request.Headers.Authorization.Scheme));
            s.Write("\r\n"u8);
        }
        s.Position = 0;
        var bytes = SHA384.HashData(s);
        var str = bytes.ToHexString();
        return str;
    }
}
