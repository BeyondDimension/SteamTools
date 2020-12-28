<img src="https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Icon/LOGO1.png" alt="logo" width="130" height="130" align="left" />

<h1>Steam++工具箱</h1>

> 「Steam++」是一个包含多种Steam工具功能的工具箱，
>  此工具的大部分功能都是需要您下载安装Steam才能使用。
<br/>

[![Release Version](https://img.shields.io/github/v/release/rmbadmin/SteamTools?style=flat-square)](https://github.com/rmbadmin/SteamTools/releases/latest)
[![GitHub license](https://img.shields.io/github/license/rmbadmin/SteamTools?style=flat-square)](LICENSE)
[![GitHub Star](https://img.shields.io/github/stars/rmbadmin/SteamTools?style=flat-square)](https://github.com/rmbadmin/SteamTools/stargazers)
[![GitHub Fork](https://img.shields.io/github/forks/rmbadmin/SteamTools?style=flat-square)](https://github.com/rmbadmin/SteamTools/network/members)
![GitHub repo size](https://img.shields.io/github/repo-size/rmbadmin/SteamTools?style=flat-square&color=3cb371)

## 效果展示
-------
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/s.png)  
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/e.png)  
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/e2.png)  
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/t.png)  
------

## 核心功能

### 1. 反代Steam的社区网页使其能正常访问
功能类似羽翼城大佬的[steamcommunity_302](https://www.dogfight360.com/blog/686/),使用[Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)开源项目进行本地反代，相比302工具具有更快的启动速度，以及支持简单的脚本注入。
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/1.png)  
---
### 2. 快速切换当前PC已经记住登陆的Steam账号

该功能是读取Steam路径下存储的本地用户登录记录直接展示操作，可以多账号切换无需重新输入密码和令牌。
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/2.png)
---
### 3. Steam游戏的成就统计管理功能
功能参考[SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager)进行二次开发，修改了游戏列表的加载和操作易用性。
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/3.png)
---

### 4. Steam本地两步身份验证器
功能参考[WinAuth](https://github.com/winauth/winauth)开发，可以使您不用启动移动版Steam App也能查看您的令牌，功能类似的软件有[WinAuth](https://github.com/winauth/winauth)、[SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)。
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/4.png)
---

### 5. 一些游戏工具
目前已有强制游戏无边框窗口化，CSGO修复VAC屏蔽。
这一块是随缘做一些我经常用或者闲着没事捣鼓的功能。
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/5.png)
![](https://raw.githubusercontent.com/rmbadmin/SteamTools/develop/Data/6.png)
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

## 运行环境
> 程序使用C# WPF在 .NET Framework4.7.2环境下开发，如果无法运行请下载安装[.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472)。  
> 缺失的程序集为我自己修改后的项目  
* [Titanium-Web-Proxy](https://github.com/rmbadmin/Titanium-Web-Proxy)
* [MetroRadiance](https://github.com/rmbadmin/MetroRadiance)

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