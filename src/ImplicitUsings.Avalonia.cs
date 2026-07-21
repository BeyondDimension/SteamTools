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
global using ReactiveUI.Avalonia;
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
global using AppWindow = FluentAvalonia.UI.Windowing.FAAppWindow;
global using DrillInNavigationTransitionInfo = FluentAvalonia.UI.Media.Animation.FADrillInNavigationTransitionInfo;
global using EntranceNavigationTransitionInfo = FluentAvalonia.UI.Media.Animation.FAEntranceNavigationTransitionInfo;
global using Frame = FluentAvalonia.UI.Controls.FAFrame;
global using FrameNavigationOptions = FluentAvalonia.UI.Navigation.FAFrameNavigationOptions;
global using IApplicationSplashScreen = FluentAvalonia.UI.Windowing.IFAApplicationSplashScreen;
global using IconSource = FluentAvalonia.UI.Controls.FAIconSource;
global using CommandBar = FluentAvalonia.UI.Controls.FACommandBar;
global using CommandBarButton = FluentAvalonia.UI.Controls.FACommandBarButton;
global using FontIcon = FluentAvalonia.UI.Controls.FAFontIcon;
global using FontIconSource = FluentAvalonia.UI.Controls.FAFontIconSource;
global using ImageIcon = FluentAvalonia.UI.Controls.FAImageIcon;
global using ImageIconSource = FluentAvalonia.UI.Controls.FAImageIconSource;
global using ItemsRepeater = FluentAvalonia.UI.Controls.FAItemsRepeater;
global using MenuFlyoutItem = FluentAvalonia.UI.Controls.FAMenuFlyoutItem;
global using MenuFlyoutSeparator = FluentAvalonia.UI.Controls.FAMenuFlyoutSeparator;
global using MenuFlyoutSubItem = FluentAvalonia.UI.Controls.FAMenuFlyoutSubItem;
global using NumberBox = FluentAvalonia.UI.Controls.FANumberBox;
global using PathIconSource = FluentAvalonia.UI.Controls.FAPathIconSource;
global using ProgressRing = FluentAvalonia.UI.Controls.FAProgressRing;
global using RadioMenuFlyoutItem = FluentAvalonia.UI.Controls.FARadioMenuFlyoutItem;
global using SettingsExpander = FluentAvalonia.UI.Controls.FASettingsExpander;
global using SettingsExpanderItem = FluentAvalonia.UI.Controls.FASettingsExpanderItem;
global using Symbol = FluentAvalonia.UI.Controls.FASymbol;
global using SymbolIcon = FluentAvalonia.UI.Controls.FASymbolIcon;
global using SymbolIconSource = FluentAvalonia.UI.Controls.FASymbolIconSource;
global using TabView = FluentAvalonia.UI.Controls.FATabView;
global using TabViewTabCloseRequestedEventArgs = FluentAvalonia.UI.Controls.FATabViewTabCloseRequestedEventArgs;
global using ToggleMenuFlyoutItem = FluentAvalonia.UI.Controls.FAToggleMenuFlyoutItem;
global using UniformGridLayout = FluentAvalonia.UI.Controls.FAUniformGridLayout;
global using InfoBar = FluentAvalonia.UI.Controls.FAInfoBar;
global using InfoBarClosedEventArgs = FluentAvalonia.UI.Controls.FAInfoBarClosedEventArgs;
global using InfoBarClosingEventArgs = FluentAvalonia.UI.Controls.FAInfoBarClosingEventArgs;
global using InfoBarCloseReason = FluentAvalonia.UI.Controls.FAInfoBarCloseReason;
global using InfoBarSeverity = FluentAvalonia.UI.Controls.FAInfoBarSeverity;
global using NavigationEventArgs = FluentAvalonia.UI.Navigation.FANavigationEventArgs;
global using NavigationTransitionInfo = FluentAvalonia.UI.Media.Animation.FANavigationTransitionInfo;
global using NavigationViewBackRequestedEventArgs = FluentAvalonia.UI.Controls.FANavigationViewBackRequestedEventArgs;
global using NavigationViewItem = FluentAvalonia.UI.Controls.FANavigationViewItem;
global using NavigationViewItemInvokedEventArgs = FluentAvalonia.UI.Controls.FANavigationViewItemInvokedEventArgs;
global using SlideNavigationTransitionEffect = FluentAvalonia.UI.Media.Animation.FASlideNavigationTransitionEffect;
global using SlideNavigationTransitionInfo = FluentAvalonia.UI.Media.Animation.FASlideNavigationTransitionInfo;
global using SuppressNavigationTransitionInfo = FluentAvalonia.UI.Media.Animation.FASuppressNavigationTransitionInfo;
global using TaskDialog = FluentAvalonia.UI.Controls.FATaskDialog;
global using TaskDialogButton = FluentAvalonia.UI.Controls.FATaskDialogButton;
global using TaskDialogClosingEventArgs = FluentAvalonia.UI.Controls.FATaskDialogClosingEventArgs;
global using TaskDialogFooterVisibility = FluentAvalonia.UI.Controls.FATaskDialogFooterVisibility;
global using TaskDialogStandardResult = FluentAvalonia.UI.Controls.FATaskDialogStandardResult;