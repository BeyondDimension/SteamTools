# Steam++ 发布说明

## 发布产物

| 方式 | 默认输出 | 产物 |
| --- | --- | --- |
| Windows Native AOT | `build/publish/native/<RID>/` | `Steam++.exe`、`modules/Accelerator/Steam++.Accelerator.exe`、`native/<RID>/` 原生依赖及 `native/.wtts-native-manifest.json`。 |
| Linux Native AOT | `build/publish/native/<RID>/` | `Steam++`、加速子进程、原生依赖和 SHA-256 清单。 |
| macOS Native AOT | `build/publish/native/<RID>/` | `Steam++.app` 和 SHA-256 清单；提供安装模板时生成同级 `<RID>.dmg`。 |
| Windows JIT | `src/BD.WTTS.Client.Avalonia.App/bin/Release/Publish/` | 程序目录、`.7z`、`.msix` / `.msixbundle` 和 `.json` 发布清单。 |

## 发布指令

在仓库根目录执行对应平台的命令：

```powershell
# Windows x64 Native AOT
pwsh -NoProfile -ExecutionPolicy Bypass -File ./build/publish-native.ps1 -RuntimeIdentifier win-x64

# Linux x64 Native AOT
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier linux-x64

# macOS Native AOT（Apple Silicon / Intel）
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier osx-arm64
bash ./build/native-dtls/build-macos-x64.sh
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier osx-x64

# Windows x64 JIT
dotnet run --project ./src/BD.WTTS.Client.Tools.Publish/BD.WTTS.Client.Tools.Publish.csproj -c Release -- run --rids win-x64
```

## 注意事项

- 初始化子模块并安装 [global.json](../global.json) 指定的 .NET SDK。Native AOT 使用 64 位 PowerShell，在目标系统上发布；Windows 需 C++ 构建工具和 Windows SDK，Linux 需 Clang 和相关开发库，macOS 需 Xcode 和 .NET macOS 工作负载。Windows JIT 打包需 MSIX 工具和签名配置。
- 覆盖发布前关闭程序；也可用 `-OutputDirectory` 指定新目录。分发时保留完整目录与清单，主程序和加速子进程一并更新，不打包用户数据、缓存和日志。
- macOS 可用 `-DiskImageTemplate` 指定 DMG 模板（默认 `~/Documents/安装 Steam++.dmg`）。公开分发需 Developer ID 签名和公证。

## 参考链接

- [Native AOT 发布脚本](../build/publish-native.ps1)
- [JIT 发布工具](../src/BD.WTTS.Client.Tools.Publish/Commands/IDotNetPublishCommand.cs)
- [静态图形库构建脚本](../build/native-graphics/build-static-graphics.ps1)
- [Native AOT 前置条件及限制](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)