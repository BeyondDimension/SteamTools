extern alias AvaloniaSkia;

using Avalonia.Platform;
using SkiaOptions = AvaloniaSkia::Avalonia.SkiaOptions;

// https://github.com/AvaloniaUI/Avalonia/blob/0.10.13/src/Skia/Avalonia.Skia/SkiaPlatform.cs#L18-L27

namespace Avalonia.Skia
{
    public static class SkiaPlatform2
    {
        public static void Initialize(SkiaOptions options)
        {
            var customGpu = options.CustomGpuFactory?.Invoke();
            var renderInterface = new PlatformRenderInterface(customGpu, options.MaxGpuResourceSizeBytes);

            AvaloniaLocator.CurrentMutable
                .Bind<IPlatformRenderInterface>().ToConstant(renderInterface);
        }
    }
}
