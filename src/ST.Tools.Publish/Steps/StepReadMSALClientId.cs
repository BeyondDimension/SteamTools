using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using TextCopy;
using static System.Application.Utils;
using static System.ProjectPathUtil;

namespace System.Application.Steps
{
    internal static class StepReadMSALClientId
    {
        public static void Add(RootCommand command)
        {
            var rmci = new Command("read_msal_client_id", "读取 MSALClientId 写入txt的pfx文件中");
            rmci.AddAlias("rmci");
            rmci.AddOption(new Option<string>("-val", () => string.Empty, "通过命令行将值传入"));
            rmci.Handler = CommandHandler.Create((string val) =>
            {
                if (string.IsNullOrWhiteSpace(val))
                {
                    var text = ClipboardService.GetText();
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        Console.WriteLine("错误：读取剪切板值无效！");
                        return;
                    }
                    else
                    {
                        val = text;
                    }
                }

                if (!Guid.TryParse(val, out var guid))
                {
                    Console.WriteLine("错误：值无效！");
                    return;
                }

                var filePath = Path.Combine(projPath, "masl-client-id.pfx");
                IOPath.FileIfExistsItDelete(filePath);
                File.WriteAllBytes(filePath, guid.ToByteArray());

                Console.WriteLine("完成。");
                Console.WriteLine(filePath);
            });
            command.AddCommand(rmci);
        }
    }
}