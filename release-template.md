### 新增内容
1. CLR 更新至 6.0 RTM
2. 新增 捐助功能，在关于中可使用 **爱发电**、**Ko-fi**、**Patreon** 平台捐助
3. 新增 ASF 本地挂卡功能 (Beta)
4. 新增 本地令牌搜索功能
5. 新增 库存游戏右键菜单导航到 Steam 客户端
6. 新增 Windows 11 上可设置材质 [云母(Mica)](https://docs.microsoft.com/zh-cn/windows/apps/design/style/mica)
7. 新增 搜索框支持拼音搜索
8. 新增 框架依赖部署模式(FDE)，可通过共享运行库减少磁盘占用空间，仅支持 Windows 与 Linux
9. 新增 Windows 上可将动态壁纸设置为程序背景
10. 新增 桌面端 背景材质设置，并修复之前AcrylicBlur透明效果异常问题
11. 改进 桌面端 UI 适配 Windows 11 风格
12. 改进 Hosts 文件在 Windows 上默认使用 UTF8WithBOM 编码
13. 改进 账号注销现需要通过手机号或昵称验证
14. 改进 令牌账号加密、导出的界面UI和导入过程中的提示
15. 改进 Steam 账号切换支持头像框、等级和游戏中信息的显示
16. 改进 文本框窗口弹出时将自动设置焦点
17. 改进 Windows 上端口占用提示文本显示占用该端口的进程名
18. 改进 Linux 上存储数据遵循 [XDG Base Directory Specification](https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html)
19. 改进 主题切换现在不在需要重启程序，提升主题切换速度
20. 改进 本地令牌账号导入过程中的说明提示
21. 改进 Windows 上账号切换启动 Steam 时，默认不在以管理员身份启动
22. 改进 macOS 上修改 hosts 文件可以不用每次输入密码
<!--

. 改进 Android UI
. 改进 Android 冷启动速度
. 新增 Android x86 架构包，适用于 Intel、AMD 芯片的设备
. 改进 Android 导入令牌成功后回到列表页

-->

### 修复问题
1. 修复 本地令牌 中确认交易时 Http 302 重定向错误
2. 修复 Linux 与 macOS 上代理错误
3. 修复 库存游戏无限加载
4. 修复 脚本未启用时保存状态会全部未启用
5. 修复 桌面端 上主题运行时切换与跟随系统
6. 修复 Windows 上窗口边缘滚动条难以拖拽
7. 修复 本地令牌 确认交易登录时会错误的提示没有开启加速
8. 修复 本地令牌 确认交易有时会卡在提示登录中的问题
9. 修复 Windows 上资源管理器重启后托盘消失，以及尝试修复开机自启时有时不显示托盘
10. 修复 桌面端 上导航栏的弹出菜单失去焦点时不会自动隐藏的问题
11. 修复 在 2.6.0 中账号切换、挂时长列表、ASF-UI 无效或缺失的问题

<!--

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

## [下载指南](./download-guide.md)

|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.6.1.7z  | f8525e772904a5696e651bae5fbf726861b013ecd6c1a859804e371cd0581e0b |
| Steam++_win_x64_fde_v2.6.1.7z  | cbe17c26b2e4ae1da24ea4c620c11fb98180f872c96e1d1c7d9fb56af35dbf53 |
| | |
| Steam++_win_x64_v2.6.1.exe  | 3f69999ecde4699420e65ad63126e809e43462655ce4ae1fa04c384e069d93aa |
| Steam++_win_x64_fde_v2.6.1.exe  | 8dc15d27399cea1769df76d52d0276967bfd8a1941df23ae5a7418f22d161a81 |
| | |
| Steam++_linux_x64_v2.6.1.7z  | 457dc4dd21c5acaf7c5515fc9c77e4d8abe2a75f674acda8e74cdfbc062bfb0f |
| Steam++_linux_arm64_v2.6.1.7z  | 6b83fdbeae5597befc892d44e603dbc4b35924be33a5774b733258b264bed1cf |
| Steam++_linux_arm_v2.6.1.7z  | 3f9df21a378884bb7a1c4c867c1d59b07a9568bba0c658755c3d12c89b97fea0 |
| | |
| Steam++_linux_x64_fde_v2.6.1.7z  | d20cb26e54ff6b9ecdb02712e2c585d34543dcc1ae35a25821f887a8938aa831 |
| Steam++_linux_arm64_fde_v2.6.1.7z  | dc17bd72b3df2332ce580bbe42f97ed3994ca0b387f3e92f3852b5672c41bd39 |
| Steam++_linux_arm_fde_v2.6.1.7z  | f99ff076f28ee26581baad1e848bc977d740a7450cd672e8d24e06e4f758f371 |
| | |
| Steam++_linux_x64_v2.6.1.deb  | 29fc0a5da7396006793ee43508a4db3ab7b49663cb83c57b7f732443b8e4b74e |
| Steam++_linux_arm64_v2.6.1.deb  | 60e240bf6d698e7a3fa33356f1589d6cf3bbf8b85a61021c8cbb9b586420cd7d |
| Steam++_linux_arm_v2.6.1.deb  | 7977d15618b03cd2b40f29cea23680019d357c3b7d72046bc828a49d29e15633 |
| | |
| Steam++_linux_x64_fde_v2.6.1.deb  | 6a1f14a1135179acaa436d33d1eae213bc85a1bd37a6ff2fba616d8d86740f19 |
| Steam++_linux_arm64_fde_v2.6.1.deb  | 4be901e1961af44cbc814ca8bf9cdf56cdae9db3360be0d425291e75fa41df20 |
| Steam++_linux_arm_fde_v2.6.1.deb  | 2570a0ee03634aa2b15c67c9f9e91a3f12e8e5bcd3f6c44b4a31dd44e2d2bf54 |
| | |
| Steam++_linux_x64_v2.6.1.rpm  | d7709a9373c9dd9b5f0957b02337ad7ac25dae5aa43bcc1ae3f10bd2cdfc0745 |
| Steam++_linux_arm64_v2.6.1.rpm  | d3238980442df1d1f1293ac0a6acbc6528d53966426af1c41474283ce0a5f922 |
| Steam++_linux_arm_v2.6.1.rpm  | b9d6990c5c2c989490464afa1bfa39ccf20b851547f912082447ee1d6a669247 |
| | |
| Steam++_linux_x64_fde_v2.6.1.rpm  | 83345f36ec61f609f88d61c6c048efbbe5a8498fcfe1f4ae0aafeeaad93cc122 |
| Steam++_linux_arm64_fde_v2.6.1.rpm  | 8c2a22ddd05b0f41bc2e83bfc496bd0ed3e22316cae5864123a06268c3817e33 |
| Steam++_linux_arm_fde_v2.6.1.rpm  | dd726f53d14587808667ff546f2cdd63c7cd860f0a50e71bb30db6c72bbe7e66 |
| | |
| Steam++_macos_x64_v2.6.1.dmg  | dd2a2f435aa12536fb5b0bc4329473c9db96e93a173405970710dae6d3533db4 |
| Steam++_macos_arm64_v2.6.1.dmg  | SHA256 |
| Steam++_macos_x64_v2.6.1.7z  | b1d580bc8f0352389d8c680febb44f762ddf133bd9216cf84102a9cc9c0b548b |
| Steam++_macos_arm64_v2.6.1.7z  | ee11cf563ef57dbbfa3e75abc4798ce1e9461274fd476d28685df468fced17bb |
| | |
| Steam++_android_arm64_v8a_v2.6.1.apk  | SHA256 |
| Steam++_android_armeabi_v7a_v2.6.1.apk  | SHA256 |
| Steam++_android_x86_v2.6.1.apk  | SHA256 |

<!-- ***
由于程序体积较大，推荐从 [官网 https://steampp.net](https://steampp.net) 中下载 -->
