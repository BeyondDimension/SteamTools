# Steam++ 发布说明

在仓库根目录执行以下命令。首次发布前，运行 `git submodule update --init --recursive` 并安装仓库指定的 .NET SDK。Native AOT 在目标操作系统上构建。Linux 和 macOS 的 Native AOT 发布需要将 PowerShell 7（`pwsh`）加入 PATH。 [SDK 版本](../global.json)

在 Windows 和 Linux 上，`pub run` 默认同时发布 JIT 和 Native AOT，使用 `--aot false` 可只发布 JIT。以下命令覆盖当前 GitHub Actions 的发布目标。

## Windows

安装 Visual Studio 或 Build Tools 的「使用 C++ 的桌面开发」工作负载及 Windows SDK。生成 EXE 安装包时，将 NSIS-Build 放在 SteamTools 仓库旁边的 `../NSIS-Build`。 [NSIS-Build](https://github.com/SteamTools-Team/NSIS-Build)

Windows Native AOT 发布优先使用 PowerShell 7，找不到时使用系统自带的 Windows PowerShell 5.1（`powershell.exe`）。

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

安装与 .NET macOS 工作负载匹配的完整 Xcode，当前 GitHub Actions 使用 Xcode 27.0。下面以 `/Applications/Xcode.app` 为例。Native AOT 打包使用 PowerShell 7 以及 macOS 自带的 `hdiutil` 和 `codesign`。构建 Intel 版还需要 Rust（`rustup` 和 `cargo`），用于预先生成 x64 的 DTLS 原生库。

```bash
export DEVELOPER_DIR='/Applications/Xcode.app/Contents/Developer'
xcodebuild -version
dotnet workload install macos --skip-manifest-update
brew install --cask powershell

pub='./src/BD.WTTS.Client.Tools.Publish/BD.WTTS.Client.Tools.Publish.csproj'
```

发布 Apple Silicon 版时执行下面的命令。

```bash
dotnet run --project "$pub" -c Release --no-launch-profile -- run --rids osx-arm64
```

发布 Intel 版时执行下面的命令。

```bash
bash ./build/native-dtls/build-macos-x64.sh
dotnet run --project "$pub" -c Release --no-launch-profile -- run --rids osx-x64
```

以上命令默认生成 JIT 和 Native AOT 两个 `.dmg`，等同于显式传入 `--aot true`。只需要 JIT 安装包时，在命令末尾添加 `--aot false`。两种安装包都使用仓库自带的模板来保留 Finder 安装布局、背景和图标，并放入本次构建的 `Steam++.app` 和「应用程序」快捷方式。 [仓库模板](../build/macos/installer-template.dmg)

单独运行 Native AOT 发布脚本时，默认使用 `build/publish/native/`。以下命令生成 `build/publish/native/osx-arm64/` 和同级的 `build/publish/native/osx-arm64.dmg`。

```bash
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier osx-arm64
```

要使用自定义模板，可在这条命令后附加 `-DiskImageTemplate "/path/to/安装 Steam++.dmg"`。自定义模板需包含 `.DS_Store`、`.VolumeIcon.icns`、`b.png`、`l.icns` 和卷的 Finder 图标属性。 [镜像打包脚本](../build/macos/create-dmg.sh)

## 发布产物

以上 `pub` 命令的产物位于 `src/BD.WTTS.Client.Avalonia.App/bin/Release/Publish/`。程序目录和发布包以应用版本、目标架构和构建时间戳命名。macOS 的文件名格式如下。

- JIT `[Release] Steam++_v<版本>_osx_<架构>_<时间戳>.dmg`
- Native AOT `[Release] Steam++(Native AOT)_v<版本>_osx_<架构>_<时间戳>.dmg`

Native AOT 的内部校验记录位于 `build/artifacts/`。

| 平台 | `pub run` 生成的包 | 后续打包 |
| --- | --- | --- |
| Windows x64 | `.7z`、MSIX 和 `.msixbundle` | `pub nsis` 生成 `.exe` 安装包 |
| Linux x64、ARM64 | `.tar.gz` | GitHub Actions 以 `.tgz` 文件名上传 |
| macOS Intel、Apple Silicon | JIT 和 Native AOT `.dmg` | — |

手动运行 Publish 工作流会构建并上传三端安装包和压缩包。推送标签时，工作流还会制作 Windows EXE 安装包，汇总产物并生成 `SHA256SUMS.txt`，然后创建草稿 Release。 [发布工作流](../.github/workflows/publish.yml)

## 参考链接

- [发布工具](../src/BD.WTTS.Client.Tools.Publish/README.md)
- [Native AOT 发布脚本](../build/publish-native.ps1) · [Native AOT 前置条件](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
