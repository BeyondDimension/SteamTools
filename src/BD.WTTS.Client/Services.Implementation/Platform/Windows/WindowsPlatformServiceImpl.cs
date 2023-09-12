#if WINDOWS
using static Microsoft.NodejsTools.SharedProject.SystemUtility;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed partial class WindowsPlatformServiceImpl : IPlatformService
{
    const string TAG = "WindowsPlatformS";

    /// <summary>
    /// 用于 <see cref="IOPath.GetCacheFilePath(string, string, string)"/> 中 dirName，临时文件夹名称，在程序退出时将删除整个文件夹
    /// </summary>
    internal const string CacheTempDirName = "Temporary";

    public WindowsPlatformServiceImpl()
    {
        // 平台服务依赖关系过于复杂，在构造函数中不得注入任何服务，由函数中延时加载调用服务
    }

    /// <summary>
    /// %windir%
    /// </summary>
    static readonly Lazy<string> _windir = new(() =>
    {
        var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        return windir.ThrowIsNull();
    });

    static readonly Lazy<string> _explorer_exe = new(() => Path.Combine(_windir.Value, "explorer.exe"));

    /// <summary>
    /// %windir%\explorer.exe
    /// </summary>
    public static string Explorer => _explorer_exe.Value;

    /// <summary>
    /// 带参数(可选/null)启动 %windir%\explorer.exe
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Process? StartProcessExplorer(string args)
    {
        var process = Process2.Start(Explorer, args, workingDirectory: _windir.Value);
        return process;
    }

    /// <summary>
    /// 使用 Explorer 降权运行进程
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public static Process? StartAsInvokerByExplorer(string fileName, string? arguments = null)
    {
        if (string.IsNullOrEmpty(arguments))
            return StartProcessExplorer($"\"{fileName}\"");
        //var processName = Path.GetFileNameWithoutExtension(fileName);
        var cacheCmdFile = IOPath.GetCacheFilePath(CacheTempDirName, "StartAsInvokerByExplorer", FileEx.CMD);
        File.WriteAllText(cacheCmdFile, $"@echo {Constants.HARDCODED_APP_NAME} StartAsInvokerByExplorer{Environment.NewLine}start \"\" \"{fileName}\" {arguments}{Environment.NewLine}del %0");
        var process = StartProcessExplorer($"\"{cacheCmdFile}\"");
        //if (process != null)
        //{
        //    TryDeleteInDelay(process, cacheCmdFile, processName);
        //}
        return process;

        //static async void TryDeleteInDelay(Process process, string filePath, string processName, int millisecondsDelay = 9000, int processWaitMillisecondsDelay = 9000)
        //{
        //    try
        //    {
        //        var waitForExitResult = process.WaitForExit(processWaitMillisecondsDelay);
        //        if (!waitForExitResult)
        //        {
        //            try
        //            {
        //                process.KillEntireProcessTree();
        //            }
        //            catch
        //            {

        //            }
        //        }
        //    }
        //    catch
        //    {
        //        millisecondsDelay = 0;
        //    }
        //    if (millisecondsDelay > 0)
        //    {
        //        var token = new CancellationTokenSource(millisecondsDelay);
        //        try
        //        {
        //            await Task.Run(async () =>
        //            {
        //                while (true)
        //                {
        //                    await Task.Delay(100, token.Token);
        //                    if (Process.GetProcessesByName(processName).Any())
        //                    {
        //                        return;
        //                    }
        //                }
        //            }, token.Token);
        //        }
        //        catch (TaskCanceledException)
        //        {

        //        }
        //    }
        //    IOPath.FileTryDelete(filePath);
        //}
    }

    public Process? StartAsInvoker(string fileName, string? arguments = null)
    {
        if (IsPrivilegedProcess)
        {
            // runas /trustlevel:0x20000 没有真正的降权，只是作为具有限制特权，使用 explorer 最好，但不接受参数，可以创建一个临时cmd脚本启动
            //return StartAsInvokerByRunas(fileName, arguments);
            //return StartAsInvokerByExplorer(fileName, arguments);
            var currentDirectory = Path.GetDirectoryName(fileName) ?? "";
            // 通过 explorer 降权启动，但在 Win11 上 explorer 启动非常缓慢😅
            ExecuteProcessUnElevated(fileName, arguments ?? "", currentDirectory);
            return null;

        }
        else
        {
            var process = Process2.Start(fileName, arguments);
            return process;
        }
    }

#if DEBUG

    bool IPlatformService.DwmIsCompositionEnabled => Interop.DWMApi.DwmIsCompositionEnabled();

#endif
}
#endif