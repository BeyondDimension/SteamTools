namespace BD.WTTS.Settings;

public static partial class SteamSettings
{

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    static SteamSettings()
    {
        if (!IApplication.IsDesktop())
            return;

        IsRunSteamMinimized.ValueChanged += IsRunSteamMinimized_ValueChanged;
        IsRunSteamNoCheckUpdate.ValueChanged += IsRunSteamNoCheckUpdate_ValueChanged;
        IsRunSteamChina.ValueChanged += IsRunSteamChina_ValueChanged;
        IsRunSteamVGUI.ValueChanged += IsRunSteamVGUI_ValueChanged;
    }

    private static void SetRunSteamParameter(string parameter, bool isSet)
    {
        if (isSet)
            SteamStratParameter.Value += $" {parameter}";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace(parameter, "").Trim();
    }

    private static void IsRunSteamVGUI_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool> e)
    {
        SetRunSteamParameter("-vgui", e.NewValue);
    }

    static void IsRunSteamNoCheckUpdate_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool> e)
    {
        SetRunSteamParameter("-noverifyfiles", e.NewValue);
    }

    static void IsRunSteamMinimized_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool> e)
    {
        SetRunSteamParameter("-silent", e.NewValue);
    }

    static void IsRunSteamChina_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool> e)
    {
        SetRunSteamParameter("-steamchina", e.NewValue);
    }
#endif

}
