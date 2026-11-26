using BD.SteamClient.Models;
using BD.WTTS.GameList.Categories;

namespace BD.WTTS.UnitTest;

[TestFixture]
public sealed class CategoryLocalAchievementsTest
{
    [Test]
    public void ReadsHighAchievementBitAndIgnoresLockedArchivedEntries()
    {
        var game = Group("620", Group("stats", Group("1", Number("type", 4),
            Group("bits",
                Group("0", Text("name", "First")),
                Group("2", Text("name", "Archived"), Number("archived", 1)),
                Group("31", Text("name", "HighBit"))))));
        var cache = Group("cache", Number("crc", 1), Group("1", Number("data", int.MinValue)));

        var result = CategoryLocalAchievements.Summarize(game, cache, 620);
        Assert.That(result, Is.EqualTo(((int?)3, (decimal?)50)));
        Assert.That(CategoryLocalAchievements.Summarize(game, null, 620).Completion, Is.Null);
    }

    static KeyValue Group(string name, params KeyValue[] elements) =>
        new() { Name = name, Valid = true, Children = [.. elements] };

    static KeyValue Number(string name, int value) =>
        new() { Name = name, Valid = true, Type = KeyValueType.Int32, Value = value };

    static KeyValue Text(string name, string value) =>
        new() { Name = name, Valid = true, Type = KeyValueType.String, Value = value };
}
