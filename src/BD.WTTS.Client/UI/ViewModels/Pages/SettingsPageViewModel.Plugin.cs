using DynamicData;
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public sealed partial class SettingsPageViewModel
{
    public void LoadPlugins()
    {
        if (Startup.Instance.TryGetPluginResults(out var plugins))
        {
            Plugins = new ObservableCollection<PluginResult<IPlugin>>(plugins);
        }
    }
}
