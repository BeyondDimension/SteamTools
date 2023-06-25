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
- [Windows](./program-file-structure/Windows.md)
- [Linux](./program-file-structure/Linux.md)
- [macOS](./program-file-structure/macOS.md)


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

<!--
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
-->

### 自定义 .NET Host 文档
- [编写自定义 .NET 主机以从本机代码控制 .NET 运行时](https://learn.microsoft.com/zh-cn/dotnet/core/tutorials/netcore-hosting)  
- [.NET 分发打包](https://learn.microsoft.com/zh-cn/dotnet/core/distribution-packaging)  
- [环境变量 - 指定 .NET 运行时的位置](https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-environment-variables#dotnet_root-dotnet_rootx86)  