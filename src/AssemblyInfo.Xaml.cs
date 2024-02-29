#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using Avalonia.Metadata;

// steampp
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.Enums")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.Models")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.Models.Abstractions")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.UI.ViewModels")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.UI.ViewModels.Abstractions")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.UI.Views")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.UI.Styling")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.UI.Views.Controls")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.UI.Views.Pages")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.UI.Views.Windows")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.Converters")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.Markup")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.Client.Resources")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "Mobius.Models")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "Mobius")]

[assembly: XmlnsDefinition("https://steampp.net/services", "BD.WTTS.Services")]
[assembly: XmlnsDefinition("https://steampp.net/services", "BD.WTTS.Plugins")]
[assembly: XmlnsDefinition("https://steampp.net/services", "BD.WTTS.Plugins.Abstractions")]
[assembly: XmlnsDefinition("https://steampp.net/settings", "BD.WTTS.Settings")]

#endif