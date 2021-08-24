# 前言
Steam++是完全开源的，请不要在非官方渠道下载软件，对于软件报毒情况确实是误报问题。我们也制作了自己的产品[官网](https://www.steampp.net)，大家可以去官网或者开源仓库地址下载。

# Steam++工具箱

## 2.4.10更新内容

### 新增内容
1. Linux/macOS 版本中的 CLR 升级至 .NET 6 Preview 7
2. Desktop 现已适配 WinUI 3 / Windows 11 / Fluent Design System 样式风格新UI
3. 新增 Android 上从图库选择二维码图片导入令牌
4. 新增 Android 上文件导入选择二维码图片导入令牌
5. 新增 Desktop 上现可刷新头像
6. 新增 UI设置-主题色设置
7. 新增 UI设置-自定义背景图片设置

### 修复问题
1. 修复 Desktop 上账号切换功能
2. 改进 Desktop 上受保护的成就不在支持勾选
3. 修复 Linux 上因字体引发的启动时闪退
4. 修复 Android 上扫码导入功能
5. 修复 Android 上 Toast 不能正常显示
6. 改进 Android 上的令牌导入方式
7. 修复 Android 上导入带密码的令牌时不显示密码输入框
8. 修复 导入带密码的令牌时密码输入文本框窗口不能正确取消
9. 修复 Desktop 上使用火狐浏览器无法进行快速登录
10. 修复 Android 8.0 以下启动时闪退
11. 修复 Desktop 上部分用户库存游戏已安装游戏无法读取
12. 尝试修复 Windows 上托盘菜单有时无法打开窗口
13. 改进 Android 上确认交易页面上的显示隐藏逻辑

### 已知问题
- Desktop 
	- macOS
		- 尚未公证，这会影响 macOS Catalina（版本 10.15）以上
		- 某些窗口顶部会有两个标题栏
		- 自动更新不可用
	- Linux
		- 托盘不生效，这将影响程序不能正常退出
		- 窗口弹出位置不正确
		- 自动更新不可用
	- Shared
		- 主题切换需重启软件后生效，且跟随系统暂不可用
- Mobile
	- Android
		- 本地令牌倒计时存在误差不够精确，可能导致令牌值不一致
		- 确认交易列表刷新后数据显示不正确
		- 自动更新暂不可用

***

## 工具介绍

「Steam++」是一个包含多种Steam工具功能的工具箱，所有功能完全免费，开源发布于Github，如果您对发布的二进制文件不放心，可以自行下载源码编译运行。

 此工具的大部分功能都是需要您下载安装Steam才能使用。

 工具预计将整合进大部分常用的Steam相关工具，并且尽力做到比原工具更好用。

软件截图
[hide]
[attachimg]1228394[/attachimg]
[attachimg]1228395[/attachimg]
[attachimg]1228396[/attachimg]
[attachimg]1228397[/attachimg]
[/hide]

## 核心功能


### 1. 通过本地反代理Steam的社区等网页使其能正常访问

功能使用Titanium-Web-Proxy开源项目进行本地反代，相比SteamCommunity302工具具有更快的启动速度。让用户能够连接上诸如Steam社区（个人资料页）、Discord语音聊天、Twitch直播观看、Origin下载加速、谷歌验证码、Pixiv图片等。  
[attachimg]1228388[/attachimg]

### 2. 快速切换当前PC已经记住登陆的Steam账号

该功能是读取Steam路径下存储的本地用户登录记录直接展示操作，可以多账号切换无需重新输入密码和令牌。如果您的ip地址更换，会导致登陆状态失效。在Steam++2.0新版本支持了动态头像。  
[attachimg]1228389[/attachimg]

### 3. Steam游戏的成就统计管理功能

能够读取任何游戏的成就信息（包括隐藏成就），并且能进行成就修改，包括成就解锁以及成就反解锁，还能够修改成就统计信息

参考SAM（SteamAchievementManager）进行二次开发，修复了成就语言有中文却依然是英文成就信息的BUG，修改了游戏列表的加载和操作易用性。

注意：滥用成就管理功能可能会导致开发者封禁！  
[attachimg]1228390[/attachimg]

### 4. Steam本地两步身份验证器

该功能能够将Steam手机令牌储存到您的电脑中，并能够进行与手机端一样的操作。

不仅能查看手机令牌，还能进行交易报价确认，并且能一键全部确认。

功能参考WinAuth开发，功能类似的软件有WinAuth、SteamDesktopAuthenticator。  
[attachimg]1228391[/attachimg]  

本地令牌交易市场报价确认
[attachimg]1262615[/attachimg]

### 5. Steam内置浏览器脚本注入
通过反代加速功能将js脚本内置插入了Steam客户端内的浏览器，达到了与油猴类似的效果。目前2.0.0.6版本新增了脚本工坊，可以从我们的服务器上下载到已经兼容的脚本。我们也会不断添加新的脚本，同时也会对旧的脚本进行维护。

### 6. 更多的游戏工具功能
支持各种老游戏无边框窗口化等等

### 6. 更新计划（画饼时间到）
我们预计会在6月份将推出本地挂卡、挂时长功能。并且同步推出iOS、安卓端App，用户可以利用手机查看多个账号的令牌，一键确认上架，并且支持手机挂卡。

#### 并且，以上功能全部免费。

------


## 运行环境

> 程序使用C# 在 .NET 6.0环境下开发，新版采用独立部署编译，不需要运行环境也可以运行)。

## 下载
> [官网](https://steampp.net/)  
> [Github](https://github.com/rmbadmin/SteamTools/releases)  
> 分流下载：  
> [hide]
> [蓝奏云](https://cliencer.lanzous.com/b0165duja)  
> 提取码：1234
> [/hide]  
   使用过程中可能遇到windows defender误报，您可以选择添加信任。