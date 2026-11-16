# Steam++ 发布说明

本文说明桌面版的发布方式、发布文件、启动入口和自定义 .NET Host。以下发布命令均在仓库根目录执行。

## 发布方式

### 环境准备

- 安装 [global.json](../global.json) 指定的 .NET SDK，并准备项目引用的子模块和依赖。
- Windows Native AOT 需要 Visual Studio Build Tools 的“使用 C++ 的桌面开发”工作负载，以及目标架构的 C++ 工具和 Windows SDK。
- Linux Native AOT 需要 PowerShell 7、clang、目标 C 运行库和 zlib 开发库；macOS 需要 PowerShell 7、Xcode 命令行工具及 .NET macOS 工作负载。
- Native AOT 必须在目标操作系统上发布。脚本接受 `win-x64`、`win-arm64`、`linux-x64`、`linux-arm64`、`osx-x64`、`osx-arm64`；各目标需要对应工具链和原生依赖。

### Native AOT 发布

Windows x64 默认使用 SkiaSharp：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build\publish-native.ps1 -RuntimeIdentifier win-x64
```

PowerShell 7 可使用 `pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier <RID>`，将 `<RID>` 替换为目标平台标识。

脚本同时发布主程序和加速子进程，7 个内置插件静态编译进程序；不支持额外加载托管插件 DLL。使用脚本统一配置 `WttsNativeAot=true`，不要向整个项目引用图直接传递 `PublishAot=true`。

默认输出：

- 可运行目录：`build/publish/native/<RID>`。
- 编译中间文件和符号：`build/artifacts/native/<RID>`。
- 分阶段日志：`build/logs/native/<RID>/<运行标识>`。

可选发布参数：

| 参数 | 用途 |
| --- | --- |
| `-OutputDirectory <路径>` | 仅修改最终发布目录。 |
| `-BuildRoot <路径>` | 修改 `publish`、`artifacts`、`logs` 的共同根目录。 |
| `-Renderer SimdSkia` | 使用可选 SimdSkia 后端，默认目录改为 `native-simdskia/<RID>`；需要准备 `ref/FluentAvalonia` 等源码依赖。 |
| `-StaticGraphicsComponents HarfBuzz` | 在默认 SkiaSharp 后端下静态链接已准备好的 HarfBuzz 归档。 |

这些路径参数的相对路径均相对于仓库根目录。HarfBuzz 静态归档需要先单独构建，例如 Windows x64：

```powershell
.\build\native-graphics\build-static-graphics.ps1 -RuntimeIdentifier win-x64 -Component HarfBuzz
.\build\publish-native.ps1 -RuntimeIdentifier win-x64 -StaticGraphicsComponents HarfBuzz
```

静态归档必须满足版本、架构和哈希检查；缺少归档或凭据时发布失败，不自动回退到动态库。发布脚本生成可运行目录，不自动生成压缩包、安装器或数字签名。

### 普通 JIT 发布

使用发布工具构建主程序、动态插件、加速子进程和随包运行时，例如 Windows x64：

```powershell
dotnet run --project .\src\BD.WTTS.Client.Tools.Publish\BD.WTTS.Client.Tools.Publish.csproj -c Release -- run --rids win-x64
```

发布工具默认输出到：

```text
src/BD.WTTS.Client.Avalonia.App/bin/Release/Publish/
└── [Release] Steam++_v<版本>_win_x64_<时间戳>/
```

实际目录以工具输出为准，工具不会自动把结果放入 `build/publish/jit/<RID>`。Windows 流程还包含 `.7z`、MSIX/MSIXBundle 打包及签名，需要相应的 Windows SDK 打包工具、混淆工具和证书配置。此工具会清理目标输出目录，不要将日常使用的程序目录作为构建输出。

查看工具参数：

```powershell
dotnet run --project .\src\BD.WTTS.Client.Tools.Publish\BD.WTTS.Client.Tools.Publish.csproj -c Release -- run --help
```

## 发布后文件

### Native AOT 目录

Windows x64 的核心布局如下；其余本机库和许可文件随所选渲染及静态链接选项变化，以 `.wtts-native-manifest.json` 为完整文件清单：

```text
发布目录/
├── Steam++.exe
├── e_sqlite3.dll
├── modules/
│   └── Accelerator/
│       └── Steam++.Accelerator.exe
└── .wtts-native-manifest.json
```

- `Steam++.exe` 是原生主程序，不依赖单独安装的 .NET 运行时；包中不需要 JIT 的 `assemblies`、`dotnet` 或独立托管插件 DLL。
- 根目录的 SQLite 原生库由主程序和加速子进程共用。不要只复制 EXE，或将加速子进程移出完整发布目录。
- 分发时保留清单、对应本机库和许可文件；不要混入 JIT 启动器或启动脚本。

### JIT 目录

Windows 的核心布局：

```text
发布目录/
├── Steam++.exe
├── Steam++.exe.config
├── assemblies/
│   ├── Steam++.dll
│   ├── Steam++.deps.json
│   └── Steam++.runtimeconfig.json
├── modules/
├── native/
│   └── win-x64/
└── dotnet/
    ├── host/fxr/<版本>/hostfxr.dll
    └── shared/
        ├── Microsoft.NETCore.App/<版本>/
        └── Microsoft.AspNetCore.App/<版本>/
