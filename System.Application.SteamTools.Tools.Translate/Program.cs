using Microsoft.Extensions.DependencyInjection;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 自动翻译工具
    /// </summary>
    class Program
    {
        static readonly string[] langs = new[] {
            "en",
            "ja",
            "ru",
            "zh-Hant",
        };

        static readonly string projPath = GetProjectPath();

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
        }

        static async Task Main(string[] args)
        {
            ReadAzureTranslationKey();

            DI.Init(ConfigureServices);

            var r = await Translatecs.TranslateTextAsync(route + to_ + "en", "测试翻译文本");

            //await Handle(@"System.Common.CoreLib\Properties\SR");

            Console.ReadLine();
        }

        static void ReadAzureTranslationKey()
        {
            var azure_translation_key = Path.Combine(projPath, "azure-translation-key.pfx");
            if (!File.Exists(azure_translation_key)) throw new FileNotFoundException(azure_translation_key);
            var text = File.ReadAllText(azure_translation_key);
            var items = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (items.Length != 3) throw new ArgumentOutOfRangeException();
            Translatecs.Settings = new TranslatecsSettings()
            {
                Key = items[0],
                Endpoint = items[1],
                Region = items[2],
            };
        }

        const string to_ = "&to=";
        const string route = "/translate?api-version=3.0&from=zh-Hans";

        static async Task Handle(string path, bool isReadOrWrite = false)
        {
            path = Path.Combine(projPath, path);
            if (!path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase)) path += ".resx";
            if (!File.Exists(path)) throw new FileNotFoundException(nameof(path));

            var value = ReadResx(path);
            var dict = new Dictionary<string, HashSet<string>>();

            foreach (var item in langs)
            {
                var itemPath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path) + $".{item}.resx";
                if (!File.Exists(itemPath)) continue;
                var itemValue = ReadResx(itemPath);
                dict.Add(item, new HashSet<string>(itemValue.Keys));
            }

            dict = dict.ToDictionary(x => x.Key, v => new HashSet<string>(value.Keys.Except(v.Value)));

            foreach (var item in value)
            {
                foreach (var item2 in dict)
                {
                    if (item2.Value.Contains(item.Key)) continue;
                    item2.Value.Add(item.Key);
                }
            }

            var fileName = path.Replace(Path.DirectorySeparatorChar, '_');
            var excelFilePath = Path.Combine(AppContext.BaseDirectory, fileName + $".xlsx");

            // 读取未翻译的键值，使用 translate 翻译后 导出 excel [审阅]后再导入

            if (isReadOrWrite)
            {
            }
            else
            {
                var allKeys = dict.Values.SelectMany(x => x).Distinct().ToArray();
                if (!allKeys.Any()) return;
                IOPath.FileIfExistsItDelete(excelFilePath);
                using var fs = File.Create(excelFilePath);
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("sheet");
                var index = 0;
                var row = sheet.CreateRow(index++);
                var cell = row.CreateCell(0);
                cell.SetCellValue("Key");
                cell = row.CreateCell(1);
                cell.SetCellValue("Value");
                var index_langs = 0;
                var dict_langs_cell = new Dictionary<string, int>();
                foreach (var item in langs)
                {
                    var index_langs_cell = 2 + index_langs++;
                    cell = row.CreateCell(index_langs_cell);
                    cell.SetCellValue(item);
                    dict_langs_cell.Add(item, index_langs_cell);
                }
                cell.SetCellValue("zh-Hans");
                foreach (var itemKey in allKeys)
                {
                    var inputText = value[itemKey];

                    row = sheet.CreateRow(index++);
                    cell = row.CreateCell(0);
                    cell.SetCellValue(itemKey);
                    cell = row.CreateCell(1);
                    cell.SetCellValue(value[itemKey]);

                    var query = dict.Where(x => x.Value.Contains(itemKey)).Select(x => x.Key);
                    var url = route + to_ + string.Join(to_, query);
                    var r = await Translatecs.TranslateTextAsync(url, inputText);
                }

                //foreach (var item in dict)
                //{
                //    var excelFilePath = Path.Combine(AppContext.BaseDirectory, fileName + $".{item.Key}.xlsx");
                //    IOPath.FileIfExistsItDelete(excelFilePath);
                //    using var fs = File.Create(excelFilePath);
                //    var workbook = new XSSFWorkbook();
                //    var sheet = workbook.CreateSheet("sheet");
                //    var index = 0;
                //    foreach (var item2 in item.Value)
                //    {
                //        var row = sheet.CreateRow(index++);
                //        var cell = row.CreateCell(0);
                //        cell.SetCellValue(item2.Value);
                //    }
                //    workbook.Write(fs);
                //}
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