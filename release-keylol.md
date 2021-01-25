# 前言

Steam++项目创建于2018年5月8日，不过并不是耗时两年才做出来，之前一直由于工作原因，并没有多少空闲时间，直到今年中旬辞职了，才开始猛肝。
本来是想一直做到完全体了，才考虑发布，不过现在感慨一下，果然是没法这么快就整完的，所以还是一版一版更新做到完全体吧。
Steam++只在keylol论坛与github发布，为了账号安全，请不要到其它不明地方下载。

# Steam++工具箱


## 已知的未修复问题
* 部分用户无法加载游戏列表，原因可能是因为第一次加载游戏列表时未加载完成程序停止运行了，导致生成了错误的缓存文件apps.json，如果遇到此问题可尝试手动删除程序目录下的apps.json然后重启程序。


## 更新内容

### 1.1.0
```
本地令牌市场交易确认功能上线啦(目前功能较简单，待完善)
新增内置steam史低查询脚本
新增Steam启动消息通知设置项
新增github gist代理服务支持
修复某些情况配置文件读取出错
修复discord更新下载和图片失效
修复twitch聊天频道连接失效
修复gihub头像资源加载失败
新增pixiv图片的代理选项来修复pixiv图片失效
修改了本地反代解析域名的方式，这样出问题以后可以直接热更新
还修复了一些没有记下来的bug...
```


## 工具介绍

   `Steam++`是一个包含多种Steam工具功能的工具箱，开源发布于[Github](https://github.com/rmbadmin/SteamTools)，如果您对发布的二进制文件不放心，可以自行下载源码编译运行。
   此工具的大部分功能都是需要您下载安装Steam才能使用。
   工具预计将整合进大部分常用的Steam相关工具，并且尽力做到比原工具更好用，在各种整合添加功能的同时，也会注意体积尽量的控制到最小。

软件截图
[hide]
[attachimg]1228394[/attachimg]
[attachimg]1228395[/attachimg]
[attachimg]1228396[/attachimg]
[attachimg]1228397[/attachimg]
[/hide]

## 核心功能


### 1. 反代Steam的社区网页使其能正常访问

 功能类似羽翼城大佬的[steamcommunity_302](https://www.dogfight360.com/blog/686/),使用[Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)开源项目进行本地反代，相比302工具具有更快的启动速度，以及支持简单的脚本注入。该功能也可以配合羽翼城大佬的[UsbEAm Hosts Editor](https://www.dogfight360.com/blog/475/)里的网页相关-steamcommunity_302 社区/api/商店加载速度选项的hosts提升加载速度。
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

本地令牌交易市场报价确认
[attachimg]1262615[/attachimg]

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
> [蓝奏云](https://wws.lanzous.com/icwdPkpnu1e)  
> [百度云](https://pan.baidu.com/s/1EG8e1Oi4Mg2KCiF42oKnig)  
> 提取码：stpp
> [/hide]  
> EXE 大小：4.87MB  
> MD5：8965D1A2EC7688F1EDEF3DA81667A009
> [查毒链接](https://www.virustotal.com/gui/file/9931d6034c3b8f6c362ecb991a21d205623734fbee201e62f813d05f50347e64/detection)
   虽然没有什么杀毒软件报毒，但是使用过程中可能遇到windows defender误报，您可以选择添加信任。