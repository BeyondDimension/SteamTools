using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Constants;
using static System.Utils;

namespace System.Commands
{
    /// <summary>
    /// 校对 xlsx 文件
    /// </summary>
    static class ProofreadCommand
    {
        public static bool EnableAzureTranslation { get; } = true;

        public static void Add(RootCommand command)
        {
            // t proofread-xlsx -resx all -lang all
            var proofread_xlsx = new Command("proofread-xlsx", "校对 xlsx")
            {
                Handler = CommandHandler.Create((string resx, string lang)
                    => ValidateAsync((resx, lang), ProofreadXlsxAsync)),
            };
            proofread_xlsx.AddOption(new Option<string>("-resx", ResxDesc));
            proofread_xlsx.AddOption(new Option<string>("-lang", LangDesc));
            command.AddCommand(proofread_xlsx);
        }

        static async Task<IList<string>?> ProofreadXlsxAsync((string resxFilePath, string lang) args)
        {
            List<string> messages = new();

            ReadAzureTranslationKey();
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

            int index_cell_value = -1, index_cell_proofread = -1;
            while (true) // 定位 Value Cell Index
            {
                var cellnum = index_cell++;
                cell = row.GetCell(cellnum);
                if (cell == null) break;
                if (cell.StringCellValue.Contains(ColumnHeaderHumanTranslation))
                {
                    index_cell_value = cellnum;
                }
                if (cell.StringCellValue.Contains(ColumnHeaderMachineProofread))
                {
                    index_cell_proofread = cellnum;
                }
            }
            if (index_cell_value == -1) return messages;

            if (index_cell_proofread == -1) index_cell_proofread = index_cell - 1;
            cell = row.CreateCell(index_cell_proofread);
            cell.SetCellValue(ColumnHeaderMachineProofread);

            int index_row_body = index_row;
            HashSet<string> values = new();
            while (true) // 循环 Body 区域
            {
                row = sheet.GetRow(index_row++);
                if (row == null) break;
                var valueCell = row.GetCell(index_cell_value);
                var value = valueCell?.StringCellValue;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }

            var url = string.Format(route_, args.lang);
            var i = 0;
            Dictionary<string, string> dict = new();
            foreach (var value in values)
            {
                var value_i = i++;
                Console.WriteLine($"translating({args.lang})：{value_i}/{values.Count}");
                string value_translation;
                if (EnableAzureTranslation)
                {
                    var translationResults = await Translatecs.TranslateTextAsync(url, value);
                    var translationResult = translationResults.First(x => x != null).Translations.First(x => x.To.Equals(args.lang, StringComparison.OrdinalIgnoreCase));
                    value_translation = translationResult.Text;
                }
                else
                {
                    value_translation = value_i.ToString();
                }
                dict.Add(value, value_translation);
            }

            index_row = index_row_body;
            while (true) // 循环 Body 区域
            {
                row = sheet.GetRow(index_row++);
                if (row == null) break;
                var valueCell = row.GetCell(index_cell_value);
                var value = valueCell?.StringCellValue;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var proofread = dict[value];
                    var proofreadCell = row.CreateCell(index_cell_proofread);
                    proofreadCell.SetCellValue(proofread);
                }
            }

            using var ms = new MemoryStream();
            workbook.Write(ms);
            workbook.Close();
            fs.Dispose();
            File.Delete(excelFilePath);
            using var fs2 = File.OpenWrite(excelFilePath);
            ms.Position = 0;
            ms.CopyTo(fs2);
            fs2.Flush();

            return messages;
        }
    }
}