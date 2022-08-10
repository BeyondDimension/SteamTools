### 公告
1. 非简中语言将默认隐藏加速和脚本功能，仅能通过切换语言并重启程序的方式还原被隐藏的功能
2. 因经济状况原因，现已停止短信服务节约开销，后续会推出邮箱注册登录，对于仅使用手机号登录的用户请绑定第三方快速登录，否则注销后将无法再次登录，需要等待至邮箱服务推出后支持会暂时在开放短信服务提供换绑邮箱。
3. 自动更新目前仅 Windows 端可用，且由于下载渠道限速可能导致无法更新成功，推荐在官网链接的网盘或群文件中下载压缩包解压覆盖更新(应用商店版由商店更新不受此影响)
4. 在 Android 上因系统限制，目前的加速功能无法正常使用，所以此功能已放弃继续开发，如仍想使用需要自行导入证书到系统目录，使用 adb 工具或 Magisk 之类的软件操作，未来会使用不需要证书的加速功能替换此功能
5. fde 版本需要安装 [ASP.NET Core 运行时 6.0.8 (x64) 与 .NET Core 运行时 6.0.8 (x64)](https://dotnet.microsoft.com/zh-cn/download/dotnet/6.0)
6. Windows x86 与 x64 版本令牌本机加密互不兼容，使用两者版本时注意令牌加密后的文件不能共用。
7. 由于新版本加速功能重构，调整了部分加速项目，这会影响旧版本程序使用加速功能
8. 为了能继续维持开发，从此版本开始将会添加程序内广告，赞助用户可以在设置中关闭所有广告

### 版本亮点
1. 修复与改进一些已知问题

### 修复问题
1. 修复 Windows 上版本更新在 2.8.x 中非 FDE 版本识别为 FDE 版
2. 改进 调整代理模式顺序，Hosts 模式置顶
3. 修复 划词翻译等一些 JS 脚本
4. 改进 macOS 上的证书安装
5. 修复 可能导致内存泄露的问题
6. 改进 非简中默认隐藏加速与脚本仅在 Windows 上生效
7. 改进 减少 Android 上的启动时间
8. 修复 退出程序时可能引发的闪退

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
[![Steam++ v2.8.3](https://img.shields.io/badge/Steam++-v2.8.3-brightgreen.svg?style=flat-square&color=512bd4)]()
  
  
##### [不知道该下载哪个文件?](./download-guide.md)
---

### 文件校验
|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.8.3.7z  | SHA256 |
| Steam++_win_x64_fde_v2.8.3.7z  | SHA256 |
| | |
| Steam++_win_x64_v2.8.3.exe  | SHA256 |
| Steam++_win_x64_fde_v2.8.3.exe  | SHA256 |
| | |
| Steam++_win_x86_v2.8.3.7z  | SHA256 |
| Steam++_win_x86_fde_v2.8.3.7z  | SHA256 |
| | |
| Steam++_win_x86_v2.8.3.exe  | SHA256 |
| Steam++_win_x86_fde_v2.8.3.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.8.3.7z  | SHA256 |
| Steam++_linux_arm64_v2.8.3.7z  | SHA256 |
| | |
| Steam++_linux_x64_v2.8.3.deb  | SHA256 |
| Steam++_linux_arm64_v2.8.3.deb  | SHA256 |
| | |
| Steam++_linux_x64_v2.8.3.rpm  | SHA256 |
| Steam++_linux_arm64_v2.8.3.rpm  | SHA256 |
| | |
| Steam++_macos_x64_v2.8.3.dmg  | SHA256 |
| Steam++_macos_arm64_v2.8.3.dmg  | SHA256 |
| | |
| Steam++_android_v2.8.3.apk  | SHA256 |
