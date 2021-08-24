### 新增内容
1. Linux/macOS 版本中的 CLR 升级至 .NET 6 Preview 7
2. Desktop 现已适配 WinUI 3 / Windows 11 / Fluent Design System 样式风格
3. 新增 Android 上从图库选择二维码图片导入令牌
4. 新增 Android 上文件导入选择二维码图片导入令牌
5. 新增 Desktop 上现可刷新头像

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
		- 某些窗口顶部会有两个标题栏
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
| osx-arm64  | ✅ | β |
| linux-arm64  | ✅ | β |
| linux-arm  | ✅ | β |
| ios-arm64  | ❌ |  |
| win-arm64  | ❌ | |

## 下载指南
- macOS
	- 如果你使用 ARM 芯片的 Mac（较为**稀有**），例如 **M1**，则下载文件名中带有 **macos_arm64** 的文件
	- 如果你使用 Intel、AMD 芯片的 Mac（较为**普遍**），则下载文件名中带有 **macos_x64** 的文件
- Linux
	- 如果你使用 Intel、AMD 芯片的 PC（较为**普遍**），则下载文件名中带有 **linux_x64** 的文件
	- 如果你使用 ARM64 芯片的 PC（较为**稀有**），例如 **Raspberry Pi Model 3+**，则下载文件名中带有 **linux_arm64** 的文件
	- 如果你使用 ARM 芯片的 PC（较为**稀有**），例如 **Raspberry Pi Model 2+**，则下载文件名中带有 **linux_arm** 的文件
- Android
	- 如果你使用 ARM64 芯片的设备（较为**普遍**），则下载文件名中带有 **android_arm64_v8a** 的文件
	- 如果你使用 ARM 芯片的设备（较为**稀有**），通常为 **2014** 年下半年之前生产的手机、平板等设备，则下载文件名中带有 **android_armeabi_v7a** 的文件

|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.4.10.7z  | SHA256 |
| Steam++_win_x64_v2.4.10.exe  | SHA256 |
| Steam++_win_arm64_v2.4.10.7z  | SHA256 |
| Steam++_win_arm64_v2.4.10.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.4.10.7z  | SHA256 |
| Steam++_linux_arm64_v2.4.10.7z  | SHA256 |
| Steam++_linux_arm_v2.4.10.7z  | SHA256 |
| | |
| Steam++_macos_x64_v2.4.10.dmg  | SHA256 |
| Steam++_macos_arm64_v2.4.10.dmg  | SHA256 |
| | |
| Steam++_android_arm64_v8a_v2.4.10.apk  | SHA256 |
| Steam++_android_armeabi_v7a_v2.4.10.apk  | SHA256 |

<!-- ***

由于程序体积较大，推荐从 [官网 https://steampp.net](https://steampp.net) 中下载 -->
