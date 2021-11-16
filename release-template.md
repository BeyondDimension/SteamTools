### 新增内容
1. CLR 更新至 6.0 RTM
2. 新增 ASF Plus 本地挂卡
3. 新增 本地令牌搜索框
4. 新增 库存游戏右键菜单导航到 Steam 客户端
5. 改进 桌面端 UI 适配 Windows 11 风格
6. 新增 Windows 11 上可设置材质 [云母(Mica)](https://docs.microsoft.com/zh-cn/windows/apps/design/style/mica)
7. 改进 Android UI
8. 改进 Android 冷启动速度
9. 新增 Android x86 架构包，适用于 Intel、AMD 芯片的设备
10. 改进 Hosts 文件在 Windows 上默认使用 UTF8WithBOM 编码
11. 改进 Android 导入令牌成功后回到列表页
12. 改进 账号注销现需要通过手机号或昵称验证
13. 新增 搜索框支持拼音搜索
14. 改进 令牌账号导入与提示 UI
15. 新增 Windows 上将动态壁纸设置为程序背景
16. 改进 支持 Steam 头像框显示与等级和游戏中信息
17. 改进 文本框窗口弹出时将自动设置焦点
18. 改进 Windows 上端口占用提示文本显示占用该端口的进程名
19. 改进 Linux 上存储数据遵循 [XDG Base Directory Specification](https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html)
20. 改进 主题切换时响应速度
21. 新增 框架依赖部署模式(FDE)，可通过共享运行库减少磁盘占用空间

### 修复问题
1. 修复 本地令牌中确认交易时 Http 302 重定向错误
2. 修复 Linux 与 macOS 上代理错误
3. 修复 库存游戏无限加载
4. 修复 脚本未启用时保存状态会全部未启用
5. 修复 Android 上屏幕捕获设置项不生效
6. 修复 Android 上令牌列表有时不显示值
7. 修复 Desktop 上主题运行时切换与跟随系统
8. 修复 Windows 上窗口边缘滚动条难以拖拽

### 已知问题
- Desktop 
	- macOS
		- 尚未公证，这会影响 macOS Catalina（版本 10.15）以上
		- 某些窗口顶部会有两个标题栏
		- 自动更新不可用
	- Linux
		- 在 Deepin 中托盘不生效，可通过 ```Exit.sh``` 退出程序
		- 窗口弹出位置不正确
		- 窗口顶部会有两个标题栏
		- 自动更新不可用
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
		- 如果你已安装了 [.NET Runtime 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) 则下载文件名中带有 **fde** 的文件，可减少磁盘占用空间
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
| Steam++_linux_x64_fde_v2.6.0.7z  | SHA256 |
| Steam++_linux_arm64_fde_v2.6.0.7z  | SHA256 |
| Steam++_linux_arm_fde_v2.6.0.7z  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.0.deb  | SHA256 |
| Steam++_linux_arm64_v2.6.0.deb  | SHA256 |
| Steam++_linux_arm_v2.6.0.deb  | SHA256 |
| Steam++_linux_x64_fde_v2.6.0.deb  | SHA256 |
| Steam++_linux_arm64_fde_v2.6.0.deb  | SHA256 |
| Steam++_linux_arm_fde_v2.6.0.deb  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.0.rpm  | SHA256 |
| Steam++_linux_arm64_v2.6.0.rpm  | SHA256 |
| Steam++_linux_arm_v2.6.0.rpm  | SHA256 |
| Steam++_linux_x64_fde_v2.6.0.rpm  | SHA256 |
| Steam++_linux_arm64_fde_v2.6.0.rpm  | SHA256 |
| Steam++_linux_arm_fde_v2.6.0.rpm  | SHA256 |
| | |
| Steam++_macos_x64_v2.6.0.dmg  | SHA256 |
| Steam++_macos_x64_fde_v2.6.0.dmg  | SHA256 |
| Steam++_macos_arm64_v2.6.0.dmg  | SHA256 |
| Steam++_macos_arm64_fde_v2.6.0.dmg  | SHA256 |
| | |
| Steam++_macos_x64_v2.6.0.7z  | SHA256 |
| Steam++_macos_x64_fde_v2.6.0.7z  | SHA256 |
| Steam++_macos_arm64_v2.6.0.7z  | SHA256 |
| Steam++_macos_arm64_fde_v2.6.0.7z  | SHA256 |
| | |
| Steam++_android_arm64_v8a_v2.6.0.apk  | SHA256 |
| Steam++_android_armeabi_v7a_v2.6.0.apk  | SHA256 |
| Steam++_android_x86_v2.6.0.apk  | SHA256 |

<!-- ***

由于程序体积较大，推荐从 [官网 https://steampp.net](https://steampp.net) 中下载 -->
