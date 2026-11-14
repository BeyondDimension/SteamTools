#if WTTS_NATIVE_AOT && (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using static BD.WTTS.AssemblyInfo;

namespace BD.WTTS.Plugins;

partial class PluginsCore
{
    static (string UniqueEnglishName, Func<PluginBase> Create)[]? builtInPlugins;

    /// <summary>
    /// Registers the statically linked official plugins before startup loads any modules.
    /// Factories defer construction until after module selection, including in subprocesses.
    /// </summary>
    public static void RegisterBuiltInPlugins(params (string UniqueEnglishName, Func<PluginBase> Create)[] plugins)
    {
        builtInPlugins = plugins;
    }

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

    static readonly IComparer<PluginResult<IPlugin>> builtInPluginComparer =
        Comparer<PluginResult<IPlugin>>.Create(static (left, right) =>
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left is null) return -1;
            if (right is null) return 1;
            var result = left.IsDisable.CompareTo(right.IsDisable);
            if (result != 0) return result;
            result = right.Data.IsOfficial.CompareTo(left.Data.IsOfficial);
            if (result != 0) return result;
            result = OrderPlugin(left.Data).CompareTo(OrderPlugin(right.Data));
            return result != 0 ? result : Comparer<string>.Default.Compare(left.Data.UniqueEnglishName, right.Data.UniqueEnglishName);
        });

    internal static SortedSet<PluginResult<IPlugin>>? InitPlugins(
        HashSet<string>? disablePluginNames = null,
        params string[] loadModules)
    {
        var registrations = builtInPlugins ?? throw new InvalidOperationException("Built-in plugins must be registered before startup.");
        SortedSet<PluginResult<IPlugin>>? results = null;
        foreach (var registration in registrations)
        {
            if (loadModules is { Length: > 0 } &&
                !loadModules.Contains(registration.UniqueEnglishName, StringComparer.OrdinalIgnoreCase))
                continue;

            try
            {
                IPlugin plugin = registration.Create();
                if (string.IsNullOrWhiteSpace(plugin.UniqueEnglishName) ||
                    string.Equals(plugin.UniqueEnglishName, IPlatformService.IPCRoot.moduleName, StringComparison.OrdinalIgnoreCase))
                    continue;

                bool hasValue;
                string? error;
                try
                {
                    hasValue = plugin.HasValue(out error);
                }
                catch (Exception ex)
                {
                    hasValue = false;
                    error = ex.ToString();
                }

                if (!hasValue)
                    Log.Error(TAG, "Built-in plugin validation failed, name: {name}, error: {error}.", plugin.UniqueEnglishName, error);

                var disabled = !hasValue || disablePluginNames?.Contains(registration.UniqueEnglishName) == true;
                if (disabled)
                    plugin = new DisablePlugin(plugin, hasValue, error);

                results ??= new(builtInPluginComparer);
                results.Add(new(disabled, plugin));
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "Failed to initialize built-in plugin {name}.", registration.UniqueEnglishName);
            }
        }
        return results;
    }
}
#endif
