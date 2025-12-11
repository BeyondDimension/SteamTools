using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Packaging.Targets;
using Packaging.Targets.Deb;
using Packaging.Targets.IO;
using Packaging.Targets.Rpm;

namespace BD.WTTS.Client.Tools.Publish.Commands;

interface IInstallPackageCommand : ICommand
{
    const string commandName = "install";

    static Command ICommand.GetCommand()
    {
        var rpm = new Option<bool>("--rpm", "Create CentOS/RedHat Linux installer");
        var deb = new Option<bool>("--deb", "Create Ubuntu/Debian Linux installer");
        var pkg = new Option<bool>("--pkg", "Create macOS installer");
        var msi = new Option<bool>("--msi", "Create Windows Installer (msi) package");
        var rids = new Option<string[]>("--rids", "RID is short for runtime identifier");
        var command = new Command(commandName, "Create install package")
        {
            rpm, deb, pkg, msi, rids,
        };
        command.SetHandler(Handler, rpm, deb, pkg, msi, rids);
        return command;
    }

    internal static void Handler(bool rpm, bool deb, bool pkg, bool msi, string[] rids)
    {
        var tasks = AppPublishInfo.Instance.
              Where(x => rids.Contains(x.RuntimeIdentifier)).
              Select(x =>
              {
                  return GetTasks();
                  IEnumerable<ThreadTask> GetTasks()
                  {
                      switch (x.Platform)
                      {
                          case Platform.Windows:
                              //if (msi)
                              //    yield return InBackground(() =>
                              //    {
                              //        GenerateMsiPackage(x);
                              //    });
                              break;
                          case Platform.Apple:
                              switch (x.DeviceIdiom)
                              {
                                  case DeviceIdiom.Desktop:
                                      //if (pkg)
                                      //    yield return InBackground(() =>
                                      //    {
                                      //        GeneratePkgPackage(x);
                                      //    });
                                      break;
                              }
                              break;
                          case Platform.Linux:
                              if (rpm)
                                  yield return InBackground(() =>
                                  {
                                      GenerateRpmPackage(x);
                                  });
                              if (deb)
                                  yield return InBackground(() =>
                                  {
                                      GenerateDebPackage(x);
                                  });
                              break;
                      }
                  }
              }).SelectMany(x => x).ToArray();
        ThreadTask.WaitAll(tasks);
        Console.WriteLine($"{commandName} OK");
    }

    static void GenerateDebPackage(AppPublishInfo item)
    {
        var debPath = GetPackPath(item, FileEx.DEB);
        var debTarXzPath = GetPackPath(item, FileEx.DEB_TAR_XZ);
        using var targetStream = File.Open(debPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        using var tarStream = new MemoryStream();

        ArchiveBuilder2 archiveBuilder2 = new()
        {
            Log = TaskLoggingHelper.Instance,
        };
        ArchiveBuilder archiveBuilder = archiveBuilder2;

        var archiveEntries = archiveBuilder.FromDirectory(
            item.DirectoryPath,
            Constants.HARDCODED_APP_NAME,
            LinuxPackConstants.Prefix,
            Array.Empty<ITaskItem>());

        AddFileNameDesktop(archiveBuilder2, archiveEntries);

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
            dependencies.Add(LinuxPackConstants.aspnetcore_runtime);
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
            GetVersion(),
            DebTask.GetPackageArchitecture(item.RuntimeIdentifier),
            LinuxPackConstants.CreateUser,
            LinuxPackConstants.UserName,
            LinuxPackConstants.InstallService,
            LinuxPackConstants.ServiceName,
            LinuxPackConstants.Prefix,
            //LinuxPackConstants.DebSection,
            "utils",
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

    sealed class ArchiveBuilder2 : ArchiveBuilder
    {
        public new void AddFile(string entry, string relativePath, string prefix, List<ArchiveEntry> value, ITaskItem[] metadata)
        {
            base.AddFile(entry, relativePath, prefix, value, metadata);
        }
    }

    static void AddFileNameDesktop(ArchiveBuilder2 archiveBuilder2, List<ArchiveEntry> archiveEntries)
    {
        var metadata = new Dictionary<string, string>()
        {
            { "CopyToPublishDirectory", "Always" },
            { "LinuxPath", "/usr/share/applications/" + LinuxPackConstants.FileNameDesktop },
            { "Link", LinuxPackConstants.FileNameDesktop },
        };

        var taskItem = new TaskItem(LinuxPackConstants.FileNameDesktop, metadata);
        var taskItems = new ITaskItem[] { taskItem };
        archiveBuilder2.AddFile(
            Path.Combine(AppContext.BaseDirectory, LinuxPackConstants.FileNameDesktop),
            LinuxPackConstants.FileNameDesktop,
            LinuxPackConstants.Prefix,
            archiveEntries,
            taskItems);
    }

    static PgpKeyRingGenerator? krgen;
    static PgpSecretKeyRing? secretKeyRing;
    static PgpPrivateKey? privateKey;

    static readonly Lazy<bool> init = new(() =>
    {
        try
        {
            krgen = PgpSigner.GenerateKeyRingGenerator("dotnet", "dotnet");
            secretKeyRing = krgen.GenerateSecretKeyRing();
            privateKey = secretKeyRing.GetSecretKey().ExtractPrivateKey("dotnet".ToCharArray());
            return true;
        }
        catch
        {
            return false;
        }
    });

    static void GenerateRpmPackage(AppPublishInfo item)
    {
        if (!init.Value) return;

        var rpmPath = GetPackPath(item, FileEx.RPM);
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
           item.DirectoryPath,
           Constants.HARDCODED_APP_NAME,
           LinuxPackConstants.Prefix,
           Array.Empty<ITaskItem>());

        AddFileNameDesktop(archiveBuilder2, archiveEntries);

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
            dependencies.Add(new PackageDependency { Name = LinuxPackConstants.aspnetcore_runtime });
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
            GetVersion(),
            RpmTask.GetPackageArchitecture(item.RuntimeIdentifier),
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
}