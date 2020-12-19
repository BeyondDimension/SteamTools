# 前言

Steam++项目创建于2018年5月8日，不过并不是耗时两年才做出来，之前一直由于工作原因，并没有多少空闲时间，直到今年中旬辞职了，才开始猛肝。
本来是想一直做到完全体了，才考虑发布，不过现在感慨一下，果然是没法这么快就整完的，所以还是一版一版更新做到完全体吧。
Steam++只在keylol论坛与github发布，为了账号安全，请不要到其它不明地方下载。
已经收到了B叔制作的图标，下个版本可以换上更好看的图标了。

# Steam++工具箱

   `Steam++`是一个包含多种Steam工具功能的工具箱，开源发布于[Github](https://github.com/rmbadmin/SteamTools)，如果您对发布的二进制文件不放心，可以自行下载源码编译运行。
   此工具的大部分功能都是需要您下载安装Steam才能使用。
   工具预计将整合进大部分常用的Steam相关工具，并且尽力做到比原工具更好用，在各种整合添加功能的同时，也会注意体积尽量的控制到最小。
   查毒链接[https://www.virustotal.com/gui/file/3889babb8b0dcf79b16045679a5cb57a70cc6d60447078fb65b8a8979da2f14d/detection](https://www.virustotal.com/gui/file/3889babb8b0dcf79b16045679a5cb57a70cc6d60447078fb65b8a8979da2f14d/detection)
   虽然没有什么杀毒软件报毒，但是使用过程中可能遇到windows defender误报，您可以选择添加信任。
软件截图
[hide]
[attachimg]1224039[/attachimg]
[attachimg]1224038[/attachimg]
[attachimg]1224040[/attachimg]
[/hide]

## 核心功能


### 1. 反代Steam的社区网页使其能正常访问

 功能类似羽翼城大佬的steamcommunity_302,使用[Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)开源项目进行本地反代，相比302工具具有更快的启动速度，以及支持简单的脚本注入。该功能也可以配合羽翼城大佬的[UsbEAm Hosts Editor](https://www.dogfight360.com/blog/475/)里的网页相关-steamcommunity_302 社区/api/商店加载速度选项的hosts提升加载速度。
[attachimg]1224032[/attachimg]

### 2. 快速切换当前PC已经记住登陆的Steam账号

该功能是读取Steam路径下存储的本地用户登录记录直接展示操作，可以多账号切换无需重新输入密码和令牌。
[attachimg]1224033[/attachimg]

### 3. Steam游戏的成就统计管理功能

 功能参考[SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager)进行二次开发，修复了成就语言有中文却依然是英文成就信息的BUG，修改了游戏列表的加载和操作易用性。
[attachimg]1224034[/attachimg]

### 4. Steam本地两步身份验证器

功能参考[WinAuth](https://github.com/winauth/winauth)开发，可以使您不用启动移动版Steam App也能查看您的令牌，功能类似的软件有[WinAuth](https://github.com/winauth/winauth)、[SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)。
[attachimg]1224035[/attachimg]

### 5. 一些游戏工具

目前已有强制游戏无边框窗口化，CSGO修复VAC屏蔽。
这一块是随缘做一些我经常用或者闲着没事捣鼓的功能。
  
将任何游戏强制无边框窗口化
[attachimg]1224036[/attachimg]
使任何窗口化游戏变成动态桌面壁纸（终于可以用《山》当壁纸了）
[attachimg]1224037[/attachimg]

------


## 预计后续添加的功能


### Steam自动挂卡

尝试用社区反代功能结合成就解锁功能来重新实现，目的是实现在软件内不用登录Steam即可直接获取徽章卡片信息并开始挂卡。

### Steam皮肤设计器

挖坑画大饼，可视化编辑Steam皮肤，而且如果软件能上架Steam的话打算支持创意工坊分享设计的Steam皮肤，短期内肯定做不完。

### 插件形式加载运行ASF

以插件形式支持ASF在工具内运行并增强ASF在Windows Desktop环境下的使用。

### Steam自定义封面管理

 增强Steam自定义封面的管理以及从[SteamGridDB](https://www.steamgriddb.com/)快速匹配下载应用封面。

------


## 运行环境


> 程序使用C# WPF在 .NET Framework4.7.2环境下开发，如果无法运行请下载安装[.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472)。

## 下载

> [Github](https://github.com/rmbadmin/SteamTools/releases)
> [蓝奏云](https://wws.lanzous.com/ijIlwjg2bjc)
> [百度云](https://pan.baidu.com/s/19XCB3-X7isygG6UFpXcCtg) 
> 查看百度云提取码
> [hide]
> 提取码：zbs6
> [/hide]
> exe体积：4.81MB
> md5：AEECBD9B268C0BB8D619555CE125C9A5