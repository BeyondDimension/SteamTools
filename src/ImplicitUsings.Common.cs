// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using BD.Common;
global using BD.Common.Columns;
global using BD.Common.Enums;
#if _IMPORT_COMMON_CONTROLLERS__
global using BD.Common.Controllers;
global using BD.Common.Controllers.Abstractions;
#endif
#if _IMPORT_COMMON_IDENTITY__
global using BD.Common.Identity;
global using BD.Common.Identity.Abstractions;
#endif
#if _IMPORT_COMMON_SERVICES__
global using BD.Common.Services;
#endif
#if USE_SMS
global using BD.Common.Models.SmsSender;
global using BD.Common.Services.Implementation.SmsSender;
#endif
#if _IMPORT_COMMON_MIDDLEWARE__
global using BD.Common.Middleware;
#endif