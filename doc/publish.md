# Steam++ v3.X Publish Document  

### Assets
- Windows
    - x64
        - ```Steam++_vx.y.z_win_x64.msix```
        - ```Steam++_vx.y.z_win_x64.7z```
    - x86
        - ```Steam++_vx.y.z_win_x86.msix```
        - ```Steam++_vx.y.z_win_x86.7z```
    - Arm64
        - ```Steam++_vx.y.z_win_arm64.msix```
        - ```Steam++_vx.y.z_win_arm64.7z```
- Linux
    - x64
        - ```Steam++_vx.y.z_linux_x64.tar.zst```
        - ```Steam++_vx.y.z_linux_x64.deb```
        - ```Steam++_vx.y.z_linux_x64.rpm```
    - Arm64
        - ```Steam++_vx.y.z_linux_arm64.tar.zst```
        - ```Steam++_vx.y.z_linux_arm64.deb```
        - ```Steam++_vx.y.z_linux_arm64.rpm```
- macOS  
    - x64
        - ```Steam++_vx.y.z_macos_x64.pkg```
    - Arm64
        - ```Steam++_vx.y.z_macos_arm64.pkg```
- Android  
    - Arm64
        - ```Steam++_vx.y.z_android_arm64.apk```

### Program File Structure
- [Windows](./program-file-structure/Windows.md)
- [Linux](./program-file-structure/Linux.md)
- [macOS](./program-file-structure/macOS.md)

### JIT 发布优化

普通版本继续使用 CoreCLR/JIT，保留动态托管插件、反射、多语言和现有模块布局；不对无法在构建时看到的第三方插件进行全局裁剪。

- Windows 还原后选择 `Splat.Drawing` 与 `Microsoft.VisualStudio.Threading.Only` 的跨平台资产，在框架解析前避免附带 WPF 依赖。发布工具不再从 NuGet 缓存选择“最新版”替换 DLL，程序集必须与生成的 deps 文件一致。
- 打包器按主程序和模块的实际 `runtimeconfig.json` 收集共享框架依赖闭包。现有 Windows 启动器仍需要 Core、AspNet 和同版本 hostfxr；只有声明了 Desktop 的程序或模块才附带 WindowsDesktop。框架内部文件保持完整，缺少所需版本或架构不符时明确失败，不再静默生成缺运行时的包。
- Windows Arm64 包的 x64 主程序沿用 x64 共享运行时，原有自包含 Arm64 加速子进程不依赖这份运行时；其他平台仍须单独发布和验证。
- 主程序和加速子进程发布时，将 NuGet 本机库附带的 `.pdb` / `.dbg` 单独保存到构建目录的 `symbols` 中，不再放进分发目录；保留对应版本符号供诊断。此项减少磁盘体积，不代表运行时内存下降。
- Windows Release/RID 发布默认只为主程序、Client、Client.Avalonia、Avalonia.Base、Avalonia.Controls、FluentAvalonia 生成普通 ReadyToRun；其余程序集保留 IL，未开启 composite R2R。源码仍由 JIT 优化热点，第三方 DLL 仍动态加载。手工发布可通过 `-p:PublishReadyToRun=false` 关闭；不对 Debug 或完整 Native AOT 启用此项。
- 插件验证和 MEF 发现共用一次初始化内的类型枚举结果，不建立常驻类型缓存；额外的属性导出、依赖导入及禁用插件的可回收上下文仍保留。
- 视图模型从序列化快照恢复并加入活动缓存后释放旧快照；恢复失败仍保留原数据，并且不会删除并发写入的新快照。
- 图片流池的块大小为 64 KiB，避免小块缓冲区进入 LOH；空闲小块池为 4 MiB。连续大缓冲区使用指数尺寸桶，每桶最多保留 4 MiB，只有 1/2/4 MiB 桶能保留缓冲区，总计约 12 MiB；旧线性池的 128 MiB 参数也是每桶上限，不是总池上限。仅限制归还后的空闲缓存，不限制图片、流长度或在用内存，不改变解码分辨率；大缓冲区反复使用时会增加分配和 GC。

