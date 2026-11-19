#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
#if !WTTS_NATIVE_AOT
using System.Runtime.Loader;
#endif
using static BD.WTTS.AssemblyInfo;

namespace BD.WTTS.Plugins;

public static partial class PluginsCore
{
    const string TAG = nameof(PluginsCore);

#if !WTTS_NATIVE_AOT
    /// <summary>
    /// 禁用的插件使用单独的 <see cref="AssemblyLoadContext"/> 加载与卸载
    /// </summary>
    sealed class DisablePluginsAssemblyLoadContext : AssemblyLoadContext
    {
        public DisablePluginsAssemblyLoadContext() : base($"{Constants.HARDCODED_APP_NAME_NEW}.DisablePlugins", true)
        {

        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyLoadContext = DefaultAssemblyLoadContext;
            if (assemblyLoadContext != this)
            {
                try
                {
                    var assembly = assemblyLoadContext.LoadFromAssemblyName(assemblyName);
                    if (assembly != null)
                    {
                        return assembly;
                    }
                }
                catch
                {

                }
            }
            return base.Load(assemblyName);
        }
    }

    static AssemblyLoadContext DefaultAssemblyLoadContext
    {
        get
        {
            // 在 Windows 上通过自定义 AppHost 程序集加载上下文为 IsolatedComponentLoadContext 而不是 Default
            var assemblyLoadContext = AssemblyLoadContext.GetLoadContext(typeof(PluginsCore).Assembly);
            assemblyLoadContext ??= AssemblyLoadContext.Default;
            return assemblyLoadContext;
        }
    }

    static DisablePluginsAssemblyLoadContext disablePluginsAssemblyLoadContext = new();
#endif

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    sealed record class DisablePlugin : IPlugin
    {
        bool hasValue;
        string? hasNotValueError;
        readonly string name;
        readonly string? description;

        public DisablePlugin(IPlugin plugin
#if WTTS_NATIVE_AOT
            , bool hasValue, string? hasNotValueError
#endif
            )
        {
#if !WTTS_NATIVE_AOT
            StackTrace stackTrace = new();
            var isInternalCall = ReflectionHelper.IsInternalCall<DisablePlugin>(stackTrace);
            if (!isInternalCall)
                throw new ApplicationException("Disable reflection calls to this constructor.");
#endif

            Id = plugin.Id;
            name = plugin.Name;
            NameResourceKey = plugin.NameResourceKey;
            UniqueEnglishName = plugin.UniqueEnglishName;
            Version = plugin.Version;
            description = plugin.Description;
            DescriptionResourceKey = plugin.DescriptionResourceKey;
            StoreUrl = plugin.StoreUrl;
            HelpUrl = plugin.HelpUrl;
            Author = plugin.Author;
            AuthorStoreUrl = plugin.AuthorStoreUrl;
            AssemblyLocation = plugin.AssemblyLocation;
            AppDataDirectory = plugin.AppDataDirectory;
            CacheDirectory = plugin.CacheDirectory;
            IsOfficial = plugin.IsOfficial;
            Icon = plugin.Icon;
            InstallTime = plugin.InstallTime;
            ReleaseTime = plugin.ReleaseTime;

#if WTTS_NATIVE_AOT
            this.hasValue = hasValue;
            this.hasNotValueError = hasNotValueError;
#else
            try
            {
                hasValue = plugin.HasValue(out hasNotValueError);
            }
            catch (Exception ex)
            {
                hasValue = false;
                hasNotValueError = ex.ToString();
            }
#endif
        }

        string DebuggerDisplay => $"{UniqueEnglishName} v{Version}";

        public override string ToString() => DebuggerDisplay;

        public Guid Id { get; init; }

        public string Name => NameResourceKey is { } key ? Strings.ResourceManager.GetString(key, Strings.Culture) ?? name : name;

        public string? NameResourceKey { get; }

        public string UniqueEnglishName { get; init; }

        public string Version { get; init; }

        public string? Description => DescriptionResourceKey is { } key ? Strings.ResourceManager.GetString(key, Strings.Culture) ?? description : description;

        public string? DescriptionResourceKey { get; }

        public string StoreUrl { get; init; }

        public string HelpUrl { get; init; }

        public Type? SettingsPageViewType => null;

        public string? Author { get; init; }

        public string AuthorStoreUrl { get; init; }

        public string AssemblyLocation { get; init; }

        public string AppDataDirectory { get; init; }

        public string CacheDirectory { get; init; }

        public bool IsOfficial { get; init; }

        public object? Icon { get; init; }

        public DateTimeOffset InstallTime { get; init; }

        public DateTimeOffset ReleaseTime { get; init; }

        string? IPlugin.LoadError
        {
            get => hasNotValueError;
            set
            {
                hasNotValueError = value;
                hasValue = string.IsNullOrWhiteSpace(value);
            }
        }

        public bool HasValue([NotNullWhen(false)] out string? error)
        {
            error = hasNotValueError!;
            return hasValue;
        }

        public void ConfigureDemandServices(IServiceCollection services, Startup startup)
        {
        }

        public void ConfigureRequiredServices(IServiceCollection services, Startup startup)
        {
        }

        public IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists)
        {
            return default;
        }

