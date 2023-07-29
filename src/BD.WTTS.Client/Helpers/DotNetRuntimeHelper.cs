// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static class DotNetRuntimeHelper
{
    // https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-environment-variables#net-sdk-and-cli-environment-variables

    const string EnvName_DotNetRoot = "DOTNET_ROOT";
    //const string EnvName_DotNetRootX86 = "DOTNET_ROOT(x86)";

    static readonly Lazy<string?> dotnetRoot = new(GetDotNetRoot);

    static string? GetDotNetRoot()
    {
        try
        {
            var value = Environment.GetEnvironmentVariable(EnvName_DotNetRoot);
            // C:\Program Files\dotnet\shared\Microsoft.NETCore.App\7.0.7\System.Private.CoreLib.dll
            var location = typeof(object).Assembly.Location;
            value = Path.GetFullPath(Path.Combine(location, "..", "..", "..", ".."));
            return value;
        }
        catch
        {
            return null;
        }
    }

    public static void AddEnvironment(ProcessStartInfo psi)
    {
        var rootValue = dotnetRoot.Value;
        if (rootValue != null)
        {
            psi.Environment.TryAdd(EnvName_DotNetRoot, rootValue);
        }
    }
}
