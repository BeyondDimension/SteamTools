1. 修复 开机自启动在 2.4.x 中不生效
2. 改进 深色和浅色模式的视觉效果
3. CLR 升级至 .NET 6 Preview 6
4. 新增 库存游戏可浏览已安装的游戏文件夹
5. 改进 社区加速计时逻辑
6. 更正 导出令牌窗口标题文本错误
7. 改进 添加令牌弹窗可调整窗口大小
8. 改进 本地令牌自定义修改名称操作
9. 修复 我的面板中出现垂直滚动条
10. 改进 解锁成就风险提示弹窗新增不再提示复选框
11. 修复 任务栏位于顶部时托盘菜单位置不正确
12. 新增 创意工坊划词翻译脚本
13. 修复 挂时长运行中列表移除游戏时引发的闪退
14. 新增 使用硬件加速设置项，也可通过命令行参数禁用硬件加速启动 -clt devtools -disable_gpu
15. 新增 家庭库共享排序
16. 改进 自动更新机制
17. 改进 开机自启动现仅对当前用户生效
18. 改进 程序根目录结构，相较于上一个版本移除了 Bin 文件夹，减少 dll 文件数量
19. 改进 优化了发行版本压缩包体积（移除了 CEF 模块，对于需要使用第三方账号快速登录功能可使用上一个版本的压缩包中 \Bin\CEF 将这个文件夹拷贝到 \CEF 中）
20. Android/macOS/Linux 版现已开启 Beta/Alpha 测试，可从 GitHub/Gitee 上下载

***

1. 新增 QQ 快速登录渠道
2. 新增 ASF Plus 本地挂卡
3. 修复 本地令牌上移下移功能不正确
4. 改进 部分语言的翻译文本
5. 新增 意大利语支持
6. 新增 西班牙语支持
7. 改进 第三方账号快速登录，现使用系统浏览器进行快速登录，不再依赖 CEF 模块
8. 改进 新增守护进程，当程序闪退时将自动重启

|  RuntimeIdentifier  |  Available  |  Edition  |
|  ----  |  ----  |  ----  |
| win-x64  | ✅ | |
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

***

由于程序体积较大，推荐从 [官网 https://steampp.net](https://steampp.net) 中下载
