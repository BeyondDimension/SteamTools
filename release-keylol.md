# 前言

Steam++项目创建于2018年5月8日，不过并不是耗时两年才做出来，之前一直由于工作原因，并没有多少空闲时间，直到今年中旬辞职了，才开始猛肝。
本来是想一直做到完全体了，才考虑发布，不过现在感慨一下，果然是没法这么快就整完的，所以还是一版一版更新做到完全体吧。
Steam++只在keylol论坛与github发布，为了账号安全，请不要到其它不明地方下载。

# Steam++工具箱


## 已知的未修复问题




* (严重)经反馈，程序运行时可能会影响部分steam游戏掉帧，GPU占用率变低，这个BUG目前在作者的机器上无法复现所以暂时没有修复，如果你遇到这个问题可以手动关闭steam++解决。
* 程序在高DPI缩放分辨率下可能会UI布局错位，目前暂时拖动程序可恢复。
* 部分用户无法加载游戏列表，无法复现，原因暂时不明。


## 更新内容


### 1.0.2

```
 更新了由Benares制作的图标
 新增帐户切换删除功能
 添加github图片资源文件代理服务与discord代理服务支持
 因为有问题暂时去掉了GOG Galaxy代理选项
 修复一个会导致程序启动没有反应的bug
 修复托盘菜单steam昵称过长ui错位bug
 新增steam启动参数设置
 新增启动时程序最小化到托盘菜单设置
 新增捐助列表的展示
 新增工具自动更新功能
```

## 工具介绍

   `Steam++`是一个包含多种Steam工具功能的工具箱，开源发布于[Github](https://github.com/rmbadmin/SteamTools)，如果您对发布的二进制文件不放心，可以自行下载源码编译运行。
   此工具的大部分功能都是需要您下载安装Steam才能使用。
   工具预计将整合进大部分常用的Steam相关工具，并且尽力做到比原工具更好用，在各种整合添加功能的同时，也会注意体积尽量的控制到最小。
   查毒链接[https://www.virustotal.com/gui/file/3889babb8b0dcf79b16045679a5cb57a70cc6d60447078fb65b8a8979da2f14d/detection](https://www.virustotal.com/gui/file/3889babb8b0dcf79b16045679a5cb57a70cc6d60447078fb65b8a8979da2f14d/detection)
   虽然没有什么杀毒软件报毒，但是使用过程中可能遇到windows defender误报，您可以选择添加信任。
软件截图
[hide]
[attachimg]1228394[/attachimg]
[attachimg]1228395[/attachimg]
[attachimg]1228396[/attachimg]
[attachimg]1228397[/attachimg]
[/hide]

## 核心功能


### 1. 反代Steam的社区网页使其能正常访问

 功能类似羽翼城大佬的steamcommunity_302,使用[Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)开源项目进行本地反代，相比302工具具有更快的启动速度，以及支持简单的脚本注入。该功能也可以配合羽翼城大佬的[UsbEAm Hosts Editor](https://www.dogfight360.com/blog/475/)里的网页相关-steamcommunity_302 社区/api/商店加载速度选项的hosts提升加载速度。
[attachimg]1228388[/attachimg]

### 2. 快速切换当前PC已经记住登陆的Steam账号

该功能是读取Steam路径下存储的本地用户登录记录直接展示操作，可以多账号切换无需重新输入密码和令牌。
[attachimg]1228389[/attachimg]

### 3. Steam游戏的成就统计管理功能

 功能参考[SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager)进行二次开发，修复了成就语言有中文却依然是英文成就信息的BUG，修改了游戏列表的加载和操作易用性。
[attachimg]1228390[/attachimg]

### 4. Steam本地两步身份验证器

功能参考[WinAuth](https://github.com/winauth/winauth)开发，可以使您不用启动移动版Steam App也能查看您的令牌，功能类似的软件有[WinAuth](https://github.com/winauth/winauth)、[SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)。
[attachimg]1228391[/attachimg]

### 5. 一些游戏工具

目前已有强制游戏无边框窗口化，CSGO修复VAC屏蔽。
这一块是随缘做一些我经常用或者闲着没事捣鼓的功能。
将任何游戏强制无边框窗口化
[attachimg]1228392[/attachimg]
使任何窗口化游戏变成动态桌面壁纸（终于可以用《山》当壁纸了）
[attachimg]1228393[/attachimg]

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
> 分流下载：  
> [hide]
> [蓝奏云](https://wws.lanzous.com/ihW80jkylcb)  
> [百度云](https://pan.baidu.com/s/1um0YboiVGZTs4rC6uCCa1g)  
> 提取码：pz3t
> [/hide]  
> EXE 大小：4.85MB  
> MD5：F819FEAA3463E0B9DED7E94AB87088B0