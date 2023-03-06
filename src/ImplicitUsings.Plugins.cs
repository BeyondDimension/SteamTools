// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using BD.WTTS.Plugins;
global using BD.WTTS.Plugins.Abstractions;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

global using System.Composition;
global using System.Composition.Convention;
global using System.Composition.Hosting;

global using CompositionExport = System.Composition.ExportAttribute;

#endif