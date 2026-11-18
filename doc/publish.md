# Steam++ 发布说明

## 发布交付物

| 方式 | 默认输出 | Windows 交付文件 |
| --- | --- | --- |
| Native AOT | `build/publish/native/<RID>/` | `Steam++.exe`、`modules/Accelerator/Steam++.Accelerator.exe`、原生库、随包许可证和 `.wtts-native-manifest.json`。 |
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
- 重新发布前关闭程序。已有 AOT 输出目录缺失清单或包含未知、已修改的文件时，改用新的 `-OutputDirectory`。

## 参考链接

- [发布脚本](../build/publish-native.ps1)
- [静态图形库构建脚本](../build/native-graphics/build-static-graphics.ps1)
- [发布工具](../src/BD.WTTS.Client.Tools.Publish/Commands/IDotNetPublishCommand.cs)
- [文件系统与用户数据位置](./file-system.md)
- [.NET 应用发布概述](https://learn.microsoft.com/dotnet/core/deploying/)
- [Native AOT 部署、前置条件与限制](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [自定义 .NET Host](https://learn.microsoft.com/dotnet/core/tutorials/netcore-hosting)
- [.NET 分发目录布局](https://learn.microsoft.com/dotnet/core/distribution-packaging)