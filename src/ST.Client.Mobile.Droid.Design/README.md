# Android Studio Project
- app 
    - AS 启动项目
- ui(Link Ref)
    - Android UI XML 等 res
    - 通过 ST.Tools.AndroidResourceLink 工具将文件 Link 进 csproj 项目文件中
    - [LayoutCodeBehind](https://github.com/xamarin/xamarin-android/blob/main/Documentation/guides/LayoutCodeBehind.md) 类似 [viewBinding](https://developer.android.google.cn/topic/libraries/view-binding?hl=zh-cn) 更适合在 C# 中使用
        - LayoutCodeBehind 有一个关于 AndroidX 的 bug 会导致编译失败
        - 需将修复版的 [LayoutBinding.cs](../LayoutBinding.cs) 替换到 VS 内置的 Xamarin.Android 安装目录上，且每次升级后需要重新替换一次
        - 目录通常为 ```C:\Program Files\Microsoft Visual Studio\2022\{ProductType}\MSBuild\Xamarin\Android``` 可在错误列表中找到目录
- v2ray(AAR Binding Ref)
    - 根据 v2rayNG 修改实现的 VPN 代理服务
- shadowsocks(AAR Binding Ref)
    - 根据 shadowsocks-android 修改实现的 VPN 代理服务

## C# - Java/Kotlin 互操作指南
- Java/Kotlin（供 C# 使用）
    - [Android 可调用包装器 (ACW)](https://docs.microsoft.com/zh-cn/xamarin/android/platform/java-integration/android-callable-wrappers)
    - [绑定 Java 库](https://docs.microsoft.com/zh-cn/xamarin/android/platform/binding-java-library/)
    - [绑定 Kotlin 库](https://docs.microsoft.com/zh-cn/xamarin/android/platform/binding-kotlin-library/)
- C#（供 Java/Kotlin 使用）
    - OOP 在 Java/Kotlin 的类上设置 虚方法/抽象方法，由 C# 中派生类实现
    - 单例模式，在 Java/Kotlin 建一个接口与静态方法 Set 与 Get 该接口实例，在 C# 中实现一个继承自该接口的类在启动时 Set

## [Kotlin-Java 互操作指南](https://developer.android.google.cn/kotlin/interop?hl=zh-cn)

## 编译指南
1. 在 Android Studio 中打开项目
2. Gradle 窗口中生成 aar 包
3. 将 aar 包放入对应的 Xamarin.Android Binding 项目中生成

## [Xamarin.Android Project](../ST.Client.Android/README.md)