// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
global using SteamKit2;
#endif
global using BD.SteamClient.Services;
global using BD.SteamClient.Enums;
global using BD.SteamClient.Models;
global using SteamUser = BD.SteamClient.Models.SteamUser;
global using SteamApps = BD.SteamClient.Models.SteamApps;
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
global using SteamKit2SteamUser = SteamKit2.SteamUser;
global using SteamKit2SteamApps = SteamKit2.SteamApps;
#endif
