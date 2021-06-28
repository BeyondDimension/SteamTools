using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Constants;

namespace System.Commands
{
    /// <summary>
    /// 写入 xlsx 文件
    /// </summary>
    static class WriteCommand
    {
        public static void Add(RootCommand command)
        {
            var write_xlsx = new Command("write-xlsx", "读取 resx 写入 xlsx")
            {
                Handler = CommandHandler.Create((string resx, string lang)
                    => Validate((resx, lang), WriteXlsx)),
            };
            write_xlsx.AddOption(new Option<string>("-resx", ResxDesc));
            write_xlsx.AddOption(new Option<string>("-lang", LangDesc));
            command.AddCommand(write_xlsx);
        }

        static void WriteXlsx((string resxFilePath, string lang) args)
        {
            var resxFilePathLang = args.resxFilePath.TrimEnd(".resx", StringComparison.OrdinalIgnoreCase) + $".{args.lang}.resx";
            if (!File.Exists(resxFilePathLang))
            {
                Console.WriteLine($"Error: resx file not found, path: {resxFilePathLang}");
                return;
            }
        }
    }
}
