using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Application.Utils;
using static System.ProjectPathUtil;
using System.Application.Models;

namespace System.Application.Steps
{
    internal static class Step_packasfui
    {
        public static void Add(RootCommand command)
        {
            var pau = new Command("packasfui", "X. (本地)打包 ASF-UI 到嵌入的资源中");
            pau.AddAlias("pau");
            //pau.AddOption(new Option<string>("-ver", () => string.Empty, "通过命令行将版本号传入，如果不填则自动读取"));
            pau.Handler = CommandHandler.Create((/*string ver*/) =>
            {
                var packDiaPath = Path.Combine(projPath, "..", "ASF-UI");
                var packFilePath = Path.Combine(projPath, "src", "ST.Client", "Resources", "asf-ui" + FileEx.TAR_BR);
                if (File.Exists(packFilePath)) File.Delete(packFilePath);
                var files = new List<PublishFileInfo>();
                ScanPath(packDiaPath, files);
                CreateBrotliPack(packFilePath, files);

                Console.WriteLine("OK");
            });
            command.AddCommand(pau);
        }
    }
}