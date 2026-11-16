#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
namespace BD.WTTS.Plugins;

partial class PluginsCore
{
    // Services and settings are configured before this phase. One plugin waiting for
    // elevation or I/O must not prevent unrelated plugins from starting.
    internal static Task InitializePluginsAsync(IReadOnlyCollection<IPlugin> plugins)
    {
        var tasks = new Task[plugins.Count];
        var index = 0;
        foreach (var plugin in plugins)
            tasks[index++] = InitializePluginAsync(plugin);
        return Task.WhenAll(tasks);
    }

    static async Task InitializePluginAsync(IPlugin plugin)
    {
        Log.Info(TAG, "Initializing plugin {name}.", plugin.UniqueEnglishName);
        try
        {
            await plugin.OnInitializeAsync();
            Log.Info(TAG, "Plugin startup callback completed: {name}.", plugin.UniqueEnglishName);
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "Plugin startup callback failed: {name}.", plugin.UniqueEnglishName);
        }
    }
}
#endif
