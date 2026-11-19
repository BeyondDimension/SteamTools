#if WTTS_NATIVE_AOT
extern alias AcceleratorPlugin;
extern alias GameAccountPlugin;
extern alias GameListPlugin;
extern alias AuthenticatorPlugin;
extern alias SteamIdleCardPlugin;
#if WINDOWS
extern alias ArchiSteamFarmPlusPlugin;
extern alias GameToolsPlugin;
#endif

namespace BD.WTTS;

partial class Program
{
    static void RegisterBuiltInPlugins()
    {
        Plugins.PluginsCore.RegisterBuiltInPlugins(
            (AssemblyInfo.Accelerator, static () => new AcceleratorPlugin::BD.WTTS.Plugins.Plugin()),
            (AssemblyInfo.GameAccount, static () => new GameAccountPlugin::BD.WTTS.Plugins.Plugin()),
            (AssemblyInfo.GameList, static () => new GameListPlugin::BD.WTTS.Plugins.Plugin()),
            (AssemblyInfo.Authenticator, static () => new AuthenticatorPlugin::BD.WTTS.Plugins.Plugin()),
#if WINDOWS
            (AssemblyInfo.ArchiSteamFarmPlus, static () => new ArchiSteamFarmPlusPlugin::BD.WTTS.Plugins.Plugin()),
            (AssemblyInfo.GameTools, static () => new GameToolsPlugin::BD.WTTS.Plugins.Plugin()),
#endif
            (AssemblyInfo.SteamIdleCard, static () => new SteamIdleCardPlugin::BD.WTTS.Plugins.Plugin()));
    }
}
#endif
