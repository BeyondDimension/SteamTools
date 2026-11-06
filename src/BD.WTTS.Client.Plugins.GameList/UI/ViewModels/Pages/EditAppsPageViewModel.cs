using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed class EditAppsPageViewModel : ViewModelBase
{
    public static string DisplayName => Strings.GameList_EditedAppsSaveManger;

    readonly ReadOnlyObservableCollection<SteamApp> _SteamEditedApps;

    public ReadOnlyObservableCollection<SteamApp> SteamEditedApps => _SteamEditedApps;

    public bool IsSteamEditedAppsEmpty => !SteamEditedApps.Any_Nullable();

    [Reactive]
    public bool IsExportingBackup { get; set; }

    public ICommand EditAppInfoClickCommand { get; }

    public EditAppsPageViewModel()
    {
        SteamConnectService.Current.SteamApps
          .Connect()
          .Filter(x => x.IsEdited)
          .ObserveOn(RxSchedulers.MainThreadScheduler)
          .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.AppId))
          .Bind(out _SteamEditedApps)
          .Subscribe(_ => this.RaisePropertyChanged(nameof(IsSteamEditedAppsEmpty)));

        EditAppInfoClickCommand = ReactiveCommand.Create<SteamApp>(GameListPageViewModel.EditAppInfoClick);

        LoadSteamEditedApps();
    }

    public void LoadSteamEditedApps()
    {
        SteamConnectService.Current.SteamApps.Refresh();
    }

    public async Task SaveSteamEditedApps()
    {
        var stmService = ISteamService.Instance;
        if (await stmService.SaveAppInfosToSteam())
        {
            if (await MessageBox.ShowAsync(Strings.SaveEditedAppInfo_RestartSteamTip, AssemblyInfo.Trademark, MessageBox.Button.OKCancel) == MessageBox.Result.OK)
            {
                await stmService.TryKillSteamProcess();
                stmService.StartSteamWithParameter();
            }
            Toast.Show(ToastIcon.Success, Strings.SaveEditedAppInfo_SaveToSteamSuccess);
        }
        else
        {
            Toast.Show(ToastIcon.Error, Strings.SaveEditedAppInfo_SaveToSteamFailed);
        }
    }

    public async Task ExportSteamEditedAppsBackup()
    {
        if (IsExportingBackup)
        {
            return;
        }

        if (!SteamEditedApps.Any())
        {
            Toast.Show(ToastIcon.Warning, "没有可导出的已修改游戏数据。");
            return;
        }

        var result = await FilePicker2.SaveAsync(new PickOptions
        {
            PickerTitle = "导出编辑游戏数据备份",
            InitialFileName = $"steam-edited-apps-{DateTime.Now:yyyyMMddHHmmss}.stmbak",
        });

        if (result == null)
        {
            return;
        }

        try
        {
            IsExportingBackup = true;
            var exportData = await Task.Run(async () =>
            {
                var stmService = ISteamService.Instance;
                var applist = await stmService.GetAppInfos(true);
                var editApps = SteamConnectService.Current.SteamApps.Items
                    .Where(s => s.IsEdited)
                    .ToDictionary(s => s.AppId, s => s);
                var modifiedApps = new List<ModifiedApp>();

                foreach (var app in applist)
                {
                    if (editApps.TryGetValue(app.AppId, out var editApp))
                    {
                        app.SetEditProperty(editApp);
                        modifiedApps.Add(new ModifiedApp(app));
                    }
                }

                return (ModifiedApps: modifiedApps, Bytes: Serializable.SMP(modifiedApps));
            });

            if (!exportData.ModifiedApps.Any())
            {
                Toast.Show(ToastIcon.Error, "导出失败：没有可序列化的修改数据。");
                return;
            }

            using var stream = result.OpenWrite();
            await stream.WriteAsync(exportData.Bytes, 0, exportData.Bytes.Length);
            await stream.FlushAsync();

            Toast.Show(ToastIcon.Success, $"导出成功，共 {exportData.ModifiedApps.Count} 条修改数据。");
        }
        catch
        {
            Toast.Show(ToastIcon.Error, "导出失败，请检查文件权限后重试。");
        }
        finally
        {
            IsExportingBackup = false;
        }
    }

    public async Task ImportSteamEditedAppsBackup()
    {
        var file = await FilePicker2.PickAsync(new PickOptions
        {
            PickerTitle = "导入编辑游戏数据备份",
        });

        if (file == null)
        {
            return;
        }

        try
        {
            await using var stream = await file.OpenReadAsync();
            var modifiedApps = Serializable.DMP<List<ModifiedApp>>(stream);
            if (!modifiedApps.Any_Nullable())
            {
                Toast.Show(ToastIcon.Warning, "导入文件中没有可还原的数据。");
                return;
            }

            var applyCount = SteamConnectService.Current.ApplyModifiedApps(modifiedApps);
            if (applyCount <= 0)
            {
                Toast.Show(ToastIcon.Warning, "导入完成，但未匹配到可还原的游戏。\n请确认账号或游戏库与备份来源一致。");
                return;
            }

            this.RaisePropertyChanged(nameof(IsSteamEditedAppsEmpty));
            Toast.Show(ToastIcon.Success, $"导入成功，已还原 {applyCount} 个游戏的修改数据。");
        }
        catch
        {
            Toast.Show(ToastIcon.Error, "导入失败，文件格式可能不正确。");
        }
    }

    //public async Task ClearSteamEditedApps()
    //{
    //    if (await MessageBox.ShowAsync("确定要重置所有的已修改数据吗？(该操作不可还原)", AssemblyInfo.Trademark, MessageBox.Button.OKCancel) == MessageBox.Result.OK)
    //    {

    //    }
    //}
}
