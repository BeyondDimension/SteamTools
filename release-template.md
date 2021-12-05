### 版本亮点
<!--

. 新增 Android UI
. 新增 Android 冷启动速度
. 新增 Android x86 架构包，适用于 Intel、AMD 芯片的设备
. 新增 Android 导入令牌成功后回到列表页

-->

### 修复问题
1. 改进 网络加速现默认使用阿里 DNS(223.5.5.5, 223.6.6.6)
2. 改进 自动更新包文件校验失败时提示
3. 改进 自动更新失败时将自动跳转官网
4. 改进 ASF IPC 默认端口号由 1242 改为 6242
1. 修复 Linux 与 macOS 上 ASF-UI 解压包文件夹分隔符不正确
2. 修复 Windows 上 hosts 只读时尝试取消只读属性的操作没有正确执行
3. 修复 Windows 上更新包删除缓存时因文件占用引发的中断
4. 修复 高 DPI 下动态桌面错位

<!--
. 改进 Android UI
. 改进 Android 冷启动速度
. 改进 Android 导入令牌成功后回到列表页
. 修复 Android 上屏幕捕获设置项不生效
. 修复 Android 上令牌列表有时不显示值
-->

### 已知问题
- Desktop 
	- macOS
		- 尚未公证，这会影响 macOS Catalina（版本 10.15）以上
		- 自动更新不可用
	- Linux
		- 在 Deepin 中托盘不生效，可通过 ```Exit.sh``` 退出程序
		- 窗口弹出位置不正确
		- 自动更新不可用
		- 鼠标指针浮动样式不正确
	- Windows
		- 在 CPU 不受支持的 Win11 上无法启动，Windows 日志中显示 ```Failed to create CoreCLR, HRESULT: 0x80004005```
			- 仅 .NET 6.0 受此影响，在几周后的 Insider 中会修复，见 [issue](https://github.com/dotnet/core/issues/6733)
			- **解决方案：** 可尝试使用早期版本，例如 v2.3.0
	- Shared
		- 拼音搜索不能正确的识别多音字
		- 在 仅有一个文本框的窗口 上使用回车确定可能导致弹窗死循环，例如本地令牌中的需要解密才能继续
			- **解决方案：** 点击右下方的确定按钮完成输入
- Mobile
	- Android
		- 确认交易列表刷新后数据有时会显示不正确
		- 自动更新暂不可用
		- 

[![steampp.net](https://img.shields.io/badge/WebSite-steampp.net-brightgreen.svg?style=flat-square&color=61dafb)](https://steampp.net)
[![Steam++ v2.6.2](https://img.shields.io/badge/Steam++-v2.6.2-brightgreen.svg?style=flat-square&color=512bd4)]()
  
  
##### [不知道该下载哪个文件?](./download-guide.md)
---

|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.6.2.7z  | SHA256 |
| Steam++_win_x64_fde_v2.6.2.7z  | SHA256 |
| | |
| Steam++_win_x64_v2.6.2.exe  | SHA256 |
| Steam++_win_x64_fde_v2.6.2.exe  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.2.7z  | SHA256 |
| Steam++_linux_arm64_v2.6.2.7z  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.2.deb  | SHA256 |
| Steam++_linux_arm64_v2.6.2.deb  | SHA256 |
| | |
| Steam++_linux_x64_v2.6.2.rpm  | SHA256 |
| Steam++_linux_arm64_v2.6.2.rpm  | SHA256 |
| | |
| Steam++_macos_x64_v2.6.2.dmg  | SHA256 |
| | |
| Steam++_android_arm64_v8a_v2.6.2.apk  | SHA256 |
| Steam++_android_armeabi_v7a_v2.6.2.apk  | SHA256 |
| Steam++_android_x86_v2.6.2.apk  | SHA256 |
