<h1 align="center">⬇️ Download Guide</h1>
<div align="center">

English | [Simplified Chinese](./download-guide.md)

</div>

## Windows
- Store version
	- Install in Microsoft Store
	- User data will be stored in ```%USERPROFILE%\AppData\Local\Packages\4651ED44255E.47979655102CE_k6txddmbb6c52\LocalState```
- Portable version
	- The compressed package can be decompressed to any folder and cannot be run in the compressed software
	- User data will be stored in the program root directory ```\AppData```
	- With a file extension of 7z, for example ```Steam++_win_x64_v3.0.0.7z```.
- Installation version
	- The installation package currently only supports Simplified Chinese
	- The installer will write software information in the registry
	- User data will be stored in ```%LocalAppData%\Steam++```
	- With a file extension of exe, for example ```Steam++_win_x64_v3.0.0.exe```.
- 64-bit applications (recommended)
	- For 64-bit operating systems only, [Determine whether your computer is running a 32-bit ersionor 64-bit version of the Windows operating system](https://support.microsoft.com/n-us/topic/etermine-whether-your-computer-is-running-a-32-bit-version-or-64-bit-version-f-the-windows-perating-system-1b03ca69-ac5e-4b04-827b-c0c47145944b).
	- The file name **contains** x64, such as ```Steam++_win_x64_v3.0.0.7z```.
- 32-bit application
	- For 32-bit or 64-bit operating systems.
	- The file name **contains** x86, such as ```Steam++_win_x86_v3.0.0.7z```.
- ~~ARM64 application~~(Adapting…)
	- It is applicable to devices that use Arm64 CPU, such as **Microsoft Surface Pro X/Surface ro 95G** and other devices that use Qualcomm Snapdragon and MediaTek mobile processors.
	- The file name **contains** arm64, such as ```Steam++_win_arm64_v3.0.0.7z```.
- Independent application ([SCD](https://learn.microsoft.com/en-us/dotnet/core/deploying/publish-elf-contained)) will include. NET runtime in the application. Users of this pplication do not eed to install. NET before running the application. (Recommended)
	- The file name **does not contains** fde, such as ```Steam++_win_x64_v3.0.0.7z```.
- Framework-dependent application ([FDE](https://learn.microsoft.com/en-us/dotnet/core/eploying/publish-framework-dependent)), the application does not contain the. NET runtime nd library, but nly the application and third-party dependencies.
	- The file name **contains** fde, such as ```Steam++_win_x64_fde_v3.0.0.7z```.
	- Compared with independent applications, framework-dependent applications can reduce the disk space occupied by this application because of sharing the runtime, and the runtime can be repaired by Windows Update with security patches.
	- The following runtime needs to be installed
		- ASP.NET Runtime 7.x
		- ASP.NET Core Runtime 7.x
			- This application does not require IIS support. You can only install the relevant ntime instead of the Hosting Bundle.
## macOS
- ARM64 application
	- Applicable to Mac using ARM64 (Apple Silicon) chip, such as **Apple M1/M2**.
	- The file name **contains** arm64, such as ```Steam++_macos_arm64_v3.0.0.dmg```.
- 64-bit applications
	- For Mac using Intel's x64 (x86-64) chip.
	- The file name **contains** x64, such as ```Steam++_macos_x64_v3.0.0.dmg```.
- ~~General package~~(Adapting…)
	- All versions of binaries are included in this package and are applicable to all supported evices, but the file size is large.
## Linux
- 64-bit applications
	- Applicable to devices using Intel and AMD's x64 (x86-64) chips.
	- The file name **contains** x64, such as ```Steam++_linux_x64_v3.0.0.tar.zst```.
- ARM64 application
	- Applicable to devices using ARM64 chips, such as **Raspberry Pi Model 3+**.
	- The file name **contains** x64, such as ```Steam++_linux_arm64_v3.0.0.tar.zst```.
## Android
- General package (recommended)
	- All versions of binaries are included in this package and are applicable to all supported vices, but the file size is large.
		- The file name **only contains** android, such as ```Steam++_android_v3.0.0.apk```.
- ~~arm64-v8a~~(Deprecated…)
	- Applicable to the use of current mainstream mobile phones or tablet devices
		- The file name **contains** arm64-v8a, such as ```Steam++_arm64-v8a_v3.0.0.apk```.
- ~~armeabi-v7a~~(Deprecated…)
	- It is applicable to the use of old mobile phones or tablet devices, usually those roducedbefore the second half of **2014**.
		- The file name **contains** armeabi_v7a, such as ```Steam++_armeabi_v7a_v3.0.0.apk```.
- ~~x86_64~~(Deprecated…)
	- Applicable to devices using Intel and AMD's x64 (x86-64) chips, such as simulators on PC.
		- The file name **contains** x64, such as ```Steam++_x64_v3.0.0.apk```.
## ~~iOS/iPadOS~~(In development…) <img src="../res/brands/apple.svg" width="16" height="16" />
- Download in App Store
