using SystemPlatform = System.Runtime.Devices.Platform;

namespace BD.WTTS.Models;

public sealed partial class PlatformAccount : ReactiveObject
{
    [Reactive, S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public ObservableCollection<IAccount> Accounts { get; set; }

    [Reactive, S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public bool IsEnable { get; set; }

    [Reactive, S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public bool IsLoading { get; set; }

    [Reactive, S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public PlatformSettings? PlatformSetting { get; set; }

    public string FullName { get; set; }

    public string? Icon { get; set; }

    public ThirdpartyPlatform Platform { get; init; }

    public bool IsExitBeforeInteract { get; set; }

    public bool IsRegDeleteOnClear { get; set; }

    public bool AllFilesRequired { get; set; }

    public string? ExeExtraArgs { get; set; }

    public string? DefaultExePath { get; set; }

    //public string? DefaultFolderPath { get; set; }

    public string? UniqueIdPath { get; set; }

    public string? UniqueIdRegex { get; set; }

    public UniqueIdType UniqueIdType { get; set; }

    public List<string>? PlatformIds { get; set; }

    public List<string>? ExesToEnd { get; set; }

    public Dictionary<string, string>? LoginFiles { get; set; }

    public List<string>? ClearPaths { get; set; }

    public List<string>? CachePaths { get; set; }

    public List<string>? BackupPaths { get; set; }

    public List<string>? BackupFileTypesIgnore { get; set; }

    public List<string>? BackupFileTypesInclude { get; set; }

    [S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public string PlatformLoginCache
        => Path.Combine(Plugin.Instance.AppDataDirectory, "LoginData", FullName);

    [S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public string IdsJsonPath => Path.Combine(PlatformLoginCache, "ids.json");

    [S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public string? ExeName => Path.GetFileName(DefaultExePath);

    [S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public string? ExePath => PlatformSetting?.PlatformPath ?? DefaultExePath;

    [S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public string? FolderPath => Path.GetDirectoryName(ExePath);

    [S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public bool IsSteamPlatform => Platform == ThirdpartyPlatform.Steam;

    [S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public bool IsWindwos => IsSteamPlatform && DeviceInfo2.Platform() == SystemPlatform.Windows;

    public string RegJsonPath(string accName) => Path.Combine(PlatformLoginCache, accName, "reg.json");

    public ICommand SwapToAccountCommand { get; }

    public ICommand OpenUrlToBrowserCommand { get; }

    public ICommand DeleteAccountCommand { get; }

    public ICommand SetAccountAvatarCommand { get; }

    public ICommand EditRemarkCommand { get; }

    public ICommand CopyCommand { get; }

    public ICommand OpenLinkCommand { get; }

    public ICommand CreateShortcutCommand { get; }
}
