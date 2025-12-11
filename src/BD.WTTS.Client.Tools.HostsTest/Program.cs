// https://learn.microsoft.com/zh-cn/sysinternals/downloads/handle

var fColor = Console.ForegroundColor;
var bColor = Console.BackgroundColor;
try
{
    var isProcessElevated = IsProcessElevated(Process.GetCurrentProcess());
    Console.WriteLine($"当前进程是否为管理员权限：{isProcessElevated}");

    var hostsFilePath = Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");
    Console.WriteLine($"hosts 文件路径：{hostsFilePath}");

    try
    {
        var text = File.ReadAllText(hostsFilePath);
        Console.WriteLine("hosts 文件内容：");
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine(text);
        Console.WriteLine("--------------------------------------------------");

        File.WriteAllText(hostsFilePath, text);
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("OK");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("错误：");
        Console.WriteLine(ex.ToString());

        // https://learn.microsoft.com/zh-cn/sysinternals/downloads/handle
        Console.WriteLine("查找 hosts 文件被哪个进程占用的命令，需要安装工具：");
        Console.WriteLine("https://learn.microsoft.com/zh-cn/sysinternals/downloads/handle");
        Console.WriteLine();
        Console.WriteLine(
$"""
handle.exe "{hostsFilePath}" /accepteula
""");
    }
}
finally
{
    Console.ForegroundColor = fColor;
    Console.BackgroundColor = bColor;
    Console.WriteLine("键入回车键后退出此程序：");
    Console.ReadLine();
}

/// <summary>
/// 检查指定的进程是否以管理员权限运行
/// </summary>
/// <param name="process"></param>
/// <returns></returns>
[MethodImpl(MethodImplOptions.AggressiveInlining)]
static unsafe bool IsProcessElevated(Process process)
{
    try
    {
        var handle = process.Handle;

        // IsPrivilegedProcess
        // https://github.com/dotnet/runtime/pull/77355/files#diff-1c6f0e5208d48036e96fcc9c0243d93595f3ce16f2d1a50f51ba604d930ca69dR87
        PInvoke.Kernel32.SafeObjectHandle? token = null;
        try
        {
            if (PInvoke.AdvApi32.OpenProcessToken(handle,
                PInvoke.AdvApi32.TokenAccessRights.TOKEN_READ,
                out token))
            {
                TOKEN_ELEVATION elevation = default;
                if (PInvoke.AdvApi32.GetTokenInformation(
                    token,
                    PInvoke.AdvApi32.TOKEN_INFORMATION_CLASS.TokenElevation,
                    &elevation,
                    sizeof(TOKEN_ELEVATION),
                    out _))
                {
                    return elevation.TokenIsElevated != BOOL.FALSE;
                }
            }
        }
        finally
        {
            token?.Dispose();
        }

        var error = Marshal.GetLastPInvokeError();
        throw new Win32Exception(error);

        //using WindowsIdentity identity = new(handle);
        //WindowsPrincipal principal = new(identity);
        //return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    catch (Win32Exception ex)
    {
        /* “process.Handle”引发了类型“System.ComponentModel.Win32Exception”的异常
         * Data: {System.Collections.ListDictionaryInternal}
         * ErrorCode: -2147467259
         * HResult: -2147467259
         * HelpLink: null
         * InnerException: null
         * Message: "拒绝访问。"
         * NativeErrorCode: 5
         * Source: "System.Diagnostics.Process"
         */
        if (ex.NativeErrorCode == 5)
            return true;
    }
    return false;
}

// https://msdn.microsoft.com/en-us/library/windows/desktop/bb530717.aspx
struct TOKEN_ELEVATION
{
    public BOOL TokenIsElevated;
}

/// <summary>
/// Blittable version of Windows BOOL type. It is convenient in situations where
/// manual marshalling is required, or to avoid overhead of regular bool marshalling.
/// </summary>
/// <remarks>
/// Some Windows APIs return arbitrary integer values although the return type is defined
/// as BOOL. It is best to never compare BOOL to TRUE. Always use bResult != BOOL.FALSE
/// or bResult == BOOL.FALSE .
/// </remarks>
enum BOOL : int
{
    FALSE = 0,
    TRUE = 1,
}