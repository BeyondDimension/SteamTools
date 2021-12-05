using System.CommandLine;
using System.CommandLine.Invocation;
using TextCopy;

namespace System.Application.Steps
{
    internal static class Step_readdesc
    {
        public static void Add(RootCommand command)
        {
            var rd = new Command("readdesc", "X. (本地)读取剪切板更新日志生成一行数据复制到剪切板中");
            rd.AddAlias("rd");
            rd.Handler = CommandHandler.Create(() =>
            {
                var text = ClipboardService.GetText();
                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine("错误：读取剪切板值无效！");
                    return;
                }
                var array = text.Split(Environment.NewLine);
                text = string.Join(';', array);
                text = $"\"{text}\"";
                ClipboardService.SetText(text);
                Console.WriteLine("OK");
            });
            command.AddCommand(rd);
        }
    }
}