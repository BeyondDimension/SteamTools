using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Packaging.Targets;
using Packaging.Targets.IO;
using Packaging.Targets.Rpm;
using System.Application.Models;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using static System.Application.Utils;

namespace System.Application.Steps
{
    internal static class Step_rpm
    {
        static PgpKeyRingGenerator? krgen;
        static PgpSecretKeyRing? secretKeyRing;
        static PgpPrivateKey? privateKey;
        //static PgpPublicKey? publicKey;

        public static void Init()
        {
            krgen = PgpSigner.GenerateKeyRingGenerator("dotnet", "dotnet");
            secretKeyRing = krgen.GenerateSecretKeyRing();
            privateKey = secretKeyRing.GetSecretKey().ExtractPrivateKey("dotnet".ToCharArray());
            //publicKey = secretKeyRing.GetPublicKey();
        }

        static void Handler(bool dev)
        {
            Init();

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

                HandlerItem(dev, item);
            }

            Console.WriteLine("完成");
        }

        public static void HandlerItem(bool dev, PublishDirInfo item)
        {
            var rpmPath = GetPackPath(dev, item, FileEx.RPM);
            //var cpioPath = GetPackPath(item, FileEx.CPIO);

            using var targetStream = File.Open(rpmPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            //using var cpioStream = File.Open(cpioPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            using var cpioStream = new MemoryStream();

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

            archiveEntries = archiveEntries
                .OrderBy(e => e.TargetPathWithFinalSlash, StringComparer.Ordinal)
                .ToList();

            CpioFileCreator cpioCreator = new CpioFileCreator();
            cpioCreator.FromArchiveEntries(
                archiveEntries,
                cpioStream);
            cpioStream.Position = 0;

            // Prepare the list of dependencies
            List<PackageDependency> dependencies = new List<PackageDependency>();

            if (item.DeploymentMode == DeploymentMode.FDE)
            {
                dependencies.Add(new PackageDependency { Name = LinuxPackConstants.aspnetcore_runtime_6_0 });
            }

            //if (this.RpmDotNetDependencies != null)
            //{
            //    dependencies.AddRange(
            //        this.RpmDotNetDependencies.Select(
            //            d => GetPackageDependency(d)));
            //}

            //if (this.RpmDependencies != null)
            //{
            //    dependencies.AddRange(
            //        this.RpmDependencies.Select(
            //            d => GetPackageDependency(d)));
            //}

            RpmPackageCreator rpmCreator = new RpmPackageCreator();
            rpmCreator.CreatePackage(
                archiveEntries,
                cpioStream,
                LinuxPackConstants.PackageName,
                Utils.GetVersion(dev),
                RpmTask.GetPackageArchitecture(item.Name),
                LinuxPackConstants.Release,
                LinuxPackConstants.CreateUser,
                LinuxPackConstants.UserName,
                LinuxPackConstants.InstallService,
                LinuxPackConstants.ServiceName,
                LinuxPackConstants.RpmVendor,
                LinuxPackConstants.Description,
                LinuxPackConstants.Url,
                LinuxPackConstants.Prefix,
                LinuxPackConstants.PreInstallScript,
                LinuxPackConstants.PostInstallScript,
                LinuxPackConstants.PreRemoveScript,
                LinuxPackConstants.PostRemoveScript,
                dependencies,
                null!,
                privateKey!,
                targetStream);
        }

        public static void Add(RootCommand command)
        {
            var rpm = new Command("rpm", "Create a CentOS/RedHat Linux installer");
            rpm.AddOption(new Option<bool>("-dev", DevDesc));
            rpm.Handler = CommandHandler.Create(Handler);
            command.AddCommand(rpm);
        }
    }
}
