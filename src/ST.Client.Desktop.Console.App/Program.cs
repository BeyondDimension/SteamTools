using NLog;
using System;
using System.Application;
using System.Application.Services;
using System.Application.UI;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using static System.Application.Program;
using static System.Application.Services.ISteamService;
using Process = System.Diagnostics.Process;

#if WINDOWS_DESKTOP_BRIDGE
if (!DesktopBridgeHelper2.Init()) return 0;
#endif
var logDirPath = InitLogDir("_console");

logger = LogManager.GetCurrentClassLogger();

if (!args.Any())
{
    var mainFilePath = AppHelper.ProgramPath;
    var endsWithMark = DI.Platform == Platform.Windows ? ".console.exe" : ".console";
    if (mainFilePath.EndsWith(endsWithMark, StringComparison.OrdinalIgnoreCase))
    {
        mainFilePath = mainFilePath.Substring(0, mainFilePath.Length - endsWithMark.Length);
        if (DI.Platform == Platform.Windows) mainFilePath += ".exe";
        if (File.Exists(mainFilePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = mainFilePath,
                UseShellExecute = true,
            });
            return 0;
        }
    }
}

var rootCommand = new RootCommand("命令行工具(Command Line Tools/CLT)");

var getstmauth = new Command("getstmauth", "获取 Steam 客户端自动登录数据");
getstmauth.AddOption(new Option<string>("-key"));
getstmauth.Handler = CommandHandler.Create(async (string key) =>
{
    if (!string.IsNullOrWhiteSpace(key))
    {
        var (connStr, rsaPK) = Serializable.DMPB64U<(string connStr, string rsaPK)>(key)!;
        var s = DISafeGet.GetLoginUsingSteamClientAuth();
        using var rsa = RSAUtils.CreateFromJsonString(rsaPK);
        using var aes = AESUtils.Create();
#if DEBUG
        if (DI.Platform == Platform.Windows)
        {
            var dps = DI.Get<IDesktopPlatformService>();
            Console.WriteLine($"IsAdministrator: {dps.IsAdministrator}");
        }
#endif
        (LoginUsingSteamClientResultCode resultCode, string[]? cookies) result;
        try
        {
            result = await s.GetLoginUsingSteamClientCookiesAsync();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        var cookiesBytes = Serializable.SMP(result);
        cookiesBytes = aes.Encrypt(cookiesBytes);
        var aesKey = aes.ToParamsByteArray();
        aesKey = rsa.Encrypt(aesKey);
        var fileBytes = Serializable.SMP((cookiesBytes, aesKey));
        File.WriteAllBytes(connStr, fileBytes);
    }
});
rootCommand.AddCommand(getstmauth);

if (DI.Platform == Platform.Windows)
{
    var opendesktopicon = new Command("opendesktopicon", "打开桌面图标设置");
    opendesktopicon.AddAlias("odo");
    opendesktopicon.Handler = CommandHandler.Create(() =>
    {
        Startup.Init(DILevel.Min);
        var dps = DI.Get<IDesktopPlatformService>();
        dps.OpenDesktopIconsSettings();
    });
    rootCommand.AddCommand(opendesktopicon);

    var opengamecont = new Command("opengamecont", "打开游戏控制器(手柄)设置");
    opengamecont.AddAlias("ogc");
    opengamecont.Handler = CommandHandler.Create(() =>
    {
        Startup.Init(DILevel.Min);
        var dps = DI.Get<IDesktopPlatformService>();
        dps.OpenGameControllers();
    });
    rootCommand.AddCommand(opengamecont);

    var protproc = new Command("protproc", "查看占用端口的进程");
    protproc.AddAlias("pp");
    protproc.AddOption(new Option<ushort>(new[] { "-port", "-p" }));
    protproc.AddOption(new Option<bool>("-udp"));
    protproc.AddOption(new Option<bool>("-kill"));
    protproc.AddOption(new Option<bool>("-entireProcessTree"));
    protproc.Handler = CommandHandler.Create((ushort port, bool udp, bool kill, bool entireProcessTree) =>
    {
        Startup.Init(DILevel.Min);
        var dps = DI.Get<IDesktopPlatformService>();
        var p = dps.GetProcessByPortOccupy(port, !udp);
        if (p == null)
        {
            Console.WriteLine("没有找到进程");
        }
        else
        {
            Console.WriteLine($"PID: {p.Id}");
            Console.WriteLine($"ProcessName: {p.ProcessName}");
            Console.WriteLine($"StartTime: {p.StartTime}");
            Console.WriteLine($"HandleCount: {p.HandleCount}");
            try
            {
                Console.WriteLine($"FileName: {p.MainModule?.FileName}");
            }
            catch
            {
            }
            if (kill)
            {
                p.Kill(entireProcessTree);
            }
        }
    });
    rootCommand.AddCommand(protproc);
}

#if DEBUG
var ipc = new Command("ipc");
ipc.AddOption(new Option<string>("-key"));
ipc.Handler = CommandHandler.Create((string key) =>
{
    if (!string.IsNullOrWhiteSpace(key))
    {
        var connStr = Serializable.DMPB64U<string>(key)!;
        Console.WriteLine("connStr: " + connStr);
        using var pipeClient = new AnonymousPipeClientStream(PipeDirection.In, connStr);
        Console.WriteLine("[CLIENT] Current TransmissionMode: {0}.", pipeClient.TransmissionMode);

        using var sr = new StreamReader(pipeClient);
        // Display the read text to the console
        string? temp;

        // Wait for 'sync message' from the server.
        do
        {
            Console.WriteLine("[CLIENT] Wait for sync...");
            temp = sr.ReadLine();
        }
        while (temp != null && !temp.StartsWith("SYNC"));

        // Read the server data and echo to the console.
        while ((temp = sr.ReadLine()) != null)
        {
            Console.WriteLine("[CLIENT] Echo: " + temp);
        }
    }
});
rootCommand.AddCommand(ipc);

var ipc2 = new Command("ipc2");
ipc2.AddOption(new Option<string>("-key"));
ipc2.Handler = CommandHandler.Create((string key) =>
{
    if (!string.IsNullOrWhiteSpace(key))
    {
        var connStr = Serializable.DMPB64U<string>(key)!;
        Console.WriteLine("connStr: " + connStr);

        File.WriteAllText(connStr, "TestFileW");
    }
});
rootCommand.AddCommand(ipc2);
#endif

var r = rootCommand.InvokeAsync(args).Result;
#if DEBUG
Console.ReadLine();
#endif
return r;