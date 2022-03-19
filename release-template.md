### 版本亮点
1. macOS 支持 Arm64(Apple Silicon)
2. Android 支持 VPN 模式(推荐) 进行本地加速
3. Windows 支持 DNS 驱动拦截模式(推荐) 进行本地加速
4. ASF 升级至 V5.x.x.x

### 修复问题
1. 修复 本地令牌 无令牌刷新时提示密码错误
2. 改进 Android 端 本地令牌 列表样式第一条与最后一条的上下外边距
3. 改进 俄语翻译，由 vanja-san 提供
4. 改进 .NET 运行时升级至 6.0.3
5. 改进 脚本配置 未启动时的内存占用，以及减少总体内存占用率
6. 修复 Hosts 加速模式下使用仅启用脚本功能导致死循环
7. 改进 Linux 端 可监听 443 端口配置
8. 修复 Windows 端，动态桌面背景窗口显示时一些可能导致闪退的潜在问题
9. 修复 Android 端，因 CheckBox 导致在低于 6.0 系统上引发的闪退

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
				- 运行程序时提示 计算机中丢失 api-ms-win-core-winrt-l1-1-0.dll
			- **解决方案**
				- 因 [Windows 7 延长结束日期](https://support.microsoft.com/zh-cn/windows/windows-7-%E6%94%AF%E6%8C%81%E4%BA%8E-2020-%E5%B9%B4-1-%E6%9C%88-14-%E6%97%A5%E7%BB%88%E6%AD%A2-b75d4580-2cc7-895a-2c9c-1466d9a53962)以于 2020 年 1 月 14 日结束支持
					- 所以必须安装 [Extended Security Update](https://docs.microsoft.com/zh-cn/troubleshoot/windows-client/windows-7-eos-faq/windows-7-extended-security-updates-faq) 支持，在安装第三年的补丁后[结束支持日期](https://docs.microsoft.com/zh-cn/lifecycle/products/windows-7)可延长至 2023 年 1 月 10 日
					- 可安装 *第三方* 补丁整合包例如 **[UpdatePack7R2](https://cn.bing.com/search?q=UpdatePack7R2)** *或* 购买官方 ESU 产品密钥 解决
				- 下载 api-ms-win-core-winrt-l1-1-0.dll 文件放入程序根目录(Steam++.exe 所在文件夹)
					- [从 Github 上直接下载](https://github.com/BeyondDimension/SteamTools/raw/develop/references/runtime.win7-x64.Microsoft.NETCore.Windows.ApiSets/api-ms-win-core-winrt-l1-1-0.dll)
					- [从 Gitee 上直接下载](https://gitee.com/rmbgame/SteamTools/raw/develop/references/runtime.win7-x64.Microsoft.NETCore.Windows.ApiSets/api-ms-win-core-winrt-l1-1-0.dll)
					- [从 NuGet 上下载后提取](https://www.nuget.org/api/v2/package/runtime.win7-x64.Microsoft.NETCore.Windows.ApiSets/1.0.1)
						- .nupkg 文件可使用解压工具打开或解压，找到此文件复制即可

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
