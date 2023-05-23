using static BD.WTTS.Settings.Abstractions.ISteamSettings;

namespace BD.WTTS.Settings;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class SteamSettings_ : ISteamSettings, ISettings
{
    public const string Name = nameof(SteamSettings);

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public string? SteamStratParameter { get; set; }

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public string? SteamSkin { get; set; }

    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public string? SteamProgramPath { get; set; }

    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public bool? IsAutoRunSteam { get; set; }

    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public bool? IsRunSteamMinimized { get; set; }

    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public bool? IsRunSteamNoCheckUpdate { get; set; }

    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public bool? IsRunSteamChina { get; set; }

    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public bool? IsEnableSteamLaunchNotification { get; set; }

    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public OSExitMode? DownloadCompleteSystemEndMode { get; set; }

    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public bool? IsRunSteamAdministrator { get; set; }
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
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    static readonly ISteamService steamService = Ioc.Get<ISteamService>();

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

    /// <inheritdoc cref="ISteamSettings.SteamStratParameter"/>
    public static SettingsProperty<string, SteamSettings_> SteamStratParameter { get; } = new();

    /// <inheritdoc cref="ISteamSettings.SteamSkin"/>
    public static SettingsProperty<string, SteamSettings_> SteamSkin { get; } = new();

    /// <inheritdoc cref="ISteamSettings.SteamProgramPath"/>
    public static SettingsProperty<string, SteamSettings_> SteamProgramPath { get; } = new(steamService.SteamProgramPath);

    /// <inheritdoc cref="ISteamSettings.IsAutoRunSteam"/>
    public static SettingsStructProperty<bool, SteamSettings_> IsAutoRunSteam { get; }
        = new(DefaultIsAutoRunSteam);

    /// <inheritdoc cref="ISteamSettings.IsRunSteamMinimized"/>
    public static SettingsStructProperty<bool, SteamSettings_> IsRunSteamMinimized { get; }
        = new(DefaultIsRunSteamMinimized);

    /// <inheritdoc cref="ISteamSettings.IsRunSteamNoCheckUpdate"/>
    public static SettingsStructProperty<bool, SteamSettings_> IsRunSteamNoCheckUpdate { get; }
        = new(DefaultIsRunSteamNoCheckUpdate);

    /// <inheritdoc cref="ISteamSettings.IsRunSteamChina"/>
    public static SettingsStructProperty<bool, SteamSettings_> IsRunSteamChina { get; }
        = new(DefaultIsRunSteamChina);

    /// <inheritdoc cref="ISteamSettings.IsEnableSteamLaunchNotification"/>
    public static SettingsStructProperty<bool, SteamSettings_> IsEnableSteamLaunchNotification { get; }
        = new(DefaultIsEnableSteamLaunchNotification);

    /// <inheritdoc cref="ISteamSettings.DownloadCompleteSystemEndMode"/>
    public static SettingsStructProperty<OSExitMode, SteamSettings_> DownloadCompleteSystemEndMode { get; }
        = new(DefaultDownloadCompleteSystemEndMode);

    /// <inheritdoc cref="ISteamSettings.IsRunSteamAdministrator"/>
    public static SettingsStructProperty<bool, SteamSettings_> IsRunSteamAdministrator { get; }
        = new(DefaultIsRunSteamAdministrator);
#endif
}

