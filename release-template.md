### 版本亮点
1. Android 支持 VPN 模式(推荐) 进行本地加速
2. 新增 Steam 游戏信息编辑功能，可修改游戏名称、图片、启动项等数据并同步至 Steam 客户端生效
3. ASF 升级至 V5.2.5.4
4. 优化了显示图片时 GPU 占用
5. 优化库存游戏和脚本内存占用
6. 库存游戏编辑功能支持从 SteamGridDB 匹配预览和下载图片
7. 网络加速新增 MEGA 网盘反代服务
8. macOS 支持 Arm64(Apple Silicon)

<!--
. ~~Windows 支持 DNS 驱动拦截模式(推荐) 进行本地加速~~
-->

### 修复问题
1. 修复 本地令牌 无令牌刷新时提示密码错误
2. 改进 Android 端 本地令牌 列表样式第一条与最后一条的上下外边距
3. 改进 俄语翻译，由 vanja-san 提供
4. 改进 .NET 运行时升级至 6.0.4(仅 Desktop 端)
5. 改进 脚本配置 未启动时的内存占用，以及减少总体内存占用率
6. 修复 Hosts 加速模式下使用仅启用脚本功能导致死循环
7. 改进 Linux 端 可监听 443 端口配置
8. 修复 Windows 端，动态桌面背景窗口显示时一些可能导致闪退的潜在问题
9. 修复 Android 端，因 CheckBox 导致在低于 6.0 Marshmallow 系统上引发的闪退
10. 修复 Desktop 高 DPI 分辨率下菜单图标会显示模糊的问题
11. 修复 Windows 端，切换至网络加速菜单时可能会出现 UI 错乱的问题
12. 修复 Desktop 端，某些情况库存游戏会卡住无限加载的问题
13. 修复 网络加速 Onedrive 加速失效问题
14. 修复 消息框不再提醒复选框勾上可能不生效的问题
15. 修复 Windows 端, JumpList 切换 Steam Beta 账号失效的问题
16. 修复 ASF，当使用 IPC.config 时，程序内打开网页端口号值不正确

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


[![steampp.net](https://img.shields.io/badge/WebSite-steampp.net-brightgreen.svg?style=flat-square&color=61dafb)](https://steampp.net)
[![Steam++ v2.7.0](https://img.shields.io/badge/Steam++-v2.7.0-brightgreen.svg?style=flat-square&color=512bd4)]()
  
  
##### [不知道该下载哪个文件?](./download-guide.md)
---

### 文件校验
|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.7.0.7z  | SHA256 |
| Steam++_win_x64_fde_v2.7.0.7z  | SHA256 |
| | |
| Steam++_win_x64_v2.7.0.exe  | SHA256 |
| Steam++_win_x64_fde_v2.7.0.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.7.0.7z  | SHA256 |
| Steam++_linux_arm64_v2.7.0.7z  | SHA256 |
| | |
| Steam++_linux_x64_v2.7.0.deb  | SHA256 |
| Steam++_linux_arm64_v2.7.0.deb  | SHA256 |
| | |
| Steam++_linux_x64_v2.7.0.rpm  | SHA256 |
| Steam++_linux_arm64_v2.7.0.rpm  | SHA256 |
| | |
| Steam++_macos_x64_v2.7.0.dmg  | SHA256 |
| Steam++_macos_arm64_v2.7.0.dmg  | SHA256 |
| | |
| Steam++_android_v2.7.0.apk  | SHA256 |
