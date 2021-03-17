using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System
{
    /// <summary>
    /// 半自动翻译工具
    /// </summary>
    class Program
    {
        static readonly string[] langs = new[] {
            "en",
            "ja",
            "ru",
            "zh-Hant",
        };

        static readonly string projectPath = GetProjectPath();

        static void Main(string[] args)
        {
            Handle(@"System.Common.CoreLib\Properties\SR");

            Console.ReadLine();
        }

        static void Handle(string path, bool isReadOrWrite = false)
        {
            var fileName = path.Replace(Path.DirectorySeparatorChar, '_');
            path = Path.Combine(projectPath, path);
            if (!path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase)) path += ".resx";
            if (!File.Exists(path)) throw new FileNotFoundException(nameof(path));

            var value = ReadResx(path);
            var dict = new Dictionary<string, IDictionary<string, string>>();

            foreach (var item in langs)
            {
                var itemPath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path) + $".{item}.resx";
                if (!File.Exists(itemPath)) continue;
                var itemValue = ReadResx(itemPath);
                dict.Add(item, itemValue);
            }

            foreach (var item in value)
            {
                foreach (var item2 in dict)
                {
                    if (item2.Value.ContainsKey(item.Key)) continue;
                    item2.Value.Add(item.Key, item.Value);
                }
            }

            // 读取未翻译的键值，导出 excel 使用 translate 后再导入

            if (isReadOrWrite)
            {
            }
            else
            {
                foreach (var item in dict)
                {
                    var excelFilePath = Path.Combine(AppContext.BaseDirectory, fileName + $".{item.Key}.xlsx");
                    IOPath.FileIfExistsItDelete(excelFilePath);
                    using var fs = File.Create(excelFilePath);
                    var workbook = new XSSFWorkbook();
                    var sheet = workbook.CreateSheet("sheet");
                    var index = 0;
                    foreach (var item2 in item.Value)
                    {
                        var row = sheet.CreateRow(index++);
                        var cell = row.CreateCell(0);
                        cell.SetCellValue(item2.Value);
                    }
                    workbook.Write(fs);
                }
            }
        }

        static IDictionary<string, string> ReadResx(string path)
        {
            var dict = new Dictionary<string, string>();
            using var reader = File.OpenText(path);
            string? line;
            var lastName = (string?)null;
            do
            {
                line = reader.ReadLine();
                if (line == null) break;
                if (line.Contains("<data") && line.Contains("xml:space=\"preserve\""))
                {
                    lastName = line.Substring("name=\"", "\"");
                }
                else if (lastName != null && line.Contains("<value>"))
                {
                    var value = line.Substring("<value>", "</value>");
                    if (Is_zh_Hans(value))
                    {
                        dict.Add(lastName, value);
                    }
                }
            } while (line != null);
            return dict;
        }

        static string GetProjectPath(string? path = null)
        {
            path ??= AppContext.BaseDirectory;
            if (!Directory.GetFiles(path, "*.sln").Any())
            {
                var parent = Directory.GetParent(path);
                if (parent == null) return string.Empty;
                return GetProjectPath(parent.FullName);
            }
            return path;
        }

        public static bool Is_zh_Hans(string? input) => input?.Any(Is_zh_Hans) ?? false;

        public static bool Is_zh_Hans(char input) => input >= 0x4e00 && input <= 0x9fbb;
    }
}