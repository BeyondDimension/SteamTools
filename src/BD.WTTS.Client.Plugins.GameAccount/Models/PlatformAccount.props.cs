namespace BD.WTTS.Models;

public sealed partial class PlatformAccount : ReactiveObject
{
    public string? FullName { get; set; }

    public string? Icon { get; set; }

    public ThirdpartyPlatform Platform { get; set; }

    public ObservableCollection<Account>? Accounts { get; set; }

    public bool IsEnable { get; set; }

    public bool ExitBeforeInteract { get; set; }

    public string? DefaultExePath { get; set; }

    public string? ExeExtraArgs { get; set; }

    public string? DefaultFolderPath { get; set; }

    public List<string>? PlatformIds { get; set; }

    public List<string>? ExesToEnd { get; set; }

    public List<string>? LoginFiles { get; set; }

    public static List<string>? CachePaths { get; set; }

    public static List<string>? BackupPaths { get; set; }

    public static List<string>? BackupFileTypesIgnore { get; set; }

    public static List<string>? BackupFileTypesInclude { get; set; }

    public static ClosingMethod ClosingMethod { get; set; }

    public static StartingMethod StartingMethod { get; set; }
}
