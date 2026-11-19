# Steam++ 发布说明

## 发布交付物

| 方式 | 默认输出 | Windows 交付文件 |
| --- | --- | --- |
| Native AOT | `build/publish/native/<RID>/` | 根目录只有 `Steam++.exe` 一个文件。原生 DLL 集中在 `native/<RID>/`，清单位于 `native/.wtts-native-manifest.json`；加速子进程位于 `modules/Accelerator/Steam++.Accelerator.exe`，许可证位于 `licenses/`（如适用）。 |
| JIT | `src/BD.WTTS.Client.Avalonia.App/bin/Release/Publish/` | 带时间戳的发布目录（`Steam++.exe`、`Steam++.exe.config`、`assemblies/`、`modules/`、`native/`、`dotnet/`），以及 `.7z`、`.msix`、`.msixbundle` 和发布清单 `.json`。 |

## 发布指令

在仓库根目录执行（以 Windows x64 为例）：

```powershell
# Native AOT
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build\publish-native.ps1 -RuntimeIdentifier win-x64

# JIT
dotnet run --project .\src\BD.WTTS.Client.Tools.Publish\BD.WTTS.Client.Tools.Publish.csproj -c Release -- run --rids win-x64
```

静态链接 HarfBuzz（可选）：

```powershell
.\build\native-graphics\build-static-graphics.ps1 -RuntimeIdentifier win-x64 -Component HarfBuzz
.\build\publish-native.ps1 -RuntimeIdentifier win-x64 -StaticGraphicsComponents HarfBuzz
```

## 注意事项

- 使用 [global.json](../global.json) 指定的 .NET SDK，并初始化所需子模块。Native AOT 须在目标操作系统发布；Windows 需 C++ 桌面生成工具和对应架构的 Windows SDK，Linux/macOS 使用 64 位 PowerShell 7 和对应原生工具链。
- `-Renderer SimdSkia` 需要 `ref/FluentAvalonia` 源码，默认输出为 `build/publish/native-simdskia/<RID>/`。
- Windows JIT 打包需 MSIX 工具、混淆工具和签名配置；启动器依赖 .NET Framework。
- 分发时保留完整目录结构。AOT 按清单核对文件及 SHA-256 并随包保留清单；不要打包用户数据、缓存、日志或调试符号。
- 重新发布前关闭程序。Windows 脚本可以原地迁移旧布局：先校验旧根目录清单和每项 SHA-256，再替换清单拥有的文件。不会清理 `AppData`、`Cache`、`Log`/`Logs` 或用户压缩包。若已有生成文件未登记、缺失或被修改，改用新的 `-OutputDirectory`。

## macOS Native AOT

在 Mac 上安装 `global.json` 要求的 .NET SDK、`macos` 工作负载、匹配的 Xcode 和 PowerShell 7，然后在仓库根目录执行。

```bash
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier osx-arm64

# DTLS 0.5.1 缺少 Intel 原生资产。安装 Rust 后，先从该版本的源码构建这些资产。
bash ./build/native-dtls/build-macos-x64.sh
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier osx-x64
```

每个架构的目录 `build/publish/native/<RID>/` 包含 `Steam++.app` 和 SHA-256 清单。加速子进程位于 `Steam++.app/Contents/MonoBundle/modules/Accelerator/`。脚本使用 ad hoc 签名。本机可以用 `codesign --verify --deep --strict` 检查应用包，公开分发前需要 Developer ID 签名与公证。

macOS 发布默认从 `~/Documents/安装 Steam++.dmg` 读取安装布局。模板存在时，脚本会把新版 `Steam++.app` 打入 `build/publish/native/osx-arm64.dmg` 或 `build/publish/native/osx-x64.dmg`，并保留模板的窗口背景和卷图标。挂载后，将 `Steam++.app` 拖到「应用程序」即可安装。

可用 `-DiskImageTemplate "/绝对路径/安装 Steam++.dmg"` 指定该模板的位置。没有模板时，新的输出目录只生成应用包。更新已有 DMG 时需要提供模板。使用自定义 `-OutputDirectory` 时，DMG 路径为 `<OutputDirectory>.dmg`。

