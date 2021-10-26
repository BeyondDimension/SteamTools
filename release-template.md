### 新增内容
1. CLR 更新至 6.0 RC2
2. 新增 ASF Plus 本地挂卡
3. 新增 本地令牌搜索框
4. 新增 库存游戏右键菜单导航到 Steam 客户端
5. 改进 桌面端 UI 适配 Windows 11 风格
6. 新增 Windows 11 上可设置材质 [云母(Mica)](https://docs.microsoft.com/zh-cn/windows/apps/design/style/mica)
7. 改进 Android UI
8. 改进 Android 冷启动速度
9. 新增 Android x86 架构包，适用于 Intel、AMD 芯片的设备
10. 改进 Windows 上现使用单文件打包，不再需要 ```Bin``` 文件夹
11. 改进 Hosts 文件在 Windows 上默认使用 UTF8WithBOM 编码

### 修复问题
1. 修复 本地令牌中确认交易时 Http 302 重定向错误
2. 修复 Linux 与 macOS 上代理错误
3. 修复 库存游戏无限加载
4. 修复 脚本未启用时保存状态会全部未启用
5. 修复 Android 上屏幕捕获设置项不生效

### 已知问题
- Desktop 
	- macOS
		- 尚未公证，这会影响 macOS Catalina（版本 10.15）以上
		- 某些窗口顶部会有两个标题栏
		- 自动更新不可用
	- Linux
		- 在 Deepin 中托盘不生效，可通过 Exit.sh 退出程序
		- 窗口弹出位置不正确
		- 窗口顶部会有两个标题栏
		- 自动更新不可用
	- Shared
		- 主题切换需重启软件后生效，且跟随系统暂不可用
- Mobile
	- Android
		- 确认交易列表刷新后数据有时会显示不正确
		- 自动更新暂不可用

***

|  RuntimeIdentifier  |  Available  |  Edition  |
|  ----  |  ----  |  ----  |
| win-x64  | ✅ | Stable |
| osx-x64  | ✅ | β |
| linux-x64  | ✅ | α |
| android-arm64  | ✅ | α |
| android-arm  | ✅ | α |
| android-x86  | ✅ | α |
| linux-arm64  | ✅ | α |
| linux-arm  | ✅ | α |
| osx-arm64  | ❌ |  |
| win-arm64  | ❌ |  |
| ios-arm64  | ❌ |  |
| android-x64  | ❌ |  |

## 下载指南
- Windows
	- 如果你使用 Intel、AMD 的 x64 芯片的 Mac（较为**普遍**），则下载文件名中带有 **win_x64** 的文件
	- **[暂未支持]** ~~如果你使用 ARM64 芯片的 PC（极为**稀有**），例如 **Surface Pro X**，则下载文件名中带有 **win_arm64** 的文件~~
- macOS
	- 如果你使用 Intel、AMD 的 x64 芯片的 Mac（较为**普遍**），则下载文件名中带有 **macos_x64** 的文件
	- 如果你使用 ARM64 芯片的 Mac（较为**稀有**），例如 **M1**，则下载文件名中带有 **macos_x64** 的文件可通过 [Rosetta 2](https://support.apple.com/zh-cn/HT211861) 运行
	- **[暂未支持]** ~~如果你使用 ARM64 芯片的 Mac（较为**稀有**），例如 **M1**，则下载文件名中带有 **macos_arm64** 的文件~~
- Linux
	- 如果你使用 Intel、AMD 的 x64 芯片的 PC（较为**普遍**）则下载文件名中带有 **linux_x64** 的文件
	- 如果你使用 ARM64 芯片的 PC（较为**稀有**）例如 **Raspberry Pi Model 3+**，则下载文件名中带有 **linux_arm64** 的文件
	- 如果你使用 ARM32 芯片的 PC（较为**稀有**）例如 **Raspberry Pi Model 2+**，则下载文件名中带有 **linux_arm** 的文件
- Android
	- 如果你使用 ARM64 芯片的设备（较为**普遍**）则下载文件名中带有 **android_arm64_v8a** 的文件
	- 如果你使用 ARM32 芯片的设备（较为**稀有**）通常为 **14** 年下半年之前生产的设备，则下载文件名中带有 **android_armeabi_v7a** 的文件
	- 如果你使用 Intel、AMD 的 x86 芯片的设备（较为**稀有**）则下载文件名中带有 **android_x86** 的文件

|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.5.0.7z  | SHA256 |
| Steam++_win_x64_v2.5.0.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.5.0.7z  | SHA256 |
| Steam++_linux_arm64_v2.5.0.7z  | SHA256 |
| Steam++_linux_arm_v2.5.0.7z  | SHA256 |
| | |
| Steam++_macos_x64_v2.5.0.dmg  | SHA256 |
| Steam++_macos_x64_v2.5.0.app.zip  | SHA256 |
| Steam++_macos_x64_v2.5.0.7z  | SHA256 |
| | |
| Steam++_android_arm64_v8a_v2.5.0.apk  | SHA256 |
| Steam++_android_armeabi_v7a_v2.5.0.apk  | SHA256 |
| Steam++_android_x86_v2.5.0.apk  | SHA256 |

<!-- ***

由于程序体积较大，推荐从 [官网 https://steampp.net](https://steampp.net) 中下载 -->