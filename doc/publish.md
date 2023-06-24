# Steam++ v3.X Publish Document  

### Assets
- Windows
    - x64
        - ```Steam++_vx.y.z_win_x64.msix```
        - ```Steam++_vx.y.z_win_x64.7z```
    - x86
        - ```Steam++_vx.y.z_win_x86.msix```
        - ```Steam++_vx.y.z_win_x86.7z```
    - Arm64
        - ```Steam++_vx.y.z_win_arm64.msix```
        - ```Steam++_vx.y.z_win_arm64.7z```
- Linux
    - x64
        - ```Steam++_vx.y.z_linux_x64.tar.zst```
        - ```Steam++_vx.y.z_linux_x64.deb```
        - ```Steam++_vx.y.z_linux_x64.rpm```
    - Arm64
        - ```Steam++_vx.y.z_linux_arm64.tar.zst```
        - ```Steam++_vx.y.z_linux_arm64.deb```
        - ```Steam++_vx.y.z_linux_arm64.rpm```
- macOS  
    - x64
        - ```Steam++_vx.y.z_macos_x64.pkg```
    - Arm64
        - ```Steam++_vx.y.z_macos_arm64.pkg```
- Android  
    - Arm64
        - ```Steam++_vx.y.z_android_arm64.apk```

### Program File Structure
- Windows
    - dotnet 共享运行时，删除后将使用已安装的运行时，此目录参考 ```C:\Program Files\dotnet```，可自行升级运行库小版本号，二进制兼容
    	- host
    		- fxr
    			- x.y.z
    				- hostfxr.dll
    	- shared
    		- Microsoft.AspNetCore.App
    			- x.y.z
    		- Microsoft.NETCore.App
    			- x.y.z
    - native
        - win-x64
            - 7z.dll
            - av_libglesv2.dll
            - e_sqlite3.dll
            - libHarfBuzzSharp.dll
            - libSkiaSharp.dll
            - WebView2Loader.dll
            - WinDivert.dll
            - WinDivert64.sys
    - assemblies 主模块程序集
    - modules 可选模块
        - ~~Update 自更新模块，删除该文件夹后禁用更新~~
            - Steam++.Update.exe 更新程序，CDN 分发更新包，下载与解压在主程序中，此进程仅退出主程序执行覆盖操作
        - Accelerator 网络加速
            - Steam++.Accelerator.exe 控制台子服务进程，使用匿名管道与主进程通信，无参数或指定某个参数(待定)启动时可读取配置文件启动加速，可完全独立运行，ASP.NET Core Web API 项目，可支持 Docker
            - Steam++.Plugins.Accelerator.dll 插件程序集
        - AccountSwitch 账号切换
            - Steam++.Plugins.AccountSwitch.dll
        - ~~ArchiSteamFarm~~
            - Steam++.ArchiSteamFarm.exe
            - Steam++.Plugins.ArchiSteamFarm.dll
        - GameList 库存游戏
            - Steam++.Plugins.GameList.dll
        - LocalAuth 本地令牌
            - Steam++.Plugins.LocalAuth.dll
        - GameTools 游戏工具
            - Steam++.Plugins.GameTools.dll
    - Steam++.exe (主启动程序).NET Framework 二进制主程序  
    - Steam++.exe.config 使用该配置文件以允许在 .NET Framework 3.5 ~ 4.8.1 中任意版本中兼容运行(从 Windows 7 ~ 11 中所有系统自带运行时)  
    - ~~Steam++.Uninstall.exe 卸载程序(WinForms?AOT?)~~ 
