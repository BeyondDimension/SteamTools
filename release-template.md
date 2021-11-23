### 新增内容
1. CLR 更新至 6.0 RTM
2. 新增 捐助功能，在关于中可使用 **爱发电**、**Ko-fi**、**Patreon** 平台捐助
3. 新增 ASF 本地挂卡功能 (Beta)
4. 新增 本地令牌搜索功能
5. 新增 库存游戏右键菜单导航到 Steam 客户端
6. 新增 Windows 11 上可设置材质 [云母(Mica)](https://docs.microsoft.com/zh-cn/windows/apps/design/style/mica)
7. 新增 搜索框支持拼音搜索
8. 新增 框架依赖部署模式(FDE)，可通过共享运行库减少磁盘占用空间，仅支持 Windows 与 Linux
9. 新增 Windows 上可将动态壁纸设置为程序背景
10. 新增 桌面端 背景材质设置，并修复之前AcrylicBlur透明效果异常问题
11. 改进 桌面端 UI 适配 Windows 11 风格
12. 改进 Hosts 文件在 Windows 上默认使用 UTF8WithBOM 编码
13. 改进 账号注销现需要通过手机号或昵称验证
14. 改进 令牌账号加密、导出的界面UI和导入过程中的提示
15. 改进 Steam 账号切换支持头像框、等级和游戏中信息的显示
16. 改进 文本框窗口弹出时将自动设置焦点
17. 改进 Windows 上端口占用提示文本显示占用该端口的进程名
18. 改进 Linux 上存储数据遵循 [XDG Base Directory Specification](https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html)
19. 改进 主题切换现在不在需要重启程序，提升主题切换速度
20. 改进 本地令牌账号导入过程中的说明提示
21. 改进 Windows 上账号切换启动 Steam 时，默认不在以管理员身份启动
22. 改进 macOS 上修改 hosts 文件可以不用每次输入密码
<!--

. 改进 Android UI
. 改进 Android 冷启动速度
. 新增 Android x86 架构包，适用于 Intel、AMD 芯片的设备
. 改进 Android 导入令牌成功后回到列表页

-->

### 修复问题
1. 修复 本地令牌 中确认交易时 Http 302 重定向错误
2. 修复 Linux 与 macOS 上代理错误
3. 修复 库存游戏无限加载
4. 修复 脚本未启用时保存状态会全部未启用
5. 修复 桌面端 上主题运行时切换与跟随系统
6. 修复 Windows 上窗口边缘滚动条难以拖拽
7. 修复 本地令牌 确认交易登录时会错误的提示没有开启加速
8. 修复 本地令牌 确认交易有时会卡在提示登录中的问题
9. 修复 Windows 上资源管理器重启后托盘消失，以及尝试修复开机自启时有时不显示托盘
10. 修复 桌面端 上导航栏的弹出菜单失去焦点时不会自动隐藏的问题

<!--

. 修复 Android 上屏幕捕获设置项不生效
. 修复 Android 上令牌列表有时不显示值

-->

### 已知问题
- Desktop 
	- macOS
		- 尚未公证，这会影响 macOS Catalina（版本 10.15）以上
		- 自动更新不可用
	- Linux
		- 在 Deepin 中托盘不生效，可通过 ```Exit.sh``` 退出程序
		- 窗口弹出位置不正确
		- 自动更新不可用
		- 鼠标指针浮动样式不正确
	- Windows
		- 在 CPU 不受支持的 Win11 上无法启动，Windows 日志中显示 ```Failed to create CoreCLR, HRESULT: 0x80004005```
			- 仅 .NET 6.0 受此影响，在几周后的 Insider 中会修复，见 [issue](https://github.com/dotnet/core/issues/6733)
			- **解决方案：** 可尝试使用早期版本，例如 v2.3.0
	- Shared
		- 拼音搜索不能正确的识别多音字
- Mobile
	- Android
		- 确认交易列表刷新后数据有时会显示不正确
		- 自动更新暂不可用

## 下载指南
- Desktop
	- Windows
		- 如果你使用 Intel、AMD 的 x64 芯片的 Mac（较为**普遍**），则下载文件名中带有 **win_x64** 的文件
		- 如果你使用 ARM64 芯片的 PC（极为**稀有**），例如 **Surface Pro X**，则下载文件名中带有 **win_x64** 的文件可通过 Win11 x86 模拟运行
		- **[暂未支持]** ~~如果你使用 ARM64 芯片的 PC（极为**稀有**），例如 **Surface Pro X**，则下载文件名中带有 **win_arm64** 的文件~~
	- macOS
		- 如果你使用 Intel、AMD 的 x64 芯片的 Mac（较为**普遍**），则下载文件名中带有 **macos_x64** 的文件
		- 如果你使用 ARM64 芯片的 Mac（较为**稀有**），例如 **M1**，则下载文件名中带有 **macos_x64** 的文件可通过 [Rosetta 2](https://support.apple.com/zh-cn/HT211861) 运行
		- **[尚未测试]** ~~如果你使用 ARM64 芯片的 Mac（较为**稀有**），例如 **M1**，则下载文件名中带有 **macos_arm64** 的文件~~
	- Linux
		- 如果你使用 Intel、AMD 的 x64 芯片的 PC（较为**普遍**）则下载文件名中带有 **linux_x64** 的文件
		- 如果你使用 ARM64 芯片的 PC（较为**稀有**）例如 **Raspberry Pi Model 3+**，则下载文件名中带有 **linux_arm64** 的文件
		- 如果你使用 ARM32 芯片的 PC（较为**稀有**）例如 **Raspberry Pi Model 2+**，则下载文件名中带有 **linux_arm** 的文件
	- Shared
		- 如果你已安装了 [ASP.NET Core Runtime 6.0.0](https://dotnet.microsoft.com/download/dotnet/6.0) 则下载文件名中带有 **fde** 的文件，可减少磁盘占用空间
			- [在 Windows 上安装 .NET](https://docs.microsoft.com/en-us/dotnet/core/install/windows)
				- 在 Windows 上不需要 Hosting Bundle 和 IIS support
				- [下载安装程序并手动安装(aspnetcore-runtime-6.0.0-win-x64.exe)](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-aspnetcore-6.0.0-windows-x64-installer)
				- [使用 PowerShell 自动化安装](https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60#install-with-powershell-automation)
			- [在 Linux 上安装 .NET](https://docs.microsoft.com/en-us/dotnet/core/install/linux)
				- 推荐 [通过 Snap 安装 .NET Runtime](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux-snap)
				- ```sudo snap install dotnet-runtime-60 --classic```
- Mobile
	- Android
		- 如果你使用 ARM64 芯片的设备（较为**普遍**）则下载文件名中带有 **android_arm64_v8a** 的文件
		- 如果你使用 ARM32 芯片的设备（较为**稀有**）通常为 **14** 年下半年之前生产的设备，则下载文件名中带有 **android_armeabi_v7a** 的文件
		- 如果你使用 Intel、AMD 的 x86 芯片的设备（较为**稀有**）则下载文件名中带有 **android_x86** 的文件

|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.6.0.7z  | SHA256 |
| Steam++_win_x64_fde_v2.6.0.7z  | SHA256 |
| | |
| Steam++_win_x64_v2.6.0.exe  | SHA256 |
| Steam++_win_x64_fde_v2.6.0.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.0.7z  | SHA256 |
| Steam++_linux_arm64_v2.6.0.7z  | SHA256 |
| Steam++_linux_arm_v2.6.0.7z  | SHA256 |
| | |
| Steam++_linux_x64_fde_v2.6.0.7z  | SHA256 |
| Steam++_linux_arm64_fde_v2.6.0.7z  | SHA256 |
| Steam++_linux_arm_fde_v2.6.0.7z  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.0.deb  | SHA256 |
| Steam++_linux_arm64_v2.6.0.deb  | SHA256 |
| Steam++_linux_arm_v2.6.0.deb  | SHA256 |
| | |
| Steam++_linux_x64_fde_v2.6.0.deb  | SHA256 |
| Steam++_linux_arm64_fde_v2.6.0.deb  | SHA256 |
| Steam++_linux_arm_fde_v2.6.0.deb  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.0.rpm  | SHA256 |
| Steam++_linux_arm64_v2.6.0.rpm  | SHA256 |
| Steam++_linux_arm_v2.6.0.rpm  | SHA256 |
| | |
| Steam++_linux_x64_fde_v2.6.0.rpm  | SHA256 |
| Steam++_linux_arm64_fde_v2.6.0.rpm  | SHA256 |
| Steam++_linux_arm_fde_v2.6.0.rpm  | SHA256 |
| | |
| Steam++_macos_x64_v2.6.0.dmg  | SHA256 |
| Steam++_macos_arm64_v2.6.0.dmg  | SHA256 |
| Steam++_macos_x64_v2.6.0.7z  | SHA256 |
| Steam++_macos_arm64_v2.6.0.7z  | SHA256 |
| | |
| Steam++_android_arm64_v8a_v2.6.0.apk  | SHA256 |
| Steam++_android_armeabi_v7a_v2.6.0.apk  | SHA256 |
| Steam++_android_x86_v2.6.0.apk  | SHA256 |

<!-- ***
由于程序体积较大，推荐从 [官网 https://steampp.net](https://steampp.net) 中下载 -->
