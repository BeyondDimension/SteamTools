## 客户端发布命令行工具

### 发布应用

`pub run` 默认同时构建 JIT 和 Native AOT，等同于 `--aot true`。`--aot false` 仅构建 JIT。macOS 为每种构建模式生成一个 `.dmg` 安装包。以下命令适用于已编译的发布工具。 [依赖和产物路径](../../doc/publish.md)

```sh
# Windows x64
pub run --rids win-x64 --aot true
pub nsis --rids win-x64 --t latest --aot true

# Linux（按宿主架构选一条）
pub run --rids linux-x64 --aot true
pub run --rids linux-arm64 --aot true

# macOS（构建 Intel 包前先准备 x64 的 DTLS 原生库）
pub run --rids osx-arm64
pub run --rids osx-x64
```

### 扫描发布文件夹命令
```
pub scan --rids win-x64 win-x86 win-arm64
pub scan --rids linux-x64 linux-arm64
pub scan --rids osx-x64 osx-arm64
pub scan --sha256 false --rids win-x64 win-x86 win-arm64
pub scan --signature false --rids win-x64 win-x86 win-arm64
```

### 启动应用程序测试命令
```
pub launch --rids win-x64 win-x86 win-arm64
pub launch --rids linux-x64 linux-arm64
pub launch --rids osx-x64 osx-arm64
```

### 创建压缩包命令
```
pub compressed --7z --rids win-x64 win-x86 win-arm64
pub compressed --zstd --rids linux-x64 linux-arm64
```