```

`assemblies` 还包含主程序依赖和语言资源；`modules` 保存动态插件及子进程；`native` 保存对应架构的本机库。`dotnet` 按实际运行时配置收集依赖，只有需要时才附带 `Microsoft.WindowsDesktop.App`，不包含 SDK。Windows Arm64 JIT 包的主程序使用 x64 兼容运行时，加速子进程使用独立的 Arm64 发布。

### 分发和更新

- `AppData`、`Cache`、`Log`/`Logs` 及运行生成的数据库、日志不属于分发文件，数据位置见[文件系统说明](./file-system.md)。
- 打包已运行过的 AOT 目录时，仅收集清单记录且哈希匹配的文件和清单本身，不要直接压缩整个目录。
- 重复 AOT 发布前关闭程序。脚本只替换清单记录且未被修改的生成文件，保留用户数据；遇到未知文件、已修改文件或无清单的旧混合目录时拒绝覆盖，应改用新的输出目录。
- 构建目录、日志和调试符号不放入普通分发包；保留对应版本的符号供崩溃诊断。若发布失败并报告保留了 `previous-payload`，先按报错处理恢复，不要直接删除备份。

## 如何执行

先解压完整分发包，再进入发布目录。Windows 的 JIT 和 AOT 均从根目录启动：

```powershell
.\Steam++.exe
```

Linux 根据发布方式选择入口：

```bash
# Native AOT
./Steam++

# 带随包运行时和启动脚本的 JIT 包
bash ./Steam++.sh
```

macOS 的 `.app` 包使用：

```bash
open ./Steam++.app
```

若发布产物是裸原生可执行文件而非 `.app`，直接执行 `./Steam++`。加速子进程由主程序启动，不作为桌面入口；不要用 `dotnet Steam++.exe` 启动 AOT 程序。

## 自定义 .NET Host（nethost / hostfxr）

此部分仅适用于普通 JIT 发布，对应 [BD.WTTS.Client.AppHost](../src/BD.WTTS.Client.AppHost/Program.cs)。Windows JIT 启动器本身使用 .NET Framework；随包的现代 .NET 运行时不替代启动器所需的 .NET Framework。

本项目自行定位并加载 `hostfxr`，再根据 `assemblies/Steam++.runtimeconfig.json` 初始化 CoreCLR，调用 `assemblies/Steam++.dll` 中的 `BD.WTTS.Program.CustomEntryPoint`。不需要手动执行 `nethost`、`hostfxr.dll`，也不要求单独分发一个 `.nethost` 文件。

Windows 启动器按以下顺序查找可用运行时：

1. 发布根目录下的 `dotnet`。
2. 当前进程对应的 `Program Files/dotnet`。
3. 环境变量 `DOTNET_ROOT` 指定的运行时根目录。

当前启动器要求 `host/fxr`、`Microsoft.NETCore.App` 和 `Microsoft.AspNetCore.App` 具有相同的所选版本，并与主进程架构匹配。保持完整的运行时目录结构，不要只复制 `hostfxr.dll`。`DOTNET_ROOT` 仅在前两处没有可用运行时时参与选择，不会覆盖随包运行时。

通过 `hostfxr_initialize_for_runtime_config` 加载的主程序集必须保持框架依赖发布；不要将它改成自包含组件再交给该 Host 加载。自包含子进程和完整 Native AOT 程序应作为独立可执行文件启动。

## 相关文档

- [发布脚本](../build/publish-native.ps1)
- [静态图形库构建脚本](../build/native-graphics/build-static-graphics.ps1)
- [发布工具](../src/BD.WTTS.Client.Tools.Publish/Commands/IDotNetPublishCommand.cs)
- [文件系统与用户数据位置](./file-system.md)
- [.NET 应用发布概述](https://learn.microsoft.com/dotnet/core/deploying/)
- [Native AOT 部署、前置条件与限制](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [Native AOT 原生库互操作](https://learn.microsoft.com/dotnet/core/deploying/native-aot/interop)
- [自定义 .NET Host](https://learn.microsoft.com/dotnet/core/tutorials/netcore-hosting)
- [.NET 分发目录布局](https://learn.microsoft.com/dotnet/core/distribution-packaging)
- [.NET 运行时环境变量](https://learn.microsoft.com/dotnet/core/tools/dotnet-environment-variables#dotnet_root-dotnet_rootx86-dotnet_root_x86-dotnet_root_x64)
- [Host 错误码](https://github.com/dotnet/runtime/blob/main/docs/design/features/host-error-codes.md)