using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Constants;

namespace System
{
    static class Utils
    {
        /// <summary>
        /// 读取 Xlsx 文件
        /// </summary>
        /// <param name="originalDict">中文原文字典数据</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="messages">错误消息</param>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, (string value, string comment)>? ReadXlsx(IReadOnlyDictionary<string, (string value, string comment)>? originalDict, string filePath, string author, out IList<string> messages)
        {
            messages = new List<string>();
            using var fs = File.OpenRead(filePath);
            var workbook = new XSSFWorkbook(fs);
            if (workbook.NumberOfSheets <= 0) return null;
            var sheet = workbook.GetSheetAt(0);
            IRow row;
            ICell cell;
            int index_row = 0, index_cell = 0;

            bool keyIsFirst = false;
            do // 定位 Header
            {
                row = sheet.GetRow(index_row++);
                if (row == null) return null;
                cell = row.GetCell(0);
                keyIsFirst = cell.StringCellValue.Contains("标识符") ||
                    cell.StringCellValue.Contains(ColumnHeaderKey);
            } while (!(keyIsFirst ||
                    cell.StringCellValue.Contains("中文文本")));

            int index_cell_value = -1;
            var index_cell_original = -1;
            while (true) // 定位 Value Cell Index
            {
                var cellnum = index_cell++;
                cell = row.GetCell(cellnum);
                if (cell == null) return null;
                if (cell.StringCellValue.Contains("翻译文本") ||
                    cell.StringCellValue.Contains("人工翻译"))
                {
                    index_cell_value = cellnum;
                    break;
                }
                if (cell.StringCellValue.Contains("中文文本"))
                {
                    index_cell_original = cellnum;
                }
            }
            if (index_cell_value == -1 && index_cell_original == -1) return null;

            Dictionary<string, (string value, string comment)> dict = new();
            while (true) // 循环 Body 区域
            {
                row = sheet.GetRow(index_row++);
                if (row == null) break;
                var keyCell = row.GetCell(0);
                var valueCell = row.GetCell(index_cell_value);
                var commentCell = row.GetCell(index_cell_value + 1);
                var key = keyCell.StringCellValue;
                var value = valueCell.StringCellValue;
                if (!keyIsFirst)
                {
                    if (!originalDict.Any_Nullable())
                    {
                        messages.Add($"{Path.GetFileName(filePath)} 缺少 Key 标识，originalDict is null");
                        return null;
                    }
                    var originalValue = row.GetCell(index_cell_original).StringCellValue;
                    var findKey = originalDict!.FirstOrDefault(x => x.Value.value == originalValue);
                    if (findKey.Key == default)
                    {
                        messages.Add($"{Path.GetFileName(filePath)} 缺少 Key 标识，使用 Value 在原文中查不到标识，原文值：{key}，译文值：{value}");
                    }
                    else
                    {
                        key = findKey.Key;
                    }
                }

                var comment = commentCell?.StringCellValue ?? string.Empty;
                if (!string.IsNullOrEmpty(author))
                {
                    var comment2 = Deserialize(comment);
                    comment2.AddOrReplace(AuthorKey, author);
                    comment = Serialize(comment2);
                }

                dict.AddOrReplace(key, (value, comment));
            }

            return dict.Any() ? dict : null;
        }
    }
}