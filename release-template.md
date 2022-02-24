### 版本亮点
1. ASF 升级至 **V5.2.2.5**
2. ASF 配置 中新增 设置自定义密钥(ASF_CRYPTKEY) 与 导入 Bot 文件
3. Linux 发行压缩包由 7z 格式更改为 tar.zst
4. 网络加速 支持 Steam 社区/讨论区
5. Android 端 新增 关于 - 捐助页

### 修复问题
1. 修复 Android 端 程序启动时自动运行 网络加速 与 ASF 服务
2. 修复 Android 端 网络加速 VPN 模式
3. 修复 Android 端 切换系统语言时可能导致的闪退
4. 修复 Android 端 拒绝授予存储权限导致的闪退
5. 改进 网络加速 中的代理证书生成
6. 改进 账号切换 中读取 Vdf 配置操作
7. 修复 Desktop 端 家庭共享库管理 在 Steam Beta 版上不能正常使用

### 已知问题
- 除 Windows 之外的平台此软件自动更新尚不可用
- Desktop 
	- macOS
		- [尚未公证](https://support.apple.com/zh-cn/guide/mac-help/mh40616/10.15/mac/10.15)，这会影响 macOS Catalina（版本 10.15）以上版本
	- Linux
		- **Hosts 代理模式**可能无法配置成功，推荐使用**系统代理模式**
		- 系统代理模式下 Discord 更新下载加速不可用
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
[![Steam++ v2.6.8](https://img.shields.io/badge/Steam++-v2.6.8-brightgreen.svg?style=flat-square&color=512bd4)]()
  
  
##### [不知道该下载哪个文件?](./download-guide.md)
---

### 文件校验
|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.6.8.7z  | SHA256 |
| Steam++_win_x64_fde_v2.6.8.7z  | SHA256 |
| | |
| Steam++_win_x64_v2.6.8.exe  | SHA256 |
| Steam++_win_x64_fde_v2.6.8.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.8.7z  | SHA256 |
| Steam++_linux_arm64_v2.6.8.7z  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.8.deb  | SHA256 |
| Steam++_linux_arm64_v2.6.8.deb  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.8.rpm  | SHA256 |
| Steam++_linux_arm64_v2.6.8.rpm  | SHA256 |
| | |
| Steam++_macos_x64_v2.6.8.dmg  | SHA256 |
| | |
| Steam++_android_v2.6.8.apk  | SHA256 |
