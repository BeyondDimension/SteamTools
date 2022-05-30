using System.Application;
using System.Application.Models;

return _(args, "客户端发布命令行工具(Command Line Tools/CLT)", init: () =>
{
    if (args.FirstOrDefault() == "apphostpatcher")
    {
        var args_ = args.Skip(1).ToArray();
        return AppHostPatcher.Program.M(args_);
    }

    FileSystem2.InitFileSystem();
    return null;
}, action: ToolConfig.AddInitCommand);