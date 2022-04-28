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
#if WINDOWS
            builder.UseWin32();
#else
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
                    builder.UseWin32();
                    break;
                case OperatingSystemType.OSX:
                    builder.UseAvaloniaNative();
                    break;
                default:
                    builder.UseX11();
                    break;
            }
#endif
            builder.UseSkia();
            return builder;
        }
    }
}