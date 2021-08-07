namespace System
{
    static class Constants
    {
        public const string Title = "Resx 翻译命令行工具(Resx Translation Command Line Tools/RTCLT)";

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

        public const string ClientLibDroid = "Common.ClientLib.Droid";
        public const string CoreLib = "Common.CoreLib";
        public const string ST = "ST";
        public const string STClient = "ST.Client";
        public const string STClientDesktop = "ST.Client.Desktop";
        public const string STClientDesktop_AppResources = "ST.Client.Desktop[AppResources]";
        public const string STServicesCloudServiceModels = "ST.Services.CloudService.Models";
        public const string STToolsWin7Troubleshoot = "ST.Tools.Win7Troubleshoot";

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

        public const string DataXmlStart = "<data name=\"";
        public const string DataXmlEnd = "\" xml:space=\"preserve\">";
        public const string ValueXmlStart = "<value>";
        public const string ValueXmlEnd = "</value>";
        public const string CommentXmlStart = "<comment>";
        public const string CommentXmlEnd = "</comment>";

        public const string to_ = "&to=";
        public const string route = "https://api.translator.azure.cn/translate?api-version=3.0&from=zh-Hans";
        public const string route_ = "https://api.translator.azure.cn/translate?api-version=3.0&from={0}" + to_ + "zh-Hans";
    }
}