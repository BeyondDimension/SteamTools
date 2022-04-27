using Avalonia.Controls;
using Avalonia.Platform;

// https://github.com/AvaloniaUI/Avalonia/blob/0.10.13/src/Avalonia.Desktop/AppBuilderDesktopExtensions.cs

namespace Avalonia
{
    public static class AppBuilderDesktopExtensions
    {
        public static TAppBuilder UsePlatformDetect<TAppBuilder>(this TAppBuilder builder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
        {
            var os = builder.RuntimePlatform.GetRuntimeInfo().OperatingSystem;

            // We don't have the ability to load every assembly right now, so we are
            // stuck with manual configuration  here
            // Helpers are extracted to separate methods to take the advantage of the fact
            // that CLR doesn't try to load dependencies before referencing method is jitted
            // Additionally, by having a hard reference to each assembly,
            // we verify that the assemblies are in the final .deps.json file
            //  so .NET Core knows where to load the assemblies from,.
            switch (os)
            {
                case OperatingSystemType.WinNT:
                    LoadWin32(builder);
                    break;
                case OperatingSystemType.OSX:
                    LoadAvaloniaNative(builder);
                    break;
                default:
                    LoadX11(builder);
                    break;
            }
            LoadSkia(builder);
            return builder;
        }

        static void LoadAvaloniaNative<TAppBuilder>(TAppBuilder builder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
             => builder.UseAvaloniaNative();

        static void LoadWin32<TAppBuilder>(TAppBuilder builder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
             => builder.UseWin32();

        static void LoadX11<TAppBuilder>(TAppBuilder builder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
             => builder.UseX11();

        static void LoadSkia<TAppBuilder>(TAppBuilder builder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new()
             => builder.UseSkia();
    }
}