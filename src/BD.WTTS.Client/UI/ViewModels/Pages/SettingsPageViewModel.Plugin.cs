using DynamicData;
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public sealed partial class SettingsPageViewModel : TabItemViewModel
{

    public void LoadPlugins()
    {
        if (Startup.Instance.TryGetPluginResults(out var plugins))
        {
            Plugins = new ObservableCollection<PluginResult<IPlugin>>(plugins);
        }
    }

    public void CheckUpdate()
    {
        Toast.Show(ToastIcon.Info, Strings.IsExistUpdateFalse);
    }

    public void OpenPluginDirectory(string assemblyLocation)
    {
        var path = Path.GetDirectoryName(assemblyLocation);
        if (Directory.Exists(path))
            IPlatformService.Instance.OpenFolder(path);
        else
            Toast.Show(ToastIcon.Info, Strings.Error);
    }

    public void OpenPluginCacheDirectory(string path)
    {
        if (Directory.Exists(path))
            IPlatformService.Instance.OpenFolder(path);
        else
            Toast.Show(ToastIcon.Info, Strings.Error);
    }

    public void DeletePlugin(IPlugin plugin)
    {
        if (Plugins!.Any(x => x.IsDisable && x.Data.Id == plugin.Id))
        {
            var path = Path.GetDirectoryName(plugin.AssemblyLocation);
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                    Toast.Show(ToastIcon.Info, Strings.Plugins_DeleteSuccess);
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(SettingsPageViewModel), ex, "插件删除失败");
                    Toast.Show(ToastIcon.Info, Strings.Plugins_DeleteError);
                }
            }
            else
                Toast.Show(ToastIcon.Info, Strings.Error);
        }
        else
            Toast.Show(ToastIcon.Info, Strings.Plugins_NeedDisable);

    }
}
