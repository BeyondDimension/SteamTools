# Steam++ 发布说明

## 发布产物

| 方式 | 默认输出 | 产物 |
| --- | --- | --- |
| Windows Native AOT | `build/publish/aot-packages/<RID>/<批次>/` | EXE 安装包、7z 便携包、MSIX 和 `SHA256SUMS.txt`。 |
| Linux Native AOT | `build/publish/native/<RID>/` | `Steam++`、加速子进程、原生依赖和 SHA-256 清单。 |
| macOS Native AOT | `build/publish/native/<RID>/` | `Steam++.app` 和 SHA-256 清单；有模板时另生成同级 `.dmg`。 |
| JIT | `src/BD.WTTS.Client.Avalonia.App/bin/Release/Publish/` | 程序目录和发布清单；Windows 可生成 EXE、7z 和 MSIX / MSIXBundle，Linux 生成 tar.gz，macOS 生成 7z。 |

## 发布指令

在仓库根目录执行。`--mode jit` 只发布 JIT（省略 `--mode` 时的默认值），`--mode aot` 只发布 Native AOT，`--mode both` 依次发布两套产物。两种模式使用同一组 `--rids`，产物分目录保存，不会互相覆盖。
旧的 `aot` 子命令改用 `run --mode aot`，原有 `run --rids ...` 仍只发布 JIT。

### Windows

```powershell
$pub = './src/BD.WTTS.Client.Tools.Publish/BD.WTTS.Client.Tools.Publish.csproj'

# 仅 JIT，包含 EXE 安装包
dotnet run --project $pub -c Release -- run --mode jit --rids win-x64 --formats exe 7z msix

# 仅 Native AOT
dotnet run --project $pub -c Release -- run --mode aot --rids win-x64

# 一次发布 JIT 和 Native AOT，默认两套都生成 EXE、7z 和 MSIX
dotnet run --project $pub -c Release -- run --mode both --rids win-x64

# 用指定的 NSIS-Build 目录，仅生成两套 EXE
dotnet run --project $pub -c Release -- run --mode both --rids win-x64 --formats exe --nsis-build '../NSIS-Build'
```

已编译发布工具时，可以直接调用 `pub run --mode both --rids win-x64`；用 `pub run --help` 查看完整参数。已有 JIT 7z 可用 `pub nsis --rids win-x64 --t latest` 单独生成 EXE，不需要重新编译程序。

### Linux / macOS

```bash
pub='./src/BD.WTTS.Client.Tools.Publish/BD.WTTS.Client.Tools.Publish.csproj'

# Linux：使用 linux-x64 或 linux-arm64
dotnet run --project "$pub" -c Release -- run --mode both --rids linux-x64

# macOS：Apple Silicon
dotnet run --project "$pub" -c Release -- run --mode both --rids osx-arm64

# macOS：Intel，先准备 DTLS 原生库
bash ./build/native-dtls/build-macos-x64.sh
dotnet run --project "$pub" -c Release -- run --mode both --rids osx-x64
```

## 注意事项

- 初始化子模块并安装 [global.json](../global.json) 指定的 SDK。Native AOT 需 PowerShell 7，且须在目标操作系统发布：Windows 需 C++ 构建工具和 Windows SDK，Linux 需 Clang 和原生开发库，macOS 需 Xcode 和 .NET macOS 工作负载（Intel 还需 Rust）。
- JIT 打包需要目标架构的 .NET / ASP.NET Core 共享运行时；macOS 生成 7z 时还需 `7zz`（`brew install sevenzip`）。
- Native AOT 和 `both` 支持各平台的 x64、ARM64；`win-x86` 仅支持 JIT。`--debug` 只影响 JIT。`--output` 只覆盖 AOT 输出根目录。
- Windows `--formats` 可选 `exe 7z msix`；未指定时，JIT 单独发布默认 `7z msix`，`aot` / `both` 默认三种。EXE 共用原 [NSIS-Build](https://github.com/SteamTools-Team/NSIS-Build) 模板和编译器；将该仓库放在 SteamTools 同级，或用 `--nsis-build` 指定。
- EXE 安装版沿用 JIT 的安装目录、卸载项和 `%LOCALAPPDATA%\Steam++`。退出主程序和加速子进程后，直接覆盖安装即可，无需卸载 JIT；旧包中未被替换的文件不主动删除。7z 便携版仍使用解压目录中的 `AppData`。
- `--sign-thumbprint <指纹>` 为选中的 Windows EXE / MSIX 使用 `CurrentUser/My` 中的证书。不指定时 AOT 不签名，JIT 保留原有签名流程。MSIX 必须保留原 `Publisher`，使用主题为 `CN=A90E406B-B2D3-4A23-B061-0FA1D65C4F66` 且受目标系统信任的证书；同包族递增版本可保留原 `LocalState`。
- macOS 自定义 DMG 模板时，直接调用 [Native AOT 发布脚本](../build/publish-native.ps1) 并传入 `-DiskImageTemplate`（默认 `~/Documents/安装 Steam++.dmg`）；公开分发需签名和公证。

## 参考链接

- [发布工具](../src/BD.WTTS.Client.Tools.Publish/README.md)
- [Native AOT 发布脚本](../build/publish-native.ps1) · [Native AOT 前置条件](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [NSIS-Build](https://github.com/SteamTools-Team/NSIS-Build) · [MSIX 签名](https://learn.microsoft.com/windows/msix/package/signing-package-overview)