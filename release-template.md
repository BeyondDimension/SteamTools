### 新增内容
1. Linux/macOS 版本中的 CLR 升级至 .NET 6 Preview 7
2. 桌面端现已适配 WinUI 3 / Windows 11 / Fluent Design System 样式风格
3. 新增 Android 上从图库选择二维码图片导入令牌
4. 新增 Android 上文件导入选择二维码图片导入令牌

### 修复问题
1. 修复 账号切换功能
2. 改进 受保护的成就不在支持勾选
3. 修复 Linux 上因字体引发的启动时闪退
4. 修复 Android 上扫码导入功能
5. 修复 Android 上 Toast 不能正常显示
6. 改进 Android 上的令牌导入方式
7. 修复 Android 上导入带密码的令牌时不显示密码输入框
8. 修复 导入带密码的令牌时密码输入文本框窗口不能正确取消
9. 修复 使用火狐浏览器无法进行快速登录
10. 修复 Android 8.0 以下启动时闪退
11. 修复 部分用户库存游戏已安装游戏无法读取
12. 尝试修复 Windows 上托盘菜单有时无法打开窗口
13. 改进 Android 上确认交易页面上的显示隐藏逻辑

### 已知问题
- macOS
	- 尚未公证，这会影响 macOS Catalina（版本 10.15）以上
	- 自动更新不可用
- Linux
	- 托盘不生效，这将影响程序不能正常退出
	- 窗口弹出位置不正确
	- 自动更新不可用

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
| win-arm64  | ❌ | |
| osx-arm64  | ❌ | |
| linux-arm64  | ❌ | |
| ios-arm64  | ❌ |  |

|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.4.10.7z  | SHA256 |
| Steam++_win_x64_v2.4.10.exe  | SHA256 |
| | |
| Steam++_win_arm64_v2.4.10.7z  | SHA256 |
| Steam++_win_arm64_v2.4.10.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.4.10.7z  | SHA256 |
| | |
| Steam++_linux_arm64_v2.4.10.7z  | SHA256 |
| | |
| Steam++_linux_arm_v2.4.10.7z  | SHA256 |
| | |
| Steam++_macos_x64_v2.4.10.dmg  | SHA256 |
| Steam++_macos_arm64_v2.4.10.dmg  | SHA256 |
| | |
| Steam++_android_arm64_v8a_v2.4.10.apk  | SHA256 |
| Steam++_android_armeabi_v7a_v2.4.10.apk  | SHA256 |

<!-- ***

由于程序体积较大，推荐从 [官网 https://steampp.net](https://steampp.net) 中下载 -->
