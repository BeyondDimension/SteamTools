namespace BD.WTTS.Client.Tools.Publish.Commands.Abstractions;

public interface ICommand
{
    internal static abstract Command GetCommand();

    static void AddCommand<TCommand>(RootCommand rootCommand) where TCommand : ICommand
    {
        var command = TCommand.GetCommand();
        rootCommand.AddCommand(command);
    }
}
