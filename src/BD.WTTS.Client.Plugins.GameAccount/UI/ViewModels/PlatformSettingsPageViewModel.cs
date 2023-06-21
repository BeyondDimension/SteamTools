namespace BD.WTTS.UI.ViewModels;

public class PlatformSettingsPageViewModel : ViewModelBase
{
    public PlatformSettings PlatformSettings { get; init; }

    public PlatformAccount? Platform { get; init; }

    public PlatformSettingsPageViewModel()
    {
        PlatformSettings = new();
    }

    public PlatformSettingsPageViewModel(PlatformAccount platform)
    {
        Platform = platform;
        platform.PlatformSetting = PlatformSettings = platform.PlatformSetting ?? new();
    }

    public async void SelectProgramPath()
    {
        if (Platform is null)
            return;

        FilePickerFileType fileTypes = new ValueTuple<string, string[]>[]
        {
              (Platform.FullName, new[] { Platform.ExeName ?? "*" }),
        };

        await FilePicker2.PickAsync((path) =>
        {
            if (!string.IsNullOrEmpty(path))
                PlatformSettings.PlatformPath = path;
        }, fileTypes);
    }
}
