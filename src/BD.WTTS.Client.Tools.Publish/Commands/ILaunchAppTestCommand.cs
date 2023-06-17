using BD.WTTS.Client.Tools.Publish.Commands.Abstractions;
using System.CommandLine;

namespace BD.WTTS.Client.Tools.Publish.Commands;

/// <summary>
/// 启动应用程序测试
/// </summary>
interface ILaunchAppTestCommand : ICommand
{
    static Command ICommand.GetCommand()
    {
        var command = new Command("launchapp", "启动应用程序测试")
        {
            // options
        };
        command.SetHandler(Handler);
        return command;
    }

    internal static void Handler()
    {

    }
}
