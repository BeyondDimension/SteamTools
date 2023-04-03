#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using XmlnsDefinition = Avalonia.Metadata.XmlnsDefinitionAttribute;

// steampp
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.Models")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.Models.Abstractions")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.UI.ViewModels")]
[assembly: XmlnsDefinition("https://steampp.net/ui", "BD.WTTS.UI.ViewModels.Abstractions")]
[assembly: XmlnsDefinition("https://steampp.net/services", "BD.WTTS.Services")]
[assembly: XmlnsDefinition("https://steampp.net/settings", "BD.WTTS.Settings")]

#endif