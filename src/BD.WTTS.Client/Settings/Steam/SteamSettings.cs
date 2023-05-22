using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.Settings;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class SteamSettings_ : ISteamSettings
{
    public const string Name = nameof(SteamSettings_);

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public string? SteamStratParameter { get; set; }

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public string SteamSkin { get; set; } = string.Empty;

    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public string SteamProgramPath { get; set; } = string.Empty;

    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public string CustomSteamPath { get; set; } = string.Empty;

    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public bool IsAutoRunSteam { get; set; } = false;

    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public bool IsRunSteamMinimized { get; set; } = false;

    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public bool IsRunSteamNoCheckUpdate { get; set; } = false;

    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public bool IsRunSteamChina { get; set; } = false;

    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public bool IsEnableSteamLaunchNotification { get; set; } = true;

    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public OSExitMode DownloadCompleteSystemEndMode { get; set; } = OSExitMode.Sleep;

    [MPKey(10), MP2Key(10), JsonPropertyOrder(10)]
    public bool IsRunSteamAdministrator { get; set; } = true;
}

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(SteamSettings_))]
internal partial class SteamSettingsContext : JsonSerializerContext
{
    static SteamSettingsContext? instance;

    public static SteamSettingsContext Instance
        => instance ??= new SteamSettingsContext(ISettings.GetDefaultOptions());
}

partial class SteamSettings_ : ISettings<SteamSettings_>
{
    public static JsonTypeInfo<SteamSettings_> JsonTypeInfo => SteamSettingsContext.Instance.SteamSettings_;

    public static JsonSerializerContext JsonSerializerContext => SteamSettingsContext.Instance;

    static string ISettings.Name => Name;

    static JsonTypeInfo ISettings.JsonTypeInfo => SteamSettingsContext.Instance.SteamSettings_;
}

[SettingsGeneration]
public static class SteamSettings
{
    //private static readonly ISteamService _service = Ioc.Get<ISteamService>();

    static SteamSettings()
    {
        if (!IApplication.IsDesktop()) return;
        IsRunSteamMinimized.ValueChanged += IsRunSteamMinimized_ValueChanged;
        IsRunSteamNoCheckUpdate.ValueChanged += IsRunSteamNoCheckUpdate_ValueChanged;
        IsRunSteamChina.ValueChanged += IsRunSteamChina_ValueChanged;
    }

    static void IsRunSteamNoCheckUpdate_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
    {
        if (e.NewValue)
            SteamStratParameter.Value += " -noverifyfiles";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace("-noverifyfiles", "").Trim();
    }

    static void IsRunSteamMinimized_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
    {
        if (e.NewValue)
            SteamStratParameter.Value += " -silent";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace("-silent", "").Trim();
    }

    static void IsRunSteamChina_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
    {
        if (e.NewValue)
            SteamStratParameter.Value += " -steamchina";
        else if (SteamStratParameter.Value != null)
            SteamStratParameter.Value = SteamStratParameter.Value.Replace("-steamchina", "").Trim();
    }

    /// <inheritdoc cref="ISteamSettings.SteamStratParameter"/>
    public static SettingsProperty<string?, SteamSettings_> SteamStratParameter { get; } = new();

    /// <inheritdoc cref="ISteamSettings.SteamSkin"/>
    public static SettingsProperty<string, SteamSettings_> SteamSkin { get; } = new();

    /// <inheritdoc cref="ISteamSettings.SteamProgramPath"/>
    public static SettingsProperty<string, SteamSettings_> SteamProgramPath = new();

    /// <inheritdoc cref="ISteamSettings.CustomSteamPath"/>
    public static SettingsProperty<string, SteamSettings_> CustomSteamPath { get; } = new();

    /// <inheritdoc cref="ISteamSettings.IsAutoRunSteam"/>
    public static SettingsProperty<bool, SteamSettings_> IsAutoRunSteam { get; } = new();

    /// <inheritdoc cref="ISteamSettings.IsRunSteamMinimized"/>
    public static SettingsProperty<bool, SteamSettings_> IsRunSteamMinimized { get; } = new();

    /// <inheritdoc cref="ISteamSettings.IsRunSteamNoCheckUpdate"/>
    public static SettingsProperty<bool, SteamSettings_> IsRunSteamNoCheckUpdate { get; } = new();

    /// <inheritdoc cref="ISteamSettings.IsRunSteamChina"/>
    public static SettingsProperty<bool, SteamSettings_> IsRunSteamChina { get; } = new();

    /// <inheritdoc cref="ISteamSettings.IsEnableSteamLaunchNotification"/>
    public static SettingsProperty<bool, SteamSettings_> IsEnableSteamLaunchNotification { get; } = new();

    /// <inheritdoc cref="ISteamSettings.DownloadCompleteSystemEndMode"/>
    public static SettingsProperty<OSExitMode, SteamSettings_> DownloadCompleteSystemEndMode { get; } = new();

    /// <inheritdoc cref="ISteamSettings.IsRunSteamAdministrator"/>
    public static SettingsProperty<bool, SteamSettings_> IsRunSteamAdministrator { get; } = new();
}
