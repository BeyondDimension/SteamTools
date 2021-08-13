### 新增内容
1. CLR 升级至 .NET 6 Preview 7
2. 新增 库存游戏可浏览已安装的游戏文件夹
3. 新增 创意工坊划词翻译脚本
4. 新增 通用设置-使用硬件加速设置项，也可通过命令行参数禁用硬件加速启动 -clt devtools -disable_gpu
5. 新增 账号切换内新增家庭库共享排序功能，该功能可调整当前设备的多个家庭共享账号库存的优先级顺序。
6. 新增 QQ 快速登录渠道
7. 新增 意大利语支持
8. 新增 西班牙语支持
9. 新增 通用设置内可支持访问程序的 AppData、Cache、Logs 文件夹和查看缓存及日志占用空间大小情况
10. 新增 本地令牌可以查看令牌二维码导入到手机端
11. Android/macOS/Linux 版现已开启 Beta/Alpha 测试，可从 GitHub/Gitee 上下载
12. 现已通过 Desktop Bridge 转制为 UWP 上架 Microsoft Store 

### 修复问题
1. 修复 开机自启动在 2.4.x 中不生效
2. 改进 深色和浅色模式的视觉效果
3. 改进 社区加速计时逻辑
4. 更正 导出令牌窗口标题文本错误
5. 改进 添加令牌弹窗可调整窗口大小
6. 改进 本地令牌自定义修改名称操作
7. 修复 我的面板中出现垂直滚动条
8. 改进 解锁成就风险提示弹窗可以设置不再提示
9. 修复 任务栏位于顶部时托盘菜单位置不正确
10. 修复 挂时长运行中列表移除游戏时引发的闪退
11. 改进 自动更新机制
12. 改进 开机自启动现仅对当前用户生效
13. 改进 第三方账号快速登录，现使用系统浏览器进行快速登录
14. 改进 现已编译为 ReadyToRun (R2R) 格式，这将改进应用程序的启动时间和延迟（仅 Windows 版）
15. 修复 库存游戏中部分游戏被错误屏蔽没有加载
16. 改进 部分语言的翻译文本
17. 改进 本地令牌上移下移功能顺序错乱问题
18. 修复 程序在加载库存游戏时有概率闪退的问题
19. 修复 打开托盘菜单有概率导致程序闪退的问题
20. 修复 程序启动时界面不会加载任何内容无法正常使用的问题
21. 改进 账号切换功能UI
22. 改进 移除了CEF模块，缩减程序体积和运行时内存占用
23. 改进 库存游戏支持显示已安装游戏占用空间大小
24. 改进 库存游戏封面大小调节可支持滚动调节
25. 改进 hosts 文件编码在 Windows 上使用系统的活动代码页(ANSICodePage)，例如 GB2312/936，其他操作系统则使用 UTF-8，还原 V1 版本行为
***

<!-- 1. 新增 ASF Plus 本地挂卡
3. 改进 新增守护进程，当程序闪退时将自动重启 -->

|  RuntimeIdentifier  |  Available  |  Edition  |
|  ----  |  ----  |  ----  |
| win-x64  | ✅ | Stable |
| win-x64(DesktopBridge/msix)  | ✅ | β |
| osx-x64  | ✅ | β |
| linux-x64  | ✅ | α |
| android-arm64  | ✅ | α |
| android-arm  | ✅ | α |
| win-arm64  | ❌ | |
| osx-arm64  | ❌ | |
| linux-arm64  | ❌ | |
| ios-arm64  | ❌ |  |

|  File  | Checksum (SHA256)  |
|  ----  |  ----  |
| Steam++_win_x64_v2.4.9.7z  | SHA256 |
| Steam++_win_x64_v2.4.9.exe  | SHA256 |
| Steam++_win_x64_v2.4.9.msix  | SHA256 |
| | |
| Steam++_win_arm64_v2.4.9.7z  | SHA256 |
| Steam++_win_arm64_v2.4.9.exe  | SHA256 |
| Steam++_win_arm64_v2.4.9.msix  | SHA256 |
| | |
| Steam++_linux_x64_v2.4.9.7z  | SHA256 |
| Steam++_linux_x64_v2.4.9.deb  | SHA256 |
| | |
| Steam++_macos_x64_v2.4.9.dmg  | SHA256 |
| Steam++_macos_arm64_v2.4.9.dmg  | SHA256 |
| | |
| Steam++_android_arm64_v8a_v2.4.9.apk  | SHA256 |
| Steam++_android_armeabi_v7a_v2.4.9.apk  | SHA256 |

<!-- ***

由于程序体积较大，推荐从 [官网 https://steampp.net](https://steampp.net) 中下载 -->
