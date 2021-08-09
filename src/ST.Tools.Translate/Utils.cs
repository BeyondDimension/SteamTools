using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Constants;
using static System.ProjectPathUtil;
using R = System.Properties.Resources;

namespace System
{
    static class Utils
    {
        static string GetResxFilePathCore(params string[] dirs) => Path.Combine(projPath, "src") + Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar, dirs);

        /// <summary>
        /// 根据[有 resx 文件的项目名]获取文件路径
        /// </summary>
        /// <param name="resx"></param>
        /// <returns></returns>
        public static string GetResxFilePath(string resx) => resx switch
        {
            ClientLibDroid => GetResxFilePathCore(new[] { resx, "Application", "Properties", "SR.resx" }),
            CoreLib => GetResxFilePathCore(new[] { resx, "Properties", "SR.resx" }),
            ST => GetResxFilePathCore(new[] { resx, "Properties", "SR.resx" }),
            STClient => GetResxFilePathCore(new[] { resx, "Properties", "SR.resx" }),
            STClientDesktop => GetResxFilePathCore(new[] { resx, "Properties", "SR.resx" }),
            STClientDesktop_AppResources => GetResxFilePathCore(new[] { STClientDesktop, "UI", "Resx", "AppResources.resx" }),
            STServicesCloudServiceModels => GetResxFilePathCore(new[] { resx, "Properties", "SR.resx" }),
            STToolsWin7Troubleshoot => GetResxFilePathCore(new[] { resx, "Properties", "SR.resx" }),
            _ => throw new ArgumentOutOfRangeException(nameof(resx), resx, null),
        };

