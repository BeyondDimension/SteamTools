using System.Globalization;
using System.Text.Json;
using BD.SteamClient.Helpers;
using ValveKeyValue;
using JsonException = System.Text.Json.JsonException;

namespace BD.WTTS.GameList.Categories;

internal sealed record CategoryInventoryGame(SteamApp App, CategoryGame Facts);

internal static class CategoryInventory
{
    public static async Task<CategoryInventoryGame[]> ReadAsync(CancellationToken token)
    {
        var service = ISteamService.Instance;
        var conn = SteamConnectService.Current;
        var live = service.IsRunningSteamProcess && conn.IsConnectToSteam && conn.CurrentSteamUser != null;
        var user = live ? conn.CurrentSteamUser : service.GetRememberUserList().FirstOrDefault(u => u.MostRecent);
        var apps = new Dictionary<uint, SteamApp>();
        var installed = service.GetDownloadingAppList().Where(app => app.IsInstalled)
            .GroupBy(app => app.AppId).ToDictionary(group => group.Key, group => group.First());

        var directory = service.SteamDirPath;
        (KVObject? history, HashSet<uint>? hidden, HashSet<uint>? favourite) = ReadAccount(directory, user);
        var historyById = new Dictionary<uint, KVObject>();
        var ids = installed.Keys.ToHashSet();
        foreach (var node in history?.Children ?? [])
        {
            if (!uint.TryParse(node.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id == 0)
                continue;
            historyById[id] = node;
            ids.Add(id);
        }
        ids.UnionWith(hidden ?? []);
        ids.UnionWith(favourite ?? []);

        if (live)
        {
            foreach (var app in conn.SteamApps.Items)
                apps[app.AppId] = app;
        }
        else
        {
            // The offline appinfo catalog can include unowned games. Join only locally evidenced IDs.
            foreach (var app in conn.SteamApps.Items)
                if (ids.Contains(app.AppId))
                    apps[app.AppId] = app;
        }

        if (ids.Any(id => !apps.ContainsKey(id)) && directory != null)
        {
            try
            {
                await service.ReadAppInfosInBatchesAsync(batch =>
                {
                    foreach (var app in batch)
                        if (ids.Contains(app.AppId))
                            apps.TryAdd(app.AppId, app);
                }, token).ConfigureAwait(false);
            }
            catch (IOException ex)
            {
                Log.Error(nameof(CategoryInventory), ex, "Steam appinfo unavailable for Categories.");
            }
        }

        var toolkitHidden = GameLibrarySettings.HideGameList.Value;
        return ids.Concat(apps.Keys).Distinct().Select(id =>
        {
            token.ThrowIfCancellationRequested();
            var local = installed.GetValueOrDefault(id);
            var app = apps.GetValueOrDefault(id) ?? local ?? new SteamApp(id);
            var entry = historyById.GetValueOrDefault(id);
            var lastPlayed = ReadTime(entry, "LastPlayed");
            var recent = ReadMinutes(entry, "Playtime2wks");
            if (lastPlayed is { } last && last < DateTimeOffset.UtcNow.AddDays(-14))
                recent = 0;
            var (achievementCount, achievementCompletion) = CategoryLocalAchievements.Read(directory, user, id);
            return new CategoryInventoryGame(app, new(id, app.DisplayName)
            {
                PlaytimeMinutes = ReadMinutes(entry, "Playtime"),
                RecentPlaytimeMinutes = recent,
                LastPlayed = lastPlayed,
                Installed = local != null || app.IsInstalled,
                SizeOnDisk = local?.SizeOnDisk ?? (app.IsInstalled ? null : 0),
                Hidden = toolkitHidden?.ContainsKey(id) == true ? true : hidden?.Contains(id),
                Favourite = favourite?.Contains(id),
                AchievementCount = achievementCount,
                AchievementCompletion = achievementCompletion,
                Type = app.Type == SteamAppType.Unknown ? null : app.Type.ToString().ToLowerInvariant(),
                Platforms = app.OSList?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                ReleaseYear = FromUnix(app.SteamReleaseDate)?.Year,
                Developer = app.Developer,
                Publisher = app.Publisher,
            });
        }).OrderBy(game => game.Facts.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    static (KVObject? History, HashSet<uint>? Hidden, HashSet<uint>? Favourite) ReadAccount(string? directory, SteamUser? user)
    {
        if (directory == null || user == null)
            return default;
        var config = Path.Combine(directory, user.UserdataPath, "config");
        KVObject? history = null;
        try
        {
            var local = VdfHelper.Read(Path.Combine(config, "localconfig.vdf"));
            history = Child(Child(Child(Child(local, "Software"), "Valve"), "Steam"), "Apps");
        }
        catch (Exception ex)
        {
            Log.Error(nameof(CategoryInventory), ex, "Steam play history unavailable for Categories.");
        }

        try
        {
            var path = Path.Combine(config, "cloudstorage", "cloud-storage-namespace-1.json");
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var document = JsonDocument.Parse(stream);
            return (history, ReadCollection(document.RootElement, "hidden"), ReadCollection(document.RootElement, "favorite"));
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return (history, null, null);
        }
    }

    static HashSet<uint>? ReadCollection(JsonElement root, string id)
    {
        if (root.ValueKind != JsonValueKind.Array)
            return null;
        var key = "user-collections." + id;
        foreach (var pair in root.EnumerateArray())
        {
            if (pair.ValueKind != JsonValueKind.Array || pair.GetArrayLength() != 2 ||
                pair[0].ValueKind != JsonValueKind.String || pair[0].GetString() != key)
                continue;
            if (pair[1].ValueKind != JsonValueKind.Object || !pair[1].TryGetProperty("value", out var value) ||
                value.ValueKind != JsonValueKind.String)
                return null;
            using var payload = JsonDocument.Parse(value.GetString()!);
            if (payload.RootElement.ValueKind != JsonValueKind.Object ||
                !payload.RootElement.TryGetProperty("added", out var add) || add.ValueKind != JsonValueKind.Array)
                return null;
            var ids = add.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.Number && x.TryGetUInt32(out _))
                .Select(x => x.GetUInt32()).ToHashSet();
            if (payload.RootElement.TryGetProperty("removed", out var remove) && remove.ValueKind == JsonValueKind.Array)
                ids.ExceptWith(remove.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.Number && x.TryGetUInt32(out _))
                    .Select(x => x.GetUInt32()));
            return ids;
        }
        return null;
    }

    static KVObject? Child(KVObject? parent, string name) =>
        parent?.Children.FirstOrDefault(child => child.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    static decimal? ReadMinutes(KVObject? app, string field) =>
        uint.TryParse(Child(app, field)?.Value.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var minutes)
            ? minutes : null;

    static DateTimeOffset? ReadTime(KVObject? app, string field) =>
        uint.TryParse(Child(app, field)?.Value.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var unix)
            ? FromUnix(unix) : null;

    static DateTimeOffset? FromUnix(uint? unix) => unix is > 0 ? DateTimeOffset.FromUnixTimeSeconds(unix.Value) : null;
}
