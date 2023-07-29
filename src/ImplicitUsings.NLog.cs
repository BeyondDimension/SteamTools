// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using NLog;
global using NLog.Common;
global using NLog.Config;
global using NLog.Targets;
global using NLog.Extensions.Logging;
global using LogLevel = Microsoft.Extensions.Logging.LogLevel;
global using NLogLevel = NLog.LogLevel;
global using NInternalLogger = NLog.Common.InternalLogger;
global using NLogManager = NLog.LogManager;