using System.Reflection;
using static System.Application.ResSecrets;

namespace System.Application
{
    static class PlatformResSecrets
    {
        internal static string? GetResValue(string name, bool isSingle, ResValueFormat format, string namespacePrefix = Prefix_Res, Assembly? assembly = null)
        {
            assembly ??= typeof(PlatformResSecrets).Assembly;
            return ResSecrets.GetResValue(name, isSingle, format, namespacePrefix, assembly);
        }
    }
}
