using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

var matchEx = new[] { ".htm", ".html" };
var files = Directory.GetFiles(AppContext.BaseDirectory, "*.*", SearchOption.AllDirectories).Where(x => matchEx.Any(y => x.EndsWith(y, StringComparison.OrdinalIgnoreCase)));
if (files.Any())
{
    foreach (var file in files)
    {
        var fileName = Path.GetFileName(file);
        var source = File.ReadAllText(file);
        var content = source;
        content = Regex.Replace(content, @"\n|\t", " ");
        content = Regex.Replace(content, @">\s+<", "><").Trim();
        content = Regex.Replace(content, @"\s{2,}", " ");
        var span = content.AsSpan();
        var umiVerIndex = span.IndexOf("<script> //! umi version:");
        if (umiVerIndex > 0)
        {
            var umiVerAfterStr = span[umiVerIndex..];
            var scriptEnd = "</script>";
            var umiVerIndexLast = umiVerAfterStr.IndexOf(scriptEnd);
            if (umiVerIndexLast < 0)
            {
                Console.WriteLine("Invalid file, fileName: {fileName}");
                continue;
            }
            else
            {
                content = content.Substring(0, umiVerIndex) + umiVerAfterStr[(umiVerIndexLast + scriptEnd.Length)..].ToString();
            }
        }
        if (content != source)
        {
            File.WriteAllText(file, content);
            Console.WriteLine($"Minify, fileName: {fileName}");
        }
        else
        {
            Console.WriteLine($"Not changed, fileName: {fileName}");
        }
    }
}
else
{
    Console.WriteLine("File not found.");
}

Console.WriteLine("Complete.");