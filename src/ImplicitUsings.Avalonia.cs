// C# 10 定义全局 using

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0005
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
#pragma warning disable SA1211 // Using alias directives should be ordered alphabetically by alias name

global using Avalonia;
global using Avalonia.Animation;
global using Avalonia.Controls;
global using Avalonia.Controls.Platform;
global using Avalonia.Controls.ApplicationLifetimes;
global using Avalonia.Controls.Primitives;
global using Avalonia.Controls.Presenters;
global using Avalonia.Controls.Metadata;
global using Avalonia.Controls.Templates;
global using Avalonia.Collections;
global using Avalonia.Data;
global using Avalonia.Data.Converters;
global using Avalonia.Metadata;
global using Avalonia.Markup;
global using Avalonia.Markup.Xaml;
global using Avalonia.Markup.Xaml.Styling;
global using Avalonia.Platform;
global using Avalonia.Media;
global using Avalonia.Media.Imaging;
global using Avalonia.Media.Immutable;
global using Avalonia.Threading;
global using Avalonia.Visuals;
global using Avalonia.Styling;
global using Avalonia.Input;
#if _IMPORT_AVALONIA_GIF__
global using AvaloniaGif;
global using AvaloniaGif.Decoding;
#endif
global using Avalonia.Utilities;
global using Avalonia.Logging;
global using Avalonia.VisualTree;
global using Avalonia.Interactivity;
global using Avalonia.Skia;
global using Avalonia.Layout;
global using Avalonia.LogicalTree;
global using Avalonia.Rendering;
global using Avalonia.ReactiveUI;
global using Avalonia.Platform.Storage;
global using Cursor = Avalonia.Input.Cursor;
global using KeyboardNavigation = Avalonia.Input.KeyboardNavigation;
global using Key = Avalonia.Input.Key;

// FluentAvaloniaUI
global using FluentAvalonia.Interop;
global using FluentAvalonia.Styling;
global using FluentAvalonia.UI.Controls;
global using FluentAvalonia.UI.Windowing;
global using FluentAvalonia.UI;
global using FluentAvalonia.UI.Media;