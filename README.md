<img src="https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/icon/LOGO1.png" alt="logo" width="130" height="130" align="left" />

<h1>Steam++ 工具箱</h1>

>  「Steam++」是一个包含多种Steam工具功能的工具箱，  
>   此工具的大部分功能都是需要您下载安装Steam才能使用。
<br/>

![Release Download](https://img.shields.io/github/downloads/rmbadmin/SteamTools/latest/total?style=flat-square)
[![Release Version](https://img.shields.io/github/v/release/rmbadmin/SteamTools?style=flat-square)](https://github.com/rmbadmin/SteamTools/releases/latest)
[![GitHub license](https://img.shields.io/github/license/rmbadmin/SteamTools?style=flat-square)](LICENSE)
[![GitHub Star](https://img.shields.io/github/stars/rmbadmin/SteamTools?style=flat-square)](https://github.com/rmbadmin/SteamTools/stargazers)
[![GitHub Fork](https://img.shields.io/github/forks/rmbadmin/SteamTools?style=flat-square)](https://github.com/rmbadmin/SteamTools/network/members)
![GitHub repo size](https://img.shields.io/github/repo-size/rmbadmin/SteamTools?style=flat-square&color=3cb371)

# Steam++

[English](https://github.com/rmbadmin/SteamTools/blob/develop/README.en.md)

[简体中文](https://github.com/rmbadmin/SteamTools/blob/develop/README.md)

## 系统要求
- Windows 10 1607+
- Windows 8.1 / Windows Server 2012 R2
	- 先决条件
	- Microsoft Visual C++ 2015-2019 Redistributable [64 位](https://aka.ms/vs/16/release/vc_redist.x64.exe) / [32 位](https://aka.ms/vs/16/release/vc_redist.x86.exe)
- Windows 7 SP1 [ESU](https://docs.microsoft.com/zh-cn/troubleshoot/windows-client/windows-7-eos-faq/windows-7-extended-security-updates-faq)
	- 先决条件
	- Microsoft Visual C++ 2015-2019 Redistributable [64 位](https://aka.ms/vs/16/release/vc_redist.x64.exe) / [32 位](https://aka.ms/vs/16/release/vc_redist.x86.exe)
	- KB3063858 [64 位](https://www.microsoft.com/zh-cn/download/details.aspx?id=47442) / [32 位](https://www.microsoft.com/zh-cn/download/details.aspx?id=47409)
- ~~macOS 10.13 High Sierra 或更高版本~~
- ~~Alpine Linux 3.11+~~
- ~~CentOS 7+~~
- ~~Debian 9+~~
- ~~Fedora 32+~~
- ~~Linux Mint 18+~~
- ~~openSUSE 15+~~
- ~~Red Hat Enterprise Linux 7+~~
- ~~SUSE Enterprise Linux (SLES) 12 SP2+~~
- ~~Ubuntu 20.10, 20.04, 18.04, 16.04~~
- ~~Android 5.0+~~
- ~~iOS 8.0+~~

## 效果展示
-------
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/s.png)  
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/e.png)  
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/e-1.png)  
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/t.png)  
------

## 核心功能

### 1. 反代Steam的社区网页使其能正常访问
功能类似羽翼城大佬的[steamcommunity_302](https://www.dogfight360.com/blog/686/),使用[Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)开源项目进行本地反代，使国内用户可以正常访问steam社区页，相比302工具具有更快的启动速度，以及支持简单的脚本注入。(还顺便支持了Pixiv、Discord、Twitch等网站的反代支持)
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/1.png)  
---
### 2. 快速切换当前PC已经记住登陆的Steam账号
该功能是读取Steam路径下存储的本地用户登录记录直接展示操作，可以多账号切换无需重新输入密码和令牌。
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/2.png)
---
### 3. Steam游戏的成就统计管理功能
功能参考[SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager)进行二次开发，修改了游戏列表的加载和操作易用性。
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/3.png)
---

### 4. Steam本地两步身份验证器
功能参考[WinAuth](https://github.com/winauth/winauth)开发，可以使您不用启动移动版Steam App也能查看您的令牌，功能类似的软件有[WinAuth](https://github.com/winauth/winauth)、[SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)。
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/4.png)  

本地令牌交易市场报价确认
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/4-1.png) 
---

### 5. 一些游戏工具
目前已有强制游戏无边框窗口化，CSGO修复VAC屏蔽。
这一块是随缘做一些我经常用或者闲着没事捣鼓的功能。
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/5.png)
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/resources/5-1.png)
---

## 预计后续添加的功能

### Steam自动挂卡
尝试用社区反代功能结合成就解锁功能来重新实现，目的是实现在软件内无需登录Steam帐户即可直接获取徽章卡片信息并开始挂卡。

### Steam皮肤设计器
挖坑画大饼，可视化编辑Steam皮肤，而且如果软件能上架Steam的话打算支持创意工坊分享设计的Steam皮肤，短期内肯定做不完。

### 插件形式加载运行ASF
以插件形式支持ASF在工具内运行并增强ASF在Windows Desktop环境下的使用。

### Steam自定义封面管理
增强Steam自定义封面的管理以及从[SteamGridDB](https://www.steamgriddb.com/)快速匹配下载应用封面。

### 更多其它游戏平台功能


### 更多非作弊游戏功能

---

## 开发环境
[Visual Studio 2019 版本 16.8 或更高版本](https://visualstudio.microsoft.com/zh-hans/vs/) 或 [Visual Studio for Mac](https://visualstudio.microsoft.com/zh-hans/vs/mac/) 或 [JetBrains Rider](https://www.jetbrains.com/rider/) 或 [Visual Studio Code](https://code.visualstudio.com/)
- 工作负载
	- Web 和云
		- ASP.NET 和 Web 开发
	- 桌面应用和移动应用
		- .NET 桌面开发
		- 使用 .NET 的移动开发
	- 其他工具集
		- .NET Core 跨平台开发
- 单个组件
	- GitHub Extension for Visual Studio
- [Visual Studio Marketplace](https://marketplace.visualstudio.com/)
	- [Avalonia for Visual Studio](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaforVisualStudio)
	- [NUnit VS Templates](https://marketplace.visualstudio.com/items?itemName=NUnitDevelopers.NUnitTemplatesforVisualStudio)
	
---

## License
> [GPL-3.0 License](https://github.com/rmbadmin/SteamTools/blob/develop/LICENSE)  
> 根据GPLv3许可发布的开源/免费软件。

---

## 感谢以下开源项目
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
* [MetroRadiance](https://github.com/Grabacr07/MetroRadiance)
* [MetroTrilithon](https://github.com/Grabacr07/MetroTrilithon)
* [Livet](https://github.com/runceel/Livet)
* [StatefulModel](https://github.com/ugaya40/StatefulModel)
* [Hardcodet.NotifyIcon](https://github.com/HavenDV/Hardcodet.NotifyIcon.Wpf.NetCore)
* [System.Reactive](https://github.com/dotnet/reactive)
* [Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)
* [Ninject](https://github.com/ninject/Ninject)
* [log4net](https://github.com/apache/logging-log4net)
* [SteamDB-API](https://github.com/SteamDB-API/api)
* [SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager)
* [ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm)
* [WinAuth](https://github.com/winauth/winauth)
* [SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)
* [Gameloop.Vdf](https://github.com/shravan2x/Gameloop.Vdf)
* [Costura.Fody](https://github.com/Fody/Costura)