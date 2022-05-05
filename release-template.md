### 版本亮点
1. ASF 升级至 V5.2.5.5

<!--
1. Android 支持 VPN 模式(推荐) 进行本地加速
2. 新增 Steam 游戏信息编辑功能，可修改游戏名称、图片、启动项等数据并同步至 Steam 客户端生效
3. ASF 升级至 V5.2.5.4
4. 优化了显示图片时 GPU 占用
5. 优化库存游戏和脚本内存占用
6. 库存游戏编辑功能支持从 SteamGridDB 匹配预览和下载图片
7. 网络加速新增 MEGA 网盘反代服务
8. macOS 支持 Arm64(Apple Silicon)

. ~~Windows 支持 DNS 驱动拦截模式(推荐) 进行本地加速~~
-->

### 修复问题
1. 修复 判断 Administrator 或 Root 权限函数错误，例如导致 Windows 上开机自启失效等其他问题

<!--
1. 修复 库存游戏中挂时长失效
2. 修复 Steam 库存游戏在 Steam 账号切换后自动刷新失败的问题
3. 修复 Steam 库存游戏编辑带有本地化名称的游戏无效的问题
4. 修复 Android 上登录后出现两个退出登录按钮
5. 改进 Android 上第三方快速登录使用系统默认浏览器
6. 修复 一些图片加载失败不显示的问题
7. 改进 从本地加载图片减少不必要的内存分配
-->

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
[![StmToolkit v2.7.2](https://img.shields.io/badge/StmToolkit-v2.7.2-brightgreen.svg?style=flat-square&color=512bd4)]()
  
  
##### [不知道该下载哪个文件?](./download-guide.md)
---

### 文件校验
|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| StmToolkit_win_x64_v2.7.2.7z  | SHA256 |
| StmToolkit_win_x64_fde_v2.7.2.7z  | SHA256 |
| | |
| StmToolkit_win_x64_v2.7.2.exe  | SHA256 |
| StmToolkit_win_x64_fde_v2.7.2.exe  | SHA256 |
| | |
| StmToolkit_linux_x64_v2.7.2.7z  | SHA256 |
| StmToolkit_linux_arm64_v2.7.2.7z  | SHA256 |
| | |
| StmToolkit_linux_x64_v2.7.2.deb  | SHA256 |
| StmToolkit_linux_arm64_v2.7.2.deb  | SHA256 |
| | |
| StmToolkit_linux_x64_v2.7.2.rpm  | SHA256 |
| StmToolkit_linux_arm64_v2.7.2.rpm  | SHA256 |
| | |
| StmToolkit_macos_x64_v2.7.2.dmg  | SHA256 |
| StmToolkit_macos_arm64_v2.7.2.dmg  | SHA256 |
| | |
| StmToolkit_android_v2.7.2.apk  | SHA256 |
