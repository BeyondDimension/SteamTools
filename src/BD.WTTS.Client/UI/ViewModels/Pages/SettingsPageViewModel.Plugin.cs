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
        // 禁用插件配置文件修改
        if (plugin.IsDisable)
            GeneralSettings.DisablePlugins.Add(plugin.Data.UniqueEnglishName, true, false);
        else
            GeneralSettings.DisablePlugins.Remove(plugin.Data.UniqueEnglishName, true, false);

        // Todo 通知主页菜单栏Tab显示或隐藏，以及程序集unload
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
            if (Directory.Exists(path))
            {
                var msg = string.Empty;
                try
                {
                    Directory.Delete(path, true);

                    Toast.Show(ToastIcon.Success, Strings.Plugin_DeleteSuccess.Format(plugin.Name));
                }
                catch (Exception ex)
                {
                    Toast.LogAndShowT(ex);
                }
            }
            Toast.Show(ToastIcon.Warning, Strings.Plugin_FileError);
        }
        else
            Toast.Show(ToastIcon.Warning, Strings.Plugin_NeedDisable);
    }

    private void PluginOpenFolder(string path)
    {
        try
        {
            var hasKey = clickOpenFolderTimeRecord.TryGetValue(path, out var dt);
            var now = DateTime.Now;
            if (hasKey && (now - dt).TotalSeconds <= clickOpenFolderIntervalSeconds)
                return;
            IPlatformService.Instance.OpenFolder(path);
            if (!clickOpenFolderTimeRecord.TryAdd(path, now)) clickOpenFolderTimeRecord[path] = now;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
        }

    }
}
