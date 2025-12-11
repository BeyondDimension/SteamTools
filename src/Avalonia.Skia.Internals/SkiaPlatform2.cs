//extern alias AvaloniaSkia;

//using Avalonia.Platform;
//using SkiaOptions = AvaloniaSkia::Avalonia.SkiaOptions;

//// https://github.com/AvaloniaUI/Avalonia/blob/0.10.13/src/Skia/Avalonia.Skia/SkiaPlatform.cs#L18-L27

//namespace Avalonia.Skia;

//public static class SkiaPlatform2
//{
//    public static readonly IPlatformRenderInterface PlatformRenderInterface = GetPlatformRenderInterface();

//    static IPlatformRenderInterface GetPlatformRenderInterface()
//    {
//        var options = AvaloniaLocator.Current.GetService<SkiaOptions>() ?? new();
//        var renderInterface = new PlatformRenderInterface(options.MaxGpuResourceSizeBytes);
//        return renderInterface;
//    }
//}
