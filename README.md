<h1 align="center">Steam++ 工具箱 🧰</h1>

<div align="center">

[English](./README.en.md) | 简体中文

「Steam++」是一个包含多种Steam工具功能的工具箱，此工具的大部分功能都是需要您下载安装Steam才能使用。

![Release Download](https://img.shields.io/github/downloads/BeyondDimension/SteamTools/total?style=flat-square)
[![Release Version](https://img.shields.io/github/v/release/BeyondDimension/SteamTools?style=flat-square)](https://github.com/BeyondDimension/SteamTools/releases/latest)
[![GitHub license](https://img.shields.io/github/license/BeyondDimension/SteamTools?style=flat-square)](LICENSE)
[![GitHub Star](https://img.shields.io/github/stars/BeyondDimension/SteamTools?style=flat-square)](https://github.com/BeyondDimension/SteamTools/stargazers)
[![GitHub Fork](https://img.shields.io/github/forks/BeyondDimension/SteamTools?style=flat-square)](https://github.com/BeyondDimension/SteamTools/network/members)
![GitHub Repo size](https://img.shields.io/github/repo-size/BeyondDimension/SteamTools?style=flat-square&color=3cb371)
[![GitHub Repo Languages](https://img.shields.io/github/languages/top/BeyondDimension/SteamTools?style=flat-square)](https://github.com/BeyondDimension/SteamTools/search?l=c%23)
[![NET 6.0](https://img.shields.io/badge/dotnet-6.0-purple.svg?style=flat-square&color=512bd4)](https://docs.microsoft.com/zh-cn/dotnet/core/whats-new/dotnet-6)
[![C# 10.0](https://img.shields.io/badge/c%23-10.0-brightgreen.svg?style=flat-square&color=6da86a)](https://docs.microsoft.com/zh-cn/dotnet/csharp/whats-new/csharp-10)

[![Desktop UI](https://img.shields.io/badge/ui@desktop-AvaloniaUI-purple.svg?style=flat-square&color=8c45ab)](https://github.com/AvaloniaUI/Avalonia)
[![Mobile GUI](https://img.shields.io/badge/gui@mobile-Xamarin.Forms-blue.svg?style=flat-square&color=3498db)](https://github.com/xamarin/Xamarin.Forms)
[![Official WebSite](https://img.shields.io/badge/website@official-Ant%20Design%20of%20React-blue.svg?style=flat-square&color=61dafb)](https://github.com/ant-design/ant-design)
[![BackManage WebSite](https://img.shields.io/badge/website@back_manage-Ant%20Design%20of%20Blazor-purple.svg?style=flat-square&color=512bd4)](https://github.com/ant-design-blazor/ant-design-blazor)

[![Build Status](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Factions-badge.atrox.dev%2FSteamTools-Team%2FSteamTools%2Fbadge%3Fref%3Ddevelop&style=flat-square)](https://actions-badge.atrox.dev/BeyondDimension/SteamTools/goto?ref=develop)
[![GitHub Star](https://img.shields.io/github/stars/BeyondDimension/SteamTools.svg?style=social)](https://github.com/BeyondDimension/SteamTools)
[![Gitee Star](https://gitee.com/rmbgame/SteamTools/badge/star.svg)](https://gitee.com/rmbgame/SteamTools)
[![QQ群](https://img.shields.io/badge/QQ群-960746023-blue.svg?style=flat-square&color=12b7f5)](https://jq.qq.com/?_wv=1027&k=3JKPt4xC)
</div>

<div align="center"><img src="./resources/screenshots.jpg" /></div>

## 🚀 下载渠道
- [GitHub Releases](https://github.com/BeyondDimension/SteamTools/releases)
- [Gitee Releases](https://gitee.com/rmbgame/SteamTools/releases)
- [Official WebSite](https://steampp.net)
- [![Microsoft Store](./resources/MSStore_English.png)](https://www.microsoft.com/store/apps/9MTCFHS560NG)
- [Arch Linux Package](https://aur.archlinux.org/packages/steam%2B%2B-bin)(By [zhanghua000](https://github.com/zhanghua000))

## ✨ 功能
1. 反代 Steam 的社区网页使其能正常访问
	- 功能类似羽翼城大佬的 [steamcommunity_302](https://www.dogfight360.com/blog/686/)
	- 使用 [Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy) 开源项目进行本地反代，使国内用户可以正常访问 Steam 社区页
	- 相比 **302工具** 具有更快的启动速度，以及支持简单的脚本注入（还顺便支持了Pixiv、Discord、Twitch等网站的反代支持）
2. 快速切换当前设备已记住登陆的 Steam 账号
	- 该功能是读取 Steam 路径下存储的本地用户登录记录直接展示操作，可以多账号切换无需重新输入密码和令牌
3. Steam游戏的成就统计管理功能
	- 功能参考 [SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager) 进行二次开发，修改了游戏列表的加载和操作易用性
4. Steam本地两步身份验证器
	- 功能参考 [WinAuth](https://github.com/winauth/winauth) 开发，可以使您不用启动移动版 Steam App 也能查看您的令牌
	- 本地令牌交易市场报价确认
	- 功能类似的软件例如：
		- [WinAuth](https://github.com/winauth/winauth)
		- [SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)
5. 其他游戏工具
	- 目前已有强制游戏无边框窗口化

<!--发布配置SelfContained=true时会自动打包VC++相关程序集-->
<!--先决条件 Microsoft Visual C++ 2015-2019 Redistributable [64 位](https://aka.ms/vs/16/release/vc_redist.x64.exe) / [32 位](https://aka.ms/vs/16/release/vc_redist.x86.exe)-->
## 🖥 系统要求
### Windows

OS                                    | Version                 | Architectures   | Lifecycle
--------------------------------------|-------------------------|-----------------|----------
[Windows Client][Windows-client]      | 7 SP1(**\***), 8.1      | x64        | [Windows][Windows-lifecycle]
[Windows 10 Client][Windows-client]   | Version 1607+(**\***)   | x64        | [Windows][Windows-lifecycle]
[Windows Server][Windows-Server]      | 2012+                   | x64        | [Windows Server][Windows-Server-lifecycle]

**\*** Windows 7 SP1 is supported with [Extended Security Updates](https://docs.microsoft.com/troubleshoot/windows-client/windows-7-eos-faq/windows-7-extended-security-updates-faq) installed.  
**\*** Microsoft Store/Desktop Bridge Version 1809+

[Windows-client]: https://www.microsoft.com/windows/
[Windows-lifecycle]: https://support.microsoft.com/help/13853/windows-lifecycle-fact-sheet
[win-client-docker]: https://hub.docker.com/_/microsoft-windows
[Windows-Server-lifecycle]: https://docs.microsoft.com/windows-server/get-started/windows-server-release-info
[Nano-Server]: https://docs.microsoft.com/windows-server/get-started/getting-started-with-nano-server
[Windows-Server]: https://docs.microsoft.com/windows-server/

### Linux

OS                                    | Version               | Architectures     | Lifecycle
--------------------------------------|-----------------------|-------------------|----------
[Alpine Linux][Alpine]                | 3.13+                 | x64, Arm64        | [Alpine][Alpine-lifecycle]
[CentOS][CentOS]                      | 7+                    | x64               | [CentOS][CentOS-lifecycle]
[Debian][Debian]                      | 10+                   | x64, Arm64        | [Debian][Debian-lifecycle]
[Fedora][Fedora]                      | 33+                   | x64               | [Fedora][Fedora-lifecycle]
[openSUSE][OpenSUSE]                  | 15+                   | x64               | [OpenSUSE][OpenSUSE-lifecycle]
[Red Hat Enterprise Linux][RHEL]      | 7+                    | x64, Arm64        | [Red Hat][RHEL-lifecycle]
[SUSE Enterprise Linux (SLES)][SLES]  | 12 SP2+               | x64               | [SUSE][SLES-lifecycle]
[Ubuntu][Ubuntu]                      | 16.04, 18.04, 20.04+  | x64, Arm64        | [Ubuntu][Ubuntu-lifecycle]
[Deepin / UOS][Deepin]                | 20+                   | x64               | [Deepin][Deepin-lifecycle]
[Arch Linux][Arch]                    |                       | x64               | 

[Alpine]: https://alpinelinux.org/
[Alpine-lifecycle]: https://wiki.alpinelinux.org/wiki/Alpine_Linux:Releases
[CentOS]: https://www.centos.org/
[CentOS-lifecycle]:https://wiki.centos.org/FAQ/General
[CentOS-docker]: https://hub.docker.com/_/centos
[CentOS-pm]: https://docs.microsoft.com/dotnet/core/install/linux-package-manager-centos8
[Debian]: https://www.debian.org/
[Debian-lifecycle]: https://wiki.debian.org/DebianReleases
[Debian-pm]: https://docs.microsoft.com/dotnet/core/install/linux-package-manager-debian10
[Fedora]: https://getfedora.org/
[Fedora-lifecycle]: https://fedoraproject.org/wiki/End_of_life
[Fedora-docker]: https://hub.docker.com/_/fedora
[Fedora-msft-pm]: https://docs.microsoft.com/dotnet/core/install/linux-package-manager-fedora32
[Fedora-pm]: https://fedoraproject.org/wiki/DotNet
[OpenSUSE]: https://opensuse.org/
[OpenSUSE-lifecycle]: https://en.opensuse.org/Lifetime
[OpenSUSE-docker]: https://hub.docker.com/r/opensuse/leap
[OpenSUSE-pm]: https://docs.microsoft.com/dotnet/core/install/linux-package-manager-opensuse15
[RHEL]: https://www.redhat.com/en/technologies/linux-platforms/enterprise-linux
[RHEL-lifecycle]: https://access.redhat.com/support/policy/updates/errata/
[RHEL-msft-pm]: https://docs.microsoft.com/dotnet/core/install/linux-package-manager-rhel8
[RHEL-pm]: https://access.redhat.com/documentation/en-us/red_hat_enterprise_linux/8/html/developing_.net_applications_in_rhel_8/using-net-core-on-rhel_gsg#installing-net-core_gsg
[SLES]: https://www.suse.com/products/server/
[SLES-lifecycle]: https://www.suse.com/lifecycle/
[SLES-pm]: https://docs.microsoft.com/dotnet/core/install/linux-package-manager-sles15
[Ubuntu]: https://ubuntu.com/
[Ubuntu-lifecycle]: https://wiki.ubuntu.com/Releases
[Ubuntu-pm]: https://docs.microsoft.com/dotnet/core/install/linux-package-manager-ubuntu-2004
[Deepin]: https://www.deepin.org/
[Deepin-lifecycle]: https://www.deepin.org/release-notes
[Arch]: https://archlinux.org/

### macOS

OS                            | Version                   | Architectures     |
------------------------------|---------------------------|-------------------|
[macOS][macOS]                | 10.14+                    | x64               |

[macOS]: https://support.apple.com/macos

OS                            | Version                 | Architectures                                                      |
------------------------------|-------------------------|--------------------------------------------------------------------|
[Android][Android]            | 5.0(API 21)+            | [x64][Android-x64], [Arm64][Android-Arm64], [Arm32][Android-Arm32] |

[Android]: https://support.google.com/android
[Android-x64]: https://developer.android.google.cn/ndk/guides/abis?hl=zh_cn#86-64
[Android-Arm32]: https://developer.android.google.cn/ndk/guides/abis?hl=zh_cn#v7a
[Android-Arm64]: https://developer.android.google.cn/ndk/guides/abis?hl=zh_cn#arm64-v8a

### ~~iOS / iPadOS~~

OS                            | Version                 | Architectures     |
------------------------------|-------------------------|-------------------|
[iOS][iOS]                    | 10.0+                   | x64, Arm32, Arm64 |

[iOS]: https://support.apple.com/ios

## ⛔ 不受支持的操作系统
- Windows 8
	- [由于微软官方对该产品的支持已结束](https://docs.microsoft.com/zh-cn/lifecycle/products/windows-8)，故本程序无法在此操作系统上运行，[建议升级到 Windows 8.1](https://support.microsoft.com/zh-cn/windows/%E4%BB%8E-windows-8-%E6%9B%B4%E6%96%B0%E5%88%B0-windows-8-1-17fc54a7-a465-6b5a-c1a0-34140afd0669)
- Windows Server 2008 R2 SP1
	- 仅可使用 1.X 版本，2.X 开始不受支持，建议升级到更高版本
- 无桌面 GUI 的 Windows Server / Linux 版本
- Xbox or Windows Mobile / Phone

## 🌏 路线图
查看这个 [milestones](https://github.com/BeyondDimension/SteamTools/milestones) 来了解我们下一步的开发计划，并随时提出问题。

## ⌨️ 开发环境
[Visual Studio 2022](https://visualstudio.microsoft.com/zh-hans/vs/)   
[JetBrains Rider](https://www.jetbrains.com/rider/)  
~~[Visual Studio 2022 for Mac Preview](https://visualstudio.microsoft.com/zh-hans/vs/mac/)~~  
~~[Visual Studio Code](https://code.visualstudio.com/)~~
- 系统要求
	- [Windows 10 版本 2004 或更高版本：家庭版、专业版、教育版和企业版（不支持 LTSC 和 Windows 10 S，在较早的操作系统上可能不受支持）](https://docs.microsoft.com/zh-cn/visualstudio/releases/2019/system-requirements)
	- [macOS 10.14 Mojave 或更高版本](https://docs.microsoft.com/zh-cn/visualstudio/productinfo/vs2019-system-requirements-mac)
- 工作负载
	- Web 和云
		- ASP.NET 和 Web 开发
	- 桌面应用和移动应用
		- .NET 桌面开发
		- 通用 Windows 平台开发
		- 使用 .NET 的移动开发
	- 其他工具集
		- .NET Core 跨平台开发
- 单个组件
	- GitHub Extension for Visual Studio
	- Windows 10 SDK (10.0.19041.0)
- [Visual Studio Marketplace](https://marketplace.visualstudio.com/)
	- [Avalonia for Visual Studio](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaforVisualStudio)
	- [NUnit VS Templates](https://marketplace.visualstudio.com/items?itemName=NUnitDevelopers.NUnitTemplatesforVisualStudio)

[OpenJDK 11](https://docs.microsoft.com/zh-cn/java/openjdk/download#openjdk-11)  
[Android Studio 2021.1.1 或更高版本](https://developer.android.google.cn/studio/)  
[Xcode 13 或更高版本](https://developer.apple.com/xcode/)

## 🏗️ [项目结构](./src/README.md)

<!--
* [LibVLCSharp](https://github.com/videolan/libvlcsharp)
* [Chromium Embedded Framework (CEF)](https://github.com/chromiumembedded/cef)
* [CefNet](https://github.com/CefNet/CefNet)
* [CefSharp](https://github.com/cefsharp/CefSharp)
* [ZXing.Net.Mobile](https://github.com/Redth/ZXing.Net.Mobile)
* [Floating Action Button Speed Dial](https://github.com/leinardi/FloatingActionButtonSpeedDial)
-->
<!--👇图标如果发生更改，还需更改 Tools.OpenSourceLibraryList(Program.OpenSourceLibraryListEmoji) -->
## 📄 感谢以下开源项目
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
* [MetroRadiance](https://github.com/Grabacr07/MetroRadiance)
* [MetroTrilithon](https://github.com/Grabacr07/MetroTrilithon)
* [Livet](https://github.com/runceel/Livet)
* [StatefulModel](https://github.com/ugaya40/StatefulModel)
* [Hardcodet.NotifyIcon](https://github.com/HavenDV/Hardcodet.NotifyIcon.Wpf.NetCore)
* [System.Reactive](https://github.com/dotnet/reactive)
* [Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)
* [Portable.BouncyCastle](https://github.com/novotnyllc/bc-csharp)
* [Ninject](https://github.com/ninject/Ninject)
* [log4net](https://github.com/apache/logging-log4net)
* [SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager)
* [ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm)
* [Steam4NET](https://github.com/SteamRE/Steam4NET)
* [WinAuth](https://github.com/winauth/winauth)
* [SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)
* [Gameloop.Vdf](https://github.com/shravan2x/Gameloop.Vdf)
* [DnsClient.NET](https://github.com/MichaCo/DnsClient.NET)
* [Costura.Fody](https://github.com/Fody/Costura)
* [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp)
* [CSharpVitamins.ShortGuid](https://github.com/AigioL/CSharpVitamins.ShortGuid)
* [Nito.Comparers](https://github.com/StephenCleary/Comparers)
* [Nito.Disposables](https://github.com/StephenCleary/Disposables)
* [Crc32.NET](https://github.com/force-net/Crc32.NET)
* [gfoidl.Base64](https://github.com/gfoidl/Base64)
* [sqlite-net-pcl](https://github.com/praeclarum/sqlite-net)
* [AutoMapper](https://github.com/AutoMapper/AutoMapper)
* [Polly](https://github.com/App-vNext/Polly)
* [TaskScheduler](https://github.com/dahall/taskscheduler)
* [SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
* [SevenZipSharp](https://github.com/squid-box/SevenZipSharp)
* [ZstdNet](https://github.com/skbkontur/ZstdNet)
* [Depressurizer](https://github.com/Depressurizer/Depressurizer)
* [NLog](https://github.com/nlog/NLog)
* [NUnit](https://github.com/nunit/nunit)
* [ReactiveUI](https://github.com/reactiveui/reactiveui)
* [MessageBox.Avalonia](https://github.com/AvaloniaUtils/MessageBox.Avalonia)
* [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
* [AvaloniaGif](https://github.com/jmacato/AvaloniaGif)
* [Avalonia XAML Behaviors](https://github.com/wieslawsoltes/AvaloniaBehaviors)
* [FluentAvalonia](https://github.com/amwx/FluentAvalonia)
* [APNG.NET](https://github.com/jz5/APNG.NET)
* [Moq](https://github.com/moq/moq4)
* [NPOI](https://github.com/nissl-lab/npoi)
* [Fleck](https://github.com/statianzo/Fleck)
* [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
* [AspNet.Security.OpenId.Providers](https://github.com/aspnet-contrib/AspNet.Security.OpenId.Providers)
* [AspNet.Security.OAuth.Providers](https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers)
* [ZXing.Net](https://github.com/micjahn/ZXing.Net)
* [QRCoder](https://github.com/codebude/QRCoder)
* [QR Code Generator for .NET](https://github.com/manuelbl/QrCodeGenerator)
* [TinyPinyin](https://github.com/promeG/TinyPinyin)
* [TinyPinyin.Net](https://github.com/hueifeng/TinyPinyin.Net)
* [Packaging utilities for .NET Core](https://github.com/qmfrederik/dotnet-packaging)
* [React](https://github.com/facebook/react)
* [Ant Design](https://github.com/ant-design/ant-design)
* [Ant Design Blazor](https://github.com/ant-design-blazor/ant-design-blazor)
* [Toast messages for Xamarin.iOS](https://github.com/andrius-k/Toast)
* [ImageCirclePlugin](https://github.com/jamesmontemagno/ImageCirclePlugin)
* [Visual Studio App Center SDK for .NET](https://github.com/microsoft/appcenter-sdk-dotnet)
* [AppCenter-XMac](https://github.com/nor0x/AppCenter-XMac)
* [MSBuild.Sdk.Extras](https://github.com/novotnyllc/MSBuildSdkExtras)
* [Xamarin.Essentials](https://github.com/xamarin/essentials)
* [Xamarin.Forms](https://github.com/xamarin/Xamarin.Forms)
* [Open Source Components for Xamarin](https://github.com/xamarin/XamarinComponents)
* [Google Play Services / Firebase / ML Kit for Xamarin.Android](https://github.com/xamarin/GooglePlayServicesComponents)
* [Picasso](https://github.com/square/picasso)
* [OkHttp](https://github.com/square/okhttp)
* [Material Components for Android](https://github.com/material-components/material-components-android)
* [AndroidX for Xamarin.Android](https://github.com/xamarin/AndroidX)
* [Android Jetpack](https://github.com/androidx/androidx)
* [ConstraintLayout](https://github.com/androidx/constraintlayout)
* [Entity Framework Core](https://github.com/dotnet/efcore)
* [ASP.NET Core](https://github.com/dotnet/aspnetcore)
* [Windows Forms](https://github.com/dotnet/winforms)
* [Windows Presentation Foundation (WPF)](https://github.com/dotnet/wpf)
* [C#/WinRT](https://github.com/microsoft/CsWinRT)
* [command-line-api](https://github.com/dotnet/command-line-api)
* [.NET Runtime](https://github.com/dotnet/runtime)
* [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons)
* [Material design icons](https://github.com/google/material-design-icons)