        public IEnumerable<MenuTabItemViewModel>? GetMenuTabItems()
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

        public ValueTask OnCommandRun(params string[] commandParams)
        {
            return ValueTask.CompletedTask;
        }

        public void OnUnhandledException(Exception ex, string name, bool? isTerminating = null)
        {
        }

        public Task<int> RunSubProcessMainAsync(string moduleName, string pipeName, string processId, string? encodedArgs)
        {
            return Task.FromResult(0);
        }
    }

#if !WTTS_NATIVE_AOT
    static bool IsModuleSupported(string name) =>
        OperatingSystem.IsWindows() ||
        (!string.Equals(name, GameTools, StringComparison.OrdinalIgnoreCase) &&
         !string.Equals(name, ArchiSteamFarmPlus, StringComparison.OrdinalIgnoreCase));

    //static readonly HashSet<string> ignoreDirNames = new(StringComparer.OrdinalIgnoreCase)
    //{
    //    Update,
    //};

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Assembly LoadFrom(bool disable, string assemblyPath)
    {
        AssemblyLoadContext? assemblyLoadContext;
        if (disable && disablePluginsAssemblyLoadContext != null)
        {
            assemblyLoadContext = disablePluginsAssemblyLoadContext;
        }
        else
        {
            assemblyLoadContext = DefaultAssemblyLoadContext;
        }
        var assembly = assemblyLoadContext.LoadFromAssemblyPath(assemblyPath);
        return assembly;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static HashSet<PluginResult<Assembly>>? LoadAssemblies(
        HashSet<string>? disablePluginNames = null,
        params string[] loadModules)
    {
        HashSet<PluginResult<Assembly>>? assemblies = null;
#if DEBUG // DEBUG 模式遍历项目查找模块
        var projPath = ProjectUtils.ProjPath;
        if (!string.IsNullOrWhiteSpace(projPath))
        {
            var modules = loadModules.Any_Nullable() ? loadModules : new[] {
                Accelerator,
                GameAccount,
                GameList,
                ArchiSteamFarmPlus,
                Authenticator,
                GameTools,
                SteamIdleCard,
            };
            foreach (var item in modules)
            {
                if (!IsModuleSupported(item))
                    continue;

                var disable = disablePluginNames != null && disablePluginNames.Contains(item);

                //HashSet<string?>? entryAssemblyNames_ = null;
                //HashSet<string?> GetEntryAssemblyNames()
                //{
                //    entryAssemblyNames_ ??= new HashSet<string?>(DependencyContext.Load(Assembly.GetEntryAssembly()!)?.GetDefaultAssemblyNames().Select(x => x.Name) ?? Array.Empty<string?>());
                //    return entryAssemblyNames_;
                //}

                var assemblyPath = Path.Combine(projPath, "src", $"BD.WTTS.Client.Plugins.{item}", "bin", "Debug", ProjectUtils.tfm, $"BD.WTTS.Client.Plugins.{item}.dll");
#if WINDOWS
                // MSBuild also accepts the Windows platform version with a trailing zero component.
                if (!File.Exists(assemblyPath))
                    assemblyPath = Path.Combine(projPath, "src", $"BD.WTTS.Client.Plugins.{item}", "bin", "Debug", ProjectUtils.tfm + ".0", $"BD.WTTS.Client.Plugins.{item}.dll");
#endif
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
        }
#endif
        var pluginsPath = Path.Combine(AppContext.BaseDirectory, "modules");
        if (Directory.Exists(pluginsPath))
        {
            bool EachDirectories(string directory, string? dirName = null)
            {
                if (string.IsNullOrEmpty(dirName))
                {
                    dirName = new DirectoryInfo(directory).Name;
                }
                if (string.IsNullOrEmpty(dirName) || !IsModuleSupported(dirName))
                {
                    return true;
                }
                var disable = disablePluginNames != null && disablePluginNames.Contains(dirName);
                var dllPath = Path.Combine(directory,
                    $"BD.WTTS.Client.Plugins.{dirName}.dll");
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
                            Log.Error(TAG, e,
                                $"AssemblyLoadFrom fail, assemblyPath: {assemblyPath}");
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
        return assemblies;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static IEnumerable<(bool IsDisable, Type[] Types)> VerifyAssemblies(IEnumerable<PluginResult<Assembly>> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            Type[]? types = null;
            try
            {
                // Validate every type once, then reuse the result for MEF discovery.
                types = assembly.Data.GetTypes();
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, $"assembly.GetTypes fail, assembly: {assembly}");
            }
            if (types != null) yield return (assembly.IsDisable, types);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static HashSet<IPlugin>? GetExports(
        HashSet<IPlugin> plugins,
        HashSet<IPlugin> disablePlugins,
        IEnumerable<Type> types)
    {
        ConventionBuilder conventions = new();
        conventions.ForTypesDerivedFrom<IPlugin>().Export<IPlugin>();

        var configuration = new ContainerConfiguration().WithParts(types, conventions);

        try
        {
            using CompositionHost container = configuration.CreateContainer();
            var exports = container.GetExports<IPlugin>().ToArray();
            foreach (var plugin in exports)
            {
                if (string.IsNullOrWhiteSpace(plugin.UniqueEnglishName) ||
                    string.Equals(plugin.UniqueEnglishName,
                    IPlatformService.IPCRoot.moduleName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue; // 屏蔽此名词
                }

                var isPluginBase = false;
                if (plugin.HasValue(out var error))
                {
                    if (plugin is PluginBase pluginBase)
                    {
                        isPluginBase = true;
#if DEBUG
                        var isOfficial = pluginBase.IsOfficial;
#endif
                        plugins.Add(pluginBase);
                        continue;
                    }
                }
                Log.Error(TAG,
                        "CompositionHost.GetExports plugin validation failed, name: {name}, isPluginBase: {isPluginBase}, error: {error}.",
                        plugin.UniqueEnglishName,
                        isPluginBase,
                        error);
                disablePlugins.Add(plugin);
            }
        }
        catch (Exception e)
        {
            Log.Error(TAG, e, "CompositionHost.GetExports fail.");
            return null;
        }
        return plugins;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //static HashSet<IPlugin>? GetExports(IEnumerable<Assembly> assemblies)
    //{
    //    var plugins = assemblies.Select(x =>
    //    {
    //        try
    //        {
    //            var pluginObj = x.CreateInstance("BD.WTTS.Plugins.Plugin", false);
    //            //Dictionary<string, object?> dict = new()
    //            //{
    //            //    { "pluginObj",
    //            //        pluginObj?.ToString()
    //            //    },
    //            //    { "pluginObjType",
    //            //        pluginObj?.GetType()?.ToString()
    //            //    },
    //            //    { "pluginObjTypeFullName",
    //            //        pluginObj?.GetType()?.FullName
    //            //    },
    //            //    { "pluginObjTypeBaseType",
    //            //        pluginObj?.GetType()?.BaseType?.ToString()
    //            //    },
    //            //    { "pluginObjTypeBaseTypeBaseType == PluginBase",
    //            //        typeof(PluginBase) == pluginObj?.GetType()?.BaseType?.BaseType
    //            //    },
    //            //    { "PluginBase(AssemblyLoadContext)",
    //            //        AssemblyLoadContext.GetLoadContext(typeof(PluginBase).Assembly)?.Name
    //            //    },
    //            //    { "pluginObj(AssemblyLoadContext)",
    //            //        AssemblyLoadContext.GetLoadContext(pluginObj?.GetType()?.BaseType?.BaseType.Assembly!)?.Name
    //            //    },
    //            //    { "pluginObjTypeBaseTypeBaseType",
    //            //        pluginObj?.GetType()?.BaseType?.BaseType?.ToString()
    //            //    },
    //            //    { "pluginObjTypeBaseTypeBaseTypeBaseType",
    //            //        pluginObj?.GetType()?.BaseType?.BaseType?.BaseType?.ToString()
    //            //    },
    //            //};
    //            //Log.Error("Plugin", string.Join(Environment.NewLine, dict.Select(x => $"{x.Key}={x.Value}")));
    //            if (pluginObj is PluginBase plugin)
    //            {
    //                IPlugin plugin1 = plugin;
    //                return plugin1;
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error(TAG, e, "CompositionHost.GetExports fail.");
    //        }
    //        return null;
    //    }).ToHashSet();
    //    plugins.Remove(null);
    //    return plugins!;
    //}

    internal static SortedSet<PluginResult<IPlugin>>? InitPlugins(
        HashSet<string>? disablePluginNames = null,
        params string[] loadModules)
    {
        var assemblies = LoadAssemblies(disablePluginNames, loadModules);
        if (!assemblies.Any_Nullable()) return null;

        // Keep reflection results local so disabled plugin contexts remain collectible.
        var assemblyTypes = VerifyAssemblies(assemblies).ToArray();
        if (assemblyTypes.Length == 0) return null;

        static int OrderPlugin(IPlugin plugin) => plugin.UniqueEnglishName switch
        {
            Accelerator => 25,
            GameAccount => 35,
            GameList => 45,
            Authenticator => 55,
            ArchiSteamFarmPlus => 65,
            SteamIdleCard => 75,
            GameTools => 85,
            _ => ushort.MaxValue,
        };

        var comparer = ComparerBuilder.For<PluginResult<IPlugin>>()
            .OrderBy(x => x.IsDisable)
            .ThenBy(x => x.Data.IsOfficial, descending: true)
            .ThenBy(x => OrderPlugin(x.Data))
            .ThenBy(x => x.Data.UniqueEnglishName);
        SortedSet<PluginResult<IPlugin>> pluginResults = new(comparer);
        HashSet<IPlugin> disablePlugins = new();
        HashSet<IPlugin> activePlugins = new();
        GetExports(activePlugins, disablePlugins, assemblyTypes.Where(x => !x.IsDisable).SelectMany(x => x.Types));
        foreach (var plugin in activePlugins)
        {
            pluginResults.Add(new(false, plugin));
        }
        GetExports(disablePlugins, disablePlugins, assemblyTypes.Where(x => x.IsDisable).SelectMany(x => x.Types));
        foreach (var plugin in disablePlugins)
        {
            try
            {
                var plugin_ = new DisablePlugin(plugin);
                pluginResults.Add(new(true, plugin_));
            }
            catch (Exception e)
            {
                Log.Error(TAG, e, "Failed to initialize disabled plugin.");
            }
        }
        disablePluginsAssemblyLoadContext.Unload();
        disablePluginsAssemblyLoadContext = null!;
        return pluginResults;
    }
#endif
}
#endif
