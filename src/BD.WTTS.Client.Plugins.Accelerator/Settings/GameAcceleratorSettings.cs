// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(GameAcceleratorSettings_))]
internal partial class GameAcceleratorSettingsContext : JsonSerializerContext
{
    static GameAcceleratorSettingsContext? instance;

    public static GameAcceleratorSettingsContext Instance
        => instance ??= new GameAcceleratorSettingsContext(ISettings.GetDefaultOptions());
}

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class GameAcceleratorSettings_ : ISettings, ISettings<GameAcceleratorSettings_>
{
    public const string Name = nameof(GameAcceleratorSettings);

    static string ISettings.Name => Name;

    static JsonSerializerContext ISettings.JsonSerializerContext
        => GameAcceleratorSettingsContext.Instance;

    static JsonTypeInfo ISettings.JsonTypeInfo
        => GameAcceleratorSettingsContext.Instance.GameAcceleratorSettings_;

    static JsonTypeInfo<GameAcceleratorSettings_> ISettings<GameAcceleratorSettings_>.JsonTypeInfo
        => GameAcceleratorSettingsContext.Instance.GameAcceleratorSettings_;

    /// <summary>
    /// 我的加速游戏列表
    /// </summary>
    [MP2Key(0), JsonPropertyOrder(0)]
    public Dictionary<int, XunYouGameViewModel> MyGames { get; set; } = [];

    /// <summary>
    /// 加速插件安装路径
    /// </summary>
    [MP2Key(1), JsonPropertyOrder(1)]
    public string WattAcceleratorDirPath { get; set; } = DefaultWattAcceleratorDirPath;

    public static readonly string DefaultWattAcceleratorDirPath = OperatingSystem.IsWindows() ?
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "WattAccelerator") : string.Empty;

}

public static partial class GameAcceleratorSettings
{
    /// <inheritdoc cref="GameAcceleratorSettingsModel.MyGames"/>
    public static SettingsProperty<int, XunYouGameViewModel, Dictionary<int, XunYouGameViewModel>, GameAcceleratorSettings_> MyGames { get; }
        = new();

    /// <inheritdoc cref="GameAcceleratorSettingsModel.WattAcceleratorDirPath"/>
    public static SettingsProperty<string, GameAcceleratorSettings_> WattAcceleratorDirPath { get; }
        = new(GameAcceleratorSettings_.DefaultWattAcceleratorDirPath);
}