#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

namespace BD.WTTS.Plugins;

public static class PluginsCore
{
    const string TAG = nameof(PluginsCore);

    static readonly HashSet<string> ignoreDirNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Update",
    };

    internal static HashSet<Assembly>? LoadAssemblies()
    {
        HashSet<Assembly>? assemblies = null;
        var pluginsPath = Path.Combine(AppContext.BaseDirectory, "modules");
        if (Directory.Exists(pluginsPath))
        {
            var directories = Directory.EnumerateDirectories(pluginsPath);
            foreach (var directory in directories)
            {
                var dirName = Path.GetDirectoryName(directory);
                if (dirName == null) continue;
                if (ignoreDirNames.Contains(dirName)) continue;
                var dllPath = Path.Combine(directory,
                    $"Steam++.Plugins.{dirName}.dll");
                if (File.Exists(dllPath))
                {
                    foreach (string assemblyPath in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
                    {
                        Assembly assembly;
                        try
                        {
                            assembly = Assembly.LoadFrom(assemblyPath);
                        }
                        catch (Exception e)
                        {
                            Log.Error(TAG, e, $"AssemblyLoadFrom fail, assemblyPath: {assemblyPath}");
                            continue;
                        }

                        assemblies ??= new();
                        assemblies.Add(assembly);
                    }
                }
            }
        }
        return assemblies;
    }

    static IEnumerable<Assembly> VerifyAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            var isOk = false;
            try
            {
                // This call is bare minimum to verify if the assembly can load itself
                assembly.GetTypes();
                isOk = true;
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, $"assembly.GetTypes fail, assembly: {assembly}");
            }
            if (isOk) yield return assembly;
        }
    }

    internal static HashSet<IPlugin>? InitPlugins()
    {
        HashSet<Assembly>? assemblies = LoadAssemblies();
        if (!assemblies.Any_Nullable()) return null;

        var assemblies_ = VerifyAssemblies(assemblies).ToArray();
        if (!assemblies_.Any()) return null;

        ConventionBuilder conventions = new();
        conventions.ForTypesDerivedFrom<IPlugin>().Export<IPlugin>();

        var configuration = new ContainerConfiguration().WithAssemblies(assemblies, conventions);

        HashSet<IPlugin> activePlugins;
        try
        {
            using CompositionHost container = configuration.CreateContainer();
            activePlugins = container.GetExports<IPlugin>().ToHashSet();
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "CompositionHost.GetExports fail.");
            return null;
        }
        return activePlugins;
    }
}
#endif