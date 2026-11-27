using BD.WTTS.Client.Resources;

namespace BD.WTTS.Services.Implementation;

public static class ScriptVersionComparer
{
    internal static string FormatVersionDisplay(string? version)
    {
        var text = TrimVersionPrefix(version);
        return text.Length == 0 ? Strings.Script_Inspect_None : "v" + text;
    }

    internal static string TrimVersionPrefix(string? version)
    {
        var text = version?.Trim() ?? string.Empty;
        return text is ['v' or 'V', ..] ? text[1..] : text;
    }

    public static int Compare(string? left, string? right)
    {
        var (leftCore, leftPreRelease) = Parse(left);
        var (rightCore, rightPreRelease) = Parse(right);

        for (var i = 0; i < Math.Max(leftCore.Length, rightCore.Length); i++)
        {
            var result = CompareIdentifier(i < leftCore.Length ? leftCore[i] : "0",
                i < rightCore.Length ? rightCore[i] : "0");
            if (result != 0)
                return result;
        }

        if (leftPreRelease.Length == 0 || rightPreRelease.Length == 0)
            return (leftPreRelease.Length == 0).CompareTo(rightPreRelease.Length == 0);

        for (var i = 0; i < Math.Min(leftPreRelease.Length, rightPreRelease.Length); i++)
        {
            var result = CompareIdentifier(leftPreRelease[i], rightPreRelease[i]);
            if (result != 0)
                return result;
        }

        return leftPreRelease.Length.CompareTo(rightPreRelease.Length);
    }

    static (string[] Core, string[] PreRelease) Parse(string? version)
    {
        version = version?.Trim() ?? string.Empty;
        if (version.Length > 1 && char.IsAsciiDigit(version[1]))
            version = TrimVersionPrefix(version);

        var buildIndex = version.IndexOf('+');
        if (buildIndex >= 0)
            version = version[..buildIndex];

        var suffixIndex = version.IndexOf('-');
        return suffixIndex >= 0
            ? (version[..suffixIndex].Split('.'), version[(suffixIndex + 1)..].Split('.'))
            : (version.Length == 0 ? [] : version.Split('.'), []);
    }

    static int CompareIdentifier(string left, string right)
    {
        var leftIsNumber = left.Length != 0 && left.All(char.IsAsciiDigit);
        var rightIsNumber = right.Length != 0 && right.All(char.IsAsciiDigit);
        if (leftIsNumber && rightIsNumber)
        {
            left = left.TrimStart('0');
            right = right.TrimStart('0');
            var lengthOrder = left.Length.CompareTo(right.Length);
            return lengthOrder != 0 ? lengthOrder : string.CompareOrdinal(left, right);
        }

        if (leftIsNumber != rightIsNumber)
            return leftIsNumber ? -1 : 1;

        return string.Compare(left, right, StringComparison.Ordinal);
    }
}