- Linux
    - dotnet 共享运行时，删除后将使用已安装的运行时，此目录参考 [aspnetcore-runtime-7.0.7-linux-x64.tar.gz](https://download.visualstudio.microsoft.com/download/pr/c1e2729e-ab96-4929-911d-bf0f24f06f47/1b2f39cbc4eb530e39cfe6f54ce78e45/aspnetcore-runtime-7.0.7-linux-x64.tar.gz)，可自行升级运行库小版本号，二进制兼容
    	- host
    		- fxr
    			- x.y.z
    				- hostfxr.dll
    	- shared
    		- Microsoft.AspNetCore.App
    			- x.y.z
    		- Microsoft.NETCore.App
    			- x.y.z
    - native
        - linux-x64
            - libe_sqlite3.so  
            - libHarfBuzzSharp.so  
            - libSkiaSharp.so  
    - Steam++ (主启动程序) AppHost
    - assemblies 主模块程序集
    - modules 可选模块
        - Accelerator 网络加速
            - Steam++.Accelerator 控制台子服务进程，使用匿名管道与主进程通信，无参数或指定某个参数(待定)启动时可读取配置文件启动加速，可完全独立运行，ASP.NET Core Web API 项目，可支持 Docker
            - Steam++.Plugins.Accelerator 插件程序集
        - AccountSwitch 账号切换
            - Steam++.Plugins.AccountSwitch.dll
        - ~~ArchiSteamFarm~~
            - Steam++.ArchiSteamFarm.exe
            - Steam++.Plugins.ArchiSteamFarm.dll
        - GameList 库存游戏
            - Steam++.Plugins.GameList.dll
        - LocalAuth 本地令牌
            - Steam++.Plugins.LocalAuth.dll
- macOS
    - Steam++.app (包含 arm64 与 x64 以及 .NET Runtime x2 与 ASP.NET Core Runtime 文件大小应该较大)  
      - Contents  
          - Info.plist 
          - MacOS  
              - Steam++ (主启动程序)
          - MonoBundle  
              - dotnet 运行时，其中 Microsoft.NETCore.App 与 tfm macos 中将存在两份重复的，此目录参考 [aspnetcore-runtime-7.0.7-osx-arm64.tar.gz](https://ownload.visualstudio.microsoft.com/download/pr/97bb1f46-3b87-4475-bc06-e5cb7f4e6d0a/3e36e0c804c5805d2fe856505d7b1b3c/aspnetcore-runtime-7.0.7-osx-arm64.tar.gz)
                - host
    	            - fxr
    		            - x.y.z
    			            - hostfxr.dll
                    - shared
    	                - Microsoft.AspNetCore.App
    		                - x.y.z
    	                - Microsoft.NETCore.App
    		                - x.y.z
              - modules 可选模块
                  - Accelerator 网络加速
                      - Steam++.Accelerator 控制台子服务进程，使用匿名管道与主进程通信，无参数或指定某个参数(待定)启动时可读取配置文件启动加速，可完全独立运行，ASP.NET Core Web API 项目，可支持 ocker
                      - Steam++.Plugins.Accelerator.dll 插件程序集
                  - AccountSwitch 账号切换
                      - Steam++.Plugins.AccountSwitch.dll
                  - ~~ArchiSteamFarm~~
                      - Steam++.ArchiSteamFarm.dll
                      - Steam++.Plugins.ArchiSteamFarm.dll
                  - GameList 库存游戏
                      - Steam++.Plugins.GameList.dll
                  - LocalAuth 本地令牌
                      - Steam++.Plugins.LocalAuth.dll
          - PkgInfo  
          - Resources  


<!--
对子服务进程二进制程序的 RID 使用 ```RuntimeInformation.ProcessArchitecture.ToString()```  
通过 ```PublishFolderType="Assembly"``` 指定 Copy 到 MonoBundle 中，没有其他选项能够将其放在 MacOS 文件夹中

改成 Windows 的目录结构，但在 AOT 的 AppHost 中无法加载已使用框架依赖发布的程序集，错误 Initialization for self-contained components is not supported
参考  
https://github.com/dotnet/runtime/issues/35329  
https://github.com/dotnet/runtime/search?l=C%2B%2B&q=get_is_framework_dependent  
https://github.com/dotnet/runtime/blob/6702dc5c7814e624a42ab4615224920a5635beeb/src/native/corehost/runtime_config.cpp  
https://github.com/dotnet/runtime/blob/main/docs/design/features/host-error-codes.md  
https://github.com/dotnet/samples/blob/91355ef22a10ec614a2e8daefd68785066860d57/core/hosting/src/NativeHost/nativehost.cpp  
-->


### 安装 ASP.NET Core 运行时 & .NET 运行时
- Windows 
    - [使用 Windows 包管理器 (winget) 进行安装](https://learn.microsoft.com/zh-cn/dotnet/core/install/windows?tabs=net70#install-with-windows-installer)  
    ```winget install Microsoft.DotNet.AspNetCore.7```
- Linux
    - [使用安装脚本或通过提取二进制文件在 Linux 上安装 .NET](https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-scripted-manual)  
    ```wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh```  
    ```sudo chmod +x ./dotnet-install.sh```  
    ```./dotnet-install.sh --version latest --runtime aspnetcore```
    - [排查与 Linux 上缺少文件相关的 .NET 错误](https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-package-mixup?pivots=os-linux-redhat)
- macOS
    - [使用 Bash 自动化安装](https://learn.microsoft.com/zh-cn/dotnet/core/install/macos#install-with-bash-automation)  
    ```./dotnet-install.sh --channel 7.0 --runtime aspnetcore```


### 自定义 .NET Host 文档
- [编写自定义 .NET 主机以从本机代码控制 .NET 运行时](https://learn.microsoft.com/zh-cn/dotnet/core/tutorials/netcore-hosting)  
- [.NET 分发打包](https://learn.microsoft.com/zh-cn/dotnet/core/distribution-packaging)  
- [环境变量 - 指定 .NET 运行时的位置](https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-environment-variables#dotnet_root-dotnet_rootx86)  