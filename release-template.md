### 公告
这是一个修复目前累积问题的版本，也是最后一个.NET 6 的版本，
之后桌面端程序将使用.NET 7 + Avalonia 11.0 重构发布新版本。
届时也会放弃继续对 Win7/8.1 系统的支持，所以这也意味着这是最后一个支持 Win7/8.1 的版本。
也请各位敬请期待之后的 3.0 版本吧~

### 版本亮点
1. .NET 运行时升级至 6.0.11，使用 fde 版本需要升级运行时
2. ASF 升级至 V5.3.2.4

### 修复问题
1. 改进 Mac 安装证书的提示，显示执行命令
2. 修复 Steam 启动参数在一些场景下无效
3. 修复 网络加速 关闭 http 重定向到 https 依然监听80端口错误
4. 修复 网络加速 仅启用脚本功能 导致代理网站无法访问的错误
5. 修复 网络加速 开启系统代理和 PAC 代理后，非代理网站无法正常访问的错误
6. 修改 证书安装路径 为本地计算机 而不在是 当前用户 以支持跨用户访问，这项改动可能导致大家需要重新安装一次证书
7. 修复 库存游戏 因 Steam 客户端更新导致的加载游戏数据失败问题
8. 修复 捐助列表金额显示错位问题
9. 修复 Twitch 聊天连接失败问题
10. 修复 discord.gg 无法访问导致无法加入频道分享链接问题


<!--

### 已知问题
- 除 Windows 之外的平台此软件自动更新尚不可用
- Desktop 
	- macOS
		- [尚未公证](https://support.apple.com/zh-cn/guide/mac-help/mh40616/10.15/mac/10.15)，这会影响 macOS Catalina（版本 10.15）以上版本
	- Linux
		- 窗口弹出位置不正确
		- 鼠标指针浮动样式不正确
	- Windows
		- Windows 11 
			- 在 CPU 不受支持的 Win11 上无法启动，Windows 日志中显示 ```Failed to create CoreCLR, HRESULT: 0x80004005```
			- 仅 .NET 6.0 受此影响，在内部版本 22509 中修复，见 [issue](https://github.com/dotnet/core/issues/6733)
			- **解决方案：** 可尝试使用旧版本 例如 v2.3.0
		- Windows 7
			- 先决条件
				- 需要安装 Extended Security Update
			- 在不符合先决条件的情况下运行可能导致
				- 程序无法正常运行
					- **解决方案**
						- 使用 Windows Update 更新系统补丁
				- 运行程序时提示 计算机中丢失 api-ms-win-core-winrt-l1-1-0.dll
					- **解决方案**
						- 下载 api-ms-win-core-winrt-l1-1-0.dll 文件放入程序根目录(Steam++.exe 所在文件夹)
							- [从 Github 上直接下载](https://github.com/BeyondDimension/SteamTools/raw/develop/references/runtime.win7-x64.Microsoft.NETCore.Windows.ApiSets/api-ms-win-core-winrt-l1-1-0.dll)
							- [从 Gitee 上直接下载](https://gitee.com/rmbgame/SteamTools/raw/develop/references/runtime.win7-x64.Microsoft.NETCore.Windows.ApiSets/api-ms-win-core-winrt-l1-1-0.dll)
	- Android
		- 本地加速
			- 因 Android 7(Nougat API 24) 之后的版本不在信任用户证书，所以此功能已放弃继续开发，如仍想使用需要自行导入证书到系统目录，使用 adb 工具或 Magisk 之类的软件操作，未来会使用不需要证书的加速功能替换此功能

-->


[![WebSite steampp.net](https://img.shields.io/badge/WebSite-steampp.net-brightgreen.svg?style=flat-square&color=61dafb)](https://steampp.net)
[![Steam++ v2.8.5](https://img.shields.io/badge/Steam++-v2.8.5-brightgreen.svg?style=flat-square&color=512bd4)]()
  
  
##### [不知道该下载哪个文件?](./download-guide.md)
---

### 文件校验
|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.8.5.7z  | SHA256 |
| Steam++_win_x64_fde_v2.8.5.7z  | SHA256 |
| | |
| Steam++_win_x64_v2.8.5.exe  | SHA256 |
| Steam++_win_x64_fde_v2.8.5.exe  | SHA256 |
| | |
| Steam++_win_x86_v2.8.5.7z  | SHA256 |
| Steam++_win_x86_fde_v2.8.5.7z  | SHA256 |
| | |
| Steam++_win_x86_v2.8.5.exe  | SHA256 |
| Steam++_win_x86_fde_v2.8.5.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.8.5.7z  | SHA256 |
| Steam++_linux_arm64_v2.8.5.7z  | SHA256 |
| | |
| Steam++_linux_x64_v2.8.5.deb  | SHA256 |
| Steam++_linux_arm64_v2.8.5.deb  | SHA256 |
| | |
| Steam++_linux_x64_v2.8.5.rpm  | SHA256 |
| Steam++_linux_arm64_v2.8.5.rpm  | SHA256 |
| | |
| Steam++_macos_x64_v2.8.5.dmg  | SHA256 |
| Steam++_macos_arm64_v2.8.5.dmg  | SHA256 |
| | |
| Steam++_android_v2.8.5.apk  | SHA256 |
