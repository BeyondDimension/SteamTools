using System.Reflection;
using static System.Application.Security.ResSecrets;

namespace System.Application.Security
{
    public static class PlatformResSecrets
    {
        public static string? GetResValue(string name, bool isSingle, ResValueFormat format, string namespacePrefix = Prefix_Res, Assembly? assembly = null)
        {
            assembly ??= typeof(PlatformResSecrets).Assembly;
            return ResSecrets.GetResValue(name, isSingle, format, namespacePrefix, assembly);
        }
    }
}
