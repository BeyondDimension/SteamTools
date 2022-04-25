#pragma warning disable SA1516 // Elements should be separated by blank line
using System;
using System.IO;
using System.Linq;
using System.Text;

const bool isDebugOrRelease = false;
const string platform = "x64";
const string configuration = isDebugOrRelease ? "Debug" : "Release";

string mark = $"<!--ST.Tools.DesktopBridgeLink({configuration}-{platform})-->";
//const string Mark_CEF_x64 = "<!--CEF(x64)-->";

var src_path = Path.Combine(ProjectPathUtil.projPath, "src");
var bridge_app_path = Path.Combine(ProjectPathUtil.projPath, "src", ProjectPathUtil.ProjectDir_AvaloniaAppBridge);
var bridge_console_path = Path.Combine(ProjectPathUtil.projPath, "src", ProjectPathUtil.ProjectDir_ConsoleAppBridge);
var bridge_app_pack_path = Path.Combine(ProjectPathUtil.projPath, "src", ProjectPathUtil.ProjectDir_ConsoleAppBridgePackage);
if (Directory.Exists(bridge_app_path) && Directory.Exists(bridge_console_path))
{
    var pub_path = $@"\bin\{configuration}\Publish\win10-{platform}";

    var bridgeProjPath = ProjectPathUtil.projPath + Path.DirectorySeparatorChar + "src" + Path.DirectorySeparatorChar + "ST.Client.Desktop.Avalonia.App.Bridge.Package";
    var bridgeProjFilePath = bridgeProjPath + Path.DirectorySeparatorChar + "ST.Client.Desktop.Avalonia.App.Bridge.Package.wapproj";
    var csprojContent = File.ReadAllText(bridgeProjFilePath);

    var csprojContentLines = csprojContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

    StringBuilder s = new();
    var index = Array.FindIndex(csprojContentLines, x => x.Trim() == mark);
    if (index < 0)
    {
        Console.WriteLine("Cannot find mark.");
        Console.ReadKey();
        return;
    }
    var top = csprojContentLines.Take(index).ToArray();
    var bottom = csprojContentLines.Skip(index).ToArray();
    index = Array.FindIndex(bottom, x => x.Trim() == "</ItemGroup>");
    bottom = bottom.Skip(index + 1).ToArray();

    Array.ForEach(top, x => s.AppendLine(x));
    s.Append("  ").AppendLine(mark);
    var bridge_app_path2 = bridge_app_path + pub_path;
    //var bridge_console_path2 = bridge_console_path + pub_path;
    if (Directory.Exists(bridge_app_path2)/* && Directory.Exists(bridge_console_path2)*/)
    {
        s.AppendLine("  <ItemGroup Condition=\"'$(Configuration)|$(Platform)'=='" + configuration + "|" + (platform.StartsWith("arm") ? platform.ToUpper() : platform) + "'\">");

        EachDirs(bridge_app_path2, bridge_app_path2);
        //EachFiles(bridge_console_path2, Directory.GetFiles(bridge_console_path2, "Steam++.Console*"));

        s.AppendLine("  </ItemGroup>");
    }

    void EachDirs(string rootPath, params string[] dirs)
    {
        foreach (var dir in dirs)
        {
            var files = Directory.GetFiles(dir);
            EachFiles(rootPath, files);
            var dirs_ = Directory.GetDirectories(dir);
            var ignoreRootDirNames = new[] { IOPath.DirName_AppData, IOPath.DirName_Cache, "Logs" };
            EachDirs(rootPath, dirs_.Where(x => !ignoreRootDirNames.Contains(new DirectoryInfo(x).Name)).ToArray());
        }
    }

    void EachFiles(string rootPath, params string[] files)
    {
        foreach (var file in files)
        {
            if (Path.GetExtension(file).Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
                Path.GetExtension(file).Equals(".pdb", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var includePath = ".." + file.Substring(src_path.Length, file.Length - src_path.Length);
            s.AppendFormat("    <Content Include=\"{0}\">", includePath);
            s.AppendLine();
            var linkPath = file.Substring(rootPath.Length, file.Length - rootPath.Length).Trim(Path.DirectorySeparatorChar);
            s.AppendFormat("      <Link>{0}</Link>", linkPath);
            s.AppendLine();
            s.AppendLine("      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>");
            s.AppendLine("    </Content>");
        }
    }

    Array.ForEach(bottom, x => s.AppendLine(x));
    csprojContent = s.ToString();

    File.WriteAllText(bridgeProjFilePath, csprojContent);

    Console.WriteLine("OK");
}
#pragma warning restore SA1516 // Elements should be separated by blank line