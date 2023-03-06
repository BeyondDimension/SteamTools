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
#if DEBUG
        var modules = new[] { "Accelerator", "AccountSwitch", "ArchiSteamFarm", "GameList", "GameTools", "LocalAuth" };
        foreach (var item in modules)
        {
            const string tfm =
#if WINDOWS
                "-windows10.0.19041.0";
#elif LINUX
                "";
#elif MACCATALYST
                "maccatalyst";
#elif MACOS
                "macos";
#endif
            //HashSet<string?>? entryAssemblyNames_ = null;
            //HashSet<string?> GetEntryAssemblyNames()
            //{
            //    entryAssemblyNames_ ??= new HashSet<string?>(DependencyContext.Load(Assembly.GetEntryAssembly()!)?.GetDefaultAssemblyNames().Select(x => x.Name) ?? Array.Empty<string?>());
            //    return entryAssemblyNames_;
            //}

            var assemblyPath = Path.Combine(Utils.ProjPath, "src", $"BD.WTTS.Client.Plugins.{item}", "bin", "Debug", $"net{Environment.Version.Major}.{Environment.Version.Minor}{tfm}", $"BD.WTTS.Client.Plugins.{item}.dll");
            if (File.Exists(assemblyPath))
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(assemblyPath);
                    //var assemblyNames = DependencyContext.Load(assembly)?.GetDefaultAssemblyNames()?.ToArray();
                    //if (assemblyNames.Any_Nullable())
                    //{
                    //    var entryAssemblyNames = GetEntryAssemblyNames();
                    //    assemblyNames = assemblyNames.Where(x => !entryAssemblyNames!.Contains(x.Name)).ToArray();
                    //    foreach (var assemblyName in assemblyNames)
                    //    {
                    //        try
                    //        {
                    //            Assembly.Load(assemblyName);
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            Log.Error(TAG, e, $"AssemblyLoad fail, assemblyName: {assemblyName}");
                    //        }
                    //    }
                    //}
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
#else
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
#endif
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

        var configuration = new ContainerConfiguration().WithAssemblies(assemblies_, conventions);

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