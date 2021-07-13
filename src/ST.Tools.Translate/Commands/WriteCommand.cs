using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Constants;

namespace System.Commands
{
    /// <summary>
    /// 写入 xlsx 文件
    /// </summary>
    static class WriteCommand
    {
        public static void Add(RootCommand command)
        {
            // t write-xlsx -resx all -lang all
            var write_xlsx = new Command("write-xlsx", "读取 resx 写入 xlsx")
            {
                Handler = CommandHandler.Create((string resx, string lang)
                    => ValidateAsync((resx, lang), WriteXlsxAsync)),
            };
            write_xlsx.AddOption(new Option<string>("-resx", ResxDesc));
            write_xlsx.AddOption(new Option<string>("-lang", LangDesc));
            command.AddCommand(write_xlsx);
        }

        static async Task WriteXlsxAsync((string resxFilePath, string lang) args)
        {
            var resxFilePathLang = args.resxFilePath.TrimEnd(".resx", StringComparison.OrdinalIgnoreCase) + $".{args.lang}.resx";
            ResxFileLangCreateByNotExists(resxFilePathLang);

            var resxFileDict = GetResxDict(args.resxFilePath, ignoreStringBuilder: true);
            if (!resxFileDict.dict.Any())
            {
                Console.WriteLine($"Error: resx file not any value, path: {args.resxFilePath}");
                return;
            }
            var resxFileDictLang = GetResxDict(resxFilePathLang, ignoreStringBuilder: true);
            if (resxFileDictLang.dict.Count > resxFileDict.dict.Count) // 译文不能比原文多
            {
                Console.WriteLine($"Error: resx file lang count incorrect, path: {resxFilePathLang}");
                return;
            }

            Dictionary<string, string> originalTexts = new();
            foreach (var item in resxFileDict.dict)
            {
                if (!resxFileDictLang.dict.ContainsKey(item.Key))
                {
                    originalTexts.Add(item.Key, item.Value.value);
                }
            }

            if (originalTexts.Any())
            {
                ReadAzureTranslationKey();
                var url = route + to_ + args.lang;
                foreach (var originalText in originalTexts)
                {
                    Console.WriteLine($"translating({args.lang})：{originalText.Key}");
                    var translationResults = await Translatecs.TranslateTextAsync(url, originalText.Value);
                    var translationResult = translationResults.First(x => x != null).Translations.First(x => x.To.Equals(args.lang, StringComparison.OrdinalIgnoreCase));
                    AddOrReplace(resxFileDictLang.dict,
                        originalText.Key, (translationResult.Text, ""));
                }
            }

            var excelFilePath = GetXlsxFilePath(resxFilePathLang, args.lang);
            IOPath.FileIfExistsItDelete(excelFilePath);
            using var fs = File.Create(excelFilePath);
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("sheet");
            var index = 0;
            var row = sheet.CreateRow(index++);
            ICell cell;

            cell = row.CreateCell(0);
            cell.SetCellValue(ColumnHeaderKey);
            sheet.SetColumnWidth(0, KeyWidth);

            cell = row.CreateCell(1);
            cell.SetCellValue(ColumnHeaderValue);
            sheet.SetColumnWidth(1, ValueWidth);

            cell = row.CreateCell(2);
            cell.SetCellValue(ColumnHeaderMachineTranslation);
            sheet.SetColumnWidth(2, ValueWidth);

            cell = row.CreateCell(3);
            cell.SetCellValue(ColumnHeaderHumanTranslation);
            sheet.SetColumnWidth(3, ValueWidth);

            cell = row.CreateCell(4);
            cell.SetCellValue(ColumnHeaderAuthor);
            sheet.SetColumnWidth(4, ValueWidth);

            cell = row.CreateCell(5);
            cell.SetCellValue(ColumnHeaderComment);
            sheet.SetColumnWidth(5, ValueWidth);

            foreach (var item in resxFileDict.dict)
            {
                row = sheet.CreateRow(index++);

                cell = row.CreateCell(0);
                cell.SetCellValue(item.Key);

                cell = row.CreateCell(1);
                cell.SetCellValue(item.Value.value);

                var langValue = resxFileDictLang.dict[item.Key];
                var commentData = Deserialize(langValue.comment);
                var author = commentData[AuthorKey];
                var valueCellNum = author == MicrosoftTranslator ? 2 : 3;

                cell = row.CreateCell(valueCellNum);
                cell.SetCellValue(langValue.value);

                cell = row.CreateCell(4);
                cell.SetCellValue(author);

                var sourceCommentData = Deserialize(resxFileDict.dict[item.Key].comment);
                if (sourceCommentData.ContainsKey(CommentKey))
                {
                    cell = row.CreateCell(5);
                    cell.SetCellValue(sourceCommentData[CommentKey]);
                }
            }

            workbook.Write(fs);

            Console.WriteLine($"OK, path: {excelFilePath}");
        }
    }
}