        /// <summary>
        /// 验证传入参数是否正确
        /// </summary>
        /// <param name="args"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static async Task<bool> ValidateAsync<TCommandArguments>(TCommandArguments args, Func<TCommandArguments, Task<IList<string>?>> handler) where TCommandArguments : CommandArguments
        {
            if (string.IsNullOrWhiteSpace(args.resx))
            {
                Console.WriteLine("resx path error.");
                return false;
            }
            if (args.resx.Equals(All, StringComparison.OrdinalIgnoreCase))
            {
                args.resx = All;
            }
            else
            {
                var resx = args.resx;
                resx = resxs.FirstOrDefault(x => x.Equals(resx, StringComparison.OrdinalIgnoreCase));
                if (resx != null)
                {
                    args.resx = GetResxFilePath(resx);
                }
                if (!args.resx.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("resx path incorrect.");
                    return false;
                }
                if (!File.Exists(args.resx))
                {
                    Console.WriteLine("resx path not found.");
                    return false;
                }
            }
            if (string.IsNullOrWhiteSpace(args.lang))
            {
                Console.WriteLine("lang value error.");
                return false;
            }
            if (args.lang.Equals(All, StringComparison.OrdinalIgnoreCase))
            {
                args.lang = All;
            }
            else
            {
                var lang = args.lang;
                lang = langs.FirstOrDefault(x => x.Equals(lang, StringComparison.OrdinalIgnoreCase));
                if (lang == null)
                {
                    Console.WriteLine("lang value incorrect.");
                    return false;
                }
                args.lang = lang;
            }
            var isAllLang = args.lang == All;
            List<string> messages_all = new();
            if (args.resx == All)
            {
                foreach (var resxFilePath in resxs)
                {
                    if (isAllLang)
                    {
                        foreach (var lang in langs)
                        {
                            args.resxFilePath = GetResxFilePath(resxFilePath);
                            args.lang = lang;
                            var messages = await handler(args);
                            if (messages.Any_Nullable()) messages_all.AddRange(messages!);
                        }
                    }
                    else
                    {
                        args.resxFilePath = GetResxFilePath(resxFilePath);
                        var messages = await handler(args);
                        if (messages.Any_Nullable()) messages_all.AddRange(messages!);
                    }
                }
            }
            else
            {
                if (isAllLang)
                {
                    foreach (var lang in langs)
                    {
                        args.resxFilePath = args.resx;
                        args.lang = lang;
                        var messages = await handler(args);
                        if (messages.Any_Nullable()) messages_all.AddRange(messages!);
                    }
                }
                else
                {
                    args.resxFilePath = args.resx;
                    var messages = await handler(args);
                    if (messages.Any_Nullable()) messages_all.AddRange(messages!);
                }
            }

            Console.WriteLine("OK");

            if (messages_all.Any())
                foreach (var message in messages_all)
                    Console.WriteLine(message);

            return true;
        }

        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }

        static readonly string[] IgnoreKeys = new[]
        {
            "ProgramUpdateCmd_",
            "VacFixCmd",
        };

        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> item) => AddOrReplace(dict, item.Key, item.Value);

        public static string Serialize(Dictionary<string, string> dict)
        {
            return string.Join(';', dict.Select(x => $"{x.Key}={x.Value}"));
        }

        public static Dictionary<string, string> Deserialize(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return new Dictionary<string, string>
                {
                    { AuthorKey, MicrosoftTranslator },
                };
            }
            else
            {
                if (!comment.Contains(';') && !comment.Contains('；') && !comment.Contains('='))
                {
                    return new Dictionary<string, string>
                    {
                        { CommentKey, comment },
                        { AuthorKey, string.Empty },
                    };
                }
                var array = comment.Split(new char[] { '；', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var dict = array.Select(x => x.Split('=', StringSplitOptions.RemoveEmptyEntries).ToArray()).Where(x => x.Length == 2).ToDictionary(x => x[0], x => x[1]);
                if (!dict.ContainsKey(AuthorKey))
                {
                    dict.Add(AuthorKey, MicrosoftTranslator);
                }
                return dict;
            }
        }

        public static (StringBuilder start, StringBuilder end, Dictionary<string, (string value, string comment)> dict) GetResxDict(
            string resxFilePath,
            string[]? ignoreKeys = null,
            bool ignoreStringBuilder = false)
        {
            ignoreKeys ??= IgnoreKeys;
            Dictionary<string, (string value, string comment)> dict = new();
            using var sr = File.OpenText(resxFilePath);
            StringBuilder? start = ignoreStringBuilder ? null : new();
            StringBuilder? end = ignoreStringBuilder ? null : new();
            string? line;
            string? key = null, value = string.Empty, comment = string.Empty;
            int lineNum = 0;
            StringBuilder? sb = start;
            do
            {
                lineNum++;
                line = sr.ReadLine();
                if (line == null) break;
                var lineTrim = line.Trim();
                if (key != null)
                {
                    if (lineTrim.StartsWith(ValueXmlStart) && lineTrim.EndsWith(ValueXmlEnd))
                    {
                        value = line.Substring(ValueXmlStart, ValueXmlEnd);
                        value = Unescape(value);
                        AddOrReplace(dict, key, (value, comment));
                        continue;
                    }
                    else if (lineTrim.StartsWith(CommentXmlStart) && lineTrim.EndsWith(CommentXmlEnd))
                    {
                        comment = line.Substring(CommentXmlStart, CommentXmlEnd);
                        comment = Unescape(comment);
                        AddOrReplace(dict, key, (value, comment));
                        continue;
                    }
                }
                if (lineTrim.StartsWith(DataXmlStart) && lineTrim.EndsWith(DataXmlEnd))
                {
                    comment = string.Empty;
                    value = string.Empty;
                    key = lineTrim.Substring(DataXmlStart, DataXmlEnd);
                    if (ignoreKeys.Contains(key))
                    {
                        key = null;
                        goto while_end;
                    }
                    sb = end;
                    continue;
                }
            while_end: sb?.AppendLine(line);
            } while (true);
            return (start!, end!, dict);
        }

        public static void ReadAzureTranslationKey()
        {
            if (Translatecs.Settings != null) return;
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

        public static string GetXlsxFilePath(string resxFilePathLang, string lang)
        {
            var path_r = Path.GetRelativePath(projPath, resxFilePathLang);
            var fileName = path_r.Replace(Path.DirectorySeparatorChar, '_');
            var dirPath = Path.Combine(AppContext.BaseDirectory, "Xlsx", lang);
            IOPath.DirCreateByNotExists(dirPath);
            var excelFilePath = Path.Combine(dirPath, fileName + ".xlsx");
            return excelFilePath;
        }

        public static void ResxFileLangCreateByNotExists(string resxFilePathLang)
        {
            if (!File.Exists(resxFilePathLang))
            {
                File.WriteAllText(resxFilePathLang, R.Resx);
            }
        }

        public static string GetResxFilePathLang(CommandArguments args)
        {
            var resxFilePathLang = args.resxFilePath.TrimEnd(".resx", StringComparison.OrdinalIgnoreCase) + $".{args.lang}.resx";
            return resxFilePathLang;
        }

        /// <summary>
        /// 读取 Xlsx 文件
        /// </summary>
        /// <param name="originalDict">中文原文字典数据</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="messages">错误消息</param>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, (string value, string comment)>? ReadXlsx(IReadOnlyDictionary<string, (string value, string comment)>? originalDict, string filePath, string author, out IList<string> messages)
        {
            messages = new List<string>();
            using var fs = File.OpenRead(filePath);
            var workbook = new XSSFWorkbook(fs);
            if (workbook.NumberOfSheets <= 0) return null;
            var sheet = workbook.GetSheetAt(0);
            IRow row;
            ICell cell;
            int index_row = 0, index_cell = 0;

            bool keyIsFirst = false;
            do // 定位 Header
            {
                row = sheet.GetRow(index_row++);
                if (row == null) return null;
                cell = row.GetCell(0);
                keyIsFirst = cell.StringCellValue.Contains("标识符") ||
                    cell.StringCellValue.Contains(ColumnHeaderKey);
            } while (!(keyIsFirst ||
                    cell.StringCellValue.Contains("中文文本")));

            int index_cell_value = -1;
            var index_cell_original = -1;
            while (true) // 定位 Value Cell Index
            {
                var cellnum = index_cell++;
                cell = row.GetCell(cellnum);
                if (cell == null) return null;
                if (cell.StringCellValue.Contains("翻译文本") ||
                    cell.StringCellValue.Contains("人工翻译"))
                {
                    index_cell_value = cellnum;
                    break;
                }
                if (cell.StringCellValue.Contains("中文文本"))
                {
                    index_cell_original = cellnum;
                }
            }
            if (index_cell_value == -1 && index_cell_original == -1) return null;

            Dictionary<string, (string value, string comment)> dict = new();
            while (true) // 循环 Body 区域
            {
                row = sheet.GetRow(index_row++);
                if (row == null) break;
                var keyCell = row.GetCell(0);
                var valueCell = row.GetCell(index_cell_value);
                var commentCell = row.GetCell(index_cell_value + 1);
                var key = keyCell.StringCellValue;
                var value = valueCell.StringCellValue;
                if (!keyIsFirst)
                {
                    if (!originalDict.Any_Nullable())
                    {
                        messages.Add($"{Path.GetFileName(filePath)} 缺少 Key 标识，originalDict is null");
                        return null;
                    }
                    var originalValue = row.GetCell(index_cell_original).StringCellValue;
                    var findKey = originalDict!.FirstOrDefault(x => x.Value.value == originalValue);
                    if (findKey.Key == default)
                    {
                        messages.Add($"{Path.GetFileName(filePath)} 缺少 Key 标识，使用 Value 在原文中查不到标识，原文值：{key}，译文值：{value}");
                    }
                    else
                    {
                        key = findKey.Key;
                    }
                }

                var comment = commentCell?.StringCellValue ?? string.Empty;
                if (!string.IsNullOrEmpty(author))
                {
                    var comment2 = Deserialize(comment);
                    comment2.AddOrReplace(AuthorKey, author);
                    comment = Serialize(comment2);
                }

                dict.AddOrReplace(key, (value, comment));
            }

            return dict.Any() ? dict : null;
        }

        public static string Escape(string str) => SecurityElement.Escape(str) ?? string.Empty;

        public static string Unescape(string str) => new SecurityElement(string.Empty, str).Text ?? string.Empty;
    }
}