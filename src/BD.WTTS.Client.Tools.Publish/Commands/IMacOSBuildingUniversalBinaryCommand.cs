namespace BD.WTTS.Client.Tools.Publish.Commands;

partial interface IMacOSBuildingUniversalBinaryCommand : ICommand
{
    // https://developer.apple.com/documentation/apple-silicon/building-a-universal-macos-binary

    const string commandName = "macos-universal-binary";

    static Command ICommand.GetCommand()
    {
        // aspnetcore-runtime-7.0.8-osx-arm64
        var arm64 = new Option<string>("--arm64", "Apple silicon binary directory");
        // aspnetcore-runtime-7.0.8-osx-x64
        var x64 = new Option<string>("--x64", "Intel-based binary directory");
        var command = new Command(commandName, "Create macOS apps and other executables that run natively on both Apple silicon and Intel-based Mac computers.")
        {
            arm64, x64,
        };
        command.SetHandler(Handler, arm64, x64);
        return command;
    }

    internal static void Handler(string arm64, string x64)
    {
        var arm64Info = new AppPublishInfo
        {
            Architecture = Architecture.Arm64,
            DeviceIdiom = DeviceIdiom.Desktop,
            Platform = Platform.Apple,
            DirectoryPath = arm64,
        };
        IScanPublicDirectoryCommand.ScanPath(null, arm64Info);
        foreach (var item in arm64Info.Files)
        {
            using var fileStream = File.OpenRead(item.FilePath);
            item.SHA384 = Hashs.String.SHA384(fileStream);
        }

        var x64Info = new AppPublishInfo
        {
            Architecture = Architecture.X64,
            DeviceIdiom = DeviceIdiom.Desktop,
            Platform = Platform.Apple,
            DirectoryPath = x64,
        };
        IScanPublicDirectoryCommand.ScanPath(null, x64Info);
        foreach (var item in x64Info.Files)
        {
            using var fileStream = File.OpenRead(item.FilePath);
            item.SHA384 = Hashs.String.SHA384(fileStream);
        }

        HashSet<string> mArm64DiffX64 = new();
        foreach (var item in arm64Info.Files)
        {
            var findItem = x64Info.Files.
                FirstOrDefault(x => x.RelativePath == item.RelativePath &&
                x.SHA384 == item.SHA384);
            if (findItem == null)
                mArm64DiffX64.Add(item.RelativePath);
        }

        HashSet<string> mX64DiffArm64 = new();
        foreach (var item in x64Info.Files)
        {
            var findItem = arm64Info.Files.
                FirstOrDefault(x => x.RelativePath == item.RelativePath &&
                x.SHA384 == item.SHA384);
            if (findItem == null)
                mX64DiffArm64.Add(item.RelativePath);
        }

        MacOSBuildingUniversalBinaryInfo info = new(mArm64DiffX64, mArm64DiffX64, arm64Info, x64Info);
        Console.WriteLine(info);
    }

}
