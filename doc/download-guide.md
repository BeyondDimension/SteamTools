<h1 align="center">⬇️ 下载指南</h1>
<div align="center">

[English](./download-guide.en.md) | 简体中文

</div>

## Windows
- 应用商店版
	- 在 Microsoft 应用商店中安装
	- 用户数据将存放于 ```%USERPROFILE%\AppData\Local\Packages\4651ED44255E.47979655102CE_k6txddmbb6c52\LocalState```
- 便携版
	- 压缩包解压到任意文件夹即可，不可在压缩软件中运行
	- 用户数据将存放于程序根目录下 ```\AppData```
	- 文件扩展名为 7z 的，例如 ```Steam++_win_x64_v3.0.0.7z```。
- 安装版
	- 安装包当前仅支持简体中文
	- 安装程序将会在注册表中写入软件信息
	- 用户数据将存放于 ```%LocalAppData%\Steam++```
	- 文件扩展名为 exe 的，例如 ```Steam++_win_x64_v3.0.0.exe```。
- 64 位应用程序(推荐)
	- 仅适用于 64 位操作系统，[确定您的计算机运行的是 32 位版本还是 64 位版本的 Windows 操作系统](https://pport.microsoft.com/zh-cn/topic/%E7%A1%AE%E5%AE%9A%E6%82%A8%E7%9A%84%E8%AE%1%E7%AE%97%6%9C%BAE8%BF%90%E8%A1%8C%E7%9A%84%E6%98%AF-32-%E4%BD%8D%E7%89%88%E6%9C%C%E8%BF%98%E6%98%F-64-%E4%BD%D%E7%89%88%E6%9C%AC%E7%9A%84-windows-%E6%93%8D%E4%BD%C%E7%B3%BB%E7%BB%F-1b03ca69-c5e-4b04-827b-c0c47145944b)。
	- 文件名中**包含** x64 的，例如 ```Steam++_win_x64_v3.0.0.exe```。
- 32 位应用程序
	- 适用于 32 位或 64 位操作系统。
	- 文件名中**包含** x86 的，例如 ```Steam++_win_x86_v3.0.0.exe```。
- ~~ARM64 应用程序~~(适配中…)
	- 适用于使用 Arm64 CPU 的设备，例如 **微软 Surface Pro X/Surface Pro 9 5G** 等使用高通骁龙、联发科动处理的设备。
	- 文件名中**包含** arm64 的，例如 ```Steam++_win_arm64_v3.0.0.exe```。
- 独立应用([SCD](https://learn.microsoft.com/zh-cn/dotnet/core/deploying/#publish-self-ontained))，会应用中包含 .NET 运行时，该应用的用户无需在运行应用前安装 .NET。 (推荐)
	- 文件名中**不包含** fde 的，例如 ```Steam++_win_x64_v3.0.0.exe```。
- 依赖于框架的应用([FDE](https://learn.microsoft.com/zh-cn/dotnet/core/deploying/#publish-ramework-ependent))，则该应用不包含 .NET 运行时和库，而仅包含该应用和第三方依赖项。
	- 文件名中**包含** fde 的，例如 ```Steam++_win_x64_fde_v3.0.0.exe```。
	- 依赖于框架的应用相比独立应用因共享运行时，可减少本应用占用的磁盘空间，且运行时可由 Windows 更新进行安全补丁修复。
	- 需要安装以下运行时
		- ASP.NET 运行时 7.x
		- ASP.NET Core 运行时 7.x
			- 此应用不需要 IIS 支持，可仅安装相关运行时而不是托管捆绑包。
## macOS
- ARM64 应用程序
	- 适用于使用 ARM64(Apple Silicon) 芯片的 Mac，例如 **Apple M1/M2**。
		- 文件名中**包含** x64 的，例如 ```Steam++_macos_arm64_v3.0.0.dmg```。
- 64 位应用程序
	- 适用于使用 Intel 的 x64(x86-64) 芯片的 Mac。
		- 文件名中**包含** x64 的，例如 ```Steam++_macos_x64_v3.0.0.dmg```。
- ~~通用包~~(适配中…)
	- 所有版本的二进制文件都包含在此包中，适用于所有支持的设备，但文件体积较大。
## Linux
- 64 位应用程序
	- 适用于使用 Intel、AMD 的 x64(x86-64) 芯片的设备。
		- 文件名中**包含** x64 的，例如 ```Steam++_linux_x64_v3.0.0.tar.zst```。
- ARM64 应用程序
	- 适用于使用 ARM64 芯片的设备，例如 **Raspberry Pi Model 3+**。
		- 文件名中**包含** x64 的，例如 ```Steam++_linux_arm64_v3.0.0.tar.zst```。
## Android
- 通用包(推荐)
	- 所有版本的二进制文件都包含在此包中，适用于所有支持的设备，但文件体积较大。
		- 文件名中**仅包含** android 的，例如 ```Steam++_android_v3.0.0.apk```。
- ~~arm64-v8a~~(已弃用…)
	- 适用于使用目前主流的手机或平板设备
		- 文件名中**包含** arm64_v8a 的，例如 ```Steam++_android_arm64_v8a_v3.0.0.apk```。
- ~~armeabi-v7a~~(已弃用…)
	- 适用于使用老旧的手机或平板设备，通常为 **2014** 年下半年之前生产的设备。
		- 文件名中**包含** armeabi_v7a 的，例如 ```Steam++_android_armeabi_v7a_v3.0.0.apk```。
- ~~x86_64~~(已弃用…)
	- 适用于使用 Intel、AMD 的 x64(x86-64) 芯片的设备，例如 PC 上的模拟器。
		- 文件名中**包含** x64 的，例如 ```Steam++_android_x64_v3.0.0.apk```。
## ~~iOS/iPadOS~~(开发中…) <img src="../res/brands/apple.svg" width="16" height="16" />
- 在 App Store 中下载

<!--
TODO: new fileName
Steam++_win_x64_v3.0.0.exe
Steam++_win_x64_v3.0.0.7z
Steam++_win_x64_with_runtime_v3.0.0.exe
Steam++_win_x64_with_runtime_v3.0.0.7z
-->