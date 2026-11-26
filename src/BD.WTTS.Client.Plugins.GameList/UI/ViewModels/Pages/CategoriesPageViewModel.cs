using BD.WTTS.GameList.Categories;
using JsonSerializer = System.Text.Json.JsonSerializer;
using static BD.WTTS.GameList.Categories.CategoryOperator;

namespace BD.WTTS.UI.ViewModels;

public sealed class CategoriesPageViewModel : TabItemViewModel
{
    readonly CategoryStorage storage = Ioc.Get<CategoryStorage>();
    readonly CategoryMetadataProvider metadata = Ioc.Get<CategoryMetadataProvider>();
    readonly ISteamCollectionWriter writer = Ioc.Get<ISteamCollectionWriter>();
    CategoryLibrary library = new(new([]), []);
    CategoryInventoryGame[] games = [];

    public override string Name => Strings.GameList_Cat_Title;

    internal GameCategory[] Categories => library.Definitions.Categories;
    internal string Region => CategoryMetadataProvider.IsRegion(library.Region) ? library.Region! : "cn";
    internal CategoryInventoryGame[] Games => games;
    internal string[] Currencies { get; private set; } = [];
    internal string[] Tags { get; private set; } = [];

    internal async Task InitializeAsync(CancellationToken token)
    {
        library = await Task.Run(storage.LoadAsync, token);
        await ReloadGamesAsync(token);
    }

    internal async Task ReloadGamesAsync(CancellationToken token)
    {
        var inventory = await Task.Run(() => CategoryInventory.ReadAsync(token), token);
        await ReadCacheAsync(inventory, token);
    }

    internal async Task ReadCacheAsync(CancellationToken token) => await ReadCacheAsync(games, token);

