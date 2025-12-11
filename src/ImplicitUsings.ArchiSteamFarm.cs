// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using ArchiSteamFarm;
global using ArchiSteamFarm.Core;
global using ArchiSteamFarm.Steam;
global using ArchiSteamFarm.Localization;
global using ArchiSteamFarm.Storage;
global using ArchiSteamFarm.Helpers;
global using ArchiSteamFarm.NLog;
global using ArchiSteamFarm.Steam.Storage;
global using ArchiSteamFarm.IPC.Requests;
global using ArchiSteamFarm.IPC.Responses;

global using Strings = ArchiSteamFarm.Localization.Strings;
global using BDStrings = BD.WTTS.Client.Resources.Strings;

global using SteamKit2;

global using MaxLengthAttribute = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

global using JsonConstructorAttribute = Newtonsoft.Json.JsonConstructorAttribute;
global using JsonExtensionDataAttribute = Newtonsoft.Json.JsonExtensionDataAttribute;
global using JsonPropertyAttribute = Newtonsoft.Json.JsonPropertyAttribute;
global using JsonConverter = Newtonsoft.Json.JsonConverter;
global using JsonSerializer = Newtonsoft.Json.JsonSerializer;
global using Formatting = Newtonsoft.Json.Formatting;
global using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;
global using DefaultContractResolver = Newtonsoft.Json.Serialization.DefaultContractResolver;