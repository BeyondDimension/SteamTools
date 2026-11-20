# Steam++ 发布说明

## 发布产物

| 方式 | 默认输出 | 产物 |
| --- | --- | --- |
| Windows Native AOT | `build/publish/aot-packages/<RID>/<批次>/` | EXE 安装包、7z 便携包、MSIX 和 `SHA256SUMS.txt`。 |
| Linux Native AOT | `build/publish/native/<RID>/` | `Steam++`、加速子进程、原生依赖和 SHA-256 清单。 |
| macOS Native AOT | `build/publish/native/<RID>/` | `Steam++.app` 和 SHA-256 清单；有模板时另生成同级 `.dmg`。 |
| Windows JIT | `src/BD.WTTS.Client.Avalonia.App/bin/Release/Publish/` | 程序目录、7z、MSIX / MSIXBundle 和发布清单。 |

## 发布指令

在仓库根目录执行对应平台的命令。

```powershell
# Windows（Native AOT / JIT）
$pub = './src/BD.WTTS.Client.Tools.Publish/BD.WTTS.Client.Tools.Publish.csproj'
dotnet run --project $pub -c Release -- aot --rids win-x64
dotnet run --project $pub -c Release -- run --rids win-x64
```

```bash
# Linux（x64 / ARM64：使用 linux-x64 或 linux-arm64）
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier linux-x64

# macOS（Apple Silicon / Intel）
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier osx-arm64
bash ./build/native-dtls/build-macos-x64.sh
pwsh -NoProfile -File ./build/publish-native.ps1 -RuntimeIdentifier osx-x64
```

## 注意事项

- 初始化子模块并安装 [global.json](../global.json) 指定的 SDK。Native AOT 在对应操作系统发布：Windows 需 PowerShell 7、C++ 构建工具和 Windows SDK，Linux 需 Clang 和原生开发库，macOS 需 Xcode 和 .NET macOS 工作负载（Intel 还需 Rust）。
- Windows `aot` 支持 `win-x64`、`win-arm64`。默认生成三种包，可用 `--formats 7z` 等选项选择格式，`--output` 指定输出根目录。EXE 需 NSIS 3（可用 `--makensis` 指定路径）；更换 EXE 安装版前先卸载旧 AOT 安装版，保留用户数据。
- 默认生成未签名的 EXE / MSIX；传 `--sign-thumbprint <指纹>` 使用 `CurrentUser/My` 中的签名证书。MSIX 安装要求签名受目标系统信任，不代表通过商店认证。
- macOS 可用 `-DiskImageTemplate` 指定 DMG 模板（默认 `~/Documents/安装 Steam++.dmg`）；公开分发需签名和公证。

## 参考链接

- [发布工具](../src/BD.WTTS.Client.Tools.Publish/README.md)
- [Native AOT 发布脚本](../build/publish-native.ps1) · [Native AOT 前置条件](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [NSIS](https://nsis.sourceforge.io/Download) · [MSIX 签名](https://learn.microsoft.com/windows/msix/package/signing-package-overview)