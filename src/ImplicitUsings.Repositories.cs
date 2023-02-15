// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using BD.Common.Enums;
global using BD.Common.Columns;
global using BD.Common.Data;
global using BD.Common.Entities;
global using BD.Common.Entities.Abstractions;
global using BD.Common.Repositories;
global using BD.Common.Repositories.Abstractions;

global using BD.WTTS.Enums;
global using BD.WTTS.Columns;
global using BD.WTTS.Entities;
//global using BD.WTTS.Entities.Abstractions;
global using BD.WTTS.Repositories;
global using BD.WTTS.Repositories.Abstractions;

global using Notification = BD.WTTS.Entities.Notification;
#if ANDROID
global using AndroidAppNotification = Android.App.Notification;
#endif