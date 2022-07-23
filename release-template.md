### 公告
1. 非简中语言将默认隐藏加速和脚本功能，仅能通过切换语言并重启程序的方式还原被隐藏的功能
2. 因经济状况原因，现已停止短信服务节约开销，后续会推出邮箱注册登录，对于仅使用手机号登录的用户请绑定第三方快速登录，否则注销后将无法再次登录，需要等待至邮箱服务推出后支持会暂时在开放短信服务提供换绑邮箱。
3. 自动更新目前仅 Windows 端可用，且由于下载渠道限速可能导致无法更新成功，推荐在官网链接的网盘或群文件中下载压缩包解压覆盖更新(应用商店版由商店更新不受此影响)
4. 在 Android 上因系统限制，目前的加速功能无法正常使用，所以此功能已放弃继续开发，如仍想使用需要自行导入证书到系统目录，使用 adb 工具或 Magisk 之类的软件操作，未来会使用不需要证书的加速功能替换此功能
5. fde 版本需要安装 [ASP.NET Core 运行时 6.0.7 (x64) 与 .NET Core 运行时 6.0.7 (x64)](https://dotnet.microsoft.com/zh-cn/download/dotnet/6.0)
6. Windows x86 与 x64 版本令牌本机加密互不兼容，使用两者版本时注意令牌加密后的文件不能共用。
7. 由于新版本加速功能重构，调整了部分加速项目，这会影响旧版本程序使用加速功能
8. 为了能继续维持开发，从此版本开始将会添加程序内广告，赞助用户可以在设置中关闭所有广告

### 版本亮点
1. 新增 Steam 云存档管理功能，可自行上传或删除 Steam 云存档
2. 库存游戏支持筛选支持 Steam 云存档的游戏
3. ASF 升级至 V5.2.7.7
4. .NET 运行时升级至 6.0.7，使用 fde 版本需要升级运行时
5. 库存游戏中解锁成就与挂时长支持 macOS 与 Linux 系统
6. 在设置中可关闭托盘，关闭托盘后关闭主窗口即退出程序
7. 使用 Yarp.ReverseProxy 重写了反代加速和脚本功能，大幅提升稳定性与性能
8. Windows 新增 DNS 驱动拦截模式进行本地加速
9. 令牌交易现在支持查看交易详情，可以确认交易方的Steam注册时间等信息
10. 恢复 Windows x86(32 位) 版本发布

### 修复问题
1. 修复 窗口在某些情况下最大化或最小化恢复时窗口大小会变化的问题
2. 修复 库存游戏 编辑 Steam 游戏封面时选择自定义图片失败的问题
3. 修复 程序内在显示某些图片时会出现错乱马赛克的问题
4. 修复 Linux 上点击关于页面可能因字体引发闪退
5. 修复 MIUI Android 11 ~ 12 中绑定或换绑手机号页面闪退
6. 修复 Android 本地加速中已知问题弹窗显示时不应同时跳转引导证书页
7. 修复 令牌交易确认要求输本地令牌密钥时点击取消也能进行交易确认的问题
8. 修复 令牌加载输入密码解密时点击取消或输入错误密码没有移除此前的数据问题
9. 修复 2.8.1 中出现因脚本导致的启动加速服务失败
10. 修复 网络加速中图标在跟随系统的浅色模式下字体颜色不应为白色
11. 修复 系统代理模式与 PAC 代理模式中监听地址为 0.0.0.0 时出现的错误
12. 修复 切换页面 UI 布局错乱

<!--

TODO：
Android 第三方快速登录回调改为 URL Scheme
修复 Android 检查更新检测不到最新版本的问题


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
[![Steam++ v2.8.1](https://img.shields.io/badge/Steam++-v2.8.1-brightgreen.svg?style=flat-square&color=512bd4)]()
  
  
##### [不知道该下载哪个文件?](./download-guide.md)
---

### 文件校验
|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.8.1.7z  | SHA256 |
| Steam++_win_x64_fde_v2.8.1.7z  | SHA256 |
| | |
| Steam++_win_x64_v2.8.1.exe  | SHA256 |
| Steam++_win_x64_fde_v2.8.1.exe  | SHA256 |
| | |
| Steam++_win_x86_v2.8.1.7z  | SHA256 |
| Steam++_win_x86_fde_v2.8.1.7z  | SHA256 |
| | |
| Steam++_win_x86_v2.8.1.exe  | SHA256 |
| Steam++_win_x86_fde_v2.8.1.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.8.1.7z  | SHA256 |
| Steam++_linux_arm64_v2.8.1.7z  | SHA256 |
| | |
| Steam++_linux_x64_v2.8.1.deb  | SHA256 |
| Steam++_linux_arm64_v2.8.1.deb  | SHA256 |
| | |
| Steam++_linux_x64_v2.8.1.rpm  | SHA256 |
| Steam++_linux_arm64_v2.8.1.rpm  | SHA256 |
| | |
| Steam++_macos_x64_v2.8.1.dmg  | SHA256 |
| Steam++_macos_arm64_v2.8.1.dmg  | SHA256 |
| | |
| Steam++_android_v2.8.1.apk  | SHA256 |
