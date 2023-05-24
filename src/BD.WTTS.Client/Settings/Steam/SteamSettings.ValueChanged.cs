using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.Settings.Steam;

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
            SteamStratParameter.ActualValue += " -noverifyfiles";
        else if (SteamStratParameter.ActualValue != null)
            SteamStratParameter.ActualValue = SteamStratParameter.ActualValue.Replace("-noverifyfiles", "").Trim();
    }

    static void IsRunSteamMinimized_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool?> e)
    {
        if (!e.NewValue.HasValue)
            return;
        if (e.NewValue.Value)
            SteamStratParameter.ActualValue += " -silent";
        else if (SteamStratParameter.ActualValue != null)
            SteamStratParameter.ActualValue = SteamStratParameter.ActualValue.Replace("-silent", "").Trim();
    }

    static void IsRunSteamChina_ValueChanged(object? sender, SettingsPropertyValueChangedEventArgs<bool?> e)
    {
        if (!e.NewValue.HasValue)
            return;
        if (e.NewValue.Value)
            SteamStratParameter.ActualValue += " -steamchina";
        else if (SteamStratParameter.ActualValue != null)
            SteamStratParameter.ActualValue = SteamStratParameter.ActualValue.Replace("-steamchina", "").Trim();
    }
}
