//using Microsoft.Extensions.DependencyInjection;
//using NPOI.XSSF.UserModel;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static System.ProjectPathUtil;

//// 已知BUG：疑似部分语言中存在键，其他语言中没有此键，则不会自动翻译？

//namespace System
//{
//    /// <summary>
//    /// Resx自动翻译工具
//    /// <para>扫描缺失的键与对应的语言</para>
//    /// <para>将缺失的值使用翻译API进行翻译后导出Excel</para>
//    /// <para>人工审阅与校对</para>
//    /// <para>从Excel中读取翻译结果自动插入resx文件中</para>
//    /// </summary>
//    static class Program
//    {
//        static readonly string[] langs = new[] {
//            "en",
//            "ja",
//            "ru",
//            "zh-Hant",
//            "ko",
//        };

//        static void ConfigureServices(IServiceCollection services)
//        {
//            services.AddHttpClient();
//        }

//        public const string CoreLib = ProjectDir_CoreLib + @"\Properties\SR";
//        public const string ClienDroidLib = ProjectDir_ClienDroidLib + @"\Properties\SR";
//        public const string ST = ProjectDir_ST + @"\Properties\SR";
//        public const string STClient = ProjectDir_STClient + @"\Properties\SR";
//        public const string STClientDesktop = ProjectDir_ClientDesktop + @"\Properties\SR";
//        public const string AppRes = ProjectDir_ClientDesktop + @"\UI\Resx\AppResources";
        
//        static async Task Main(string[] args)
//        {
//            ReadAzureTranslationKey();

//            DI.Init(ConfigureServices);

//            //var r = await Translatecs.TranslateTextAsync(route + to_ + "en", "测试翻译文本");

//            // 不带后缀的相对路径
//            var resx_path = STClientDesktop;

//            // true 读取翻译后的excel写入resx
//            // false 读取resx机翻后写入excel
//            var isReadOrWrite = true;

//            // 读取翻译的excel值 是否覆盖已有的resx值？
//            var isOverwrite = false;

//            await Handle(resx_path, isReadOrWrite, isOverwrite);

//            Console.WriteLine("OK");
//            Console.ReadLine();
//        }

//        static void ReadAzureTranslationKey()
//        {
//            var azure_translation_key = Path.Combine(projPath, "azure-translation-key.pfx");
//            if (!File.Exists(azure_translation_key)) throw new FileNotFoundException(azure_translation_key);
//            var text = File.ReadAllText(azure_translation_key);
//            var items = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
//            if (items.Length != 3) throw new ArgumentOutOfRangeException();
//            Translatecs.Settings = new TranslatecsSettings()
//            {
//                Key = items[0],
//                Endpoint = items[1],
//                Region = items[2],
//            };
//        }

//        const string to_ = "&to=";
//        const string route = "https://api.translator.azure.cn/translate?api-version=3.0&from=zh-Hans";

//        static async Task Handle(string path, bool isReadOrWrite, bool isOverwrite)
//        {
//            path = Path.Combine(projPath, "src", path);
//            if (!path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase)) path += ".resx";
//            if (!File.Exists(path)) throw new FileNotFoundException(nameof(path));

//            var path_r = Path.GetRelativePath(projPath, path);
//            var fileName = path_r.Replace(Path.DirectorySeparatorChar, '_');
//            var excelFilePath = Path.Combine(AppContext.BaseDirectory, fileName + ".xlsx");

//            // 读取未翻译的键值，使用 translate 翻译后 导出 excel [审阅]后再导入

//            if (isReadOrWrite)
//            {
//                if (!File.Exists(excelFilePath)) throw new FileNotFoundException(nameof(excelFilePath));
//                using var fs = File.OpenRead(excelFilePath);
//                var workbook = new XSSFWorkbook(fs);
//                var sheet = workbook.GetSheet("sheet");
//                int index_row = 0;
//                var headerRow = sheet.GetRow(index_row++);
//                int index_cell = 0;
//                var key_col_index = 0;
//                var dict_langs = new Dictionary<int, string>();
//                while (true)
//                {
//                    var index = index_cell++;
//                    var cell = headerRow.GetCell(index);
//                    if (cell == null) break;
//                    var cellValue = cell.StringCellValue;
//                    if (cellValue == "Key")
//                    {
//                        key_col_index = index;
//                    }
//                    else if (langs.Contains(cellValue))
//                    {
//                        dict_langs.Add(index, cellValue);
//                    }
//                }

//                var dict = new Dictionary<string, Dictionary<string, string>>();

//                foreach (var item in langs)
//                {
//                    dict.Add(item, new Dictionary<string, string>());
//                }

//                while (true)
//                {
//                    var row = sheet.GetRow(index_row++);
//                    if (row == null) break;

//                    index_cell = 0;

//                    while (true)
//                    {
//                        var index = index_cell++;
//                        var cell = row.GetCell(index);
//                        if (cell == null) break;
//                        if (dict_langs.ContainsKey(index))
//                        {
//                            var key = row.GetCell(key_col_index).StringCellValue;
//                            var value = cell.StringCellValue;
//                            dict[dict_langs[index]].Add(key, value);
//                        }
//                    }

