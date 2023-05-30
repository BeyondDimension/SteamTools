// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 由平台实现的服务
/// </summary>
public partial interface IPlatformService : IPCPlatformService
{
    /// <summary>
    /// 从 Ioc 容器中获取由平台实现的服务
    /// </summary>
    static IPlatformService Instance => Ioc.Get<IPlatformService>();

    protected const string TAG = "PlatformS";

    static class IPCRoot
    {
        internal const string moduleName = "Administrator";
        internal const string args_PipeName = "-n";
        internal const string args_ProcessId = "-p";
        internal const string CommandName = "sudo";

        static readonly TaskCompletionSource<IPCPlatformService> tcs = new();

        /// <summary>
        /// 从 IPC 框架中获取由管理员权限子进程实现的远程服务
        /// </summary>
        public static Task<IPCPlatformService> Instance => tcs.Task;

        internal static async Task SetIPC(IPCMainProcessService ipc)
        {
            try
            {
                var iPCPlatformService = await ipc.GetServiceAsync<IPCPlatformService>(moduleName);
                iPCPlatformService.ThrowIsNull();
#if DEBUG
                try
                {
                    var debugStringIPC = $"Pid: {Environment.ProcessId}, Exe: {Environment.ProcessPath}, Asm: {Assembly.GetAssembly(typeof(IPlatformService))?.FullName}{Environment.NewLine}{iPCPlatformService.GetDebugString()}";
                    Console.WriteLine($"DebugString/IPCPlatformService: {debugStringIPC}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
#endif
                tcs.TrySetResult(iPCPlatformService);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }
        }
    }
}
