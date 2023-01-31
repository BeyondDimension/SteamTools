// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using BD.WTTS.Controllers;
global using BD.WTTS.Controllers.Abstractions;

#if !BACKMANAGE
global using static BD.WTTS.MicroServices.Primitives.R.Strings;
#endif
global using static BD.WTTS.MicroServices.R.Strings;
global using static BD.WTTS.R.Strings;

global using IOFile = System.IO.File;