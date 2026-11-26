using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.GameList.Categories;

internal sealed record CategoryDefinitions(GameCategory[] Categories)
{
    public int Version { get; init; } = 1;
}

internal sealed record AppliedCategory(Guid Id, uint[] AppIds);

internal sealed record CategoryLibrary(CategoryDefinitions Definitions, AppliedCategory[] Applied, string? Region = null);

internal sealed record SteamCollectionExport(Guid Id, string Name, uint[] AppIds)
{
    public string Format { get; init; } = "watt-toolkit-steam-collection-export/v1";
}

internal interface ISteamCollectionWriter
{
    SteamCollectionExport Preview(GameCategory category, IEnumerable<uint> appIds);
}

// A portable membership snapshot. Steam's cross-file sync and conflict handling are unverified.
internal sealed class ExportOnlySteamCollectionWriter : ISteamCollectionWriter
{
    public SteamCollectionExport Preview(GameCategory category, IEnumerable<uint> appIds) =>
        new(category.Id, category.Name, appIds.Distinct().Order().ToArray());
}

internal sealed class CategoryStorage
{
    public const int MaxRuleDepth = 8;
    public const int MaxCategories = 256;
    const int MaxRules = 256;
    const int MaxImportBytes = 4 * 1024 * 1024;
    readonly SemaphoreSlim gate = new(1);
    readonly string path = Path.Combine(IOPath.AppDataDirectory, AssemblyInfo.Plugins, AssemblyInfo.GameList, "categories.json");
    CategoryLibrary? current;

    public async Task<CategoryLibrary> LoadAsync()
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (current != null)
                return current;
            if (!File.Exists(path))
                return current = new(new([]), []);
            await using var stream = File.OpenRead(path);
            var library = await ReadAsync(stream, CategoryJsonContext.Default.CategoryLibrary).ConfigureAwait(false);
            Validate(library.Definitions);
            if (library.Applied is null || library.Applied.Any(entry => entry?.AppIds is null))
                throw new InvalidDataException("Invalid saved category membership.");
            return current = library;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(CategoryLibrary previous, CategoryLibrary library)
    {
        Validate(library.Definitions);
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!ReferenceEquals(current, previous))
                throw new InvalidOperationException("The category library changed during editing.");
            await CategoryJsonFile.WriteAsync(path, library, CategoryJsonContext.Default.CategoryLibrary, CancellationToken.None)
                .ConfigureAwait(false);
            current = library;
        }
        finally
        {
            gate.Release();
        }
    }

    public static async Task<CategoryDefinitions> ImportAsync(Stream stream)
    {
        if (stream.CanSeek && stream.Length > MaxImportBytes)
            throw new InvalidDataException("Category file is too large.");
        var definitions = await ReadAsync(stream, CategoryJsonContext.Default.CategoryDefinitions).ConfigureAwait(false);
        Validate(definitions);
        return definitions;
    }

    static async Task<T> ReadAsync<T>(Stream stream, JsonTypeInfo<T> typeInfo) where T : class
    {
        return await JsonSerializer.DeserializeAsync(stream, typeInfo).ConfigureAwait(false)
            ?? throw new InvalidDataException("Empty category file.");
    }

    static void Validate(CategoryDefinitions definitions)
    {
        if (definitions is null || definitions.Version != 1 || definitions.Categories is null ||
            definitions.Categories.Length > MaxCategories)
            throw new InvalidDataException("Unsupported category document.");

        var ids = new HashSet<Guid>();
        foreach (var category in definitions.Categories)
        {
            if (category is null || category.Id == Guid.Empty || !ids.Add(category.Id) ||
                string.IsNullOrWhiteSpace(category.Name) || category.Name.Length > 100 ||
                !Enum.IsDefined(category.Mode) || category.ManualIncluded is null || category.ManualExcluded is null)
                throw new InvalidDataException("Invalid category definition.");
            var count = 0;
            ValidateRule(category.Rules, 0, ref count);
        }
    }

    static void ValidateRule(CategoryRule rule, int depth, ref int count)
    {
        if (rule is null || ++count > MaxRules || depth > MaxRuleDepth || !Enum.IsDefined(rule.Kind) ||
            rule.Field is null || rule.Values is null || rule.Children is null ||
            rule.Values.Any(value => value is null || value.Length > 500) ||
            (rule.Kind == CategoryRuleKind.Condition && rule.Children.Length != 0) ||
            (rule.Kind == CategoryRuleKind.Not && rule.Children.Length > 1))
            throw new InvalidDataException("Invalid category rule.");

        foreach (var child in rule.Children)
            ValidateRule(child, depth + 1, ref count);
    }
}

internal static class CategoryJsonFile
{
    public static async Task WriteAsync<T>(string path, T value, JsonTypeInfo<T> typeInfo, CancellationToken token)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var pending = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var stream = new FileStream(pending, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
                await JsonSerializer.SerializeAsync(stream, value, typeInfo, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            File.Move(pending, path, overwrite: true);
        }
        finally
        {
            File.Delete(pending);
        }
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true, WriteIndented = true, MaxDepth = 40)]
[JsonSerializable(typeof(CategoryLibrary))]
[JsonSerializable(typeof(CategoryDefinitions))]
[JsonSerializable(typeof(SteamCollectionExport))]
[JsonSerializable(typeof(CategoryCacheEntry))]
internal sealed partial class CategoryJsonContext : JsonSerializerContext;
