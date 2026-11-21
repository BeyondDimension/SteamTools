# Steam++ v3.X Source Code

### Native AOT 界面约定

- 页面与弹窗使用编译绑定；内置导航直接引用页面类型或工厂，不从 `Tag`、类型名或反射动作创建页面。
- 运行、加载和空状态应互斥。异步按钮在执行期间禁用；长表单在有界视口内滚动，保留确认与返回入口。
- Native AOT 页面卸载时，停用回调延迟到视觉树遍历结束后执行；若视图模型已释放，不再处置旧的激活句柄。
- XAML 设计器只预览界面。下载页通过运行时视图模型访问 Steam，不要在 XAML 静态绑定里直接创建 Steam 服务。
- 修改这些交互后，在仓库根目录运行 `pwsh -File tests/native-aot/run-import-and-asf.ps1`。脚本发布独立的 `win-x64` 测试包，使用包内数据，不读写已安装版本的应用数据。

### 🏗️ 项目结构
TODO
<!-- TODO
- Common **通用基础类库**
    - Common.AreaLib **地区数据**
    - Common.ClientLib **适用于客户端的通用基础类库**
    - Common.ClientLib.Droid **适用于 Android 的通用基础类库**
    - Common.ClientLib.iOS **适用于 iOS 的通用基础类库**
    - Common.CoreLib **全局通用基础类库**
    - Common.ServerLib **适用于服务端(AspNetCore)的通用基础类库**
    - Common.PinyinLib **汉字转拼音库**
    - Common.PinyinLib.CFStringTransform **仅适用于 iOS 平台，由 [CFStringTransform](https://developer.apple.com/documentation/corefoundation/1542411-cfstringtransform) 实现**
    - Common.PinyinLib.ChnCharInfo **由 Microsoft Visual Studio International Pack 1.0 中的 Simplified Chinese Pin-Yin Conversion Library（简体中文拼音转换类库）实现，多音字将返回首个拼音**
    - Common.PinyinLib.TinyPinyin **在 Android 上由 [TinyPinyin](https://github.com/promeG/TinyPinyin) 实现，其他平台由 [TinyPinyin.Net](https://github.com/hueifeng/TinyPinyin.Net) 实现**
    - Repositories.EFCore **EF Core 仓储层实现**
    - Repositories.sqlite-net-pcl **SQLite 仓储层实现**
    - Services.SmsSender **统一短信发送服务**
- Test **单元测试**
    - Common.UnitTest **通用基础类库的单元测试**
    - Common.UnitTest.Droid **适用于 Android 通用基础类库的单元测试**
        - Common.UnitTest.Droid.App **启动项**
- Lib **类库**
    - ST **业务通用类库**
    - ST.Client **客户端通用类库**
    - Bindings **平台原生绑定库**
    - Platforms
        - ST.Client.Windows **用于 Windows 的实现**
        - ST.Client.Mac **用于 macOS 的实现**
        - ST.Client.Linux **用于 GNU/Linux 的实现**
        - ST.Client.Android **用于 Android 的实现**
        - ST.Client.iOS **用于 iOS 的实现**
    - ResSecrets **使用资源存储的密钥**
    - UI Framework
        - ST.Client.Avalonia **使用 Avalonia 实现的 View 层**            
            - Avalonia.Ref **通过友元程序集调用内部函数或空程序集实现手动裁剪**
        - ~~ST.Client.WPF~~ **使用 Avalonia 实现的 WPF 层**
        - ~~ST.Client.WinUI~~ **使用 Avalonia 实现的 WinUI 层**
        - ST.Client.XamarinForms **使用 Xamarin.Forms/MAUI 实现的 View 层**
    - Web API
        - ST.Services.CloudService **客户端调用服务端 API 定义**
        - ST.Services.CloudService.Models **服务端 API 数据传输对象(DTO)定义**
        - ST.Services.CloudService.ViewModels **客户端视图模型**
- Tool **工具**
    - ST.Tools.AndroidResourceLink **将 Android Studio Project 中的 res 资源 Link 到 csproj 中(生成 XML)**
    - ST.Tools.AreaImport **从高德城市编码表 Excel 文件中导入地区数据**
    - ~~ST.Tools.DesktopBridgeLink~~ **Link DesktopBridge 打包中的内容，例如 CEF**
    - ~~ST.Tools.MinifyStaticSites~~ **用于将静态 html 删除空行缩小体积的命令行工具**
    - ST.Tools.OpenSourceLibraryList **开源许可协议清单生成工具**
        - 需要 [GitHub API Token](https://docs.github.com/en/github/authenticating-to-github/creating-a-personal-access-token)
    - ST.Tools.Packager **带进度的压缩与解压演示**
    - ~~ST.Tools.Packager.InstallerSetup~~ **安装程序**
    - ST.Tools.Publish **用于发布的控制台工具**
    - ST.Tools.Translate **Resx自动翻译工具**
        - 需要 [Azure Translation Key](https://azure.microsoft.com/zh-cn/services/cognitive-services/translator)
    - ~~ST.Tools.Win7Troubleshoot~~ **适用于 Windwos 7 OS 的 疑难解答助手**
        - 目标框架使用 .NET FX 3.5 并通过 App.config 配置 [supportedRuntime](https://docs.microsoft.com/zh-cn/dotnet/framework/configure-apps/file-schema/startup/supportedruntime-element) 允许在 4.X 中运行 实现在 Windows 上兼容所有的运行库环境
- Launch **启动项**
    - FDELauncher FDE(框架依赖) 启动器，判断运行时是否安装与提示，使用 .NET FX 3.5
    - ST.Client.Android.App **Android 客户端(Xamarin.Android)**
    - ST.Client.Android.App.Modern **Android 客户端(.NET 6+)**
    - ST.Client.Desktop.Avalonia.App **桌面客户端**
    - 5_DesktopBridge\ST.Client.Avalonia.App.Bridge.Package **[Desktop Bridge](https://docs.microsoft.com/zh-cn/windows/msix/desktop/desktop-to-uwp-packaging-dot-net)**
    - ST.Client.Avalonia.App.MsixPackage **桌面客户端[单项目 MSIX 打包](https://docs.microsoft.com/zh-cn/windows/apps/windows-app-sdk/single-project-msix?tabs=csharp)**
    - ~~ST.Client.Desktop.Avalonia.Demo.App~~ **桌面客户端(UI演示)**
    - ST.Client.Maui.App **MAUI 客户端**

### 🗂️ 命名空间/文件夹
- ~~中划线~~ 表示此文件夹下的命名空间使用上一级的值
- Properties
    - AssemblyInfo.cs **程序集信息**
    - InternalsVisibleTo.cs **指定 internal 对单元测试可见**
    - SR **本地化资源**
- ~~Extensions~~ **扩展函数静态类**
- Application **业务应用**
    - Columns **模型、实体列定义接口**
    - Converters **视图模型(VM)值绑定到视图(V)中的值转换器**
    - Data **EFCore DbContext**
    - Entities **ORM 表实体**
    - Filters **AspNetCore Mvc Filters**
    - Models **模型类**
    - Mvvm **MVVM 基础组件**
    - Repositories **仓储层**
    - UI
        - Assets **资源资产**
        - Styles **Xaml 样式**
        - Activities **Android 活动**
        - Adapters **Android 适配器**
        - Fragments **Android 片段**
        - ViewModels **视图模型**
        - Views **视图**
            - Controls **自定义控件**
            - Pages **页面**
            - Windows **窗口**
        - Resx **本地化资源**
    - Windows.winmd **Windows 10 UWP API 投影 Win32**
    - Resources **Android res、iOS BundleResource、其他嵌入的资源**
    - Security **应用安全**
    - Services **业务服务定义公开的接口或抽象类**
        - ~~Mvvm~~ **用于 MVVM 绑定的业务服务**
        - Implementation **业务服务的实现**
    - Serialization **业务相关的序列化、反序列化**
- Logging **日志自定义实现**
- ServiceCollectionExtensions.cs **DI 注册服务扩展类，命名空间统一使用**  
<pre>
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
</pre>
-->