    async Task ReadCacheAsync(CategoryInventoryGame[] inventory, CancellationToken token)
    {
        var region = Region;
        var snapshot = await Task.Run(async () =>
        {
            var cache = await CategoryMetadataProvider.ReadAsync(region, inventory.Select(game => game.Facts.AppId), token);
            var updated = inventory.Select(game => game with
            {
                Facts = game.Facts with { Store = cache.GetValueOrDefault(game.Facts.AppId) },
            }).ToArray();
            var currencies = cache.Values.Select(fact => fact.Currency).OfType<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
            var tags = cache.Values.SelectMany(fact => fact.Tags ?? [])
                .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.CurrentCultureIgnoreCase).ToArray();
            return (updated, currencies, tags);
        }, token);
        (games, Currencies, Tags) = snapshot;
    }

    internal Task<CategoryRefreshResult> RefreshMetadataAsync(IProgress<CategoryRefreshProgress> progress, CancellationToken token)
    {
        var region = Region;
        var ids = games.Select(game => game.Facts.AppId).ToArray();
        return Task.Run(() => metadata.RefreshAsync(region, ids, progress, token), token);
    }

    internal int? AppliedCount(Guid categoryId) => library.Applied.FirstOrDefault(entry => entry.Id == categoryId)?.AppIds.Length;

    internal async Task SaveAsync(GameCategory category)
    {
        var updated = Categories.Any(item => item.Id == category.Id)
            ? Categories.Select(item => item.Id == category.Id ? category : item).ToArray()
            : [.. Categories, category];
        var applied = library.Applied.Where(item => item.Id != category.Id);
        if (category.Mode == CategoryMode.Manual)
            applied = applied.Append(new(category.Id, category.ManualIncluded.Except(category.ManualExcluded).ToArray()));
        await SetLibraryAsync(library with { Definitions = new(updated), Applied = applied.ToArray() });
    }

    internal Task DeleteAsync(Guid id) => SetLibraryAsync(library with
    {
        Definitions = new(Categories.Where(category => category.Id != id).ToArray()),
        Applied = library.Applied.Where(entry => entry.Id != id).ToArray(),
    });

    internal async Task ImportAsync(Stream stream)
    {
        var imported = await Task.Run(() => CategoryStorage.ImportAsync(stream));
        var ids = imported.Categories.Select(item => item.Id).ToHashSet();
        var incoming = imported.Categories.Where(item => item.Mode == CategoryMode.Manual)
            .Select(item => new AppliedCategory(item.Id, item.ManualIncluded.Except(item.ManualExcluded).ToArray()));
        await SetLibraryAsync(library with
        {
            Definitions = new(Categories.Where(item => !ids.Contains(item.Id)).Concat(imported.Categories).ToArray()),
            Applied = library.Applied.Where(item => !ids.Contains(item.Id)).Concat(incoming).ToArray(),
        });
    }

    internal Task ExportAsync(Stream stream) =>
        JsonSerializer.SerializeAsync(stream, library.Definitions, CategoryJsonContext.Default.CategoryDefinitions);

    internal async Task SetRegionAsync(string region)
    {
        if (region == Region)
            return;
        await SetLibraryAsync(library with { Region = region });
    }

    async Task SetLibraryAsync(CategoryLibrary updated)
    {
        var previous = library;
        await Task.Run(() => storage.SaveAsync(previous, updated));
        library = updated;
    }

    internal async Task<int> ApplyAsync(CancellationToken token)
    {
        var categories = Categories;
        var inventory = games;
        var now = DateTimeOffset.UtcNow;
        var entries = await Task.Run(() => categories.Select(category =>
        {
            token.ThrowIfCancellationRequested();
            var ids = inventory.Where(game => CategoryRuleEngine.Match(category, game.Facts, now) != null)
                .Select(game => game.Facts.AppId);
            if (category.Mode != CategoryMode.Auto)
                ids = ids.Concat(category.ManualIncluded.Except(category.ManualExcluded));
            return new AppliedCategory(category.Id, ids.Distinct().Order().ToArray());
        }).ToArray(), token);
        token.ThrowIfCancellationRequested();
        await SetLibraryAsync(library with { Applied = entries });
        return categories.Count(category => category.Mode != CategoryMode.Manual);
    }

    internal Task<CategoryPreviewRow[]> PreviewAsync(GameCategory category, CancellationToken token)
    {
        var snapshot = games;
        return Task.Run(() =>
        {
            var now = DateTimeOffset.UtcNow;
            var rows = new List<CategoryPreviewRow>();
            foreach (var game in snapshot)
            {
                token.ThrowIfCancellationRequested();
                var match = CategoryRuleEngine.Match(category, game.Facts, now);
                if (match != null)
                    rows.Add(new(game.App, game.Facts.Name, match.Manual ? Strings.GameList_Cat_ManualMatch :
                        string.Join(" · ", match.Rules.Select(CategoryLabels.Describe))));
            }
            if (category.Mode != CategoryMode.Auto)
            {
                var known = snapshot.Select(game => game.Facts.AppId).ToHashSet();
                foreach (var id in category.ManualIncluded.Except(category.ManualExcluded).Where(id => !known.Contains(id)))
                    rows.Add(new(new SteamApp(id), id.ToString(System.Globalization.CultureInfo.InvariantCulture), Strings.GameList_Cat_ManualMatch));
            }
            return rows.ToArray();
        }, token);
    }

    internal Task<CategoryPreviewRow[]> ConflictsAsync(CancellationToken token)
    {
        var snapshots = library.Applied;
        var categories = Categories;
        var inventory = games;
        return Task.Run(() =>
        {
            var apps = inventory.ToDictionary(game => game.Facts.AppId, game => game.App);
            var names = categories.ToDictionary(category => category.Id, category => category.Name);
            return snapshots.Where(entry => names.ContainsKey(entry.Id))
                .SelectMany(entry => entry.AppIds.Select(id => (Id: id, Name: names[entry.Id])))
                .GroupBy(item => item.Id)
                .Where(group => group.Count() > 1)
                .Select(group =>
                {
                    token.ThrowIfCancellationRequested();
                    var app = apps.GetValueOrDefault(group.Key) ?? new SteamApp(group.Key);
                    return new CategoryPreviewRow(app, app.DisplayName, string.Join(" · ", group.Select(item => item.Name)));
                }).OrderBy(row => row.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
        }, token);
    }

    internal string SteamExport(GameCategory category, CategoryPreviewRow[] rows) =>
        JsonSerializer.Serialize(writer.Preview(category, rows.Select(row => row.App.AppId)),
            CategoryJsonContext.Default.SteamCollectionExport);

    internal static (string Name, CategoryRule Rule)[] Templates =>
    [
        (Strings.GameList_Cat_TemplateHiddenGems, CategoryRule.Group(CategoryRuleKind.All,
            CategoryRule.Condition("ReviewScore", AtLeast, "90"), CategoryRule.Condition("ReviewCount", LessThan, "5000"))),
        (Strings.GameList_Cat_TemplateValue, CategoryRule.Group(CategoryRuleKind.All,
            CategoryRule.Condition("ValueRating", AtLeast, "40"))),
        (Strings.GameList_Cat_TemplateBacklog, CategoryRule.Group(CategoryRuleKind.All,
            CategoryRule.Condition("PlaytimeMinutes", Equal, "0"), CategoryRule.Condition("OwnedSince", OlderThanDays, "30"))),
        (Strings.GameList_Cat_TemplateShort, CategoryRule.Group(CategoryRuleKind.All, CategoryRule.Condition("Tags", HasTag, "Short"))),
        (Strings.GameList_Cat_TemplateChinese, CategoryRule.Group(CategoryRuleKind.All,
            CategoryRule.Condition("Singleplayer", Equal, "true"), CategoryRule.Condition("Multiplayer", Equal, "false"),
            CategoryRule.Condition("Languages", HasAll, "Simplified Chinese"))),
        (Strings.GameList_Cat_TemplateDeck, CategoryRule.Group(CategoryRuleKind.All,
            CategoryRule.Condition("DeckStatus", In, "verified", "playable"))),
    ];
}

internal sealed record CategoryPreviewRow(SteamApp App, string Name, string Details);

internal static class CategoryLabels
{
    public static string Get(string key) => Strings.ResourceManager.GetString("GameList_Cat_" + key, Strings.Culture) ?? key;

    public static string Describe(CategoryRule rule)
    {
        if (rule.Kind == CategoryRuleKind.Condition)
        {
            var field = CategoryField.Registry.GetValueOrDefault(rule.Field);
            var values = field == null ? rule.Values : rule.Values.Select(value => DisplayValue(field, value));
            return $"{Get("Field_" + rule.Field)} {Get("Op_" + rule.Operator)} {string.Join(", ", values)}".TrimEnd();
        }
        return $"{Get("Group_" + rule.Kind)} ({string.Join(" · ", rule.Children.Select(Describe))})";
    }

    public static string DisplayValue(CategoryField field, string value) => field.Kind switch
    {
        CategoryValueKind.Boolean => Get("Choice_" + value.ToLowerInvariant()),
        CategoryValueKind.Choice => Get("Choice_" + field.Id + "_" + value.ToLowerInvariant()),
        _ => value,
    };
}
