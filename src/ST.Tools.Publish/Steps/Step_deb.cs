using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Packaging.Targets;
using Packaging.Targets.Deb;
using Packaging.Targets.IO;
using System.Application.Models;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using static System.Application.Utils;

namespace System.Application.Steps
{
    internal static class Step_deb
    {
        static void Handler()
        {
            var publish_json_path = PublishJsonFilePath;
            var publish_json_str = File.ReadAllText(publish_json_path);
            var dirNames = Serializable.DJSON<PublishDirInfo[]>(publish_json_str);

            if (!dirNames.Any_Nullable())
            {
                Console.WriteLine($"错误：发布配置文件读取失败！{publish_json_path}");
                return;
            }

            dirNames = dirNames.ThrowIsNull(nameof(dirNames));

            foreach (var item in dirNames)
            {
                var isLinux = item.Name.StartsWith("linux-");
                if (!isLinux) continue;

                HandlerItem(item);
            }

            Console.WriteLine("完成");
        }

        public static void HandlerItem(PublishDirInfo item)
        {
            var debPath = GetPackPath(item, FileEx.DEB);
            //var debTarPath = GetPackPath(item, FileEx.DEB_TAR);
            var debTarXzPath = GetPackPath(item, FileEx.DEB_TAR_XZ);
            using var targetStream = File.Open(debPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            //using var tarStream = File.Open(debTarPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            using var tarStream = new MemoryStream();

            ArchiveBuilder2 archiveBuilder2 = new()
            {
                Log = TaskLoggingHelper.Instance,
            };
            ArchiveBuilder archiveBuilder = archiveBuilder2;

            var archiveEntries = archiveBuilder.FromDirectory(
                item.Path,
                Constants.HARDCODED_APP_NAME,
                LinuxPackConstants.Prefix,
                Array.Empty<ITaskItem>());

            LinuxPackConstants.AddFileNameDesktop(archiveBuilder2, archiveEntries);

            //archiveEntries.AddRange(archiveBuilder.FromLinuxFolders(this.LinuxFolders));
            DebTask.EnsureDirectories(archiveEntries);

            archiveEntries = archiveEntries
                .OrderBy(e => e.TargetPathWithFinalSlash, StringComparer.Ordinal)
                .ToList();

            TarFileCreator.FromArchiveEntries(archiveEntries, tarStream);
            tarStream.Position = 0;

            // Prepare the list of dependencies
            List<string> dependencies = new List<string>();

            if (item.DeploymentMode == DeploymentMode.FDE)
            {
                dependencies.Add(LinuxPackConstants.aspnetcore_runtime_6_0);
            }

            //if (this.DebDependencies != null)
            //{
            //    var debDependencies = this.DebDependencies.Select(d => d.ItemSpec).ToArray();

            //    dependencies.AddRange(debDependencies);
            //}

            //if (this.DebDotNetDependencies != null)
            //{
            //    var debDotNetDependencies = this.DebDotNetDependencies.Select(d => d.ItemSpec).ToArray();

            //    dependencies.AddRange(debDotNetDependencies);
            //}

            // Prepare the list of recommended dependencies
            List<string> recommends = new List<string>();

            //if (this.DebRecommends != null)
            //{
            //    recommends.AddRange(this.DebRecommends.Select(d => d.ItemSpec));
            //}

            // XZOutputStream class has low quality (doesn't even know it's current position,
            // needs to be disposed to finish compression, etc),
            // So we are doing compression in a separate step
            using (var tarXzStream = File.Open(debTarXzPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (var xzStream = new XZOutputStream(tarXzStream, XZOutputStream.DefaultThreads, XZOutputStream.DefaultPreset, true))
            {
                tarStream.CopyTo(xzStream);
            }

            using (var tarXzStream = File.Open(debTarXzPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var pkg = DebPackageCreator.BuildDebPackage(
                archiveEntries,
                LinuxPackConstants.PackageName,
                LinuxPackConstants.Description,
                LinuxPackConstants.DebMaintainer,
                Utils.Version,
                DebTask.GetPackageArchitecture(item.Name),
                LinuxPackConstants.CreateUser,
                LinuxPackConstants.UserName,
                LinuxPackConstants.InstallService,
                LinuxPackConstants.ServiceName,
                LinuxPackConstants.Prefix,
                LinuxPackConstants.DebSection,
                LinuxPackConstants.DebPriority,
                LinuxPackConstants.DebHomepage,
                LinuxPackConstants.PreInstallScript,
                LinuxPackConstants.PostInstallScript,
                LinuxPackConstants.PreRemoveScript,
                LinuxPackConstants.PostRemoveScript,
                dependencies,
                recommends,
                null!);

                DebPackageCreator.WriteDebPackage(
                    archiveEntries,
                    tarXzStream,
                    targetStream,
                    pkg);
            }

            File.Delete(debTarXzPath);
        }

        public static void Add(RootCommand command)
        {
            var deb = new Command("deb", "Create a Ubuntu/Debian Linux installer");
            deb.Handler = CommandHandler.Create(Handler);
            command.AddCommand(deb);
        }
    }
}
