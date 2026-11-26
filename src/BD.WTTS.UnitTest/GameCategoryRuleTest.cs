using BD.WTTS.GameList.Categories;
using static BD.WTTS.GameList.Categories.CategoryOperator;

namespace BD.WTTS.UnitTest;

[TestFixture]
internal sealed class GameCategoryRuleTest
{
    static readonly DateTimeOffset Now = new(2026, 11, 26, 0, 0, 0, TimeSpan.Zero);
    static readonly CategoryGame Game = new(620, "Portal 2")
    {
        PlaytimeMinutes = 120,
        Installed = true,
        LastPlayed = Now.AddDays(-2),
        Store = new() { Price = 10, ReviewScore = 95, ReviewCount = 250, Tags = ["Puzzle", "Short"] },
    };

    [Test]
    public void ReportsWinningBranchesAndHonorsNegation()
    {
        var score = CategoryRule.Condition("ReviewScore", AtLeast, "90");
        var price = CategoryRule.Condition("Price", Between, "8", "12");
        var installed = CategoryRule.Condition("Installed", Equal, "true");
        var expression = CategoryRule.Group(CategoryRuleKind.All,
            price, CategoryRule.Group(CategoryRuleKind.Any, score, CategoryRule.Group(CategoryRuleKind.Not, installed)));

        var result = CategoryRuleEngine.Match(new("Test", CategoryMode.Auto, expression), Game, Now);
        Assert.That(result!.Rules, Is.EqualTo(new[] { price, score }));
        Assert.That(CategoryRuleEngine.Match(new("Test", CategoryMode.Auto,
            CategoryRule.Group(CategoryRuleKind.Not, price)), Game, Now), Is.Null);
        Assert.That(CategoryRuleEngine.Match(new("Test", CategoryMode.Auto,
            CategoryRule.Group(CategoryRuleKind.Not, price with { Values = ["30", "40"] })), Game, Now), Is.Not.Null);
    }

    [TestCase("Tags", HasTag, "short", null, true)]
    [TestCase("Tags", HasAny, "Action", "Puzzle", true)]
    [TestCase("Tags", HasAll, "Short", "Puzzle", true)]
    [TestCase("Tags", HasAll, "Short", "Action", false)]
    [TestCase("Tags", HasAll, "", null, false)]
    [TestCase("TagCount", Equal, "2", null, true)]
    [TestCase("PricePerHour", Equal, "5", null, true)]
    [TestCase("ReviewCount", LessThan, "5000", null, true)]
    [TestCase("ValueRating", Between, "15.8", "15.9", true)]
    [TestCase("LastPlayed", WithinDays, "2", null, true)]
    [TestCase("LastPlayed", WithinDays, "1", null, false)]
    public void EvaluatesFields(string field, CategoryOperator op, string first, string? second, bool expected)
    {
        var rule = second == null ? CategoryRule.Condition(field, op, first) : CategoryRule.Condition(field, op, first, second);
        Assert.That(CategoryRuleEngine.Match(new("Test", CategoryMode.Auto, rule), Game, Now) != null, Is.EqualTo(expected));
    }

    [TestCase("Price", NotEqual, "10")]
    [TestCase("Price", Between, "2")]
    [TestCase("Tags", IsEmpty, "")]
    [TestCase("ReviewScore", AtLeast, "90")]
    [TestCase("CodeFromNewerVersion", Equal, "true")]
    public void MissingOrUnsupportedFieldsDoNotMatchEvenUnderNot(string field, CategoryOperator op, string value)
    {
        var rule = CategoryRule.Condition(field, op, value);
        var missing = Game with { Store = null };
        Assert.That(CategoryRuleEngine.Match(new("Test", CategoryMode.Auto, rule), missing, Now), Is.Null);
        Assert.That(CategoryRuleEngine.Match(new("Test", CategoryMode.Auto,
            CategoryRule.Group(CategoryRuleKind.Not, rule)), missing, Now), Is.Null);
    }

    [Test]
    public void ManualOverridesWinOnlyInModesThatUseThem()
    {
        var automatic = new GameCategory("Test", CategoryMode.Auto, CategoryRule.Condition("Installed", Equal, "true"))
        {
            ManualIncluded = [620],
            ManualExcluded = [620],
        };
        Assert.That(CategoryRuleEngine.Match(automatic, Game, Now), Is.Not.Null);
        Assert.That(CategoryRuleEngine.Match(automatic with { Mode = CategoryMode.Mixed }, Game, Now), Is.Null);
        Assert.That(CategoryRuleEngine.Match(automatic with { Mode = CategoryMode.Manual, ManualExcluded = [] }, Game, Now)?.Manual, Is.True);
    }
}
