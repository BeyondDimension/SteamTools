using System.Globalization;
using System.Net;
using System.Text.Json;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.GameList.Categories;

internal sealed record CategoryCacheEntry
{
    public CategoryStoreFacts Facts { get; init; } = new();
    public DateTimeOffset PriceAt { get; init; }
    public DateTimeOffset DetailsAt { get; init; }
    public DateTimeOffset ReviewsAt { get; init; }
    public DateTimeOffset TagsAt { get; init; }
    public DateTimeOffset PlatformsAt { get; init; }
}

internal enum CategoryRefreshStop { Completed, RateLimited, Unavailable }

internal sealed record CategoryRefreshProgress(int Done, int Total, int Incomplete);

internal sealed record CategoryRefreshResult(CategoryRefreshStop Stop, CategoryRefreshProgress Progress);

internal sealed class CategoryMetadataProvider(IHttpClientFactory factory)
{
    const string Store = "https://store.steampowered.com";
    const int BatchSize = 30;
    static readonly TimeSpan PriceLifetime = TimeSpan.FromDays(1);
    static readonly TimeSpan MetadataLifetime = TimeSpan.FromDays(7);
    readonly SemaphoreSlim refreshGate = new(1);
    DateTimeOffset nextRequest;
    DateTimeOffset blockedUntil;
    int unavailableRequests;

    public static bool IsRegion(string? region) =>
        region is { Length: 2 } && region.All(char.IsAsciiLetter);

    static string CachePath(string region, uint id) =>
        Path.Combine(IOPath.CacheDirectory, AssemblyInfo.Plugins, AssemblyInfo.GameList, "categories-store-v1",
            region, id.ToString(CultureInfo.InvariantCulture) + ".json");

    public static async Task<Dictionary<uint, CategoryStoreFacts>> ReadAsync(string region, IEnumerable<uint> appIds, CancellationToken token)
    {
        var facts = new Dictionary<uint, CategoryStoreFacts>();
        foreach (var id in appIds)
        {
            token.ThrowIfCancellationRequested();
            var item = await ReadCacheAsync(region, id, token).ConfigureAwait(false);
            if (item != null)
                facts[id] = item.Facts;
        }
        return facts;
    }

    static async Task<CategoryCacheEntry?> ReadCacheAsync(string region, uint id, CancellationToken token)
    {
        var path = CachePath(region, id);
        if (!File.Exists(path))
            return null;
        try
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 4096, true);
            return await JsonSerializer.DeserializeAsync(stream, CategoryJsonContext.Default.CategoryCacheEntry, token).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    static Task WriteCacheAsync(string region, uint id, CategoryCacheEntry entry, CancellationToken token) =>
        CategoryJsonFile.WriteAsync(CachePath(region, id), entry, CategoryJsonContext.Default.CategoryCacheEntry, token);

