using System.IO;
using System.Linq;

namespace System
{
    static class Constants
    {
        public const string Title = "Resx翻译命令行工具(Resx Translation Command Line Tools/RTCLT)";

        /// <summary>
        /// 支持的语言区域名
        /// </summary>
        public static readonly string[] langs = new[] {
            "zh-Hant",
            "en",
            "ko",
            "ja",
            "ru",
        };

        public const string All = "all";
        public const char Separator = '-';
        public const string Key = "键(Key)";
        public const int KeyWidth = 30;
        public const string Value = "值(Value)";
        public const int ValueWidth = 40;
        public const string Author = "作者(Author)";
        public const string MachineTranslation = "机翻(Machine Translation)";
        public const string HumanTranslation = "人工翻译(Human Translation)";
        public const string MachineProofread = "机翻校对(Machine Proofread)";
        public const string ResxDesc = "指定 resx 文件路径或项目名";
        public const string LangDesc = "指定要生成的语言，多选或单选，使用分号分割，all 表示全选";

        const string AreaLib = "Common.AreaLib";
        const string ClientLibDroid = "Common.ClientLib.Droid";
        const string CoreLib = "Common.CoreLib";
        const string ST = "ST";
        const string STClient = "ST.Client";
        const string STClientDesktop = "ST.Client.Desktop";
        const string STClientDesktop_AppResources = "ST.Client.Desktop[AppResources]";
        const string STServicesCloudServiceModels = "ST.Services.CloudService.Models";

        /// <summary>
        /// 有 resx 文件的项目名
        /// </summary>
        public static readonly string[] resxs = new[]
        {
            AreaLib,
            ClientLibDroid,
            CoreLib,
            ST,
            STClient,
            STClientDesktop,
            STClientDesktop_AppResources,
            STServicesCloudServiceModels,
        };

        /// <summary>
        /// 根据[支持的语言区域名]获取表头
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static string GetHeaderName(string lang) => lang switch
        {
            "zh-Hans" => "[zh-Hans]简体中文(Simplified Chinese)",
            "zh-Hant" => "[zh-Hant]繁体中文(Traditional Chinese)",
            "en" => "[en]英语(English)",
            "ko" => "[ko]韩语(Koreana)",
            "ja" => "[ja]日语(Japanese)",
            "ru" => "[ru]俄语(Russian)",
            _ => throw new ArgumentOutOfRangeException(nameof(lang), lang, null),
        };

        /// <summary>
        /// 根据[有 resx 文件的项目名]获取文件路径
        /// </summary>
        /// <param name="resx"></param>
        /// <returns></returns>
        public static string GetResxFilePath(string resx) => resx switch
        {
            AreaLib => "",
            ClientLibDroid => "",
            CoreLib => "",
            ST => "",
            STClient => "",
            STClientDesktop => "",
            STClientDesktop_AppResources => "",
            STServicesCloudServiceModels => "",
            _ => throw new ArgumentOutOfRangeException(nameof(resx), resx, null),
        };

        /// <summary>
        /// 验证传入参数是否正确
        /// </summary>
        /// <param name="args"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static bool Validate((string resx, string lang) args, Action<(string resxFilePath, string lang)> handler)
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
                            handler((resxFilePath, lang));
                        }
                    }
                    else
                    {
                        handler((resxFilePath, args.lang));
                    }
                }
            }
            else
            {
                if (isAllLang)
                {
                    foreach (var lang in langs)
                    {
                        handler((args.resx, lang));
                    }
                }
                else
                {
                    handler((args.resx, args.lang));
                }
            }
            return true;
        }
    }
}