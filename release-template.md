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
