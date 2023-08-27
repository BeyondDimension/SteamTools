// ReSharper disable once CheckNamespace
using System.Linq;

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
        // Todo 通知主页菜单栏Tab显示或隐藏，以及程序集unload
        if (plugin.IsDisable)
        {
            //IViewModelManager.Instance.MainWindow2?.TabItems.Remove(plugin.Data.UniqueEnglishName);
            GeneralSettings.DisablePlugins.Add(plugin.Data.UniqueEnglishName, true, false);
        }
        else
        {
            GeneralSettings.DisablePlugins.Remove(plugin.Data.UniqueEnglishName, true, false);
        }

        Startup.ShowSettingsModifiedRestartThisSoft();
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

    public async void DeletePlugin(IPlugin plugin)
    {
        var r = await MessageBox.ShowAsync(Strings.Plugin_DeleteComfirm.Format(plugin.Name), button: MessageBox.Button.OKCancel);
        if (r.IsOK())
        {
            void ShowSuccess() => Toast.Show(ToastIcon.Success, Strings.Plugin_DeleteSuccess.Format(plugin.Name));
            var plugins = Plugins;
            if (plugins == null)
            {
                ShowSuccess();
                return;
            }
            var pluginResult = plugins.FirstOrDefault(x => x.IsDisable && x.Data.Id == plugin.Id);
            if (pluginResult != null)
            {
                var path = Path.GetDirectoryName(plugin.AssemblyLocation)!;
                if (Directory.Exists(path))
                {
                    var msg = string.Empty;
                    try
                    {
                        Directory.Delete(path, true);
                        plugins.Remove(pluginResult);
                        ShowSuccess();
                    }
                    catch (Exception ex)
                    {
                        Toast.LogAndShowT(ex);
                    }
                }
                else
                {
                    Toast.Show(ToastIcon.Error, Strings.Plugin_FileError);
                }
            }
            else
            {
                Toast.Show(ToastIcon.Warning, Strings.Plugin_NeedDisable);
            }
        }

    }

    void PluginOpenFolder(string path)
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
