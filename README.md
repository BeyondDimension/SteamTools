<h1 align="center">Watt Toolkit 🧰 (原名 Steam++)</h1>

<div align="center">

[English](./README.en.md) | 简体中文

「Watt Toolkit」是一个开源跨平台的多功能游戏工具箱，此工具的大部分功能都是需要您下载安装 Steam 才能使用。

![Release Download](https://img.shields.io/github/downloads/BeyondDimension/SteamTools/total?style=flat-square)
[![Release Version](https://img.shields.io/github/v/release/BeyondDimension/SteamTools?style=flat-square)](https://github.com/BeyondDimension/SteamTools/releases/latest)
[![GitHub license](https://img.shields.io/github/license/BeyondDimension/SteamTools?style=flat-square)](LICENSE)
[![GitHub Star](https://img.shields.io/github/stars/BeyondDimension/SteamTools?style=flat-square)](https://github.com/BeyondDimension/SteamTools/stargazers)
[![GitHub Fork](https://img.shields.io/github/forks/BeyondDimension/SteamTools?style=flat-square)](https://github.com/BeyondDimension/SteamTools/network/members)
![GitHub Repo size](https://img.shields.io/github/repo-size/BeyondDimension/SteamTools?style=flat-square&color=3cb371)
[![GitHub Repo Languages](https://img.shields.io/github/languages/top/BeyondDimension/SteamTools?style=flat-square)](https://github.com/BeyondDimension/SteamTools/search?l=c%23)
[![NET 7.0](https://img.shields.io/badge/dotnet-7.0-purple.svg?style=flat-square&color=512bd4)](https://learn.microsoft.com/zh-cn/dotnet/core/whats-new/dotnet-7)
[![C# 11](https://img.shields.io/badge/c%23-11-brightgreen.svg?style=flat-square&color=6da86a)](https://learn.microsoft.com/zh-cn/dotnet/csharp/whats-new/csharp-11)

[![爱发电](https://img.shields.io/badge/爱发电-软妹币玩家-blue.svg?style=flat-square&color=ea4aaa&logo=github-sponsors)](https://afdian.net/@rmbgame)
[![Kofi](https://img.shields.io/badge/Kofi-RMBGAME-orange.svg?style=flat-square&logo=kofi)](https://ko-fi.com/rmbgame)
[![Patreon](https://img.shields.io/badge/Patreon-RMBGAME-red.svg?style=flat-square&logo=patreon)](https://www.patreon.com/rmbgame)

[![Build Status](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Factions-badge.atrox.dev%2FSteamTools-Team%2FSteamTools%2Fbadge%3Fref%3Ddevelop&style=flat-square)](https://actions-badge.atrox.dev/BeyondDimension/SteamTools/goto?ref=develop)
[![GitHub Star](https://img.shields.io/github/stars/BeyondDimension/SteamTools.svg?logo=github)](https://github.com/BeyondDimension/SteamTools)
[![Gitee Star](https://gitee.com/rmbgame/SteamTools/badge/star.svg)](https://gitee.com/rmbgame/SteamTools)
[![Bilibili](https://img.shields.io/badge/bilibili-软妹币玩家-blue.svg?style=flat-square&logo=bilibili)](https://space.bilibili.com/797215)
[![QQ群](https://img.shields.io/badge/QQ群-960746023-blue.svg?style=flat-square&color=12b7f5&logo=qq)](https://jq.qq.com/?_wv=1027&k=3JKPt4xC)

</div>

## 🚀 下载渠道
- [![Microsoft 应用商店](./res/brands/msstore.en.png)](https://apps.microsoft.com/store/detail/watt-toolkit/9MTCFHS560NG?hl=zh-cn&gl=cn)
- [软件官网](https://steampp.net)
- [GitHub 发行版](https://github.com/BeyondDimension/SteamTools/releases)
- [码云 发行版](https://gitee.com/rmbgame/SteamTools/releases)
- [Arch 用户仓库](https://aur.archlinux.org/packages/watt-toolkit-bin)（当前 Release 构建）
- [Arch 用户仓库 dev](https://aur.archlinux.org/packages/watt-toolkit-git)（拉取最新源码构建，也许会构建失败）

## ⬇️ [下载指南](./doc/download-guide.md)
详见 [./doc/download-guide.md](./doc/download-guide.md)  

## ✨ 功能
1. 网络加速 <img src="./res/brands/windows.svg" width="16" height="16" /> <img src="./res/brands/linux.svg" width="16" height="16" /> <img src="./res/brands/apple.svg" width="16" height="16" /> <img src="./res/brands/android.svg" width="16" height="16" /> 
    - ~~使用 [Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy) 开源项目进行本地反代来支持更快的访问游戏网站。~~
	- 使用 [YARP.ReverseProxy](https://github.com/microsoft/reverse-proxy) 开源项目进行本地反代来支持更快的访问游戏网站。
2. 脚本配置 <img src="./res/brands/windows.svg" width="16" height="16" /> <img src="./res/brands/linux.svg" width="16" height="16" /> <img src="./res/brands/apple.svg" width="16" height="16" />
	- 通过加速服务拦截网络请求将一些 JS 脚本注入在网页中，提供类似网页插件的功能。
3. 账号切换 <img src="./res/brands/windows.svg" width="16" height="16" /> <img src="./res/brands/linux.svg" width="16" height="16" /> <img src="./res/brands/apple.svg" width="16" height="16" />
	- 一键切换已在当前 PC 上登录过的 Steam 账号，与管理家庭共享库排序及禁用等功能。
4. 库存管理 <img src="./res/brands/windows.svg" width="16" height="16" /> <img src="./res/brands/linux.svg" width="16" height="16" /> <img src="./res/brands/apple.svg" width="16" height="16" />
	- 让您直接管理 Steam 游戏库存，可以编辑游戏名称和[自定义封面](https://www.steamgriddb.com)，也能解锁以及反解锁 Steam 游戏成就。
	- 监控 Steam 游戏下载进度实现 Steam 游戏下载完成定时关机功能。
	- 模拟运行 Steam 游戏，让您不用安装和下载对应的游戏也能挂游玩时间和 Steam 卡片
	- 自助管理 Steam 游戏云存档，随时删除和上传自定义的存档文件至 Steam 云
5. 本地令牌 <img src="./res/brands/windows.svg" width="16" height="16" /> <img src="./res/brands/linux.svg" width="16" height="16" /> <img src="./res/brands/apple.svg" width="16" height="16" /> <img src="./res/brands/android.svg" width="16" height="16" /> 
	- 让您的手机令牌统一保存在电脑中，目前仅支持 Steam 令牌，后续会开发支持更多的令牌种类与云同步令牌。
6. 自动挂卡 <img src="./res/brands/windows.svg" width="16" height="16" /> <img src="./res/brands/linux.svg" width="16" height="16" /> <img src="./res/brands/apple.svg" width="16" height="16" /> <img src="./res/brands/android.svg" width="16" height="16" /> 
	- 集成 [ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm) 在应用内提供 挂机掉落 Steam 集换式卡牌 等功能。
7. 游戏工具 <img src="./res/brands/windows.svg" width="16" height="16" />
	- 强制游戏窗口使用无边框窗口化、更多功能待开发。

## 🖥 支持的操作系统
- Windows 11
- Windows 10 版本 1809（OS 内部版本 17763）或更高版本
- macOS 10.15 或更高版本
- Ubuntu 18.04 或更高版本
- Debian 10 或更高版本
- CentOS 7 或更高版本
- Deepin(UOS) 20 或更高版本
- ~~iOS 11 或更高版本~~（开发中…）
- Android 5.0(API 21) 或更高版本

## 🧩 截图
<img src="./res/screenshots/screenshot-windows-accelerator.webp" width="800" />
<br/>
<br/>
<img src="./res/screenshots/screenshot-android-authenticator.png" width="800" />

## 🌏 路线图
查看这个 [milestones](https://github.com/BeyondDimension/SteamTools/milestones) 来了解我们下一步的开发计划，并随时提出问题。

## ⌨️ 开发环境
[Visual Studio 2022](https://visualstudio.microsoft.com/zh-hans/vs) 或 [Visual Studio 2022 for Mac](https://visualstudio.microsoft.com/zh-hans/vs/mac)     
- 系统要求
	- [Windows 11 版本 21H2 或更高版本：家庭版、专业版、专业教育版、专业工作站版、企业版和教育版](https://learn.microsoft.com/zh-cn/visualstudio/releases/2022/system-requirements)
	- [macOS Big Sur 11.0 或更高版本](https://learn.microsoft.com/zh-cn/visualstudio/releases/2022/mac-system-requirements)
- 工作负荷
	- Web 和云
		- ASP.NET 和 Web 开发
	- 桌面应用和移动应用
		- 使用 .NET 的移动开发 / .NET Multi-platform App UI 开发
		- .NET 桌面开发
		- 通用 Windows 平台开发
- 单个组件
	- GitHub Extension for Visual Studio(可选)
- [Visual Studio Marketplace](https://marketplace.visualstudio.com)
	- [Avalonia for Visual Studio(可选)](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaforVisualStudio)  
	- [NUnit VS Templates(可选)](https://marketplace.visualstudio.com/items?itemName=NUnitDevelopers.NUnitTemplatesforVisualStudio)  

[JetBrains Rider](https://www.jetbrains.com/rider)  
[Visual Studio Code](https://code.visualstudio.com)  
[OpenJDK 17](https://learn.microsoft.com/zh-cn/java/openjdk/download#openjdk-17)  
[Android Studio Electric Eel 或更高版本](https://developer.android.google.cn/studio)  
[Xcode 14 或更高版本](https://developer.apple.com/xcode)  

## 🏗️ [项目结构](./src/README.md)
详见 [./src/README.md](./src/README.md)  

<!--👇图标如果发生更改，还需更改 Tools.OpenSourceLibraryList(Program.OpenSourceLibraryListEmoji) -->
## 📄 感谢以下开源项目
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
* [System.Reactive](https://github.com/dotnet/reactive)
* [Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)
* [YARP](https://github.com/microsoft/reverse-proxy)
* [FastGithub](https://github.com/dotnetcore/FastGithub)
* [Portable.BouncyCastle](https://github.com/novotnyllc/bc-csharp)
* [SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager)
* [ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm)
* [Steam4NET](https://github.com/SteamRE/Steam4NET)
* [WinAuth](https://github.com/winauth/winauth)
* [SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)
* [Gameloop.Vdf](https://github.com/shravan2x/Gameloop.Vdf)
* [DnsClient.NET](https://github.com/MichaCo/DnsClient.NET)
* [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp)
* [MemoryPack](https://github.com/Cysharp/MemoryPack)
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
