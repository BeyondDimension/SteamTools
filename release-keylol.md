# 前言

Steam++是完全开源的，请不要在非官方渠道下载软件，对于软件报毒情况确实是误报问题。我们也制作了自己的产品[官网](https://www.steampp.net)，大家可以去官网或者开源仓库地址下载。

# Steam++工具箱


## 2.4.12更新内容


### 新增内容
1. 新增 Desktop 上网络加速代理设置
2. 新增 Android 上屏幕捕获设置项，用于允许截图或录制视频
3. 新增 Windows 托盘菜单支持切换账号与复制令牌
4. 新增 Linux/macOS 托盘菜单改进与完善


### 修复问题
1. 修复 Desktop 上用户头像应当为圆形而不是方形
2. 修复 Android 上切换系统语言可能引发的闪退
3. 修复 Windows 10 上启动时可能出现的网络连接中断提示
4. 修复 Android 上令牌倒计时可能引发的闪退
5. 修复 Desktop 上库存游戏刷新可能引发的闪退
6. 修复 Desktop 上可能少加载了部分已安装游戏
7. 修复 Android 上暗色模式下某些区域背景为白色
8. 改进 Android 上令牌刷新倒计时
9. 改进 本地令牌名称最大长度限制 32 个字符
10. 改进 Desktop 上网络加速 UI
11. 修复 Desktop 上默认头像可能引发的闪退
12. 改进 Desktop 上左侧菜单图标


### 已知问题
- Desktop 
	- macOS
		- 尚未公证，这会影响 macOS Catalina（版本 10.15）以上
		- 某些窗口顶部会有两个标题栏
		- 自动更新不可用
	- Linux
		- 当使用 root 权限运行时托盘不生效，可通过 Exit.sh 退出程序
		- 窗口弹出位置不正确
		- 窗口顶部会有两个标题栏
		- 自动更新不可用
	- Shared
		- 主题切换需重启软件后生效，且跟随系统暂不可用
- Mobile
	- Android
		- 确认交易列表刷新后数据显示不正确
		- 自动更新暂不可用

------


## 工具介绍

`Steam++`是一个包含多种Steam工具功能的工具箱，所有功能完全免费，开源发布于Github，如果您对发布的二进制文件不放心，可以自行下载源码编译运行。
 此工具的大部分功能都是需要您下载安装Steam才能使用。
 工具预计将整合进大部分常用的Steam相关工具，并且尽力做到比原工具更好用。
软件截图
[hide]
[attachimg]1463881[/attachimg]
[attachimg]1463882[/attachimg]
[attachimg]1463884[/attachimg]
[attachimg]1463899[/attachimg]
[/hide]

## 核心功能


### 1. 通过本地反代理Steam的社区等网页使其能正常访问

功能使用Titanium-Web-Proxy开源项目进行本地反代，相比SteamCommunity302工具具有更快的启动速度。让用户能够连接上诸如Steam社区（个人资料页）、Discord语音聊天、Twitch直播观看、Origin下载加速、谷歌验证码、Pixiv图片等。  

### 2. 快速切换当前PC已经记住登陆的Steam账号

该功能是读取Steam路径下存储的本地用户登录记录直接展示操作，可以多账号切换无需重新输入密码和令牌。如果您的ip地址更换，会导致登陆状态失效。  

### 3. Steam游戏成就管理

能够读取任何游戏的成就信息（包括隐藏成就），并且能进行成就修改，包括成就解锁以及成就反解锁，还能够修改成就统计信息
参考SAM（SteamAchievementManager）进行二次开发，修复了成就语言有中文却依然是英文成就信息的BUG，修改了游戏列表的加载和操作易用性。
注意：滥用成就管理功能可能会导致开发者封禁！
[attachimg]1463887[/attachimg]

### 4. Steam本地两步身份验证器

该功能能够将Steam手机令牌储存到您的电脑中，并能够进行与手机端一样的操作。
不仅能查看手机令牌，还能进行交易报价确认，并且能一键全部确认。
功能参考WinAuth开发，功能类似的软件有WinAuth、SteamDesktopAuthenticator。
本地令牌交易市场报价确认
[attachimg]1463890[/attachimg]

### 5. Steam内置浏览器脚本注入

通过反代加速功能将js脚本内置插入了Steam客户端内的浏览器，达到了与油猴类似的效果。目前2.0.0.6版本新增了脚本工坊，可以从我们的服务器上下载到已经兼容的脚本。我们也会不断添加新的脚本，同时也会对旧的脚本进行维护。
[attachimg]1463883[/attachimg]

### 6.Steam家庭库共享排序

可以排序你的共享库存顺序，避免只能读取A的共享游戏，却想玩B的共享游戏。

### 7. 更多的游戏工具功能

支持各种老游戏无边框窗口化等等

### 8. 更新计划（画饼时间到）

我们预计会在6月份将推出ASF挂卡功能。并且同步推出iOS、安卓端App，用户可以利用手机查看多个账号的令牌，一键确认上架，并且支持手机挂卡。

#### 并且，以上功能全部免费。

Linux 截图:
[attachimg]1463884[/attachimg]

------


## 运行环境

> 程序使用C# 在 .NET 6.0环境下开发，新版采用独立部署编译，不需要运行环境也可以运行)。

RuntimeIdentifier | Available | Edition
--- | --- | ---
win-x64 | ✅ | Stable
osx-x64 | ✅ | β
linux-x64 | ✅ | α
android-arm64 | ✅ | α
android-arm | ✅ | α
linux-arm64 | ✅ | α
linux-arm | ✅ | α
osx-arm64 | ❌ | 
win-arm64 | ❌ | 
ios-arm64 | ❌ | 


## 下载

> [官网](https://steampp.net/)
> [Github](https://github.com/rmbadmin/SteamTools/releases)
> [Gitee](https://gitee.com/rmbgame/SteamTools/releases)
> 分流下载：
> [hide]
> [蓝奏云](https://cliencer.lanzoux.com/b0165duja)
> 提取码：1234
> [/hide]
>    使用过程中可能遇到windows defender误报，您可以选择添加信任。