// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using SteamKit2;
global using SteamUser = BD.WTTS.Models.SteamUser;
global using SteamApps = BD.WTTS.Models.SteamApps;
global using SteamKit2SteamUser = SteamKit2.SteamUser;
global using SteamKit2SteamApps = SteamKit2.SteamApps;