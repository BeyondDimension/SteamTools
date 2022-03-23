using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using static System.Application.Utils;
using static System.ProjectPathUtil;

namespace System.Application.Steps
{
    internal static class Step1
    {
        public static void Add(RootCommand command)
        {
            var sv = new Command("showv", "1. (本地)显示当前本地密钥");
            sv.AddAlias(nameof(sv));
            sv.Handler = CommandHandler.Create(() =>
            {
                foreach (var env in new[] { GetConfiguration(true, isLower: true), GetConfiguration(false, isLower: true) })
                {
                    Console.WriteLine($"Configuration: {env}");

                    var appIdFilePath = Path.Combine(projPath, $"app-id-{env}.pfx");

                    Console.Write("AppId: ");
                    Console.WriteLine(new Guid(File.ReadAllBytes(appIdFilePath)).ToString());

                    var pfxFilePath = Path.Combine(projPath, $"rsa-public-key-{env}.pfx");

                    Console.Write("RsaPublicKey: ");
                    Console.WriteLine(File.ReadAllText(pfxFilePath));

                    Console.WriteLine();
                }
            });
            command.AddCommand(sv);
        }
    }
}