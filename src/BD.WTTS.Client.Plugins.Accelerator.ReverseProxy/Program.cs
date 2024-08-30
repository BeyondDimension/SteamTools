using BD.Common.Repositories.Abstractions;
using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using KeyValuePair = BD.Common.Entities.KeyValuePair;

const string moduleName = AssemblyInfo.Accelerator;
const string pluginName = AssemblyInfo.Accelerator;
#if DEBUG
//Console.WriteLine($"This: {moduleName} / Program.Start");
var consoleTitle = $"[{Environment.ProcessId}, {IsProcessElevated_DEBUG_Only().ToLowerString()}] {Constants.CUSTOM_URL_SCHEME_NAME}({moduleName}) {string.Join(' ', Environment.GetCommandLineArgs().Skip(1))}";
SetConsoleTitle(consoleTitle);

[MethodImpl(MethodImplOptions.AggressiveInlining)]
static void SetConsoleTitle(string title)
{
    try
    {
        Console.Title = title;
    }
    catch
    {

    }
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
static bool IsProcessElevated_DEBUG_Only()
{
    if (!OperatingSystem.IsWindows())
    {
        return false;
    }
    // use WindowsPlatformServiceImpl.IsProcessElevated on not Debug
    using WindowsIdentity identity = WindowsIdentity.GetCurrent();
    WindowsPrincipal principal = new(identity);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}
#endif
try
{
    var exitCode = await IPCSubProcessService.MainAsync(moduleName, pluginName, ConfigureServices, static ipcProvider =>
    {
        VisualStudioAppCenterSDK.Init();

        // 添加反向代理服务（供主进程的 IPC 远程访问）
        ipcProvider.CreateIpcJoint(LazyReverseProxyServiceImpl.Instance);
        ipcProvider.CreateIpcJoint(LazyCertificateManager.Instance);
    }, args);

#if DEBUG
    if (exitCode != default)
    {
        Console.ReadLine();
    }
#endif
    return exitCode;
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Console.ReadLine();
    return 500;
}
finally
{
    try
    {
        VisualStudioAppCenterSDK.UtilsImpl.Instance.OnExit(null, EventArgs.Empty);
    }
    catch
    {

    }
}

static void ConfigureServices(IServiceCollection services)
{
    services.AddLogging(l =>
    {
        l.ClearProviders();
        l.AddNLog(LogManager.Configuration); // 添加 NLog 日志
#if DEBUG
        l.AddConsole();
#endif
        l.AddProvider(new LogConsoleService.Utf8StringLoggerProvider(moduleName));
    });

    // 设置仓储层数据库文件存放路径
    Repository.DataBaseDirectory = IOPath.AppDataDirectory;
    services.TryAddSingleton<ISecureStorage, RepositorySecureStorage>();

    services.AddHttpClient();
    services.AddCommonHttpClientFactory();
    services.AddSingleton<IHttpPlatformHelperService, HttpPlatformHelperConsoleService>();

    services.AddDnsAnalysisService();
    // 添加反向代理服务（子进程实现）
    services.AddReverseProxyService();
    services.AddSingleton<ICertificateManager, CertificateManagerImpl>();
}

sealed class RepositorySecureStorage : Repository<KeyValuePair, string>, ISecureStorage
{
    bool ISecureStorage.IsNativeSupportedBytes => true;

    static string GetKey(string key) => Hashs.String.SHA256(key);

    async Task<byte[]?> ISecureStorage.GetBytesAsync(string key)
    {
        key = GetKey(key);
        var item = await FirstOrDefaultAsync(x => x.Id == key);
        var value = item?.Value;
        var value2 = value;
        return value2;
    }

    async Task InsertOrUpdateAsync(string key, byte[] value)
    {
        var value2 = value;
        await InsertOrUpdateAsync(new KeyValuePair
        {
            Id = key,
            Value = value2.ThrowIsNull(nameof(value2)),
        });
    }

    Task ISecureStorage.SetAsync(string key, byte[]? value)
    {
        key = GetKey(key);
        if (value == null || value.Length <= 0)
        {
            return DeleteAsync(key);
        }
        else
        {
            return InsertOrUpdateAsync(key, value);
        }
    }

    async Task<bool> ISecureStorage.RemoveAsync(string key)
    {
        key = GetKey(key);
        var result = await DeleteAsync(key);
        return result > 0;
    }

    async Task<bool> ISecureStorage.ContainsKeyAsync(string key)
    {
        key = GetKey(key);
        var item = await FirstOrDefaultAsync(x => x.Id == key);
        return item != null;
    }
}

sealed class HttpPlatformHelperConsoleService : HttpPlatformHelperService
{
    new const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36 Edg/90.0.818.51";

    public override string UserAgent => DefaultUserAgent;
}