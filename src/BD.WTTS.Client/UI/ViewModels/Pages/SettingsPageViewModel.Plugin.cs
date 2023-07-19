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

    public void SwitchEnablePlugin(PluginResult<IPlugin> plugin)
    {
        throw new NotImplementedException();
    }

    public void OpenPluginDirectory(string assemblyLocation)
    {
        var path = Path.GetDirectoryName(assemblyLocation);
        PluginOpenFolder(path!);
    }

    public void OpenPluginCacheDirectory(string path)
    {
        PluginOpenFolder(path);
    }

    public void DeletePlugin(IPlugin plugin)
    {
        if (Plugins!.Any(x => x.IsDisable && x.Data.Id == plugin.Id))
        {
            var path = Path.GetDirectoryName(plugin.AssemblyLocation)!;
            var msg = string.Empty;
            try
            {
                var hasKey = clickTimeRecord.TryGetValue(path, out var dt);
                var now = DateTime.Now;
                if (hasKey && (now - dt).TotalSeconds <= clickInterval) return;
                Directory.Delete(path, true);
                if (!clickTimeRecord.TryAdd(path, now)) clickTimeRecord[path] = now;
                Toast.Show(ToastIcon.Success, Strings.Plugin_DeleteSuccess.Format(plugin.Name));
                return;
            }
            catch (Exception ex)
            {
                Toast.LogAndShowT(ex);
            }
        }
        else
            Toast.Show(ToastIcon.Info, Strings.Plugin_NeedDisable);
    }

    private void PluginOpenFolder(string path)
    {
        try
        {
            var hasKey = clickTimeRecord.TryGetValue(path, out var dt);
            var now = DateTime.Now;
            if (hasKey && (now - dt).TotalSeconds <= clickInterval) return;
            IPlatformService.Instance.OpenFolder(path);
            if (!clickTimeRecord.TryAdd(path, now)) clickTimeRecord[path] = now;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
        }

    }
}
