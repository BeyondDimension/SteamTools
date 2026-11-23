using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class EditAppsPageViewModel : ViewModelBase
{
    public static string DisplayName => Strings.GameList_EditedAppsSaveManger;

    readonly ReadOnlyObservableCollection<SteamApp> _SteamEditedApps;

    public ReadOnlyObservableCollection<SteamApp> SteamEditedApps => _SteamEditedApps;

    public bool IsSteamEditedAppsEmpty => !SteamEditedApps.Any_Nullable();

    [Reactive]
    public partial bool IsExportingBackup { get; set; }

    public ICommand EditAppInfoClickCommand { get; }
    public ICommand ApplyToSteamCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }

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
        ApplyToSteamCommand = ReactiveCommand.CreateFromTask(SaveSteamEditedApps);
        RefreshCommand = ReactiveCommand.Create(LoadSteamEditedApps);
        ExportCommand = ReactiveCommand.CreateFromTask(ExportSteamEditedAppsBackup);
        ImportCommand = ReactiveCommand.CreateFromTask(ImportSteamEditedAppsBackup);

        LoadSteamEditedApps();
    }

    public void LoadSteamEditedApps()
    {
        SteamConnectService.Current.SteamApps.Refresh();
    }

    public async Task SaveSteamEditedApps()
    {
        var steam = ISteamService.Instance;
        try
        {
            if (steam.IsRunningSteamProcess)
            {
                Toast.Show(ToastIcon.Warning, Strings.L10nGames_ExitSteamBeforeApply);
                return;
            }

            if (await steam.SaveAppInfosToSteam())
                Toast.Show(ToastIcon.Success, Strings.SaveEditedAppInfo_SaveToSteamSuccess);
            else if (steam.IsRunningSteamProcess)
                Toast.Show(ToastIcon.Warning, Strings.L10nGames_ExitSteamBeforeApply);
            else
                Toast.Show(ToastIcon.Error, Strings.SaveEditedAppInfo_SaveToSteamFailed);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(EditAppsPageViewModel), ex, "Unable to apply game edits to Steam.");
            Toast.Show(ToastIcon.Error, Strings.SaveEditedAppInfo_SaveToSteamFailed);
        }
    }

    public async Task ExportSteamEditedAppsBackup()
    {
        if (IsExportingBackup)
        {
            return;
        }

        try
        {
            IsExportingBackup = true;
            var edits = await Task.Run(() => ISteamService.Instance.GetAppInfoEdits()
                .Where(entry => !entry.Value.IsEmpty).ToDictionary(entry => entry.Key, entry => entry.Value));
            if (edits.Count == 0)
            {
                Toast.Show(ToastIcon.Warning, Strings.L10nGames_ExportEmpty);
                return;
            }

            var result = await FilePicker2.SaveAsync(new PickOptions
            {
                PickerTitle = Strings.L10nGames_ExportTitle,
                InitialFileName = $"steam-edited-apps-{DateTime.Now:yyyyMMddHHmmss}.stmbak",
            });
            if (result == null)
                return;

            using var stream = result.OpenWrite();
            await Task.Run(() => SteamAppEditBackup.Write(stream, edits));
            await stream.FlushAsync();

            Toast.Show(ToastIcon.Success, string.Format(Strings.L10nGames_ExportSuccess, edits.Count));
        }
        catch
        {
            Toast.Show(ToastIcon.Error, Strings.L10nGames_ExportFailed);
        }
        finally
        {
            IsExportingBackup = false;
        }
    }

    public async Task ImportSteamEditedAppsBackup()
    {
        try
        {
            var file = await FilePicker2.PickAsync(new PickOptions
            {
                PickerTitle = Strings.L10nGames_ImportTitle,
            });
            if (file == null)
                return;

            await using var stream = await file.OpenReadAsync();
            var edits = await Task.Run(() => SteamAppEditBackup.Read(stream));
            if (edits.Count == 0)
            {
                Toast.Show(ToastIcon.Warning, Strings.L10nGames_ImportEmpty);
                return;
            }

            var applyCount = await SteamConnectService.Current.ImportGameAppEditsAsync(edits);
            if (applyCount <= 0)
            {
                Toast.Show(ToastIcon.Warning, Strings.L10nGames_ImportNoMatches);
                return;
            }

            this.RaisePropertyChanged(nameof(IsSteamEditedAppsEmpty));
            Toast.Show(ToastIcon.Success, string.Format(Strings.L10nGames_ImportSuccess, applyCount));
        }
        catch
        {
            Toast.Show(ToastIcon.Error, Strings.L10nGames_ImportFailed);
        }
    }
}
