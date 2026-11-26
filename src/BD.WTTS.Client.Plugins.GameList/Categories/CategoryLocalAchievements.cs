using System.Globalization;
using BD.SteamClient.Models;
using KeyValue = BD.SteamClient.Models.KeyValue;

namespace BD.WTTS.GameList.Categories;

internal static class CategoryLocalAchievements
{
    public static (int? Count, decimal? Completion) Read(string? directory, SteamUser? user, uint id)
    {
        if (directory == null || user == null || id > int.MaxValue)
            return default;
        var path = Path.Combine(directory, "appcache", "stats");
        var appId = id.ToString(CultureInfo.InvariantCulture);
        var accountId = ((uint)user.SteamId64).ToString(CultureInfo.InvariantCulture);
        var playerFile = Path.Combine(path, $"UserGameStats_{accountId}_{appId}.bin");
        if (!File.Exists(playerFile))
            return default;

        var schema = KeyValue.LoadAsBinary(Path.Combine(path, $"UserGameStatsSchema_{appId}.bin"));
        var player = KeyValue.LoadAsBinary(playerFile);
        try
        {
            return Summarize(schema?[appId], player?["cache"], (int)id);
        }
        catch (InvalidOperationException)
        {
            // Duplicate keys indicate an ambiguous or partially written cache entry.
            return default;
        }
    }

    internal static (int? Count, decimal? Completion) Summarize(KeyValue? game, KeyValue? cache, int appId)
    {
        if (game is not { Valid: true } || game["stats"].Children is not { } stats)
            return default;
        var definitions = AchievementSchema.ReadAchievements(game, appId, "english", "english")
            .ToDictionary(definition => definition.Id!, StringComparer.Ordinal);
        if (cache is not { Valid: true } || !cache["crc"].Valid)
            return (definitions.Count, null);

        // Compute completion for base-game achievements. DLC and optional sets depend on account entitlements.
        var total = 0;
        var achieved = 0;
        foreach (var stat in stats)
        {
            if (AchievementSchema.GetStatType(stat) is not (UserStatType.Achievements or UserStatType.GroupAchievements))
                continue;
            var value = cache[stat.Name]["data"];
            if (value.Valid && value.Value is not int)
                return (definitions.Count, null);
            var bits = value.AsInteger(0);
            foreach (var bit in stat["bits"].Children ?? [])
            {
                var name = bit["name"].AsString(string.Empty);
                if (name == null || !definitions.TryGetValue(name, out var definition) || definition.Set is not null ||
                    !int.TryParse(bit.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var bitIndex) || bitIndex > 31)
                    continue;
                var unlocked = (bits & (1 << bitIndex)) != 0;
                if (!definition.IsArchived || unlocked)
                    total++;
                if (unlocked)
                    achieved++;
            }
        }
        return (definitions.Count, total == 0 ? null : achieved * 100m / total);
    }
}
