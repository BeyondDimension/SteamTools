using System.Globalization;
using static BD.WTTS.GameList.Categories.CategoryOperator;

namespace BD.WTTS.GameList.Categories;

internal sealed record CategoryMatch(bool Manual, CategoryRule[] Rules);

internal static class CategoryRuleEngine
{
    public static CategoryMatch? Match(GameCategory category, CategoryGame game, DateTimeOffset now)
    {
        if (category.Mode != CategoryMode.Auto)
        {
            if (category.ManualExcluded.Contains(game.AppId))
                return null;
            if (category.ManualIncluded.Contains(game.AppId))
                return new(true, []);
        }
        if (category.Mode == CategoryMode.Manual)
            return null;

        var evidence = new List<CategoryRule>();
        return Evaluate(category.Rules, game, now, evidence) == true ? new(false, [.. evidence]) : null;
    }

    // Unknown is preserved through NOT. A missing price, for example, cannot prove that a game isn't free.
    static bool? Evaluate(CategoryRule rule, CategoryGame game, DateTimeOffset now, List<CategoryRule> evidence)
    {
        var start = evidence.Count;
        var result = rule.Kind switch
        {
            CategoryRuleKind.Condition => EvaluateCondition(rule, game, now),
            CategoryRuleKind.All or CategoryRuleKind.Any => EvaluateGroup(rule, game, now, evidence),
            CategoryRuleKind.Not when rule.Children.Length == 1 => !Evaluate(rule.Children[0], game, now, evidence),
            _ => null,
        };

        if (result != true)
            evidence.RemoveRange(start, evidence.Count - start);
        else if (rule.Kind is CategoryRuleKind.Condition or CategoryRuleKind.Not)
            evidence.Add(rule);
        return result;
    }

    static bool? EvaluateGroup(CategoryRule rule, CategoryGame game, DateTimeOffset now, List<CategoryRule> evidence)
    {
        if (rule.Children.Length == 0)
            return null;
        var all = rule.Kind == CategoryRuleKind.All;
        var unknown = false;
        var matched = false;
        foreach (var child in rule.Children)
        {
            var result = Evaluate(child, game, now, evidence);
            if (all && result == false)
                return false;
            matched |= result == true;
            unknown |= result is null;
        }
        return !all && matched ? true : unknown ? null : all;
    }

    static bool? EvaluateCondition(CategoryRule rule, CategoryGame game, DateTimeOffset now)
    {
        if (!CategoryField.Registry.TryGetValue(rule.Field, out var field) ||
            !field.Operators.Contains(rule.Operator))
            return null;

        var values = rule.Values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToArray();
        var value = values.FirstOrDefault();
        var op = rule.Operator;
        if (value is null && op != IsEmpty)
            return null;

        return field.Read(game) switch
        {
            decimal number => CompareNumber(number, op, values),
            bool flag when bool.TryParse(value, out var expected) => op == Equal ? flag == expected : flag != expected,
            DateTimeOffset date => CompareDate(date, op, values, now),
            string text => CompareText(text, op, values),
            string[] items => CompareList(items, op, values),
            _ => null,
        };
    }

    static bool? CompareNumber(decimal actual, CategoryOperator op, string[] values)
    {
        if (values.Length == 0 || !TryNumber(values[0], out var first))
            return null;
        if (op != Between)
            return Compare(actual, first, op);
        return values.Length == 2 && TryNumber(values[1], out var last) && first <= last
            ? actual >= first && actual <= last : null;
    }

    static bool? CompareDate(DateTimeOffset actual, CategoryOperator op, string[] values, DateTimeOffset now)
    {
        if (op is WithinDays or OlderThanDays)
        {
            var age = now - actual;
            if (values.Length != 1 || !int.TryParse(values[0], NumberStyles.None, CultureInfo.InvariantCulture, out var days) ||
                age < TimeSpan.Zero)
                return null;
            return op == WithinDays ? age.TotalDays <= days : age.TotalDays > days;
        }
        if (values.Length == 0 || !TryDate(values[0], out var first))
            return null;
        var day = DateOnly.FromDateTime(actual.UtcDateTime);
        if (op != Between)
            return Compare(day, first, op);
        return values.Length == 2 && TryDate(values[1], out var last) && first <= last
            ? day >= first && day <= last : null;
    }

    static bool TryNumber(string value, out decimal number) =>
        decimal.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out number);

    static bool TryDate(string value, out DateOnly date) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    static bool Compare<T>(T actual, T target, CategoryOperator op) where T : IComparable<T>
    {
        var compare = actual.CompareTo(target);
        return op switch
        {
            Equal => compare == 0,
            NotEqual => compare != 0,
            GreaterThan => compare > 0,
            AtLeast => compare >= 0,
            LessThan => compare < 0,
            AtMost => compare <= 0,
            _ => false,
        };
    }

    static bool Same(string left, string right) => StringComparer.OrdinalIgnoreCase.Equals(left, right);

    static bool CompareText(string text, CategoryOperator op, string[] values) => op switch
    {
        Equal => Same(text, values[0]),
        NotEqual => !Same(text, values[0]),
        CategoryOperator.Contains => text.Contains(values[0], StringComparison.OrdinalIgnoreCase),
        In => values.Contains(text, StringComparer.OrdinalIgnoreCase),
        IsEmpty => string.IsNullOrWhiteSpace(text),
        _ => false,
    };

    static bool CompareList(string[] items, CategoryOperator op, string[] values) => op switch
    {
        CategoryOperator.Contains => items.Any(item => item.Contains(values[0], StringComparison.OrdinalIgnoreCase)),
        HasTag => items.Contains(values[0], StringComparer.OrdinalIgnoreCase),
        HasAny => items.Intersect(values, StringComparer.OrdinalIgnoreCase).Any(),
        HasAll => values.All(value => items.Contains(value, StringComparer.OrdinalIgnoreCase)),
        IsEmpty => items.Length == 0,
        _ => false,
    };
}
