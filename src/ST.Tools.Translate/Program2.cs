using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using static System.Constants;

//SetAuthor("vanja-san");
//throw new Exception("return");

try
{
    DI.Init(s =>
    {
        s.AddHttpClient();
    });

    var rootCommand = new RootCommand(Title);

    var commands = Assembly.GetExecutingAssembly().GetTypes()
        .Where(x => x.Namespace == "System.Commands" && x.IsClass && !x.IsGenericType && x.IsAbstract && x.IsSealed);
    foreach (var item in commands)
    {
        var method = item.GetMethod("Add", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(RootCommand) }, null);
        method?.Invoke(null, new[] { rootCommand });
    }

    return rootCommand.InvokeAsync(args).Result;
}
catch (Exception? e)
{
    while (e != null)
    {
        Console.WriteLine(e.ToString());
        e = e.InnerException;
    }
    return 500;
}

//static void SetAuthor(string author)
//{
//    CommandArguments args = new() { resx = STClient_AppResources, lang = "ru" };
//    args.resxFilePath = Utils.GetResxFilePath(args.resx);
//    args.resxFilePath = Utils.GetResxFilePathLang(args);
//    var resxFileDict = Utils.GetResxDict2(args.resxFilePath, ignoreStringBuilder: true);

//    foreach (var item in resxFileDict.dict)
//    {
//        var authorValue = item.Value.comment[AuthorKey];
//        if (string.IsNullOrEmpty(authorValue) || authorValue == MicrosoftTranslator)
//        {
//            item.Value.comment[AuthorKey] = author;
//        }
//    }

//    var template = System.Properties.Resources.Resx;
//    const string rootEndMark = "</root>";
//    var rootEndIndex = template.IndexOf(rootEndMark);
//    if (rootEndIndex == -1)
//    {
//        throw new Exception("template.IndexOf(\"</root>\") == -1");
//        //messages.Add("template.IndexOf(\"</root>\") == -1");
//        //return messages;
//    }

//    var sb = new StringBuilder(template.Substring(0, rootEndIndex));
//    foreach (var item in resxFileDict.dict)
//    {
//        sb.AppendFormatLine("  <data name=\"{0}\" xml:space=\"preserve\">", item.Key);
//        sb.AppendFormatLine("    <value>{0}</value>", Utils.Escape(item.Value.value));
//        var commentStr = Utils.Serialize(item.Value.comment);
//        if (!string.IsNullOrWhiteSpace(commentStr))
//        {
//            sb.AppendFormatLine("    <comment>{0}</comment>", Utils.Escape(commentStr));
//        }
//        sb.AppendLine("  </data>");
//    }
//    sb.AppendLine(rootEndMark);

//    var str = sb.ToString();
//    IOPath.FileIfExistsItDelete(args.resxFilePath);
//    File.WriteAllText(args.resxFilePath, str);
//}