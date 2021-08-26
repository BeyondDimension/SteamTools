### 新增内容


### 修复问题
1. 修复 Desktop 上用户头像应当为圆形而不是方形
2. 修复 Android 上切换系统语言可能引发的闪退
3. 修复 Windows 10 上启动时可能出现的网络连接中断提示

### 已知问题
- Desktop 
	- macOS
		- 尚未公证，这会影响 macOS Catalina（版本 10.15）以上
		- 某些窗口顶部会有两个标题栏
		- 自动更新不可用
	- Linux
		- 托盘不生效，这将影响程序不能正常退出
		- 窗口弹出位置不正确
		- 窗口顶部会有两个标题栏
		- 自动更新不可用
	- Shared
		- 主题切换需重启软件后生效，且跟随系统暂不可用
- Mobile
	- Android
		- 本地令牌倒计时存在误差不够精确，可能导致令牌值不一致
		- 确认交易列表刷新后数据显示不正确
		- 自动更新暂不可用

***

<!-- 1. 新增 ASF Plus 本地挂卡
3. 改进 新增守护进程，当程序闪退时将自动重启 -->

|  RuntimeIdentifier  |  Available  |  Edition  |
|  ----  |  ----  |  ----  |
| win-x64  | ✅ | Stable |
| osx-x64  | ✅ | β |
| linux-x64  | ✅ | α |
| android-arm64  | ✅ | α |
| android-arm  | ✅ | α |
| linux-arm64  | ✅ | α |
| linux-arm  | ✅ | α |
| osx-arm64  | ❌ |  |
| win-arm64  | ❌ |  |
| ios-arm64  | ❌ |  |

<!-- 
- macOS
	- 如果你使用 ARM 芯片的 Mac（较为**稀有**），例如 **M1**，则下载文件名中带有 **macos_arm64** 的文件
	- 如果你使用 Intel、AMD 芯片的 Mac（较为**普遍**），则下载文件名中带有 **macos_x64** 的文件
-->

## 下载指南
- Linux
	- 如果你使用 Intel、AMD 芯片的 PC（较为**普遍**）则下载文件名中带有 **linux_x64** 的文件
	- 如果你使用 ARM64 芯片的 PC（较为**稀有**）例如 **Raspberry Pi Model 3+**，则下载文件名中带有 **linux_arm64** 的文件
	- 如果你使用 ARM 芯片的 PC（较为**稀有**）例如 **Raspberry Pi Model 2+**，则下载文件名中带有 **linux_arm** 的文件
- Android
	- 如果你使用 ARM64 芯片的设备（较为**普遍**）则下载文件名中带有 **android_arm64_v8a** 的文件
	- 如果你使用 ARM 芯片的设备（较为**稀有**）通常为 **14** 年下半年之前生产的设备，则下载文件名中带有 **android_armeabi_v7a** 的文件

|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.4.12.7z  | SHA256 |
| Steam++_win_x64_v2.4.12.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.4.12.7z  | SHA256 |
| Steam++_linux_arm64_v2.4.12.7z  | SHA256 |
| Steam++_linux_arm_v2.4.12.7z  | SHA256 |
| | |
| Steam++_macos_x64_v2.4.12.dmg  | SHA256 |
| Steam++_macos_x64_v2.4.12.app.zip  | SHA256 |
| Steam++_macos_x64_v2.4.12.7z  | SHA256 |
| | |
| Steam++_android_arm64_v8a_v2.4.12.apk  | SHA256 |
| Steam++_android_armeabi_v7a_v2.4.12.apk  | SHA256 |

<!-- ***

由于程序体积较大，推荐从 [官网 https://steampp.net](https://steampp.net) 中下载 -->
