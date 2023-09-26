# Steam++ v3.X Source Code

### 🏗️ 项目结构
- Launch Project
    - Desktop
        - BD.WTTS.Client.Avalonia.App
    - Android
        - BD.WTTS.Client.Avalonia.App.Android
    - iOS
        - BD.WTTS.Client.Avalonia.App.iOS
- AppHost
    - Windows AppHost(.NET Framework/.NET X)
        - BD.WTTS.Client.AppHost
    - Windows Application Packaging Project(Deprecated, use Publish Tool)
        - BD.WTTS.Client.AppHost.Bridge(Steam++)
        - BD.WTTS.Client.AppHost.Bridge.Package
- Reference Library
    - APNG/GIF Support
        - Avalonia.Gif
        - LibAPNG
    - Steam Library
        - BD.SteamClient
        - Facepunch.Steamworks
        - Gameloop.Vdf
        - SAM.API
        - Steam4NET
        - ValveKeyValue
    - Authenticator
        - WinAuth
- Shared Library
    - Avalonia(Internal Function Call/Trimmable Assembly)
        - Avalonia.Base
        - Avalonia.Base.Internals
        - Avalonia.Controls.Internals
        - Avalonia.Desktop
        - Avalonia.Diagnostics
        - Avalonia.Native
        - Avalonia.Skia.Internals
        - Avalonia.WebView2
        - Avalonia.Win32
        - Avalonia.X11
    - Watt Toolkit WebApi Client SDK
        - BD.WTTS.MicroServices.*
        - BD.WTTS.Primitives.*
    - Watt Toolkit Client
        - BD.WTTS.Client
        - BD.WTTS.Client.Avalonia
        - BD.WTTS.Client.IPC
- Plugin Library
    - BD.WTTS.Client.Plugins.*
- Program Tool
    - Settings SourceGenerator
        - BD.Common.Settings.V4.SourceGenerator.Tools
    - Avalonia Designer Host
        - BD.WTTS.Client.Avalonia.Designer.HostApp
    - Console Tool
        - BD.WTTS.Client.Tools.*    

<!-- TODO

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