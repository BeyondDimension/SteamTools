using System;
using System.IO;
using System.Linq;
using System.Text;

const string Mark = "<!--ST.Tools.AndroidResourceLink-->";
var resPath = ProjectPathUtil.projPath + Path.DirectorySeparatorChar + "src" + Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar, new[] { "ST.Client.Mobile.Droid.Design", "ui", "src", "main", "res" });
var androidProjPath = ProjectPathUtil.projPath + Path.DirectorySeparatorChar + "src" + Path.DirectorySeparatorChar + "ST.Client.Mobile.Droid";
var androidProjFilePath = androidProjPath + Path.DirectorySeparatorChar + "ST.Client.Mobile.Droid.csproj";

var csprojContent = File.ReadAllText(androidProjFilePath);
var csprojContentLines = csprojContent.Split(Environment.NewLine);
var index = Array.FindIndex(csprojContentLines, x => x.Trim() == Mark);
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

var files = Directory.GetDirectories(resPath).Select(Directory.GetFiles).SelectMany(x => x).Where(x => !x.Contains("__dont_link")).ToArray();
var sb = new StringBuilder();
Array.ForEach(top, x => sb.AppendLine(x));
sb.Append("  ").AppendLine(Mark).AppendLine("  <ItemGroup>");
foreach (var file in files)
{
    var include = Path.GetRelativePath(androidProjPath, file);
    var link = Path.GetRelativePath(androidProjPath, resPath);
    link = "Resources" + include.Substring(link.Length, include.Length - link.Length);
    var tag = link.Contains("\\layout") /*&& !link.Contains("\\shared_") && !link.Contains("_content.xml")*/ ? "AndroidBoundLayout" : "AndroidResource";
    sb.AppendFormat("    <{1} Include=\"{0}\">", include, tag).AppendLine()
        .AppendFormat("    <Link>{0}</Link>", link).AppendLine()
        .AppendFormat("    </{0}>", tag).AppendLine();
}
sb.AppendLine("  </ItemGroup>");
Array.ForEach(bottom, x => sb.AppendLine(x));
csprojContent = sb.ToString();
File.WriteAllText(androidProjFilePath, csprojContent);

Console.WriteLine("OK");