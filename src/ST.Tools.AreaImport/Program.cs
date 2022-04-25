#pragma warning disable SA1516 // Elements should be separated by blank line
using MessagePack;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Application;
using System.Application.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

HashSet<string> shortNames_2 = new()
{
    "内蒙古",
    "广西",
    "西藏",
    "宁夏",
    "新疆",
    "香港",
    "澳门",
};
HashSet<string> shortName_3_del = new()
{
    "蒙古自治州",
    "柯尔克孜自治州",
    "自治州",
    "地区",
};
HashSet<string> minority = new()
{
    "壮族",
    "满族",
    "回族",
    "苗族",
    "维吾尔族",
    "土家族",
    "彝族",
    "蒙古族",
    "藏族",
    "布依族",
    "侗族",
    "瑶族",
    "朝鲜族",
    "白族",
    "哈尼族",
    "哈萨克族",
    "黎族",
    "傣族",
    "畲族",
    "傈僳族",
    "仡佬族",
    "东乡族",
    "高山族",
    "拉祜族",
    "水族",
    "佤族",
    "纳西族",
    "羌族",
    "土族",
    "仫佬族",
    "锡伯族",
    "柯尔克孜族",
    "达斡尔族",
    "景颇族",
    "毛南族",
    "撒拉族",
    "布朗族",
    "塔吉克族",
    "阿昌族",
    "普米族",
    "鄂温克族",
    "怒族",
    "京族",
    "基诺族",
    "德昂族",
    "保安族",
    "俄罗斯族",
    "裕固族",
    "乌兹别克族",
    "门巴族",
    "鄂伦春族",
    "独龙族",
    "塔塔尔族",
    "赫哲族",
    "珞巴族",
};
const string fileName = "AMap_adcode_citycode_20210406.xlsx";
var rootPath = Path.Combine(ProjectPathUtil.projPath, "Resources", "Areas");
var filePath = Path.Combine(rootPath, fileName);
var outFilePath = filePath + ".mpo";
if (File.Exists(outFilePath))
{
    Console.WriteLine($"文件已存在，路径：{outFilePath}");
    return;
}
using var fs = File.OpenRead(filePath);
var workbook = new XSSFWorkbook(fs);
var sheet = workbook.GetSheetAt(0);
IRow? row;
int rowIndex = 1;
List<Area> areas = new();
do
{
    row = sheet.GetRow(rowIndex++);
    if (row == null) break;
    var value_str = row.GetCell(1).StringCellValue;
    var value = value_str.TryParseInt32();
    if (!value.HasValue || value.Value == 100000) continue;
    var name = row.GetCell(0).StringCellValue;
    if (name.EndsWith("市辖区") || name == "重庆市郊县") continue;
    AreaLevel level;
    if (value_str.EndsWith("0000"))
    {
        level = AreaLevel.省或直辖市或特别行政区;
    }
    else if (value_str.EndsWith("00"))
    {
        level = AreaLevel.市_不包括直辖市;
    }
    else
    {
        level = AreaLevel.区县_县级市;
    }
    int GetEndWithZero2() => int.Parse($"{value_str.Substring(0, 4)}00");
    int GetEndWithZero4() => int.Parse($"{value_str.Substring(0, 2)}0000");
    int? GetUpBy3()
    {
        var up_value = GetEndWithZero4();
        if (areas.Any(x => x.Id == up_value)) return up_value;
        return null;
    }
    int? GetUpBy4()
    {
        var up_value = GetEndWithZero2();
        if (areas.Any(x => x.Id == up_value)) return up_value;
        return GetUpBy3();
    }
    int? up = level switch
    {
        AreaLevel.市_不包括直辖市 => GetUpBy3(),
        AreaLevel.区县_县级市 => GetUpBy4(),
        _ => null,
    };
    string? GetShortNameBy2()
    {
        if (name.EndsWith("市")) return name.TrimEnd("市");
        if (name.EndsWith("省")) return name.TrimEnd("省");
        if (name == "外国") return "海外";
        var shortName_value = shortNames_2.FirstOrDefault(x => name.Contains(x));
        return shortName_value;
    }
    string? GetShortNameBy3()
    {
        string? GetShortNameBy3_()
        {
            if (name.EndsWith("市")) return name.TrimEnd("市");
            var del = shortName_3_del.FirstOrDefault(x => name.EndsWith(x));
            if (del != default) return name.Substring(0, name.Length - del.Length);
            return default;
        }
        var r = GetShortNameBy3_();
        if (r != null)
        {
            foreach (var item in minority)
            {
                r = r.Replace(item, string.Empty);
            }
        }
        return r;
    }
    string? GetShortNameBy4()
    {
        if (name.EndsWith("自治州直辖")) return "自治州直辖";
        return default;
    }
    string? shortName = level switch
    {
        AreaLevel.省或直辖市或特别行政区 => GetShortNameBy2(),
        AreaLevel.市_不包括直辖市 => GetShortNameBy3(),
        AreaLevel.区县_县级市 => GetShortNameBy4(),
        _ => null,
    };
    areas.Add(new Area
    {
        Id = value.Value,
        Name = name,
        ShortName = shortName,
        Level = level,
        Up = up,
    });
}
while (true);
var bytes = Serializable.SMP(areas);
File.WriteAllBytes(outFilePath, bytes);
Console.WriteLine($"文件写入成功，路径：{outFilePath}");
#pragma warning restore SA1516 // Elements should be separated by blank line