# 前言

Steam++是一款开源的跨平台Steam工具箱，请不要在非官方渠道下载软件，
[官网](https://www.steampp.net)，大家可以去官网或者开源仓库地址下载。

# Steam++工具箱


## 2.6.2更新内容
微软商店版本在微软商店检查获取更新即可，一般会比桌面版晚上一天的审核时间。
### 版本亮点
1. ASF 升级至 **V5.2.0.10**
2. 新增 ASF 控制台字体大小、最大行数设置项
3. 新增 Steam 下载完成定时关机、睡眠功能
4. 新增 代理设置可自定义 DNS
5. 新增 ASF 编辑/移除 Bot 功能
6. 新增 Microsoft/Xbox 相关加速服务 (需要升级到 **2.6.2** 之后可用)
7. 新增 Uplay 相关加速服务 (需要升级到 **2.6.2** 之后可用)
8. 新增 OneDrive 等更多相关加速服务 (需要升级到 **2.6.2** 之后可用)
9. 优化 Desktop 加速代理性能
10. 优化 Desktop 脚本注入打包的性能
11. 优化 Desktop 已安装游戏加载性能
12. 从此版本开始将使用 Github Action 自动化部署发布

### 修复问题
1. 改进 Desktop 网络加速现默认使用阿里 DNS(223.5.5.5, 223.6.6.6)
2. 改进 自动更新包文件校验失败时提示
3. 改进 自动更新失败时将自动跳转官网
4. 改进 ASF IPC 默认端口号由 1242 改为 6242
5. 修复 Linux 与 macOS 中 ASF-UI 解压包文件夹分隔符不正确
6. 修复 Windows Hosts 只读时尝试取消只读属性的操作没有正确执行
7. 修复 Windows 此软件自动更新删除更新包缓存时因文件占用引发的中断
8. 修复 Desktop 高 DPI 下动态桌面错位
9. 修复 本地令牌确认交易在登录时可能会卡住
10. 修复 Windows 启用动态桌面后全屏可能导致窗口冻结无法操作
11. 修复 Windows 动态背景有时会被其他窗口遮挡
12. 改进 本地令牌 登录验证码无法加载时可点击在浏览器中查看验证码图片
13. 修复 Desktop 加速代理中可能出现的一些错误
14. 改进 Desktop 令牌详情 UI
15. 改进 Desktop 账号切换中的用户名信息现在默认隐藏
16. 改进 Desktop 深色模式与浅色模式的视觉效果
17. 修复 Pixiv 加速不能登录的问题
18. 修复 Twitch 加速不计算掉宝进度的问题
19. 修复 Discord 加速检测更新失败导致无法启动客户端的问题
20. 修复 Windows 因添加 JumpList 时可能导致的闪退 
21. 修复 Desktop 本地令牌 点击锁定后输入密码按回车解锁会循环无限触发锁定的问题


[![steampp.net](https://img.shields.io/badge/WebSite-steampp.net-brightgreen.svg?style=flat-square&color=61dafb)](https://steampp.net)
[![Steam++ v2.6.2](https://img.shields.io/badge/Steam++-v2.6.2-brightgreen.svg?style=flat-square&color=512bd4)]()

------


## 工具介绍

`Steam++`是一款开源的跨平台Steam工具箱，所有本地功能完全免费，开源发布于Github，如果您对发布的二进制文件不放心，可以自行下载源码编译运行。
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

### 7.Steam监控下载完成定时关机

可以监控Steam下载列表所有游戏的下载进度并设置下载完成后自动关机或睡眠电脑，节约你的电费。

### 8.已内置ASF挂卡功能

[ASF](https://github.com/JustArchiNET/ArchiSteamFarm)大家肯定都不陌生，它实在是太好用了，所以我们内置了ASF在Steam++内，并在后续会对ASF在UI环境下使用做更多增强体验。

### 10. 更多的游戏工具功能

支持各种老游戏无边框窗口化等等

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
> [Microsoft Store](https://www.microsoft.com/store/productId/9MTCFHS560NG)
> 分流下载：
> [hide]
> [蓝奏云](https://cliencer.lanzoux.com/b0165duja)
> 提取码：1234
> [/hide]
> 使用过程中可能遇到windows defender误报，您可以选择添加信任。