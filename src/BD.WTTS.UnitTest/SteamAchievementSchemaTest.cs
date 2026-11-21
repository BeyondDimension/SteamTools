using BD.SteamClient.Models;
using KeyValue = BD.SteamClient.Models.KeyValue;

namespace BD.WTTS.UnitTest;

public sealed class SteamAchievementSchemaTest
{
    [Test]
    public void ReadsBothStatFormatsAndOrdersLocalizedGroupsStably()
    {
        var game = Block("480",
            Block("groups",
                Block("1", Text("name", "No explicit order")),
                Block("3", Int("order", 2), Int("dlcappid", 42),
                    Block("name", Text("english", "English"), Text("german", "Deutsch"), Text("french", "Français"))),
                Block("2", Text("order", "2"), Text("dlcappid", "4294967295"),
                    Block("name", Text("english", "English"), Text("french", "Français"))),
                Block("9", Int("order", int.MaxValue), Block("name", Text("english", "English")))),
            Block("stats",
                Block("0", Int("type_int", 4), Block("bits", Bit("0", "base"))),
                Block("1", Text("type", "groupachievements"), Block("bits",
                    Bit("0", "last", Int("groupid", 1)),
                    Bit("1", "three", Int("groupid", 3)),
                    Bit("2", "two-first", Text("groupid", "2")),
                    Bit("3", "two-second", Int("groupid", 2)),
                    Bit("4", "max-order", Int("groupid", 9)),
                    Bit("5", "explicit-base", Int("groupid", 0))))));

        var rows = AchievementSchema.ReadAchievements(game, 480, "german", "french");
        Assert.That(rows.Select(x => x.Id), Is.EqualTo(new[] { "base", "explicit-base", "two-first", "two-second", "three", "max-order", "last" }));
        Assert.Multiple(() =>
        {
            Assert.That(rows[0].Set, Is.Null);
            Assert.That(rows[1].Set, Is.Null);
            Assert.That(rows[2].Set, Is.EqualTo(new AchievementSet(2, "Français", uint.MaxValue, 2)));
            Assert.That(rows[3].Set, Is.SameAs(rows[2].Set));
            Assert.That(rows[4].Set, Is.EqualTo(new AchievementSet(3, "Deutsch", 42, 2)));
            Assert.That(rows[5].Set, Is.EqualTo(new AchievementSet(9, "English", 0, int.MaxValue)));
            Assert.That(rows[6].Set, Is.EqualTo(new AchievementSet(1, "No explicit order", 0, int.MaxValue)));
        });
    }

    [Test]
    public void MalformedOrAmbiguousRestrictionsDoNotFallBackToBase()
    {
        var game = Block("480",
            Block("groups",
                Block("2", Text("dlcappid", "bad")),
                Block("3", Text("dlcappid", "4294967296")),
                Block("4", Int("dlcappid", -1)),
                Block("5", Int("dlcappid", 42)),
                Block("05")),
            Block("stats", Block("1", Text("type", "4"), Block("bits",
                Bit("0", "bad-membership", Text("groupid", "invalid")),
                Bit("1", "dangling", Int("groupid", 99)),
                Bit("2", "bad-dlc", Int("groupid", 2)),
                Bit("3", "overflow-dlc", Int("groupid", 3)),
                Bit("4", "negative-dlc", Int("groupid", 4)),
                Bit("5", "duplicate-group", Int("groupid", 5)),
                Bit("6", "duplicate-membership", Int("groupid", 0), Int("GroupId", 5)),
                Bit("7", "bad-permission", Text("permission", "x")),
                Bit("8", "bad-archived", Text("archived", "x")),
                Bit("9", "ambiguous"),
                Bit("10", "ambiguous", Int("groupid", 2)),
                Bit("11", "valid-base")))));

        Assert.That(AchievementSchema.ReadAchievements(game, 480, "german", "english").Select(x => x.Id),
            Is.EqualTo(new[] { "valid-base" }));
    }

    [Test]
    public void LegacySchemaPreservesDisplayMetadataAndDoesNotPermitArchivedEdits()
    {
        var stat = Block("0", Int("type", 4),
            Block("bits", Bit("0", "archived", Int("archived", 1), Block("display",
                Text("name", "Title"), Block("desc", Text("english", "Description")),
                Text("icon", "normal.jpg"), Text("icon_gray", "gray.jpg"), Int("hidden", 1)))),
            Block("bits", Bit("1", "protected", Text("permission", "2")), Bit("2", "editable")));
        var game = Block("480", Block("stats", stat));

        Assert.That(AchievementSchema.GetStatType(stat), Is.EqualTo(UserStatType.Achievements));
        Assert.That(AchievementSchema.GetStatType(Block("0", Text("type_int", "5"))), Is.EqualTo(UserStatType.GroupAchievements));
        Assert.That(AchievementSchema.GetStatType(Block("0", Text("type", "Integer, Achievements"))), Is.EqualTo(UserStatType.Invalid));
        Assert.That(AchievementSchema.GetStatType(Block("0", Int("type", 4), Int("type_int", 5))), Is.EqualTo(UserStatType.Invalid));

        var rows = AchievementSchema.ReadAchievements(game, 480, "german", "french");
        Assert.That(rows.Select(x => x.Id), Is.EqualTo(new[] { "archived", "protected", "editable" }));
        Assert.Multiple(() =>
        {
            Assert.That(rows[0].AppId, Is.EqualTo(480));
            Assert.That(rows[0].Name, Is.EqualTo("Title"));
            Assert.That(rows[0].Description, Is.EqualTo("Description"));
            Assert.That(rows[0].IconNormal, Is.EqualTo("normal.jpg"));
            Assert.That(rows[0].IconLocked, Is.EqualTo("gray.jpg"));
            Assert.That(rows[0].IsHidden, Is.True);
            Assert.That(rows[0].Set, Is.Null);
            Assert.That(rows[0].IsArchived, Is.True);
            Assert.That(rows[0].CanEdit, Is.False);
            Assert.That(rows[1].CanEdit, Is.False);
            Assert.That(rows[2].CanEdit, Is.True);
            Assert.That(rows[2].Name, Is.EqualTo("editable"));
        });
    }

    [TestCase("INT", UserStatType.Integer)]
    [TestCase("AVGRATE", UserStatType.AverageRate)]
    public void ReadsSteamStatNamesFromCurrentSchemas(string name, UserStatType type)
        => Assert.That(AchievementSchema.GetStatType(Block("stat", Text("type", name))), Is.EqualTo(type));

    static KeyValue Bit(string index, string id, params KeyValue[] metadata)
        => Block(index, [Text("name", id), .. metadata]);

    static KeyValue Block(string name, params KeyValue[] children)
        => new() { Name = name, Valid = true, Type = KeyValueType.None, Children = new List<KeyValue>(children) };

    static KeyValue Int(string name, int value)
        => new() { Name = name, Valid = true, Type = KeyValueType.Int32, Value = value };

    static KeyValue Text(string name, string value)
        => new() { Name = name, Valid = true, Type = KeyValueType.String, Value = value };
}
