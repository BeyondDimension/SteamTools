# 下载指南
- Desktop(桌面端)
	- Windows
		- 如果你使用 Intel、AMD 的 x64(x86-64/AMD64) 芯片的 PC，则下载文件名中带有 **win_x64** 的文件
		- **框架依赖版(framework-dependent executable / fde)**
			- 需要 **《ASP.NET Core 运行时 6.0.x》** 与 **《.NET 运行时 6.0.x》**，如果你 **已安装** 了相关运行时，则下载文件名中带有 **fde** 的文件，可减少本应用占用的磁盘空间
			- 如果你 **未安装相关运行时则应下载文件名中不包含 fde 的文件**，或在 [此下载](https://dotnet.microsoft.com/zh-cn/download/dotnet/6.0) 最新的 6.0.x 运行时
			- 在网页上点击 安装程序 x64 的链接即可下载，通常下载的文件名如下，将其安装后即可，无安装顺序要求
				- dotnet-runtime-6.0.x-win-x64.exe (.NET 运行时 6.0.x)
				- aspnetcore-runtime-6.0.x-win-x64.exe (ASP.NET Core 运行时 6.0.x)
			- 其他也包含了所需的运行时的下载项，但不推荐仅为运行本应用而安装
				- Hosting Bundle 还包含了本应用不需要的 IIS 运行时支持 (ASP.NET Core Module v2) 与 x86 的多个运行库
				- SDK 包含了较多不需要的内容
			

	- macOS
		- 如果你使用 Intel 的 x64(x86-64) 芯片的 Mac，则下载文件名中带有 **macos_x64** 的文件
		- 如果你使用 ARM64(Apple Silicon) 芯片的 Mac，例如 **Apple M1**，则下载文件名中带有 **macos_arm64** 的文件
	- Linux
		- 如果你使用 Intel、AMD 的 x64(x86-64) 芯片的 PC 则下载文件名中带有 **linux_x64** 的文件
		- 如果你使用 ARM64 芯片的 PC 例如 **Raspberry Pi Model 3+**，则下载文件名中带有 **linux_arm64** 的文件

<!--		
- Mobile(移动端)
	- Android
		- 如果你使用 ARM64 芯片的设备（较为**普遍**）则下载文件名中带有 **android_arm64_v8a** 的文件
		- 如果你使用 ARM32 芯片的设备（较为**稀有**）通常为 **14** 年下半年之前生产的设备，则下载文件名中带有 **android_armeabi_v7a** 的文件
		- 如果你使用 Intel、AMD 的 x64 芯片的设备（较为**稀有**）则下载文件名中带有 **android_x64** 的文件
-->

<!--
- 如果你使用 ARM64 芯片的 PC（极为**稀有**），例如 **Surface Pro X**，则下载文件名中带有 **win_x64** 的文件可通过 Win11 x86 模拟运行
- **[暂未支持]** ~~如果你使用 ARM64 芯片的 PC（极为**稀有**），例如 **Surface Pro X**，则下载文件名中带有 **win_arm64** 的文件~~
- **[暂未支持]** ~~如果你使用 ARM64 芯片的 Mac（较为**稀有**），例如 **M1**，则下载文件名中带有 **macos_arm64** 的文件~~
			- [在 Linux 上安装 .NET](https://docs.microsoft.com/en-us/dotnet/core/install/linux)
				- 推荐 [通过 Snap 安装 .NET Runtime](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux-snap)
				- ```sudo snap install dotnet-runtime-60 --classic```
-->
