// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(ASFSettings_))]
internal partial class ASFSettingsContext : JsonSerializerContext
{
    static ASFSettingsContext? instance;

    public static ASFSettingsContext Instance
        => instance ??= new ASFSettingsContext(ISettings.GetDefaultOptions());
}

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class ASFSettings_ : IASFSettings, ISettings, ISettings<ASFSettings_>
{
    public const string Name = nameof(ASFSettings);

    static string ISettings.Name => Name;

    static JsonSerializerContext ISettings.JsonSerializerContext
        => ASFSettingsContext.Instance;

    static JsonTypeInfo ISettings.JsonTypeInfo
        => ASFSettingsContext.Instance.ASFSettings_;

    static JsonTypeInfo<ASFSettings_> ISettings<ASFSettings_>.JsonTypeInfo
        => ASFSettingsContext.Instance.ASFSettings_;

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public string ArchiSteamFarmExePath { get; set; } = string.Empty;

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public bool AutoRunArchiSteamFarm { get; set; }

    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public int ConsoleMaxLine { get; set; } = IASFSettings.DefaultConsoleMaxLine;

    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public int ConsoleFontSize { get; set; } = IASFSettings.DefaultConsoleFontSize;

    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public int IPCPortId { get; set; } = IASFSettings.DefaultIPCPortIdValue;

    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public bool IPCPortOccupiedRandom { get; set; } = true;

    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public bool CheckArchiSteamFarmExe { get; set; }
}

public sealed partial class ASFSettings
{
    /// <summary>
    /// ASF路径
    /// </summary>
    public static SettingsProperty<string, ASFSettings_> ArchiSteamFarmExePath { get; }
        = new(string.Empty);

    /// <summary>
    /// 程序启动时自动运行ASF
    /// </summary>
    public static SettingsStructProperty<bool, ASFSettings_> AutoRunArchiSteamFarm { get; }
        = new(false);

    /// <summary>
    /// 检查文件安全性
    /// </summary>
    public static SettingsStructProperty<bool, ASFSettings_> CheckArchiSteamFarmExe { get; }
        = new(false);

    #region ConsoleMaxLine

    /// <summary>
    /// 控制台默认最大行数
    /// </summary>
    public const int DefaultConsoleMaxLine = 200;

    /// <summary>
    /// 控制台默认最大行数范围最小值
    /// </summary>
    public const int MinRangeConsoleMaxLine = DefaultConsoleMaxLine;

    /// <summary>
    /// 控制台默认最大行数范围最大值
    /// </summary>
    public const int MaxRangeConsoleMaxLine = 5000;

    public static SettingsStructProperty<int, ASFSettings_> ConsoleMaxLine { get; }
        = new(DefaultConsoleMaxLine);

    #endregion

    public const int DefaultConsoleFontSize = 14;
    public const int MinRangeConsoleFontSize = 8;
    public const int MaxRangeConsoleFontSize = 24;

    /// <summary>
    /// 控制台字体大小，默认值 14，Android 上单位为 sp
    /// </summary>
    public static SettingsStructProperty<int, ASFSettings_> ConsoleFontSize { get; }
        = new(DefaultConsoleFontSize);

    public const int DefaultIPCPortIdValue = 6242;

    /// <summary>
    /// IPC 端口号，默认值为 <see cref="DefaultIPCPortIdValue"/>
    /// </summary>
    public static SettingsStructProperty<int, ASFSettings_> IPCPortId { get; }
        = new(DefaultIPCPortIdValue);

    /// <summary>
    /// IPC 端口号被占用时是否随机一个未使用的端口号，默认值 <see langword="true"/>
    /// </summary>
    public static SettingsStructProperty<bool, ASFSettings_> IPCPortOccupiedRandom { get; }
        = new(true);
}
