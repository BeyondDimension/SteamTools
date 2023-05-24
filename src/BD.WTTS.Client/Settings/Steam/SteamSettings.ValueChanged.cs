using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.Settings;

public static partial class SteamSettings
{
    static SteamSettings()
    {
        if (!IApplication.IsDesktop()) return;
        IsRunSteamMinimized.ValueChanged += IsRunSteamMinimized_ValueChanged;
        IsRunSteamNoCheckUpdate.ValueChanged += IsRunSteamNoCheckUpdate_ValueChanged;
        IsRunSteamChina.ValueChanged += IsRunSteamChina_ValueChanged;
    }

    static void IsRunSteamNoCheckUpdate_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool?> e)
    {
        if (!e.NewValue.HasValue)
            return;
        if (e.NewValue.Value)
            SteamStratParameter.Value += " -noverifyfiles";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace("-noverifyfiles", "").Trim();
    }

    static void IsRunSteamMinimized_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool?> e)
    {
        if (!e.NewValue.HasValue)
            return;
        if (e.NewValue.Value)
            SteamStratParameter.Value += " -silent";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace("-silent", "").Trim();
    }

    static void IsRunSteamChina_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool?> e)
    {
        if (!e.NewValue.HasValue)
            return;
        if (e.NewValue.Value)
            SteamStratParameter.Value += " -steamchina";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace("-steamchina", "").Trim();
    }
}
