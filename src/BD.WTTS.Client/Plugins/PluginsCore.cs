#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.Runtime.Loader;
using static BD.WTTS.AssemblyInfo;

namespace BD.WTTS.Plugins;

public static class PluginsCore
{
    const string TAG = nameof(PluginsCore);

    /// <summary>
    /// 禁用的插件使用单独的 <see cref="AssemblyLoadContext"/> 加载与卸载
    /// </summary>
    sealed class DisablePluginsAssemblyLoadContext : AssemblyLoadContext
    {
        public DisablePluginsAssemblyLoadContext() : base($"{Constants.HARDCODED_APP_NAME_NEW}.DisablePlugins", true)
        {

        }
    }

    static DisablePluginsAssemblyLoadContext disablePluginsAssemblyLoadContext = new();

    sealed class DisablePlugin : IPlugin
    {
        public string Name { get; private set; } = null!;

        public string Version { get; private set; } = null!;

        public string? Description { get; private set; }

        public string StoreUrl { get; private set; } = null!;

        public string HelpUrl { get; private set; } = null!;

        public Type? SettingsPageViewType => null;

        public string? Author { get; private set; }

        public string AuthorStoreUrl { get; private set; } = null!;

        public string AssemblyLocation { get; private set; } = null!;

        public string AppDataDirectory { get; private set; } = null!;

        public string CacheDirectory { get; private set; } = null!;

        public bool IsOfficial { get; private set; }

        public byte[]? IconBytes { get; private set; }

        public void SetValue(IPlugin plugin)
        {
            Name = plugin.Name;
            Version = plugin.Version;
            Description = plugin.Description;
            StoreUrl = plugin.StoreUrl;
            HelpUrl = plugin.HelpUrl;
            Author = plugin.Author;
            AuthorStoreUrl = plugin.AuthorStoreUrl;
            AssemblyLocation = plugin.AssemblyLocation;
            AppDataDirectory = plugin.AppDataDirectory;
            CacheDirectory = plugin.CacheDirectory;
            IsOfficial = plugin.IsOfficial;
            IconBytes = plugin.IconBytes;
        }

        public void ConfigureDemandServices(IServiceCollection services, Startup startup)
        {
        }

        public void ConfigureRequiredServices(IServiceCollection services, Startup startup)
        {
        }

        public bool ExplicitHasValue()
        {
            return true;
        }

        public IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists)
        {
            return default;
        }

        public IEnumerable<TabItemViewModel>? GetMenuTabItems()
        {
            return default;
        }

        public void OnAddAutoMapper(IMapperConfigurationExpression cfg)
        {
        }

        public ValueTask OnExit()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnInitializeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask OnPeerConnected(bool isReconnected)
        {
            return ValueTask.CompletedTask;
        }

        public void OnUnhandledException(Exception ex, string name, bool? isTerminating = null)
        {
        }