#### JIT 实测（2026-11-15，Windows x64，.NET 11）

最终本地未签名目录为 `build/jit/win-x64`，包含启动器、7 个内置动态插件、JIT 加速子进程及所需的 Core/AspNet 共享运行时；不是 Native AOT 包，也不是仅依赖系统运行时的小目录。字体、语言、异常信息及动态插件机制保留。

对照基线是上一轮通用优化完成后的 JIT 源码。旧打包器在本机因内部版本后缀与安装目录不一致而漏复制运行时，本轮为基线显式补齐其原本要复制的同版本 Core/AspNet/Desktop，再比较完整目录；没有把缺少运行时的目录当作有效基线。目录大小不包含运行生成的数据和日志，包含旧版原本随包携带、现在单独归档的约 114.96 MiB 本机库符号。安装器、签名和压缩包未比较。

三组交替启动均使用独立数据目录、相同配置、空本地缓存及随包的同版本 CoreCLR，首窗后等待 10 秒取样；不强制 GC 或缩减工作集。系统文件缓存没有清空，首页网络内容、Steam 本地状态及后台活动仍会波动：

| 指标（启动/内存为中位数） | 原 JIT | 优化 JIT |
| --- | ---: | ---: |
| 完整目录 | 454.51 MiB / 925 文件 | 264.42 MiB / 641 文件 |
| 首个主窗口出现 | 3,714 ms | 2,674 ms |
| 取样时累计 CPU 时间 | 8,578 ms | 7,000 ms |
| 私有字节 | 348.00 MiB | 345.18 MiB |
| 工作集 | 382.43 MiB | 379.14 MiB |

完整目录减少 41.82%，本机首窗时间缩短约 28%。选择性 R2R 比仅做体积清理的版本增加约 16.4 MiB；没有为了启动速度预编译全部依赖。空闲内存差异很小，不能据此宣称整体内存明显下降。原 JIT 有一次私有内存样本达到约 705.7 MiB，该值保留在原始结果中，没有拿它计算内存降幅。原生启动器及 `GCConserveMemory=5` 未显示值得默认采用的整体收益，因此保留原启动器及 GC 策略。

独立图片池夹具在前后程序集上执行相同的 48 次真实流写入、连续缓冲区获取和读回，负载为 1～16 MiB 加 17 字节的三个循环；只在该隔离探针内收集 GC，不影响上表进程测量：

| 图片流池压力场景 | 原版 | 优化版 |
| --- | ---: | ---: |
| 归还后空闲缓冲区总量 | 160 MiB | 10 MiB |
| 该负载累计托管分配 | 271.51 MiB | 822.67 MiB |

连续大缓冲区按尺寸桶缓存，原 128 MiB 设置不是全池硬上限。新版主动丢弃大缓冲区，显著减少该场景的常驻池内存，但相同大流反复处理时分配增加约三倍；不声称这对所有图片负载都更快，也不把池内存数值当成整个程序的内存占用。

验证：12 项针对性 JIT 检查通过；新增快照生命周期检查在保留的旧 Client DLL 上两项均失败、当前版本均通过。真实打包 helper 验证了 Core/AspNet 依赖、显式 Desktop 保留、缺少版本明确失败，以及自包含配置不重复附带共享框架。最终包实际加载了全部 7 个内置插件及额外测试插件，属性导出的依赖导入成功，完成 9 个主页面及相关嵌套标签的切换和渲染。测试插件未放入最终分发目录，临时探针已清理，测量记录保留在 `build/jit-optimization-20261115`。

边界：只实测 Windows x64。额外插件验证针对现有 CoreCLR/Avalonia 插件合同，不代表任意 WPF/WinForms 依赖自动兼容。未重新验证真实账号登录/交易、ASF 实例、WinDivert、系统证书安装或其他操作系统/架构；本轮也未做相同账号库存下的长期操作延迟与 GC 暂停基准。图片池源码位于 `ref/Avalonia8` 子模块，提交时需一并保留。

