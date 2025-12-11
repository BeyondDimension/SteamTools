// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using System.CommandLine;
global using System.CommandLine.Invocation;
global using Task = Microsoft.Build.Utilities.Task;
global using ThreadTask = System.Threading.Tasks.Task;

global using BD.WTTS;
global using BD.WTTS.Client.Tools.Publish.Enums;
global using BD.WTTS.Client.Tools.Publish.Models;
global using BD.WTTS.Client.Tools.Publish.Commands;
global using BD.WTTS.Client.Tools.Publish.Commands.Abstractions;
global using static BD.WTTS.Client.Tools.Publish.Constants;