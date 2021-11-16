# 客户端发布命令行工具(Command Line Tools/CLT)

## 发布新版本操作指南
- 说明
    - (✔)Linux 上执行程序，需要程序权限中有执行，在 WinSCP 工具中右键属性，打勾执行权限
    - 发布工具程序文件名为 p 简短名字方便打命令
     - Linux 上文件在 /home/{user}/pub 文件夹中
     - Linux 上使用 ./p xxxx 执行程序
     - Windows 上使用 CMD p xxxx 执行程序
    - 发布本程序的 win-x64 在发布文件夹中打开 终端 执行命令，不可将程序复制到项目文件夹之外的地方执行

### 1. (云端)得到 RSA 公钥信息(如果是新版本号则**创建**，如果是之前的版本号发布另一个平台则使用**获取**)
- 创建新版本号并返回 RSA 公钥
    <pre>./p nv -v 2.0.0 -desc "1. xxx;2. yyy;3. ddd" -use_last_skey // 创建一个新的版本号并使用上一个版本的密钥</pre>
    <pre>./p nv -v 2.0.0 -desc "1. xxx;2. yyy;3. ddd" // 创建一个新的版本号</pre>
- 获取之前创建新版本号 RSA 公钥
    <pre>./p gv -v 2.0.0 // 获取一个已有的版本号</pre>

### 2. (本地)**手动**复制终端窗口中的 RSA 公钥值到剪切板

### 3. (本地)读取剪切板公钥值写入 txt 的 pfx 文件中
- 从剪切板中读取
    <pre>p rr -dev // 写入到测试环境中</pre>
    <pre>p rr // 写入到正式环境中</pre>
- 从命令行中传入
    <pre>p rr -dev -val XXXX // 写入到测试环境中</pre>
    <pre>p rr -val XXXX // 写入到正式环境中</pre>

### 4. (本地)**手动**在VS中发布任意一个或多个平台配置(pubxml)，后续可改成命令行自动发布
- (本地)将发布 Host 入口点重定向到 Bin 目录中，仅支持 win-x64
    <pre>p hostpath</pre>
    <pre>p hp</pre>

### 5. (本地)验证发布文件夹与统计文件
- 统计所有文件(忽略xml,pdb)写入 **Publish.json**
- 命令示例
    <pre>p sta -val "win-x64" // 仅发布 Win 平台 64位</pre>
    <pre>p sta -val "android-arm android-arm64" // 仅发布 Android 平台 ARM系列</pre>
    <pre>p sta -val "win-x64 osx-x64 osx-arm64 linux-x64 linux-arm64 linux-arm" // 发布多个桌面平台</pre>
    <pre>p sta -dev -val "win-x64" // (测试环境/Debug)仅发布 Win 平台 64位</pre>
    <pre>p sta -dev -val "win-x64 osx-x64 linux-x64" // (测试环境/Debug)发布多个桌面平台</pre>
    
### 8. (本地)读取上一步操作后的 **Publish.json** 生成压缩包并计算哈希值写入 **Publish.json**
- 命令示例
    <pre>p 7z // 打包 7z 格式压缩包</pre>

### 8.1 (本地)读取上一步操作后的 **Publish.json** 生成安装包并计算哈希值写入 **Publish.json**

#### rpm
- Create a CentOS/RedHat Linux installer
    <pre>p rpm</pre>

#### deb
- Create a Ubuntu/Debian Linux installer
    <pre>p deb</pre>

#### ~~pkg~~
- Create a macOS installer
    <pre>p pkg</pre>

#### ~~msi~~
- Create a Windows Installer (msi) package
    <pre>p msi</pre>

### 10. (本地)将 **Publish.json** 上传至云端
- 将本地 ```ST.Tools.Publish\bin\Release\Publish\win-x64\AppData\Publish.json``` 复制到云端 ```/home/{user}/pub/AppData/Publish.json``` 

### 11. (云端)读取上一步上传的数据写入数据库中
- 读取 **Publish.json** 写入数据库
- 命令示例
    <pre>./p wdb -v 2.0.0 -dev -dev_custom "url1;url2;url3" // 在测试环境中创建 2.0.0 版本发布数据并附加下载链接</pre>
    <pre>./p wdb -v 2.0.0 -gitee "url1;url2;url3" // 在正式环境中创建 2.0.0 版本发布数据并附加码云下载链接</pre>

### 12. (本地)读取 **Publish.json** 中的 SHA256 值写入 release-template.md
- 命令示例
    <pre>p rel</pre>