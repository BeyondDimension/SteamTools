using System.Globalization;
using System.Resources;

namespace BD.WTTS.Client.Resources;

internal static class ProxyStrings
{
    static readonly ResourceManager Resources = new(typeof(ProxyStrings));

    internal static string DnsPollution(CultureInfo? culture) => Resources.GetString(nameof(DnsPollution), culture)!;
}
