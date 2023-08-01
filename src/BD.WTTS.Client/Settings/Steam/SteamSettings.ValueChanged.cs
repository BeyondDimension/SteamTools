using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.Settings;

public static partial class SteamSettings
{

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    static SteamSettings()
    {
        if (!IApplication.IsDesktop()) return;
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

    private static void IsRunSteamVGUI_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool?> e)
    {
        if (!e.NewValue.HasValue)
            return;
        SetRunSteamParameter("-vgui", e.NewValue.Value);
    }

    static void IsRunSteamNoCheckUpdate_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool?> e)
    {
        if (!e.NewValue.HasValue)
            return;
        SetRunSteamParameter("-noverifyfiles", e.NewValue.Value);
    }

    static void IsRunSteamMinimized_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool?> e)
    {
        if (!e.NewValue.HasValue)
            return;
        SetRunSteamParameter("-silent", e.NewValue.Value);
    }

    static void IsRunSteamChina_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool?> e)
    {
        if (!e.NewValue.HasValue)
            return;
        SetRunSteamParameter("-steamchina", e.NewValue.Value);
    }
#endif

}
