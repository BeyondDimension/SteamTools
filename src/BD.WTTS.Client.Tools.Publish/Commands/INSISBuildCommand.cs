using BD.WTTS.Client.Tools.Publish.Helpers;
using System.Globalization;
using static BD.WTTS.Client.Tools.Publish.Commands.IDotNetPublishCommand;

namespace BD.WTTS.Client.Tools.Publish.Commands;

interface INSISBuildCommand : ICommand
{
    const string commandName = "nsis";

    static Command ICommand.GetCommand()
    {
        var debug = CommandCompat.GetOption<bool>("--debug", "Defines the build configuration");
        var rids = CommandCompat.GetOption<string[]>("--rids", "RID is short for runtime identifier");
        var timestamp = CommandCompat.GetOption<string>("--t", "Release timestamp");
        var force_sign = CommandCompat.GetOption<bool>("--force-sign", GetDefForceSign, "Mandatory verification must be digitally signed");
        var hsm_sign = CommandCompat.GetOption<bool>("--hsm-sign", "");
        var aot = CommandCompat.GetOption<bool>("--aot", () => true, "");
        var command = new Command(commandName, "NSIS build generate")
        {
           debug, rids, timestamp, force_sign, hsm_sign, aot,
        };
        command.SetHandler(Handler, debug, rids, timestamp, force_sign, hsm_sign, aot);
        return command;
    }

    internal static void Handler(bool debug, string[] rids, string timestamp, bool force_sign, bool hsm_sign, bool aot)
    {
        if (ProjectUtils.ProjPath.Contains("actions-runner"))
        {
            hsm_sign = false; // hsm 目前无法映射到 CI VM 中
        }

        var publishDirectory = Path.Combine(ProjectPath_AvaloniaApp, "bin", PublishCommandArg.GetConfiguration(debug), "Publish");
        var installer = new NsisInstaller(Path.Combine(ProjectUtils.ProjPath, "..", "NSIS-Build"));
        var useLatest = string.Equals("latest", timestamp, StringComparison.OrdinalIgnoreCase);
        if (!useLatest && !IsReleaseTimestamp(timestamp))
            throw new ArgumentException($"Use 'latest' or a release timestamp in {ReleaseTimestampFormat} format.", nameof(timestamp));

        foreach (var rid in rids)
        {
            var info = DeconstructRuntimeIdentifier(rid);
            if (info.Platform != Platform.Windows) continue;

            var targetRid = $"win-{ArchToString(info.Architecture)}";
            var batch = useLatest ? FindLatestTimestamp(publishDirectory, debug, targetRid, aot) : timestamp;
            var archive = GetArchivePath(publishDirectory, debug, targetRid, batch, aot: false);
            var aotArchive = aot ? GetArchivePath(publishDirectory, debug, targetRid, batch, aot: true) : null;

            if (!IsArchive(archive))
                throw new FileNotFoundException("The JIT publish archive was not found or is empty.", archive);
            if (aotArchive != null && !IsArchive(aotArchive))
                throw new FileNotFoundException("The Native AOT publish archive was not found or is empty.", aotArchive);

            BuildInstaller(archive);
            if (aotArchive != null)
                BuildInstaller(aotArchive);
        }

        void BuildInstaller(string archive)
        {
            var output = Path.ChangeExtension(archive, FileEx.EXE);
            IOPath.FileTryDelete(output);
            installer.Build(archive, output, AppVersion4);

            if (!debug)
            {
                var fileNames = $"\"{output}\"";
                var pfxFilePath = hsm_sign ? MSIXHelper.SignTool.pfxFilePath_HSM_CodeSigning : null;
                try
                {
                    MSIXHelper.SignTool.Start(force_sign, fileNames, pfxFilePath, publishDirectory);
                }
                catch
                {
                    MSIXHelper.SignTool.Start(force_sign, fileNames, MSIXHelper.SignTool.pfxFilePath_BeyondDimension_CodeSigning, publishDirectory);
                }
            }
        }
    }

    static string FindLatestTimestamp(string directory, bool debug, string rid, bool aot)
    {
        var prefix = GetPublishFileName(debug, rid, timestamp: "");
        var batches = from archive in Directory.EnumerateFiles(directory, $"{prefix}*{FileEx._7Z}")
                      let stem = Path.GetFileNameWithoutExtension(archive)
                      where stem.StartsWith(prefix, StringComparison.Ordinal) && IsArchive(archive)
                      let timestamp = stem[prefix.Length..]
                      where IsReleaseTimestamp(timestamp) && (!aot || IsArchive(GetArchivePath(directory, debug, rid, timestamp, aot: true)))
                      select timestamp;
        return batches.OrderDescending(StringComparer.Ordinal).FirstOrDefault() ??
            throw new FileNotFoundException($"No complete publish archive batch was found for {rid} in {directory}.");
    }

    static bool IsReleaseTimestamp(string timestamp) =>
        DateTime.TryParseExact(timestamp, ReleaseTimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    static bool IsArchive(string path) => new FileInfo(path) is { Exists: true, Length: > 0 };

    static string GetArchivePath(string directory, bool debug, string rid, string timestamp, bool aot) =>
        Path.Combine(directory, GetPublishFileName(debug, rid, FileEx._7Z, aot, timestamp));
}
