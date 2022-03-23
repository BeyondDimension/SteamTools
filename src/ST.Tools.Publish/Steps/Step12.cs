//using System.Application.Models;
//using System.Collections.Generic;
//using System.CommandLine;
//using System.CommandLine.Invocation;
//using System.CommandLine.NamingConventionBinder;
//using System.IO;
//using System.Security.Cryptography;
//using static System.Application.Utils;

//namespace System.Application.Steps
//{
//    internal static class Step12
//    {
//        public static void Add(RootCommand command)
//        {
//            var fix = new Command("fix", "12. 修复文件复制不正确");
//            fix.AddOption(new Option<string>("-path", "要扫描的文件路径"));
//            fix.Handler = CommandHandler.Create((string path) =>
//            {
//                List<PublishFileInfo> list = new List<PublishFileInfo>();

//                ScanPath(path, list);

//                foreach (var file in list)
//                {
//                    Console.WriteLine($"正在计算哈希值：{file.Path}");
//                    using (var fileStream = File.OpenRead(file.Path))
//                    {
//                        file.Length = fileStream.Length;
//                        file.SHA256 = Hashs.String.SHA256(fileStream);
//                    }
//                    var buildFileId = file.SHA256 + file.Length;
//                    File.Move(file.Path, Path.Combine(path, buildFileId + FileEx.BIN));
//                }
//            });
//            command.AddCommand(fix);
//        }
//    }
//}