如果使用自定义 `-BuildRoot`，将 `<BuildRoot>/artifacts/native-dependencies/dtls/osx-x64` 作为 Intel 资产构建脚本的输出目录传入。

## Windows AOT 布局验证

可用以下命令单独发布主程序和加速子进程。请使用新的空目录；直接发布不负责生成打包清单、迁移旧包或合并两个输出目录。

```powershell
dotnet publish .\src\BD.WTTS.Client.Avalonia.App\BD.WTTS.Client.Avalonia.App.csproj -c Release -r win-x64 -f net11.0-windows10.0.19041.0 -p:WttsDesktopTargetFrameworks=net11.0-windows10.0.19041.0 -p:WttsNativeAot=true -p:GeneratePackageOnBuild=false -o .\build\layout-check\main
dotnet publish .\src\BD.WTTS.Client.Plugins.Accelerator.ReverseProxy\BD.WTTS.Client.Plugins.Accelerator.ReverseProxy.csproj -c Release -r win-x64 -f net11.0 -p:WttsNativeAot=true -p:GeneratePackageOnBuild=false -o .\build\layout-check\accelerator
```

冒烟测试使用一次性包副本。需要非提权运行时，仅在副本上用 Windows SDK 的 `mt.exe` 和 `tests/native-aot/GameToolsProbe.manifest` 替换主程序的执行级别；不要修改待分发包或用这个副本测试清单迁移。

1. 确认两次直接发布各自在 `native/win-x64/` 包含 `e_sqlite3.dll`。再用正式脚本发布，确认根目录唯一的文件为 `Steam++.exe`，所有 DLL 直接位于 `native/win-x64/`，加速子目录没有副本；核对清单的 SHA-256。静态链接 HarfBuzz 或 Skia 时，确认只省略已选择静态链接的动态库；JIT 和 Unix 的原有布局应保持不变。
2. 迁移测试只用旧包的副本：在 `AppData`、`Cache`、`Logs` 中放入哨兵文件，重新发布后确认哈希不变且旧清单与根目录 DLL 已迁走。另一个副本中修改一个已登记 DLL，确认脚本拒绝发布且旧包仍在。
3. 在断网的测试环境使用一次性便携包和测试用户，预建 `AppData` 和 `Cache` 目录，从包外工作目录启动 `Steam++.exe`。打开加速页面，通过“查看证书”请求走真实主进程→子进程 IPC；确认返回证书信息。这会在一次性数据目录生成测试证书文件，但不要安装或删除信任证书、启用加速或修改系统代理。用 Process Explorer 或同权限的 `Get-Process -Id <PID> -Module` 核对两个进程加载的 `e_sqlite3.dll` 是否为同一个绝对路径，同时检查实际启用的动态图形库来自 `native/win-x64/`。如果某个后端没有运行，不能把“DLL 存在”当作加载成功。
4. 在未安装 Steam 的离线测试虚拟机里，用 `Steam++.exe -clt c -steamrun` 触发程序自己的 SteamAPI 导入；SteamAPI 初始化返回 false 属预期，但不得出现 DLL 加载错误，应看到 `steam_api64.dll` 从 `native/win-x64/` 加载。7z 可通过一次性测试探针调用 `ISevenZipHelper.Instance.Unpack` 解压本地小归档，确认内容及 `7z.dll` 加载路径，不触发在线更新。x64 空闲测量脚本兼容旧、新清单，但使用的是离线 `steamclient64.dll` 测试替身，不能代替 SteamAPI、7z 或加速 IPC 冒烟测试。

## 参考链接

- [发布脚本](../build/publish-native.ps1)
- [静态图形库构建脚本](../build/native-graphics/build-static-graphics.ps1)
- [发布工具](../src/BD.WTTS.Client.Tools.Publish/Commands/IDotNetPublishCommand.cs)
- [文件系统与用户数据位置](./file-system.md)
- [.NET 应用发布概述](https://learn.microsoft.com/dotnet/core/deploying/)
- [Native AOT 部署、前置条件与限制](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [自定义 .NET Host](https://learn.microsoft.com/dotnet/core/tutorials/netcore-hosting)
- [.NET 分发目录布局](https://learn.microsoft.com/dotnet/core/distribution-packaging)