//using System.Collections.Generic;
//using System.CommandLine;
//using System.CommandLine.Invocation;
//using System.CommandLine.NamingConventionBinder;
//using System.Linq;
//using System.Threading.Tasks;
//using static System.Constants;
//using static System.Utils;

//namespace System.Commands
//{
//    /// <summary>
//    /// 根据 Key 删除
//    /// </summary>
//    static class DelByKeyCommand
//    {
//        public static void Add(RootCommand command)
//        {
//            // t del -resx all -keys key1 key2
//            var del = new Command("del", "根据 Key 删除")
//            {
//                Handler = CommandHandler.Create(Handler),
//            };
//            del.AddOption(new Option<string[]>("-keys", "要移除的 Key 数组"));
//            del.AddOption(new Option<string>("-resx", ResxDesc));
//            command.AddCommand(del);
//        }

//        static Task Handler(string[] keys, string resx)
//        {
//            if (!keys.Any())
//            {
//                Console.WriteLine("Key 数组中不包含任何元素");
//                return;
//            }

//            var args = new CommandArguments
//            {
//                resx = resx,
//                lang = "all",
//            };

//            return ValidateAsync(args, _ =>
//            {
//                var resxFilePathLang = GetResxFilePathLang(args);
//                ResxFileLangCreateByNotExists(resxFilePathLang);
//                var resxFileDict = GetResxDict2(args.resxFilePath, ignoreStringBuilder: false);

//                var resxFileDictLang = GetResxDict2(resxFilePathLang, ignoreStringBuilder: false);

//                foreach (var item in keys)
//                {
//                    resxFileDict.dict.Remove(item);
//                    resxFileDictLang.dict.Remove(item);
//                }

//                throw new NotSupportedException("TODO");
//                return Task.FromResult<IList<string>?>(null);
//            });
//        }
//    }
//}
