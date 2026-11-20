using BD.WTTS.Client.Tools.Publish.Helpers;

namespace BD.WTTS.Client.Tools.Publish.Commands;

interface INativeAotPublishCommand : ICommand
{
    static Command ICommand.GetCommand()
    {
        var rids = CommandCompat.GetOption("--rids", () => new[] { "win-x64" }, "Windows targets: win-x64, win-arm64");
        rids.AllowMultipleArgumentsPerToken = true;
        var formats = CommandCompat.GetOption("--formats", () => new[] { "7z", "exe", "msix" }, "Package formats: 7z, exe, msix");
        formats.AllowMultipleArgumentsPerToken = true;
        var output = CommandCompat.GetOption<string>("--output", "Package output root (default: build/publish/aot-packages)");
        var makensis = CommandCompat.GetOption<string>("--makensis", "Path to a portable MakeNSIS installation (optional)");
        var thumbprint = CommandCompat.GetOption<string>("--sign-thumbprint", "Code signing certificate in CurrentUser\\My for EXE and MSIX (optional)");
        var command = new Command("aot", "Build and package Watt Toolkit as Windows Native AOT")
        {
            rids, formats, output, makensis, thumbprint,
        };
        command.SetAction(args => NativeAotPublisher.Publish(
            args.GetValue(rids)!, args.GetValue(formats)!, args.GetValue(output), args.GetValue(makensis), args.GetValue(thumbprint)));
        return command;
    }
}
