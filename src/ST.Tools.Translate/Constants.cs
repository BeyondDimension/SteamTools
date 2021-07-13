using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.ProjectPathUtil;
using R = System.Properties.Resources;

namespace System
{
    static class Constants
    {
        public const string Title = "Resx翻译命令行工具(Resx Translation Command Line Tools/RTCLT)";

        /// <summary>
        /// 支持的语言区域名
        /// <para>https://docs.microsoft.com/zh-cn/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c</para>
        /// </summary>
        public static readonly string[] langs = new[] {
            "zh-Hant", // 繁体中文(Traditional Chinese)
            "en", // 英语(English)
            "ko", // 韩语(Koreana)
            "ja", // 日语(Japanese)
            "ru", // 俄语(Russian)
            "es", // 西班牙语(Spanish)
            "it", // 意大利语(Italian)
        };

        public const string All = "all";
        public const char Separator = '-';
        public const string ColumnHeaderKey = "键(Key)";
        public const int KeyWidth = 30 * 256;
        public const string ColumnHeaderValue = "值(Value)";
        public const int ValueWidth = 80 * 256;
        public const string ColumnHeaderAuthor = "作者(Author)";
        public const string AuthorKey = "Author";
        public const string ColumnHeaderComment = "注释(Comment)";
        public const string CommentKey = "Comment";
        public const string MicrosoftTranslator = "MicrosoftTranslator";
        public const string ColumnHeaderMachineTranslation = "机翻(Machine Translation)";
        public const string ColumnHeaderHumanTranslation = "人工翻译(Human Translation)";
        public const string ColumnHeaderMachineProofread = "机翻校对(Machine Proofread)";
        public const string ResxDesc = "指定 resx 文件路径或项目名";
        public const string LangDesc = "指定要生成的语言，多选或单选，使用分号分割，all 表示全选";

        const string ClientLibDroid = "Common.ClientLib.Droid";
        const string CoreLib = "Common.CoreLib";
        const string ST = "ST";
        const string STClient = "ST.Client";
        const string STClientDesktop = "ST.Client.Desktop";
        const string STClientDesktop_AppResources = "ST.Client.Desktop[AppResources]";
        const string STServicesCloudServiceModels = "ST.Services.CloudService.Models";
        const string STToolsWin7Troubleshoot = "ST.Tools.Win7Troubleshoot";

        /// <summary>
        /// 有 resx 文件的项目名
        /// </summary>
        public static readonly string[] resxs = new[]
        {
            ClientLibDroid,
            CoreLib,
            ST,
            STClient,
            STClientDesktop,
            STClientDesktop_AppResources,
            STServicesCloudServiceModels,
            STToolsWin7Troubleshoot,
        };

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
        public static async Task<bool> ValidateAsync((string resx, string lang) args, Func<(string resxFilePath, string lang), Task> handler)
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
            if (args.resx == All)
            {
                foreach (var resxFilePath in resxs)
                {
                    if (isAllLang)
                    {
                        foreach (var lang in langs)
                        {
                            await handler((GetResxFilePath(resxFilePath), lang));
                        }
                    }
                    else
                    {
                        await handler((GetResxFilePath(resxFilePath), args.lang));
                    }
                }
            }
            else
            {
                if (isAllLang)
                {
                    foreach (var lang in langs)
                    {
                        await handler((args.resx, lang));
                    }
                }
                else
                {
                    await handler((args.resx, args.lang));
                }
            }
            return true;
        }

        const string DataXmlStart = "<data name=\"";
        const string DataXmlEnd = "\" xml:space=\"preserve\">";
        const string ValueXmlStart = "<value>";
        const string ValueXmlEnd = "</value>";
        const string CommentXmlStart = "<comment>";
        const string CommentXmlEnd = "</comment>";

        public static void AddOrReplace<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key, TValue value)
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
                var array = comment.Split(new char[] { '；', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var dict = array.Select(x => x.Split('=', StringSplitOptions.RemoveEmptyEntries).Take(2).ToArray()).Where(x => x.Length == 2).ToDictionary(x => x[0], x => x[1]);
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
            string? key = null, value = "", comment = "";
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
                        AddOrReplace(dict, key, (value, comment));
                        continue;
                    }
                    else if (lineTrim.StartsWith(CommentXmlStart) && lineTrim.EndsWith(CommentXmlEnd))
                    {
                        comment = line.Substring(CommentXmlStart, CommentXmlEnd);
                        AddOrReplace(dict, key, (value, comment));
                        continue;
                    }
                }
                if (lineTrim.StartsWith(DataXmlStart) && lineTrim.EndsWith(DataXmlEnd))
                {
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

        public const string to_ = "&to=";
        public const string route = "https://api.translator.azure.cn/translate?api-version=3.0&from=zh-Hans";
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
    }
}