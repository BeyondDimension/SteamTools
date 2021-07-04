#if DEBUG
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace System
{
    static class LinkHelpers
    {
        public static void Handle()
        {
            var src_path = Path.Combine(ProjectPathUtil.projPath, "src");
            var bridge_app_path = Path.Combine(ProjectPathUtil.projPath, "src", ProjectPathUtil.ProjectDir_AvaloniaAppBridge);
            var bridge_console_path = Path.Combine(ProjectPathUtil.projPath, "src", ProjectPathUtil.ProjectDir_ConsoleAppBridge);
            var bridge_app_pack_path = Path.Combine(ProjectPathUtil.projPath, "src", ProjectPathUtil.ProjectDir_ConsoleAppBridgePackage);
            if (Directory.Exists(bridge_app_path) && Directory.Exists(bridge_console_path))
            {
                var debug_pub_path = @"\bin\Debug\Publish\win10-x64";
                var release_pub_path = @"\bin\Release\Publish\win10-x64";

                StringBuilder s = new();
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

                Dictionary<bool, string> dict = new()
                {
                    { true, debug_pub_path },
                    { false, release_pub_path },
                };

                foreach (var item in dict)
                {
                    var bridge_app_path2 = bridge_app_path + item.Value;
                    var bridge_console_path2 = bridge_console_path + item.Value;
                    if (Directory.Exists(bridge_app_path2) && Directory.Exists(bridge_console_path2))
                    {
                        s.AppendLine("  <ItemGroup Condition=\"'$(Configuration)|$(Platform)'=='" + (item.Key ? "Debug" : "Release") + "|x64'\">");

                        EachDirs(bridge_app_path2, bridge_app_path2);
                        EachFiles(bridge_console_path2, Directory.GetFiles(bridge_console_path2, "Steam++.Console*"));

                        s.AppendLine("  </ItemGroup>");
                    }
                }

                var value = s.ToString();
                Clipboard.SetText(value);
                return;
            }
        }
    }
}
#endif