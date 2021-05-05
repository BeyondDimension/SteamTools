using NLog;
using System;
using System.Application;
using System.Application.UI;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using static System.Application.Program;
using Process = System.Diagnostics.Process;

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
            Process.Start(mainFilePath);
            return 0;
        }
    }
}

var rootCommand = new RootCommand("命令行工具(Command Line Tools/CLT)");

var glusca = new Command("GetLoginUsingSteamClientAuth", "获取 Steam 客户端自动登录数据");
glusca.AddAlias("glusca");
glusca.Handler = CommandHandler.Create(async () =>
{
    var s = DISafeGet.GetLoginUsingSteamClientAuth();
    var r = await s.GetLoginUsingSteamClientAuthAsync(false);
    var r2 = Serializable.SMPB64U(r);
    Console.WriteLine(r2);
});
rootCommand.AddCommand(glusca);

var r = rootCommand.InvokeAsync(args).Result;
return r;