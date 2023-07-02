// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

#if !__NOT_IMPORT_COMMON_SERVICES__
global using BD.Common.Services;
#endif

global using BD.WTTS;
global using BD.WTTS.Services;
#if !MVVM_VM
global using BD.WTTS.Services.Implementation;
#endif