using DynamicData;
using System.IO.Enumeration;
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

[MP2Obj]
public sealed partial class SettingsPageViewModel : TabItemViewModel
{
    public SettingsPageViewModel()
    {
        SelectLanguage = ResourceService.GetSelectLanguage();
        this.ObserveValue(nameof(SelectLanguage), static x => x.SelectLanguage).Skip(1)
              .Subscribe(x =>
              {
                  if (string.Equals(UISettings.Language.Value, x.Key, StringComparison.OrdinalIgnoreCase))
                      return;
                  UISettings.Language.Value = x.Key;
                  Toast.Show(ToastIcon.Info, AppResources.Settings_Language_RestartRequired);
              });

        OpenFolder_Click = ReactiveCommand.Create<string>(OpenFolder);

        CheckUpdate_Click = ReactiveCommand.Create(CheckUpdate);

        OpenPluginDirectory_Click = ReactiveCommand.Create<string>(OpenPluginDirectory);

        OpenPluginCacheDirectory_Click = ReactiveCommand.Create<string>(OpenPluginCacheDirectory);

        DeletePlugin_Click = ReactiveCommand.Create<IPlugin>(DeletePlugin);

        SwitchEnablePlugin_Click = ReactiveCommand.Create<PluginResult<IPlugin>>(SwitchEnablePlugin);

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        if (IApplication.IsDesktop())
        {
            SelectFont = ResourceService.GetSelectFont();
            this.ObserveValue(nameof(SelectFont), static x => x.SelectFont).Skip(1)
                  .Subscribe(x => UISettings.FontName.Value = x.Value);

            SelectImage_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                await FilePicker2.PickAsync(SetBackgroundImagePath, IFilePickerFileType.Images());
            });

            ResetImage_Click = ReactiveCommand.Create(() => SetBackgroundImagePath(null));
        }
#endif
    }

    public void OpenFolder(string tag)
    {
        var path = GetFolderPath(tag);
        var hasKey = clickOpenFolderTimeRecord.TryGetValue(path, out var dt);
        var now = DateTime.Now;
        if (hasKey && (now - dt).TotalSeconds <= clickOpenFolderIntervalSeconds) return;
        IPlatformService.Instance.OpenFolder(path);
        if (!clickOpenFolderTimeRecord.TryAdd(path, now)) clickOpenFolderTimeRecord[path] = now;
    }

    static string GetFolderPath(string tag) => tag switch
    {
        IOPath.DirName_AppData => IOPath.AppDataDirectory,
        IOPath.DirName_Cache => IOPath.CacheDirectory,
        IPCSubProcessFileSystem.LogDirName => IApplication.LogDirPath,
        _ => IOPath.BaseDirectory,
    };

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    public async void SelectSteamProgramLocation()
    {
        AvaloniaFilePickerFileTypeFilter fileTypes = new AvaloniaFilePickerFileTypeFilter.Item[] {
            new("Steam") {
                Patterns = OperatingSystem.IsWindows() ? new[] { "steam.exe" } : new[] { "*" },
                //MimeTypes =
                //AppleUniformTypeIdentifiers =
                },
        };
        await FilePicker2.PickAsync((path) =>
        {
            if (!string.IsNullOrEmpty(path) && !string.Equals(path, SteamSettings.SteamProgramPath.Value, StringComparison.Ordinal))
            {
                SteamSettings.SteamProgramPath.Value = path;
                Toast.Show(ToastIcon.Info, AppResources.Settings_Steam_PathNativeRestart);
            }
        }, fileTypes);
    }

    public void SetBackgroundImagePath(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            UISettings.WindowBackgroundCustomImagePath.Reset();
            return;
        }
        if (File.Exists(imagePath))
        {
            if (IOPath.TryOpenRead(imagePath, out var stream, out var _))
            {
                var (isImage, _) = FileFormat.IsImage(stream);
                if (isImage)
                {
                    UISettings.WindowBackgroundCustomImagePath.Value = imagePath;
                    return;
                }
            }
        }
        Toast.Show(ToastIcon.Error, AppResources.Settings_UI_CustomBackgroundImage_Error);
    }

    public async void EditSteamParameter()
    {
        var vm = new TextBoxWindowViewModel
        {
            InputType = TextBoxWindowViewModel.TextBoxInputType.TextBox,
            Value = SteamSettings.SteamStratParameter.Value,
        };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(vm, AppResources.Edit + " " + AppResources.Settings_Steam_SteamStratParameter, subHeader: Strings.L10nCore_SteamLaunchParametersHint, isDialog: false, isCancelButton: true))
        {
            SteamSettings.SteamStratParameter.Value = vm.Value;
        }
    }
#endif

    internal sealed class DirectorySizeMeasurement(TimeSpan maxAge)
    {
        readonly object sync = new();
        string? directory;
        Task<long>? scan;
        long startedAt;

        public Task<long> GetAsync(string path)
        {
            lock (sync)
            {
                if (directory == path && scan != null &&
                    (!scan.IsCompleted || (scan.IsCompletedSuccessfully && Stopwatch.GetElapsedTime(startedAt) < maxAge)))
                    return scan;

                directory = path;
                startedAt = Stopwatch.GetTimestamp();
                return scan = Task.Run(() => Measure(path));
            }
        }

        static long Measure(string path)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            try
            {
                var files = new FileSystemEnumerable<long>(path, static (ref FileSystemEntry entry) => entry.Length,
                    new System.IO.EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        AttributesToSkip = FileAttributes.ReparsePoint,
                        IgnoreInaccessible = false,
                    })
                {
                    ShouldIncludePredicate = static (ref FileSystemEntry entry) => !entry.IsDirectory,
                };
                long size = 0;
                foreach (var length in files)
                    size += length;
                return size;
            }
            catch (DirectoryNotFoundException) when (!Directory.Exists(path))
            {
                return 0;
            }
        }
    }

    static readonly DirectorySizeMeasurement appSize = new(TimeSpan.FromSeconds(30));
    static readonly DirectorySizeMeasurement appDataSize = new(TimeSpan.FromSeconds(30));
    static readonly DirectorySizeMeasurement cacheSize = new(TimeSpan.FromSeconds(30));
    static readonly DirectorySizeMeasurement logSize = new(TimeSpan.FromSeconds(30));

    internal static async Task<string> GetStorageSizeAsync(string tag)
    {
        var measurement = tag switch
        {
            IOPath.DirName_AppData => appDataSize,
            IOPath.DirName_Cache => cacheSize,
            IPCSubProcessFileSystem.LogDirName => logSize,
            _ => appSize,
        };
        try
        {
            var size = await measurement.GetAsync(GetFolderPath(tag)).ConfigureAwait(false);
            return IOPath.GetDisplayFileSizeString(size);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            Log.Error(nameof(SettingsPageViewModel), ex, "Could not measure storage folder {tag}.", tag);
            return AppResources.Error;
        }
    }
}
