<!-- https://keylol.com/t667906-1-1 -->
# 前言
Steam++ 是完全开源的，请不要在非官方渠道下载软件。我们也制作了自己的[官网](https://www.steampp.net)，大家可以去[官网](https://www.steampp.net)或者开源仓库地址下载。

# Steam++ 工具箱

## x.y.z 更新内容
微软商店版本在微软商店检查获取更新即可，一般会比桌面版晚上几天的审核时间。

### 版本亮点
TODO

### 修复问题
TODO


[![steampp.net](https://img.shields.io/badge/WebSite-steampp.net-brightgreen.svg?style=flat-square&color=61dafb)](https://steampp.net)
[![Steam++ vx.y.z](https://img.shields.io/badge/Steam++-vx.y.z-brightgreen.svg?style=flat-square&color=512bd4)]()

------


## 工具介绍

`Steam++` 是一款开源的跨平台 Steam 工具箱，所有本地功能完全免费，开源发布于 GitHub，如果您对发布的二进制文件不放心，可以自行下载源码编译运行。
 此工具的大部分功能都是需要您下载安装 Steam 才能使用。
 工具预计将整合进大部分常用的 Steam 相关工具，并且尽力做到比原工具更好用。
软件截图
[hide]
[attachimg]1463881[/attachimg]
[attachimg]1463882[/attachimg]
[attachimg]1463884[/attachimg]
[attachimg]1463899[/attachimg]
[/hide]

## 核心功能

### 1. 本地方法访问加快

略

### 2. 快速切换当前设备已经记住登录的 Steam 账号

该功能是读取 Steam 路径下存储的本地用户登录记录直接展示操作，可以多账号切换无需重新输入密码和令牌。如果您的 IP 地址更换，会导致登录状态失效。  

### 3. Steam 游戏成就管理

能够读取任何游戏的成就信息（包括隐藏成就），并且能进行成就修改，包括成就解锁以及成就反解锁，还能够修改成就统计信息
参考 SAM（SteamAchievementManager）进行二次开发，修复了成就语言有中文却依然是英文成就信息的 BUG，修改了游戏列表的加载和操作易用性。
注意：滥用成就管理功能可能会导致开发者封禁！
[attachimg]1463887[/attachimg]

### 4. Steam 本地两步身份验证器

该功能能够将 Steam 手机令牌储存到您的电脑中，并能够进行与手机端一样的操作。
不仅能查看手机令牌，还能进行交易报价确认，并且能一键全部确认。
功能参考 WinAuth 开发，功能类似的软件还有 SteamDesktopAuthenticator 等。
本地令牌交易市场报价确认
[attachimg]1463890[/attachimg]

### 5. Steam 内置浏览器脚本注入

通过反代加速功能将 JS 脚本内置插入了 Steam 客户端内的浏览器，达到了与油猴类似的效果。在脚本工坊中可以从我们的服务器上下载到已经兼容的脚本。我们也会不断添加新的脚本，同时也会对旧的脚本进行维护。
[attachimg]1463883[/attachimg]

### 6. Steam 家庭库共享排序

可以排序你的共享库存顺序，避免只能读取 A 的共享游戏，却想玩 B 的共享游戏。

### 7. Steam 监控下载完成定时关机

可以监控 Steam 下载列表所有游戏的下载进度并设置下载完成后自动关机或睡眠电脑，节约你的电费。

### 8. 已内置 ASF 挂卡功能

[ASF](https://github.com/JustArchiNET/ArchiSteamFarm) 大家肯定都不陌生，它实在是太好用了，所以我们内置了 [ASF](https://github.com/JustArchiNET/ArchiSteamFarm) 在 Steam++ 内，并在后续会对 [ASF](https://github.com/JustArchiNET/ArchiSteamFarm) 在 UI 环境下使用做更多增强体验。

### 10. 更多的游戏工具功能

支持各种老游戏无边框窗口化等等

#### 并且，以上功能全部免费。

Linux 截图:
[attachimg]1463884[/attachimg]

------


## 运行环境
> 当前版本采用独立部署编译，不需要运行环境也可以运行。

RuntimeIdentifier | Available | Edition
--- | --- | ---
win-x64 | ✅ | Preview
osx-x64 | ❌ | TODO
linux-x64 | ❌ | TODO
android-arm64 | ❌ | TODO
linux-arm64 | ❌ | TODO
osx-arm64 | ❌ | TODO
win-arm64 | ❌ | TODO
ios-arm64 | ❌ | TODO
<!-- 不编译 x86/32 bit 包减少测试工作量，Windows 本机加密 32 bit 与 64 bit 不兼容，涉及本地令牌加密 -->
<!-- arm64 for Windows 因 steamclient.dll 缺少 arm64 支持，以及 .NET 目前不支持 ARM64EC，无法加载本机库，涉及库存游戏中的部分功能 -->


## 下载
> [官网](https://steampp.net/)  
> [Github](https://github.com/rmbadmin/SteamTools/releases)  
> [Gitee](https://gitee.com/rmbgame/SteamTools/releases)  
> [Microsoft Store](https://www.microsoft.com/store/productId/9MTCFHS560NG)  
> 分流下载：  
> [hide]  
> [蓝奏云](https://cliencer.lanzoux.com/b0165duja)  
> 提取码：1234  
> [/hide]  
> 使用过程中可能遇到 Windows Defender 误报，您可以选择添加信任。  
