namespace BD.WTTS.Models;

public sealed partial class PlatformAccount : ReactiveObject
{
    public string PlatformLoginCache => $"{IOPath.AppDataDirectory}\\LoginCache\\{FullName}\\";

    public string IdsJsonPath => Path.Combine(PlatformLoginCache, "ids.json");

    public string RegJsonPath(string accName) => Path.Combine(PlatformLoginCache, accName, "reg.json");

    [Reactive, S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public ObservableCollection<IAccount>? Accounts { get; set; }

    [Reactive, S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public bool IsEnable { get; set; }

    [Reactive, S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public bool IsLoading { get; set; }

    [Reactive, S_JsonIgnore, MP2Ignore, N_JsonIgnore]
    public PlatformSettings? PlatformSetting { get; set; }

    public string FullName { get; set; }

    public string? Icon { get; set; }

    public ThirdpartyPlatform Platform { get; init; }

    public string? ExeName => Path.GetFileName(DefaultExePath);

    public string? ExePath => PlatformSetting?.PlatformPath ?? DefaultExePath;

    public bool IsExitBeforeInteract { get; set; }

    public bool IsRegDeleteOnClear { get; set; }

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
}
