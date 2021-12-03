using System.Application.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using TextCopy;
using static System.Application.Utils;
using static System.ProjectPathUtil;

namespace System.Application.Steps
{
    internal static class Step3
    {
        public static void Add(RootCommand command)
        {
            var rr = new Command("readrsa", "3. (本地)读取剪切板公钥值写入txt的pfx文件中");
            rr.AddAlias("rr");
            rr.AddOption(new Option<string>("-val", () => string.Empty, "通过命令行将公钥传入"));
            rr.AddOption(new Option<bool>("-dev", DevDesc));
            rr.Handler = CommandHandler.Create((string val, bool dev) =>
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

                var value = Serializable.DJSON<AppIdWithPublicKey>(val);

                Handler(value, dev);

                Console.WriteLine("完成。");
            });
            command.AddCommand(rr);
        }

        public static bool Handler(AppIdWithPublicKey? value, bool dev)
        {
            if (value == default)
            {
                Console.WriteLine("错误：读取剪切板值不能为 Null！");
                return false;
            }

            if (value.AppId == default)
            {
                Console.WriteLine("错误：读取剪切板值 AppId 无效！");
                return false;
            }
            if (string.IsNullOrWhiteSpace(value.PublicKey))
            {
                Console.WriteLine("错误：读取剪切板值 PublicKey 无效！");
                return false;
            }

            var env = GetConfiguration(dev, isLower: true);

            var pfxFilePath = Path.Combine(projPath,
                $"rsa-public-key-{env}.pfx");
            IOPath.FileIfExistsItDelete(pfxFilePath);
            File.WriteAllText(pfxFilePath, value.PublicKey);

            var appIdFilePath = Path.Combine(projPath,
                $"app-id-{env}.pfx");
            IOPath.FileIfExistsItDelete(appIdFilePath);
            File.WriteAllBytes(appIdFilePath, value.AppId.ToByteArray());

            Console.WriteLine(pfxFilePath);
            Console.WriteLine(appIdFilePath);

            return true;
        }
    }
}