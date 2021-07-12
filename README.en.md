<h1 align="center">Steam++ Tools 🧰</h1>

<div align="center">

「Steam++」is a toolkit that contains a variety of Steam tools, most of which require you to download and install Steam in order to use them.

![Release Download](https://img.shields.io/github/downloads/rmbadmin/SteamTools/total?style=flat-square)
[![Release Version](https://img.shields.io/github/v/release/rmbadmin/SteamTools?style=flat-square)](https://github.com/rmbadmin/SteamTools/releases/latest)
[![GitHub license](https://img.shields.io/github/license/rmbadmin/SteamTools?style=flat-square)](LICENSE)
[![GitHub Star](https://img.shields.io/github/stars/rmbadmin/SteamTools?style=flat-square)](https://github.com/rmbadmin/SteamTools/stargazers)
[![GitHub Fork](https://img.shields.io/github/forks/rmbadmin/SteamTools?style=flat-square)](https://github.com/rmbadmin/SteamTools/network/members)
![GitHub Repo size](https://img.shields.io/github/repo-size/rmbadmin/SteamTools?style=flat-square&color=3cb371)
[![GitHub Repo Languages](https://img.shields.io/github/languages/top/SteamTools-Team/SteamTools?style=flat-square)](https://github.com/SteamTools-Team/SteamTools/search?l=c%23)
[![NET 6.0](https://img.shields.io/badge/dotnet-6.0-purple.svg?style=flat-square&color=512bd4)](https://devblogs.microsoft.com/dotnet/announcing-net-6-preview-5)
[![C# 9.0](https://img.shields.io/badge/c%23-9.0-green.svg?style=flat-square&color=6da86a)](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9)

[![Desktop UI](https://img.shields.io/badge/ui@desktop-AvaloniaUI-purple.svg?style=flat-square&color=8c45ab)](https://github.com/AvaloniaUI/Avalonia)
![Mobile UI](https://img.shields.io/badge/ui@mobile-Platform_Native_UI-blue.svg?style=flat-square&color=3498db)
[![Official WebSite](https://img.shields.io/badge/website@official-Ant%20Design%20of%20React-blue.svg?style=flat-square&color=61dafb)](https://github.com/ant-design/ant-design)
[![BackManage WebSite](https://img.shields.io/badge/website@back_manage-Ant%20Design%20of%20Blazor-purple.svg?style=flat-square&color=512bd4)](https://github.com/ant-design-blazor/ant-design-blazor)

[![Build Status](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Factions-badge.atrox.dev%2FSteamTools-Team%2FSteamTools%2Fbadge%3Fref%3Ddevelop&style=flat-square)](https://actions-badge.atrox.dev/SteamTools-Team/SteamTools/goto?ref=develop)
[![GitHub Star](https://img.shields.io/github/stars/rmbadmin/SteamTools.svg?style=social)](https://github.com/SteamTools-Team/SteamTools)
[![Gitee Star](https://gitee.com/rmbgame/SteamTools/badge/star.svg)](https://gitee.com/rmbgame/SteamTools)
</div>

<div align="center"><img src="./resources/screenshots.en.jpg" /></div>

English | [简体中文](./README.md)

## ✨ Functions
1. Local reverse proxy for Steam's community webpage to enable normal access in **the Chinese Mainland, Mainland of China**
2. Quickly switch the Steam account that the current PC has remembered to log into
	- This feature is to read the local user login records stored under the Steam path to directly display the operation, you can switch between multiple accounts without re-entering passwords and tokens.
3. Achievement stat modifications for Steam games
	- Function reference SteamAchievementManager for secondary development, modified the game list loading and operation ease of use. 
4. Steam Local Two-Step Authenticator
    - The function reference [WinAuth](https://github.com/winauth/winauth) is developed to enable you to view your token without launching the mobile version of Steam App, similar function software are [WinAuth](https://github.com/winauth/winauth), [SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator).
5. Some game tools
	- Currently there is a forced game borderless windowing, CSGO fix mistake VAC shield.

<!--Prerequisites Microsoft Visual C++ 2015-2019 Redistributable [64 bit](https://aka.ms/vs/16/release/vc_redist.x64.exe) / [32 bit](https://aka.ms/vs/16/release/vc_redist.x86.exe)-->
## 🖥 Supported Operating Systems
- Desktop(Only X64 and ARM64 are supported)
	- Windows 10 1607+ / Windows Server, version 1909+ / Windows Server 2019 / Windows Server 2016
	- Windows 8.1 / Windows Server 2012 R2
	- Windows 7 SP1 [ESU](https://docs.microsoft.com/troubleshoot/windows-client/windows-7-eos-faq/windows-7-extended-security-updates-faq)
		- Prerequisites
		- KB3063858 [64 bit](https://www.microsoft.com/download/details.aspx?id=47442) / [32 bit](https://www.microsoft.com/download/details.aspx?id=47409)
	- macOS 10.14 Mojave Or Higher
	- Linux Distribution
		- Arch Linux
		- Alpine Linux 3.13+
		- CentOS 7+
		- Debian 10+
		- Deepin 20.1 / UOS 20
		- Fedora 32+
		- Linux Mint 18+
		- openSUSE 15+
		- Red Hat Enterprise Linux 7+
		- SUSE Enterprise Linux (SLES) 12 SP2+
		- Ubuntu 16.04, 18.04, 20.04+
- Mobile
	- Android 5.0/API 21+ (Only [arm64-v8a](https://developer.android.google.cn/ndk/guides/abis?hl=zh_cn#arm64-v8a) and [armeabiv-v7a](https://developer.android.google.cn/ndk/guides/abis?hl=zh_cn#v7a) are supported)
	- iOS 10.0+ (ARM64 only)

## ⛔ Unsupported Operating System
- Windows 8
	- [Due to Microsoft's official support for the product has ended](https://docs.microsoft.com/lifecycle/products/windows-8), so this program cannot run on this operating system, [It is recommended to upgrade to Windows 8.1](https://support.microsoft.com/windows/update-to-windows-8-1-from-windows-8-17fc54a7-a465-6b5a-c1a0-34140afd0669)
- Windows Server 2012 / 2008 R2 SP1
	- Only version 1.X is available, and version 2.X is not supported. It is recommended to upgrade to **Windows Server 2012 R2** or higher
- Windows Server / Linux version without desktop GUI

## 🌏 Roadmap
Read what we [milestones](https://github.com/SteamTools-Team/SteamTools/milestones), and feel free to ask questions.

## ⌨️ Development Environment
[Visual Studio 2019 Version 16.10 Or Higher](https://visualstudio.microsoft.com/vs/) Or [JetBrains Rider](https://www.jetbrains.com/rider/) Or ~~[Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/)~~ Or ~~[Visual Studio Code](https://code.visualstudio.com/)~~
- Supported Operating Systems
	- [Windows 10 version 2004 or higher: Home, Professional, Education, and Enterprise (LTSC and S are not supported)](https://docs.microsoft.com/en-us/visualstudio/releases/2019/system-requirements)
	- [macOS 10.13 High Sierra Or Higher](https://docs.microsoft.com/en-us/visualstudio/productinfo/vs2019-system-requirements-mac)
- Workload
	- Web and Cloud
		- ASP.NET and Web Development
	- Desktop and Mobile Applications
		- .NET Desktop Development
		- UWP Development
		- Mobile Development using .Net
	- Other Toolsets
		- .NET Core Cross Platform Development
- Single Component
	- GitHub Extension for Visual Studio
	- Windows 10 SDK (10.0.19041.0)
- [Visual Studio Marketplace](https://marketplace.visualstudio.com/)
	- [Avalonia for Visual Studio](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaforVisualStudio)
	- [NUnit VS Templates](https://marketplace.visualstudio.com/items?itemName=NUnitDevelopers.NUnitTemplatesforVisualStudio)

[Android Studio 4.2+](https://developer.android.com/studio/)  
[Xcode 13](https://developer.apple.com/xcode/)

## 📄 Thanks to the following Open Source Projects
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
* [MetroRadiance](https://github.com/Grabacr07/MetroRadiance)
* [MetroTrilithon](https://github.com/Grabacr07/MetroTrilithon)
* [Livet](https://github.com/runceel/Livet)
* [StatefulModel](https://github.com/ugaya40/StatefulModel)
* [Hardcodet.NotifyIcon](https://github.com/HavenDV/Hardcodet.NotifyIcon.Wpf.NetCore)
* [System.Reactive](https://github.com/dotnet/reactive)
* [Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)
* [BrotliSharpLib](https://github.com/master131/BrotliSharpLib)
* [Portable.BouncyCastle](https://github.com/novotnyllc/bc-csharp)
* [Ninject](https://github.com/ninject/Ninject)
* [log4net](https://github.com/apache/logging-log4net)
* [SteamDB-API](https://github.com/SteamDB-API/api)
* [SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager)
* [ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm)
* [Steam4NET](https://github.com/SteamRE/Steam4NET)
* [WinAuth](https://github.com/winauth/winauth)
* [SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)
* [Gameloop.Vdf](https://github.com/shravan2x/Gameloop.Vdf)
* [DnsClient.NET](https://github.com/MichaCo/DnsClient.NET)
* [Costura.Fody](https://github.com/Fody/Costura)
* [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp)
* [Nito.Comparers](https://github.com/StephenCleary/Comparers)
* [Crc32.NET](https://github.com/force-net/Crc32.NET)
* [gfoidl.Base64](https://github.com/gfoidl/Base64)
* [sqlite-net-pcl](https://github.com/praeclarum/sqlite-net)
* [Polly](https://github.com/App-vNext/Polly)
* [TaskScheduler](https://github.com/dahall/taskscheduler)
* [SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
* [LibVLCSharp](https://github.com/videolan/libvlcsharp)
* [Depressurizer](https://github.com/Depressurizer/Depressurizer)
* [NLog](https://github.com/nlog/NLog)
* [NUnit](https://github.com/nunit/nunit)
* [ReactiveUI](https://github.com/reactiveui/reactiveui)
* [MessageBox.Avalonia](https://github.com/AvaloniaUtils/MessageBox.Avalonia)
* [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
* [AvaloniaGif](https://github.com/jmacato/AvaloniaGif)
* [Avalonia XAML Behaviors](https://github.com/wieslawsoltes/AvaloniaBehaviors)
* [APNG.NET](https://github.com/jz5/APNG.NET)
* [Chromium Embedded Framework (CEF)](https://github.com/chromiumembedded/cef)
* [Moq](https://github.com/moq/moq4)
* [NPOI](https://github.com/nissl-lab/npoi)
* [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
* [AspNet.Security.OpenId.Providers](https://github.com/aspnet-contrib/AspNet.Security.OpenId.Providers)
* [AspNet.Security.OAuth.Providers](https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers)
* [CefNet](https://github.com/CefNet/CefNet)
* [CefSharp](https://github.com/cefsharp/CefSharp)
* [React](https://github.com/facebook/react)
* [Ant Design](https://github.com/ant-design/ant-design)
* [Ant Design Blazor](https://github.com/ant-design-blazor/ant-design-blazor)
* [Toast messages for Xamarin.iOS](https://github.com/andrius-k/Toast)
* [Floating Action Button Speed Dial](https://github.com/leinardi/FloatingActionButtonSpeedDial)
* [Visual Studio App Center SDK for .NET](https://github.com/microsoft/appcenter-sdk-dotnet)
* [AppCenter-XMac](https://github.com/nor0x/AppCenter-XMac)
* [MSBuild.Sdk.Extras](https://github.com/novotnyllc/MSBuildSdkExtras)
* [Xamarin.Essentials](https://github.com/xamarin/essentials)
* [Open Source Components for Xamarin](https://github.com/xamarin/XamarinComponents)
* [Picasso](https://github.com/square/picasso)
* [OkHttp](https://github.com/square/okhttp)
* [Material Components for Android](https://github.com/material-components/material-components-android)
* [AndroidX for Xamarin.Android](https://github.com/xamarin/AndroidX)
* [Android Jetpack](https://github.com/androidx/androidx)
* [ConstraintLayout](https://github.com/androidx/constraintlayout)
* [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet)
* [Entity Framework Core](https://github.com/dotnet/efcore)
* [ASP.NET Core](https://github.com/dotnet/aspnetcore)
* [Windows Forms](https://github.com/dotnet/winforms)
* [Windows Presentation Foundation (WPF)](https://github.com/dotnet/wpf)
* [C#/WinRT](https://github.com/microsoft/CsWinRT)
* [command-line-api](https://github.com/dotnet/command-line-api)
* [.NET Runtime](https://github.com/dotnet/runtime)
* [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons)
* [Material design icons](https://github.com/google/material-design-icons)
