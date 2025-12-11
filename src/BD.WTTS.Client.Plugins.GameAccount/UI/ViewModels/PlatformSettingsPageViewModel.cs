namespace BD.WTTS.UI.ViewModels;

public class PlatformSettingsPageViewModel : ViewModelBase
{
    public PlatformSettings PlatformSettings { get; init; }

    public PlatformAccount? Platform { get; init; }

    public PlatformSettingsPageViewModel(PlatformAccount platform)
    {
        Platform = platform;
        platform.PlatformSetting = PlatformSettings = platform.PlatformSetting ?? new();
    }

    public async void SelectProgramPath()
    {
        if (Platform is null)
            return;

        AvaloniaFilePickerFileTypeFilter fileTypes = new AvaloniaFilePickerFileTypeFilter.Item[] {
            new(Platform.FullName) {
                Patterns = new[] { Platform.ExeName ?? "*", },
                //MimeTypes =
                //AppleUniformTypeIdentifiers =
                },
        };

        await FilePicker2.PickAsync((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                Platform.PlatformSetting!.PlatformPath = path;
                GameAccountSettings.PlatformSettings.Add(Platform.FullName, Platform.PlatformSetting);
            }
        }, fileTypes);
    }
}