        public Task<int> RunSubProcessMainAsync(string moduleName, string pipeName, string processId, string encodedArgs)
        {
            return Task.FromResult(0);
        }
    }

    //static readonly HashSet<string> ignoreDirNames = new(StringComparer.OrdinalIgnoreCase)
    //{
    //    Update,
    //};

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Assembly LoadFrom(bool disable, string assemblyPath)
    {
        var assembly = (disable && disablePluginsAssemblyLoadContext != null) ?
                        disablePluginsAssemblyLoadContext.LoadFromAssemblyPath(assemblyPath) :
                        Assembly.LoadFrom(assemblyPath);
        return assembly;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static HashSet<PluginResult<Assembly>>? LoadAssemblies(
        HashSet<string>? disablePluginNames = null,
        params string[] loadModules)
    {
        HashSet<PluginResult<Assembly>>? assemblies = null;
#if DEBUG // DEBUG 模式遍历项目查找模块
        var modules = loadModules.Any_Nullable() ? loadModules : new[] {
            Accelerator,
            GameAccount,
            GameList,
            ArchiSteamFarmPlus,
            Authenticator,
            GameTools,
        };
        foreach (var item in modules)
        {
            var disable = disablePluginNames != null && disablePluginNames.Contains(item);

            //HashSet<string?>? entryAssemblyNames_ = null;
            //HashSet<string?> GetEntryAssemblyNames()
            //{
            //    entryAssemblyNames_ ??= new HashSet<string?>(DependencyContext.Load(Assembly.GetEntryAssembly()!)?.GetDefaultAssemblyNames().Select(x => x.Name) ?? Array.Empty<string?>());
            //    return entryAssemblyNames_;
            //}

            var assemblyPath = Path.Combine(ProjectUtils.ProjPath, "src", $"BD.WTTS.Client.Plugins.{item}", "bin", "Debug", ProjectUtils.tfm, $"BD.WTTS.Client.Plugins.{item}.dll");
            if (File.Exists(assemblyPath))
            {
                Assembly assembly;
                try
                {
                    assembly = LoadFrom(disable, assemblyPath);
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
                assemblies.Add(new(disable, assembly));
            }
        }
#else
        var pluginsPath = Path.Combine(AppContext.BaseDirectory, "modules");
        if (Directory.Exists(pluginsPath))
        {
            bool EachDirectories(string directory, string? dirName = null)
            {
                dirName ??= Path.GetDirectoryName(directory);
                if (dirName == null)
                    return true;
                var disable = disablePluginNames != null && disablePluginNames.Contains(dirName);
                var dllPath = Path.Combine(directory,
                    $"Steam++.Plugins.{dirName}.dll");
                if (File.Exists(dllPath))
                {
                    foreach (string assemblyPath in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
                    {
                        Assembly assembly;
                        try
                        {
                            assembly = LoadFrom(disable, assemblyPath);
                        }
                        catch (Exception e)
                        {
                            Log.Error(TAG, e, $"AssemblyLoadFrom fail, assemblyPath: {assemblyPath}");
                            return true;
                        }

                        assemblies ??= new();
                        assemblies.Add(new(disable, assembly));
                    }
                }
                return false;
            }

            if (loadModules.Any_Nullable())
            {
                foreach (var loadModule in loadModules)
                {
                    var directory = Path.Combine(pluginsPath, loadModule);
                    if (!Directory.Exists(directory))
                        continue;
                    if (EachDirectories(directory, loadModule))
                        continue;
                }
            }
            else
            {
                var directories = Directory.EnumerateDirectories(pluginsPath);
                foreach (var directory in directories)
                {
                    if (EachDirectories(directory))
                        continue;
                }
            }
        }
#endif
        return assemblies;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static IEnumerable<PluginResult<Assembly>> VerifyAssemblies(IEnumerable<PluginResult<Assembly>> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var isOk = false;
            try
            {
                // This call is bare minimum to verify if the assembly can load itself
                assembly.Data.GetTypes();
                isOk = true;
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, $"assembly.GetTypes fail, assembly: {assembly}");
            }
            if (isOk) yield return assembly;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static HashSet<IPlugin>? GetExports(IEnumerable<Assembly> assemblies)
    {
        ConventionBuilder conventions = new();
        conventions.ForTypesDerivedFrom<IPlugin>().Export<IPlugin>();

        var configuration = new ContainerConfiguration().WithAssemblies(assemblies, conventions);

        HashSet<IPlugin> plugins;
        try
        {
            using CompositionHost container = configuration.CreateContainer();
            plugins = container.GetExports<IPlugin>()
                .Where(x => x.HasValue())
                .ToHashSet();
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "CompositionHost.GetExports fail.");
            return null;
        }
        return plugins;
    }

    internal static HashSet<PluginResult<IPlugin>>? InitPlugins(
        HashSet<string>? disablePluginNames = null,
        params string[] loadModules)
    {
        var assemblies = LoadAssemblies(disablePluginNames, loadModules);
        if (!assemblies.Any_Nullable()) return null;

        var assemblies_ = VerifyAssemblies(assemblies).ToArray();
        if (!assemblies_.Any()) return null;

        HashSet<PluginResult<IPlugin>> pluginResults = new();
        var activePlugins = GetExports(assemblies_.Where(x => !x.IsDisable).Select(x => x.Data));
        if (activePlugins != null)
        {
            foreach (var plugin in activePlugins)
            {
                pluginResults.Add(new(false, plugin));
            }
        }
        var disablePlugins = GetExports(assemblies_.Where(x => x.IsDisable).Select(x => x.Data));
        if (disablePlugins != null)
        {
            foreach (var plugin in disablePlugins)
            {
                var plugin_ = new DisablePlugin();
                plugin_.SetValue(plugin);
                pluginResults.Add(new(true, plugin_));
            }
            disablePluginsAssemblyLoadContext.Unload();
            disablePluginsAssemblyLoadContext = null!;
        }
        return pluginResults;
    }
}
#endif