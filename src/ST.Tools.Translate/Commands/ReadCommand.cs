using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static System.Constants;
using static System.Utils;

namespace System.Commands
{
    /// <summary>
    /// 读取 xlsx 文件
    /// </summary>
    static class ReadCommand
    {
        public static void Add(RootCommand command)
        {
            // t read-xlsx -resx all -lang all
            var read_xlsx = new Command("read-xlsx", "读取 xlsx 写入 resx")
            {
                Handler = CommandHandler.Create((string resx, string lang)
                    => ValidateAsync<CommandArguments>((resx, lang), ReadXlsxAsync)),
            };
            read_xlsx.AddOption(new Option<string>("-resx", ResxDesc));
            read_xlsx.AddOption(new Option<string>("-lang", LangDesc));
            command.AddCommand(read_xlsx);
        }

        static Task<IList<string>?> ReadXlsxAsync(CommandArguments args)
        {
            var messages = ReadXlsx(args);
            return Task.FromResult(messages);
        }

        static IList<string>? ReadXlsx(CommandArguments args)
        {
            List<string> messages = new();

            var resxFilePathLang = GetResxFilePathLang(args);
            var excelFilePath = GetXlsxFilePath(resxFilePathLang, args.lang);
            using var fs = File.OpenRead(excelFilePath);
            var workbook = new XSSFWorkbook(fs);
            if (workbook.NumberOfSheets <= 0) return messages;
            var sheet = workbook.GetSheetAt(0);
            IRow row;
            ICell cell;
            int index_row = 0, index_cell = 0;

            do // 定位 Header
            {
                row = sheet.GetRow(index_row++);
                if (row == null) return messages;
                cell = row.GetCell(0);
            } while (!cell.StringCellValue.Contains(ColumnHeaderKey));

            int index_cell_value = -1, index_cell_author = -1,
                index_cell_comment = -1, index_cell_value_machine = -1;
            while (true) // 定位 Value Cell Index
            {
                var cellnum = index_cell++;
                cell = row.GetCell(cellnum);
                if (cell == null) break;
                if (cell.StringCellValue.Contains(ColumnHeaderMachineTranslation))
                {
                    index_cell_value_machine = cellnum;
                }
                if (cell.StringCellValue.Contains(ColumnHeaderHumanTranslation))
                {
                    index_cell_value = cellnum;
                }
                if (cell.StringCellValue.Contains(ColumnHeaderAuthor))
                {
                    index_cell_author = cellnum;
                }
                if (cell.StringCellValue.Contains(ColumnHeaderComment))
                {
                    index_cell_comment = cellnum;
                }
            }
            if (index_cell_value == -1 || index_cell_author == -1 || index_cell_comment == -1) return messages;

            Dictionary<string, (string value, Dictionary<string, string> comment)> dict = new();
            while (true) // 循环 Body 区域
            {
                row = sheet.GetRow(index_row++);
                if (row == null) break;
                var keyCell = row.GetCell(0);
                var valueCell = row.GetCell(index_cell_value);
                var valueMachineCell = index_cell_value_machine != -1 ?
                    row.GetCell(index_cell_value_machine) : null;
                var commentCell = row.GetCell(index_cell_comment);
                var authorCell = row.GetCell(index_cell_author);
                var key = keyCell.StringCellValue;
                var value = (valueCell ?? valueMachineCell)?.StringCellValue;
                var comment = commentCell.StringCellValue;
                var author = authorCell.StringCellValue;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var commentDict = new Dictionary<string, string>
                    {
                        { CommentKey, comment },
                        { AuthorKey, author },
                    };
                    dict.Add(key, (value, commentDict));
                }
            }

            var resxFileDictLangDict = GetResxDict2(resxFilePathLang, ignoreStringBuilder: true).dict;
            foreach (var item in dict)
            {
                resxFileDictLangDict.AddOrReplace(item);
            }

            var template = Properties.Resources.Resx;
            const string rootEndMark = "</root>";
            var rootEndIndex = template.IndexOf(rootEndMark);
            if (rootEndIndex == -1)
            {
                messages.Add("template.IndexOf(\"</root>\") == -1");
                return messages;
            }

            var sb = new StringBuilder(template.Substring(0, rootEndIndex));
            foreach (var item in resxFileDictLangDict)
            {
                sb.AppendFormatLine("  <data name=\"{0}\" xml:space=\"preserve\">", item.Key);
                sb.AppendFormatLine("    <value>{0}</value>", Escape(item.Value.value));
                var commentStr = Serialize(item.Value.comment);
                if (!string.IsNullOrWhiteSpace(commentStr))
                {
                    sb.AppendFormatLine("    <comment>{0}</comment>", Escape(commentStr));
                }
                sb.AppendLine("  </data>");
            }
            sb.AppendLine(rootEndMark);

            var str = sb.ToString();
            IOPath.FileIfExistsItDelete(resxFilePathLang);
            File.WriteAllText(resxFilePathLang, str);

            return messages;
        }
    }
}