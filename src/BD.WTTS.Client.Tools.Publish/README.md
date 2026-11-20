## 客户端发布命令行工具

### 发布应用

用 `pub run --mode jit|aot|both --rids <RID>` 选择发布模式；省略 `--mode` 时仅发布 JIT。完整调用方式、安装包参数和输出位置见[发布说明](../../doc/publish.md)。

### DotNet 发布命令
```
pub run --rids win-x64
pub run --rids win-x86
pub run --rids win-arm64
pub run --rids linux-x64
pub run --rids linux-arm64
pub run --rids osx-x64
pub run --rids osx-arm64
pub run --debug --rids win-x64
pub run --debug --rids win-x86
pub run --debug --rids win-arm64
pub run --debug --rids linux-x64
pub run --debug --rids linux-arm64
pub run --debug --rids osx-x64
pub run --debug --rids osx-arm64
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
pub compressed --7z --rids osx-x64 osx-arm64
```
