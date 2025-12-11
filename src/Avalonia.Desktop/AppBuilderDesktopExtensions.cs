//// https://github.com/AvaloniaUI/Avalonia/blob/0.10.13/src/Avalonia.Desktop/AppBuilderDesktopExtensions.cs

//namespace Avalonia;

//public static class AppBuilderDesktopExtensions
//{
//    public static AppBuilder UsePlatformDetect2(this AppBuilder builder)
//    {
//#if WINDOWS
//        builder.UseWin32();
//#elif MACCATALYST || MACOS
//        builder.UseAvaloniaNative();
//#elif LINUX
//        builder.UseX11();
//#else
//        throw new NotSupportedException();
//#endif
//        builder.UseSkia();
//        return builder;
//    }
//}