    public async Task<CategoryRefreshResult> RefreshAsync(string region, uint[] ids, IProgress<CategoryRefreshProgress> progress, CancellationToken token)
    {
        await refreshGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            return await RefreshCoreAsync(region, ids, progress, token).ConfigureAwait(false);
        }
        finally
        {
            refreshGate.Release();
        }
    }

    async Task<CategoryRefreshResult> RefreshCoreAsync(string region, uint[] ids, IProgress<CategoryRefreshProgress> progress, CancellationToken token)
    {
        // The app's named general client has cookies disabled. Send only regional public app IDs.
        var client = factory.CreateClient(nameof(CategoryMetadataProvider));
        var done = 0;
        var incomplete = 0;
        unavailableRequests = 0;
        try
        {
            foreach (var batch in ids.Chunk(BatchSize))
            {
                var entries = new Dictionary<uint, CategoryCacheEntry>();
                foreach (var id in batch)
                    entries[id] = await ReadCacheAsync(region, id, token).ConfigureAwait(false) ?? new();

                var priceIds = batch.Where(id => Expired(entries[id].PriceAt, PriceLifetime) &&
                    !Expired(entries[id].DetailsAt, MetadataLifetime)).ToArray();
                if (priceIds.Length > 0)
                    await RefreshPricesAsync(client, region, priceIds, entries, token).ConfigureAwait(false);
                var platformIds = batch.Where(id => Expired(entries[id].PlatformsAt, MetadataLifetime)).ToArray();
                if (platformIds.Length > 0)
                    await RefreshPlatformsAsync(client, region, platformIds, entries, token).ConfigureAwait(false);

                foreach (var id in batch)
                {
                    var entry = await RefreshAppAsync(client, region, id, entries[id], token).ConfigureAwait(false);
                    var needsWork = Expired(entry.PriceAt, PriceLifetime) || Expired(entry.DetailsAt, MetadataLifetime) ||
                        Expired(entry.ReviewsAt, MetadataLifetime) || Expired(entry.TagsAt, MetadataLifetime) ||
                        Expired(entry.PlatformsAt, MetadataLifetime);
                    if (needsWork)
                        incomplete++;
                    progress.Report(new(++done, ids.Length, incomplete));
                }
            }
            return new(CategoryRefreshStop.Completed, new(done, ids.Length, incomplete));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return new(CategoryRefreshStop.RateLimited, new(done, ids.Length, incomplete));
        }
        catch (HttpRequestException)
        {
            return new(CategoryRefreshStop.Unavailable, new(done, ids.Length, incomplete));
        }
    }

    async Task RefreshPricesAsync(HttpClient client, string region, uint[] ids,
        Dictionary<uint, CategoryCacheEntry> entries, CancellationToken token)
    {
        var list = string.Join(',', ids.Select(id => id.ToString(CultureInfo.InvariantCulture)));
        using var json = await TryJsonAsync(client, $"{Store}/api/appdetails?appids={list}&cc={region}&filters=price_overview", token).ConfigureAwait(false);
        if (json == null)
            return;
        foreach (var id in ids)
        {
            var entry = entries[id];
            if (!CategoryStoreParser.TryDetails(json.RootElement, id, out var data))
                continue;
            entry = entry with { Facts = CategoryStoreParser.ReadPrice(data, entry.Facts), PriceAt = DateTimeOffset.UtcNow };
            await WriteCacheAsync(region, id, entry, token).ConfigureAwait(false);
            entries[id] = entry;
        }
    }

    async Task RefreshPlatformsAsync(HttpClient client, string region, uint[] ids,
        Dictionary<uint, CategoryCacheEntry> entries, CancellationToken token)
    {
        var list = string.Join(',', ids.Select(id => $$"""{"appid":{{id.ToString(CultureInfo.InvariantCulture)}}}"""));
        var input = Uri.EscapeDataString(
            $$$"""{"ids":[{{{list}}}],"context":{"language":"english","country_code":"{{{region}}}"},"data_request":{"include_platforms":true}}""");
        using var json = await TryJsonAsync(client,
            $"https://api.steampowered.com/IStoreBrowseService/GetItems/v1/?input_json={input}", token).ConfigureAwait(false);
        if (json == null)
            return;
        foreach (var (id, deck) in CategoryStoreParser.ReadPlatforms(json.RootElement))
        {
            if (!entries.TryGetValue(id, out var entry))
                continue;
            entry = entry with { Facts = entry.Facts with { DeckStatus = deck }, PlatformsAt = DateTimeOffset.UtcNow };
            await WriteCacheAsync(region, id, entry, token).ConfigureAwait(false);
            entries[id] = entry;
        }
    }

    async Task<CategoryCacheEntry> RefreshAppAsync(HttpClient client, string region, uint id, CategoryCacheEntry entry, CancellationToken token)
    {
        var appId = id.ToString(CultureInfo.InvariantCulture);
        if (Expired(entry.DetailsAt, MetadataLifetime) || Expired(entry.PriceAt, PriceLifetime))
        {
            using var json = await TryJsonAsync(client, $"{Store}/api/appdetails?appids={appId}&cc={region}&l=english", token).ConfigureAwait(false);
            if (json != null && CategoryStoreParser.TryDetails(json.RootElement, id, out var data))
            {
                var now = DateTimeOffset.UtcNow;
                entry = entry with { Facts = CategoryStoreParser.ReadDetails(data, entry.Facts), DetailsAt = now, PriceAt = now };
                await WriteCacheAsync(region, id, entry, token).ConfigureAwait(false);
            }
        }
        if (Expired(entry.ReviewsAt, MetadataLifetime))
        {
            var input = Uri.EscapeDataString($$"""{"appid":{{appId}},"filter":1,"languages":["all"],"purchase_type":1,"num_per_page":0}""");
            using var json = await TryJsonAsync(client,
                $"https://api.steampowered.com/IUserReviewsService/GetAppReviews/v1/?input_json={input}", token).ConfigureAwait(false);
            if (json != null && CategoryStoreParser.ReadReviews(json.RootElement, entry.Facts) is { } facts)
            {
                entry = entry with { Facts = facts, ReviewsAt = DateTimeOffset.UtcNow };
                await WriteCacheAsync(region, id, entry, token).ConfigureAwait(false);
            }
        }
        if (Expired(entry.TagsAt, MetadataLifetime))
        {
            var page = await TryGetAsync(client, $"{Store}/app/{appId}/?l=english&cc={region}", token).ConfigureAwait(false);
            CategoryStoreFacts? facts = null;
            if (page != null)
            {
                try
                {
                    facts = CategoryStoreParser.ReadStorePage(page, entry.Facts);
                }
                catch (JsonException)
                {
                    // Steam changed the public tag fragment. Keep the previous snapshot.
                }
            }
            if (facts != null)
            {
                entry = entry with { Facts = facts, TagsAt = DateTimeOffset.UtcNow };
                await WriteCacheAsync(region, id, entry, token).ConfigureAwait(false);
            }
        }
        return entry;
    }

    static bool Expired(DateTimeOffset time, TimeSpan lifetime) => DateTimeOffset.UtcNow - time >= lifetime;

    async Task<JsonDocument?> TryJsonAsync(HttpClient client, string url, CancellationToken token)
    {
        var response = await TryGetAsync(client, url, token).ConfigureAwait(false);
        if (response is null)
            return null;
        try
        {
            return JsonDocument.Parse(response);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    async Task<string?> TryGetAsync(HttpClient client, string url, CancellationToken token)
    {
        if (unavailableRequests >= 3)
            throw new HttpRequestException("Steam metadata transport is unavailable.");
        if (blockedUntil > DateTimeOffset.UtcNow)
            throw new HttpRequestException(null, null, HttpStatusCode.TooManyRequests);
        var delay = nextRequest - DateTimeOffset.UtcNow;
        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, token).ConfigureAwait(false);
        nextRequest = DateTimeOffset.UtcNow.AddSeconds(2);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(12));
        try
        {
            using var reply = await client.GetAsync(url, timeout.Token).ConfigureAwait(false);
            if (reply.StatusCode == HttpStatusCode.TooManyRequests)
            {
                blockedUntil = reply.Headers.RetryAfter?.Date ??
                    DateTimeOffset.UtcNow.Add(reply.Headers.RetryAfter?.Delta ?? TimeSpan.FromMinutes(2));
                reply.EnsureSuccessStatusCode();
            }
            if ((int)reply.StatusCode >= 500)
            {
                unavailableRequests++;
                return null;
            }
            var content = reply.IsSuccessStatusCode ? await reply.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false) : null;
            unavailableRequests = 0;
            return content;
        }
        catch (HttpRequestException ex) when (ex.StatusCode != HttpStatusCode.TooManyRequests)
        {
            unavailableRequests++;
            return null;
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            unavailableRequests++;
            return null;
        }
    }
}
