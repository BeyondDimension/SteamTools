using BD.WTTS.Models;

const string OpenSourceLibraryListEmoji = "📄";

var path = Path.Combine(ProjectUtils.ProjPath, "doc", "open-source-library.md");

using var fs = IOPath.OpenRead(path, false);
using var sr = new StreamReader(fs!, Encoding.UTF8);

var list = new List<HyperlinkModel>();
var isOpenSourceLibraryList = false;
string? line;
do
{
    line = sr.ReadLine();
    if (line == null) break;
    if (line.StartsWith("#"))
    {
        if (isOpenSourceLibraryList)
        {
            break;
        }
        if (line.Contains(OpenSourceLibraryListEmoji) && (line.Contains("开源") || (line.Contains("Open") && line.Contains("Source"))))
        {
            isOpenSourceLibraryList = true;
        }
    }
    if (isOpenSourceLibraryList)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            if (line.StartsWith("* "))
            {
                var name = line.Substring("[", "]");
                if (string.IsNullOrWhiteSpace(line)) continue;
                var index = line.IndexOf("]");
                if (index < 0) continue;
                var url = line[index..].Substring("(", ")");
                if (string.IsNullOrWhiteSpace(url)) continue;
                var item = new HyperlinkModel(name, url);
                list.Add(item);
            }
        }
    }
}
while (true);

var bytes = Serializable.SMP2(list.OrderBy(x => x.Text).ToArray());
File.WriteAllBytes(path.TrimEnd(".md") + ".mpo", bytes);