//                    index_cell = 0;
//                }

//                foreach (var item in dict)
//                {
//                    var itemPath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path) + $".{item.Key}.resx";
//                    var itemValue = ReadResx(itemPath);

//                    var fileText = File.ReadAllText(itemPath);

//                    var @string = new StringBuilder();

//                    foreach (var kv_item in item.Value)
//                    {
//                        if (itemValue.ContainsKey(kv_item.Key))
//                        {
//                            // 已存在
//                            if (isOverwrite)
//                            {
//                                throw new NotImplementedException();
//                            }
//                        }
//                        else
//                        {
//                            @string.AppendFormatLine(
//                                "  <data name=\"{0}\" xml:space=\"preserve\">", kv_item.Key);
//                            @string.AppendFormatLine("    <value>{0}</value>", kv_item.Value);
//                            @string.AppendLine("  </data>");
//                        }
//                    }

//                    const string end_mark = "</root>";

//                    if (@string.Length > 0)
//                    {
//                        @string.Append(end_mark);
//                        var newStr = @string.ToString();
//                        fileText = fileText.Replace(end_mark, newStr);
//                        File.WriteAllText(itemPath, fileText);
//                    }
//                }
//            }
//            else
//            {
//                var value = ReadResx(path);
//                var dict = new Dictionary<string, HashSet<string>>();

//                foreach (var item in langs)
//                {
//                    var itemPath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path) + $".{item}.resx";
//                    if (!File.Exists(itemPath)) continue;
//                    var itemValue = ReadResx(itemPath);
//                    dict.Add(item, new HashSet<string>(itemValue.Keys));
//                }

//                dict = dict.ToDictionary(x => x.Key, v => new HashSet<string>(value.Keys.Except(v.Value)));

//                foreach (var item in value)
//                {
//                    foreach (var item2 in dict)
//                    {
//                        if (item2.Value.Contains(item.Key)) continue;
//                        item2.Value.Add(item.Key);
//                    }
//                }

//                var allKeys = dict.Values.SelectMany(x => x).Distinct().ToArray();
//                if (!allKeys.Any()) return;
//                IOPath.FileIfExistsItDelete(excelFilePath);
//                using var fs = File.Create(excelFilePath);
//                var workbook = new XSSFWorkbook();
//                var sheet = workbook.CreateSheet("sheet");
//                var index = 0;
//                var row = sheet.CreateRow(index++);
//                var cell = row.CreateCell(0);
//                cell.SetCellValue("Key");
//                cell = row.CreateCell(1);
//                cell.SetCellValue("Value");
//                var index_langs = 0;
//                var dict_langs_cell = new Dictionary<string, int>();
//                foreach (var item in langs)
//                {
//                    var index_langs_cell = 2 + index_langs++;
//                    cell = row.CreateCell(index_langs_cell);
//                    cell.SetCellValue(item);
//                    dict_langs_cell.Add(item, index_langs_cell);
//                }
//                foreach (var itemKey in allKeys)
//                {
//                    var inputText = value[itemKey];

//                    row = sheet.CreateRow(index++);
//                    cell = row.CreateCell(0);
//                    cell.SetCellValue(itemKey);
//                    cell = row.CreateCell(1);
//                    cell.SetCellValue(value[itemKey]);

//                    var query = dict.Where(x => x.Value.Contains(itemKey)).Select(x => x.Key);
//                    var url = route + to_ + string.Join(to_, query);
//                    var translationResults = await Translatecs.TranslateTextAsync(url, inputText);
//                    var translationResult = translationResults.FirstOrDefault(x => x != null);

//                    Console.WriteLine($"正在翻译：{itemKey}");

//                    foreach (var translation in translationResult.Translations)
//                    {
//                        var cell_t_index = dict_langs_cell[translation.To];
//                        cell = row.CreateCell(cell_t_index);
//                        cell.SetCellValue(translation.Text);
//                        Console.WriteLine($"to：{translation.To}");
//                        Console.WriteLine($"value：{translation.Text}");
//                    }
//                }
//                workbook.Write(fs);
//            }
//        }

//        static IDictionary<string, string> ReadResx(string path)
//        {
//            var dict = new Dictionary<string, string>();
//            using var reader = File.OpenText(path);
//            string? line;
//            var lastName = (string?)null;
//            do
//            {
//                line = reader.ReadLine();
//                if (line == null) break;
//                if (line.Contains("<data") && line.Contains("xml:space=\"preserve\""))
//                {
//                    lastName = line.Substring("name=\"", "\"");
//                }
//                else if (lastName != null && line.Contains("<value>"))
//                {
//                    var value = line.Substring("<value>", "</value>");
//                    if (Is_zh_Hans(value))
//                    {
//                        if (lastName != "ProgramUpdateCmd_" && lastName != "VacFixCmd")
//                        {
//                            dict.Add(lastName, value);
//                        }
//                    }
//                }
//            } while (line != null);
//            return dict;
//        }

//        public static bool Is_zh_Hans(string? input) => input?.Any(Is_zh_Hans) ?? false;

//        public static bool Is_zh_Hans(char input) => input >= 0x4e00 && input <= 0x9fbb;
//    }
//}