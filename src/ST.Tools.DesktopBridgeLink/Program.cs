using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

const string Mark_Debug_x64 = "<!--DesktopBridge.Bin(Debug_x64)-->";
const string Mark_Release_x64 = "<!--DesktopBridge.Bin(Release_x64)-->";
const string Mark_CEF_x64 = "<!--CEF(x64)-->";

var src_path = Path.Combine(ProjectPathUtil.projPath, "src");
var bridge_app_path = Path.Combine(ProjectPathUtil.projPath, "src", ProjectPathUtil.ProjectDir_AvaloniaAppBridge);
var bridge_console_path = Path.Combine(ProjectPathUtil.projPath, "src", ProjectPathUtil.ProjectDir_ConsoleAppBridge);
var bridge_app_pack_path = Path.Combine(ProjectPathUtil.projPath, "src", ProjectPathUtil.ProjectDir_ConsoleAppBridgePackage);
if (Directory.Exists(bridge_app_path) && Directory.Exists(bridge_console_path))
{
    var debug_pub_path = @"\bin\Debug\Publish\win10-x64";
    var release_pub_path = @"\bin\Release\Publish\win10-x64";

    var bridgeProjPath = ProjectPathUtil.projPath + Path.DirectorySeparatorChar + "src" + Path.DirectorySeparatorChar + "ST.Client.Desktop.Avalonia.App.Bridge.Package";
    var bridgeProjFilePath = bridgeProjPath + Path.DirectorySeparatorChar + "ST.Client.Desktop.Avalonia.App.Bridge.Package.wapproj";
    var csprojContent = File.ReadAllText(bridgeProjFilePath);

    var list = new List<Tuple<string, bool, string>>
                {
                    new (debug_pub_path, true, Mark_Debug_x64),
                    new (release_pub_path, false, Mark_Release_x64),
                };

    foreach (var item in list)
    {
        var csprojContentLines = csprojContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        StringBuilder s = new();
        var index = Array.FindIndex(csprojContentLines, x => x.Trim() == item.Item3);
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
        s.Append("  ").AppendLine(item.Item3);
        var bridge_app_path2 = bridge_app_path + item.Item1;
        var bridge_console_path2 = bridge_console_path + item.Item1;
        if (Directory.Exists(bridge_app_path2) && Directory.Exists(bridge_console_path2))
        {
            s.AppendLine("  <ItemGroup Condition=\"'$(Configuration)|$(Platform)'=='" + (item.Item2 ? "Debug" : "Release") + "|x64'\">");

            EachDirs(bridge_app_path2, bridge_app_path2);
            EachFiles(bridge_console_path2, Directory.GetFiles(bridge_console_path2, "Steam++.Console*"));

            s.AppendLine("  </ItemGroup>");
        }

        void EachDirs(string rootPath, params string[] dirs)
        {
            foreach (var dir in dirs)
            {
                var files = Directory.GetFiles(dir);
                EachFiles(rootPath, files);
                var dirs_ = Directory.GetDirectories(dir);
                EachDirs(rootPath, dirs_);
            }
        }

        void EachFiles(string rootPath, params string[] files)
        {
            foreach (var file in files)
            {
                var includePath = ".." + file.Substring(src_path.Length, file.Length - src_path.Length);
                s.AppendFormat("    <Content Include=\"{0}\">", includePath);
                s.AppendLine();
                var linkPath = "Bin" + file.Substring(rootPath.Length, file.Length - rootPath.Length);
                s.AppendFormat("      <Link>{0}</Link>", linkPath);
                s.AppendLine();
                s.AppendLine("      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>");
                s.AppendLine("    </Content>");
            }
        }

        Array.ForEach(bottom, x => s.AppendLine(x));
        csprojContent = s.ToString();
    }

    File.WriteAllText(bridgeProjFilePath, csprojContent);

    Console.WriteLine("OK");
}