using System.Application;
using System.Application.Models;
using System.Linq;

return _(args, "客户端发布命令行工具(Command Line Tools/CLT)", init: () =>
{
    if (args.FirstOrDefault() == "apphostpatcher")
    {
        var args_ = args.Skip(1).ToArray();
        return AppHostPatcher.Program.M(args_);
    }

    FileSystemDesktop.InitFileSystem();
    return null;
}, action: rootCommand =>
{
    ToolConfig.AddInitCommand(rootCommand);
});