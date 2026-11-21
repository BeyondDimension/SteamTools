# Steam++ 发布说明

在仓库根目录执行以下命令。首次发布前，运行 `git submodule update --init --recursive` 并安装仓库指定的 .NET SDK。Native AOT 需要将 PowerShell 7（`pwsh`）加入 PATH，并在目标操作系统上构建。 [SDK 版本](../global.json)

`pub run --aot true` 同时发布 JIT 和 Native AOT，这是默认行为。只发布 JIT 时使用 `--aot false`。以下命令覆盖当前 GitHub Actions 的发布目标。

## Windows

安装 Visual Studio 或 Build Tools 的「使用 C++ 的桌面开发」工作负载及 Windows SDK。生成 EXE 安装包时，将 NSIS-Build 放在 SteamTools 仓库旁边的 `../NSIS-Build`。 [NSIS-Build](https://github.com/SteamTools-Team/NSIS-Build)

```powershell
$pub = './src/BD.WTTS.Client.Tools.Publish/BD.WTTS.Client.Tools.Publish.csproj'

# 生成 Windows x64 的 JIT 和 Native AOT 包
dotnet run --project $pub -c Release --no-launch-profile -- run --rids win-x64 --aot true

# 用上一步生成的 7z 包制作两套 EXE 安装包
dotnet run --project $pub -c Release --no-launch-profile --no-build -- nsis --rids win-x64 --t latest --aot true
```

若发布目录里有多个构建批次，将 `--t latest` 改为目标压缩包文件名末尾的时间戳，例如 `--t 261121_1917009441349`。打包和制作安装包时使用相同的 `--aot` 值。

安装 MSIX 包需要使用与现有 Publisher 匹配且受目标设备信任的证书。 [MSIX 签名](https://learn.microsoft.com/windows/msix/package/signing-package-overview)

## Linux

JIT 包会复制目标架构的 .NET 和 ASP.NET Core 共享运行时。Ubuntu 上可用以下命令安装 Native AOT 的编译依赖。

```bash
sudo apt-get update
sudo apt-get install -y clang zlib1g-dev

pub='./src/BD.WTTS.Client.Tools.Publish/BD.WTTS.Client.Tools.Publish.csproj'
```

在 x64 Linux 主机上执行下面的命令。

```bash
dotnet run --project "$pub" -c Release --no-launch-profile -- run --rids linux-x64 --aot true
```

在 ARM64 Linux 主机上执行下面的命令。

```bash
dotnet run --project "$pub" -c Release --no-launch-profile -- run --rids linux-arm64 --aot true
```

## macOS

选择与 .NET macOS 工作负载匹配的 Xcode，当前 GitHub Actions 使用 Xcode 27.0。生成 7z 包需要 `7zz`。构建 Intel 版还需要 Rust（`rustup` 和 `cargo`），用于预先生成 x64 的 DTLS 原生库。

```bash
dotnet workload install macos --skip-manifest-update
brew install sevenzip

pub='./src/BD.WTTS.Client.Tools.Publish/BD.WTTS.Client.Tools.Publish.csproj'
```

发布 Apple Silicon 版时执行下面的命令。

```bash
dotnet run --project "$pub" -c Release --no-launch-profile -- run --rids osx-arm64 --aot true
```

发布 Intel 版时执行下面的命令。

```bash
bash ./build/native-dtls/build-macos-x64.sh
dotnet run --project "$pub" -c Release --no-launch-profile -- run --rids osx-x64 --aot true
```

需要单独构建原生目录时，可直接调用 `build/publish-native.ps1`，默认输出到 `build/publish/native/<RID>/`。在 macOS 上可通过 `-DiskImageTemplate` 指定已有模板，脚本也会查找 `~/Documents/安装 Steam++.dmg`。使用模板生成的镜像与输出目录同级，示例会生成 `build/publish/native/osx-arm64.dmg`。

```bash
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier osx-arm64 -DiskImageTemplate "/path/to/安装 Steam++.dmg"
```

## 发布产物

以上 `pub` 命令的产物位于 `src/BD.WTTS.Client.Avalonia.App/bin/Release/Publish/`。程序目录、归档和 JSON 发布清单以应用版本、目标架构和构建时间戳命名，Native AOT 版本带有 `(Native AOT)` 标记。

| 平台 | `pub run` 生成的包 | 后续打包 |
| --- | --- | --- |
| Windows x64 | `.7z`、MSIX 和 `.msixbundle` | `pub nsis` 生成 `.exe` 安装包 |
| Linux x64、ARM64 | `.tar.gz` | GitHub Actions 以 `.tgz` 文件名上传 |
| macOS Intel、Apple Silicon | `.7z` | 带模板运行独立的原生发布脚本可生成 `.dmg` |

推送标签时，Publish 工作流汇总三端的 JIT 和 Native AOT 包，生成 `SHA256SUMS.txt` 并创建草稿 Release。 [发布工作流](../.github/workflows/publish.yml)

## 参考链接

- [发布工具](../src/BD.WTTS.Client.Tools.Publish/README.md)
- [Native AOT 发布脚本](../build/publish-native.ps1) · [Native AOT 前置条件](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