参考：[ReadyToRun 编译与取舍](https://learn.microsoft.com/dotnet/core/deploying/ready-to-run)、[GC 配置](https://learn.microsoft.com/dotnet/core/runtime-config/garbage-collector)、[RecyclableMemoryStream 池设计](https://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream)。


### Native AOT 桌面发布

Native AOT 使用 `WttsNativeAot=true` 构建独立的原生版本；普通构建仍使用 JIT。不要在整个项目引用图上直接传递 `PublishAot=true`，源码生成器和 `netstandard` 类库不是原生可执行项目。

| 行为 | Native AOT | 普通 JIT 发布 |
| --- | --- | --- |
| 7 个内置插件 | 静态编译，保留启用/禁用设置和模块选择 | 动态加载 |
| 额外的托管插件 DLL | 不加载；需要加入编译后重新发布 | 支持原有插件加载机制 |
| 加速子进程 | 独立原生程序，位于 `modules/Accelerator` | 原有发布布局 |
| 多语言与异常堆栈 | 保留 | 保留 |

Windows 需要仓库指定的 .NET 11 SDK、Visual Studio C++ 桌面生成工具和 Windows SDK。在目标操作系统上发布对应 RID，不使用跨操作系统 Native AOT 编译。

```powershell
powershell -File build/publish-native.ps1 -RuntimeIdentifier win-x64 -OutputDirectory build/native-aot/win-x64-release
```

脚本先发布主程序，再发布加速子进程；默认输出到 `build/native-aot/<RID>`。可通过 `-OutputDirectory <新目录>` 指定输出位置。目标目录必须不存在，避免旧 DLL、自定义 AppHost 与原生文件混在一起；构建缓存单独保存在 `build/native-aot-artifacts/<RID>`，不覆盖日常调试产物。

Native AOT 编译会将项目引用的卫星资源一并嵌入原生程序。原生库的 `.pdb` / `.dbg` 文件保留在构建缓存的 `symbols/<项目>/<RID>` 目录，不随安装包分发；诊断崩溃时应保存对应版本的符号。Windows 原生程序直接使用发布目录中的本机 DLL，不执行旧托管安装包的根目录清理逻辑。

需要定位启动阶段耗时时，可在手动 `dotnet publish` 命令中添加 `-p:WttsStartupTrace=true`，输出已有的阶段计时；正常发布不启用此项。


AOT 构建在编译前选择 `Splat.Drawing` 和 `Microsoft.VisualStudio.Threading.Only` 的跨平台程序集，避免引入 WPF 的 C++/CLI 组件。升级这些依赖时须重新验证对应的程序集目标框架。GUI 和加速子进程均默认使用 `OptimizationPreference=Size`，手动发布可显式覆盖该属性；这是代码体积与峰值吞吐量的取舍，不保证所有操作更快。Windows 加速进程额外默认 `IlcDehydrate=false`：试验中的 22.05 MiB 脱水版本比非脱水版本多用约 5.5 MiB 启动私有内存，因此没有作为最终分发版本。保持全球化、错误堆栈及默认 CPU 指令集，不通过删除语言或提高 CPU 最低要求缩小体积。

设置访问使用缓存的强类型委托，保存与 JSON 枚举使用已注册的泛型和源生成元数据。IPC 的源生成上下文必须覆盖新增的参数及返回类型；缺少类型时明确报错，不回退到反射序列化。Native AOT 版本不能依赖程序集文件路径：资源使用程序集资源 API，子进程使用发布根目录下的固定布局。

Native MemoryPack 写入使用 BCL `ArrayBufferWriter<byte>` 的复用路径，规避当前 MemoryPack 内部写入器在原生执行中的崩溃。缓存每线程最多保留一个容量不超过 64 KiB 的缓冲区，归还前清空已写入数据，较大的临时缓冲区不留在缓存中；JIT 保持原有实现。Windows 图形资源缓存上限设为 64 MiB，此值是缓存上限，不是进程内存占用保证。

加速项目本地缓存继续使用原来的 MessagePack/LZ4 字节格式。缓存的项目组、递归项目及集合使用显式格式化器，不在运行时生成序列化代码；新增缓存字段时同步维护对应的数字键，并验证旧缓存读入和字节级兼容性。

UI 的只读状态使用显式 `ObservableAsPropertyHelper`，避免依赖 Fody 动态查找生成字段。通过 `TabStrip.Tag` 保存页面类型的嵌套导航保留精确的公开无参构造函数；新增子页面时同步补充该依赖。内层页面导航失败会记录实际异常，而不是只留下空白标签页。

#### 进一步缩减体积与分配

- Native AOT 必然裁剪不可达代码；本项目没有关闭反射元数据来换取无法运行的瘦包，也没有使用 EXE 压缩壳。原生发布排除只供 CoreCLR IIS 进程内托管使用的 `aspnetcorev2_inprocess.dll`。
- 两个日志窗口统一使用 AvaloniaEdit 的 `Log.xshd` 高亮规则，保留日志等级、日期、字符串、异常、堆栈和地址高亮。移除不再使用的整套 TextMate 编程语言语法和 Onigwrap 原生依赖；字体、语言及内置插件保留。
- 库存读取复用已找到的元数据节点，避免路径参数数组、重复遍历和中间集合；仅在单次展示加载内共享相同的开发商、发行商和系统列表字符串，不使用全局字符串驻留。编辑加载继续保留原始记录。子应用列表延迟分配，拼音缓存不再强引用已移除的名称，数字搜索不为每项创建 ID 字符串。
- 静态 PNG 直接进入位图解码器，不再先复制并解析为 APNG。每个 APNG 的已解码帧缓存上限为 32 MiB，超限帧按播放顺序淘汰，单个超大帧只临时解码；回绕和跳转仍能从压缩源重新解码。压缩源、合成工作位图和 GPU 缓存不计入此上限；大动画重复播放可能增加解码 CPU 开销。帧时间查找改为无闭包的二分查找。
- 代理借用未压缩 HTML 的已写入缓冲区，避免整页复制；CONNECT 头缓冲区从池中租借并在所有退出路径清空归还，仍严格限制为 8192 字节且不多读隧道数据。只读脚本快照、域名匹配和 HTTP 客户端工厂减少重复查找及委托分配，保留现有协议与生命周期。


#### 首轮 AOT 基线验证（Windows x64，.NET 11）

| 产物 | 大小 |
| --- | --- |
| `Steam++.exe` | 95.65 MiB |
| `modules/Accelerator/Steam++.Accelerator.exe` | 26.26 MiB |
| 完整分发目录（13 个文件） | 151.70 MiB |

大小不包含运行时生成的配置、缓存和日志，也不包含独立保存的约 114.96 MiB 原生库符号。该数据不是与相同版本 JIT 分发包的体积对比。

已完成两个原生可执行文件的发布、实际桌面渲染与页面切换、加速/库存/ASF 内层页面内容、中文与英文资源，以及 31 项针对性 JIT 回归检查。原生专项验证覆盖设置读写/监听/备份、API JSON 与 MemoryPack 协议、Steam 相关协议与原生回调、账号平台定义、ASF 配置与接口数据、旧 MessagePack/LZ4 加速缓存、Windows 快捷方式/降权启动/通知、真实子进程 IPC、HTTP/HTTPS 转发和 DNS 编解码。

验证边界：未使用真实账号完成 Steam 登录或交易，也未执行 ASF 实例启动、WinDivert 驱动拦截及系统证书信任安装。本机 `127.0.0.1:53` 的 DNS 回环验证超时，原因未确定；同一原生 DNS 客户端与编解码器在临时回环端口通过。未做相同库存数据、相同缓存条件下的 JIT/AOT 启动和内存基准对照，不据此宣称整体内存下降百分比。

#### 进一步优化对照（2026-11-15，Windows x64，.NET 11）

本轮完整原生分发目录为 `build/native-aot/win-x64-compact`。对照是前一轮 AOT，不是 JIT，也不是压缩包；两者均包含主程序、7 个内置插件、加速子进程及运行所需原生库，排除运行数据、日志和符号。

| 产物 | 优化前 | 优化后 |
| --- | ---: | ---: |
| 主程序 | 95.65 MiB | 88.86 MiB |
| 加速子进程 | 26.26 MiB | 25.76 MiB |
| 完整分发目录 | 151.70 MiB / 13 文件 | 143.20 MiB / 10 文件 |

完整分发体积减少 5.60%。两个 EXE 的 PE/CLI 目录均为空，是原生可执行文件，不依赖 CoreCLR。编译器体积报告显示原包仍包含约 19.8 MiB 嵌入资源，其中默认字体约 7.9 MiB；本轮保留字体和所有语言，不把改变字体或移除插件算作无损优化。

库级对照使用独立 CoreCLR 进程、相同 AOT 构建所得的前后托管程序集，以及同一份 10,000 条游戏元数据夹具；数据含 Unicode 名称、8 个开发商、4 个发行商、同一系统列表。计时及分配排除夹具构造、预热和断言，保留数据通过探针内 GC 后测量，结果仍保持存活。这些数值不是整个 Native AOT UI 的内存占用。

| 场景 | 优化前 | 优化后 |
| --- | ---: | ---: |
| 10,000 游戏展示加载分配 | 62.68 MiB | 39.03 MiB |
| 展示数据保留托管内存 | 14.93 MiB | 13.10 MiB |
| 1,000 次静态 PNG 解码分配 | 4,614,552 B | 3,022,216 B |
| 100,000 次相同动画帧时间查询分配 | 8,001,008 B | 1,008 B |
| 48 帧、768×768 APNG 的进程私有字节增量 | 112.92 MiB | 36.14 MiB |

展示加载分配减少 37.73%，保留托管数据减少 12.26%。展示、分批加载及编辑数据往返均通过；编辑模式完整保留原始字节。大动画实际缓存位图占 31.5 MiB，单帧超过 32 MiB 的夹具缓存为零，淘汰后的跳转、回绕及像素结果通过。私有字节含原生位图和分配器影响，不等同于托管堆大小或硬上限。

本轮针对性 JIT 回归共 25 项通过，覆盖库存解析/切换、借用响应切片、CONNECT 握手及日志词法。原生日志编辑器的 13 项词法断言及实际窗口渲染通过。完整程序的首窗时间、工作集和私有字节另行测量，不用上述合成夹具推导整个程序的内存下降百分比。

三组交替启动分别使用独立目录、相同配置、空本地缓存，首窗出现后等待 10 秒取样；测量期间没有构建任务，也不强制 GC 或压缩工作集。首页网络内容和机器后台活动仍可能波动，以下中位数仅为本机观测：

| 主界面场景 | 优化前 AOT | 优化后 AOT |
| --- | ---: | ---: |
| 首个主窗口出现 | 943 ms | 922 ms |
| 空闲私有字节 | 347.39 MiB | 344.51 MiB |
| 空闲工作集 | 321.14 MiB | 320.20 MiB |
| 取样时累计 CPU 时间 | 1,750 ms | 1,844 ms |

首窗和空闲内存差异较小，CPU 时间也没有降低；不宣称主界面整体性能或空闲内存有显著改善。各次进程模块列表均未加载 CoreCLR/JIT。缓存上限、托管分配、工作集和私有字节是不同指标；不能把体积减少 5.60% 当成内存降幅。

原生加速 EXE 已通过真实命名管道启动/停止、HTTP 转发、固定上游测试证书的 HTTPS CONNECT 隧道、UTF-8 HTML/gzip 脚本注入、无注入点原样转发、脚本缓存快照和并发读取，以及二级 CONNECT 分片/合包/8192 字节边界/超限/截断/407 拒绝场景。原始包在未压缩 HTML 无结束标签时因提前释放缓冲流返回 500；借用缓冲区的版本在该路径返回完整 200 响应。

非脱水配置与原始 AOT 的三组交替场景中，监听就绪私有内存中位数为 15.23 / 15.74 MiB，场景采样峰值为 27.36 / 29.10 MiB。128 次大 HTML 脚本注入的总耗时为 173 / 230 ms，子进程 CPU 为 109 / 172 ms。最终扫描循环消除重复 Span 获取后复核为 227 ms / 156 ms CPU，仍未消除该压力场景的吞吐回退，不能声称代理整体提速或常驻内存下降。另测平衡和 Speed 编译配置没有观察到稳定优于当前配置的结果。上述反映单机合成场景，不是互联网实际下载速度基准。


本轮没有安装系统信任证书、修改 hosts 或系统代理，也没有加载 WinDivert。原生进程使用测试父进程和独立数据目录；测试父进程拒绝平台修改请求。HTTPS 校验固定上游临时证书指纹，不代表测试了系统信任安装或 TLS 中间人证书签发。本轮没有完成全部插件页面的自动点击回归；真实账号登录/交易、ASF 实例、HTTP/3、DNS 拦截及其他操作系统/架构仍需各自验证。




发布成功不代表第三方依赖的所有运行路径都支持 AOT。发布时保留裁剪/AOT 警告，验证设置迁移、全部插件页面、Steam 登录、库存、数据库、系统集成及主进程/提权进程/加速进程之间的真实 IPC。其他操作系统和架构须分别执行原生发布和运行验证。

- [.NET Native AOT 部署与限制](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [Native AOT 优化取舍](https://learn.microsoft.com/dotnet/core/deploying/native-aot/optimizing)



<!--
对子服务进程二进制程序的 RID 使用 ```RuntimeInformation.ProcessArchitecture.ToString()```  
通过 ```PublishFolderType="Assembly"``` 指定 Copy 到 MonoBundle 中，没有其他选项能够将其放在 MacOS 文件夹中

改成 Windows 的目录结构，但在 AOT 的 AppHost 中无法加载已使用框架依赖发布的程序集，错误 Initialization for self-contained components is not supported
参考  
https://github.com/dotnet/runtime/issues/35329  
https://github.com/dotnet/runtime/search?l=C%2B%2B&q=get_is_framework_dependent  
https://github.com/dotnet/runtime/blob/6702dc5c7814e624a42ab4615224920a5635beeb/src/native/corehost/runtime_config.cpp  
https://github.com/dotnet/runtime/blob/main/docs/design/features/host-error-codes.md  
https://github.com/dotnet/samples/blob/91355ef22a10ec614a2e8daefd68785066860d57/core/hosting/src/NativeHost/nativehost.cpp  
-->

<!--
### 安装 ASP.NET Core 运行时 & .NET 运行时
- Windows 
    - [使用 Windows 包管理器 (winget) 进行安装](https://learn.microsoft.com/zh-cn/dotnet/core/install/windows?tabs=net70#install-with-windows-installer)  
    ```winget install Microsoft.DotNet.AspNetCore.7```
- Linux
    - [使用安装脚本或通过提取二进制文件在 Linux 上安装 .NET](https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-scripted-manual)  
    ```wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh```  
    ```sudo chmod +x ./dotnet-install.sh```  
    ```./dotnet-install.sh --version latest --runtime aspnetcore```
    - [排查与 Linux 上缺少文件相关的 .NET 错误](https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-package-mixup?pivots=os-linux-redhat)
- macOS
    - [使用 Bash 自动化安装](https://learn.microsoft.com/zh-cn/dotnet/core/install/macos#install-with-bash-automation)  
    ```./dotnet-install.sh --channel 7.0 --runtime aspnetcore```
-->

### 自定义 .NET Host 文档
- [编写自定义 .NET 主机以从本机代码控制 .NET 运行时](https://learn.microsoft.com/zh-cn/dotnet/core/tutorials/netcore-hosting)  
- [.NET 分发打包](https://learn.microsoft.com/zh-cn/dotnet/core/distribution-packaging)  
- [环境变量 - 指定 .NET 运行时的位置](https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-environment-variables#dotnet_root-dotnet_rootx86)  