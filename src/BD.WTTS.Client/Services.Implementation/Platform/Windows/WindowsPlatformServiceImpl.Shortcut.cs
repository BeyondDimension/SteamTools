#if WINDOWS
using IWshRuntimeLibrary;
using SkiaSharp;
using Res = BD.WTTS.Properties.Resources;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    /// <summary>
    /// 创建快捷方式
    /// </summary>
    /// <param name="pathLink">快捷方式的保存路径</param>
    /// <param name="targetPath">快捷方式的目标路径</param>
    /// <param name="iconSavePath">快捷方式图标的保存路径</param>
    /// <param name="accountImage">头像</param>
    /// <param name="arguments">参数</param>
    /// <param name="description">快捷键描述</param>
    /// <param name="hotkey">设置快捷键</param>
    /// <param name="workingDirectory">应用程序工作目录</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateShortcut(
        string pathLink,
        string targetPath,
        string iconSavePath,
        byte[] accountImage,
        string? arguments = null,
        string? description = null,
        string? hotkey = null,
        string? workingDirectory = null)
    {
        List<SKBitmap> bitmaps = new List<SKBitmap>();
        try
        {
            int[] resolutionSize = new int[] { 16, 24, 32, 48, 64, 96, 128, 256, 512 };
            using var imgBitmap = SKBitmap.Decode(accountImage);

            foreach (var iconSource in resolutionSize)
            {
                using var appLogo = SKBitmap.Decode((byte[])Res.ResourceManager.GetObject($"AppLogo_{iconSource}")!);
                bitmaps.Add(ResizeImage(imgBitmap, appLogo));
            }

            using var icofs = new FileStream(iconSavePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
            IcoEncoder.Encode(icofs, bitmaps);
            Shortcut(pathLink, targetPath, arguments, description, hotkey, iconSavePath, workingDirectory);
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
        }
        finally
        {
            if (bitmaps.Count > 0)
            {
                foreach (var bit in bitmaps)
                {
                    bit.Dispose();
                }
            }
        }
    }

    void IPlatformService.CreateShortcut(
        string pathLink,
        string targetPath,
        string iconSavePath,
        byte[] accountImage,
        string? arguments,
        string? description,
        string? hotkey,
        string? workingDirectory)
    {
        CreateShortcut(pathLink, targetPath, iconSavePath, accountImage, arguments, description, hotkey, workingDirectory);
    }

    static SKBitmap ResizeImage(SKBitmap image, SKBitmap icoIcon)
    {
        int icoWidth = icoIcon.Width / 2;
        int icoHeight = icoIcon.Height / 2;
        int watermarkLeft = icoIcon.Width - icoWidth;
        int watermarkTop = icoIcon.Height - icoHeight;

        SKBitmap scaledImage = new SKBitmap(icoIcon.Width, icoIcon.Height);
        image.ScalePixels(scaledImage, SKFilterQuality.High);

        using (SKCanvas canvas = new SKCanvas(scaledImage))
        {
            using SKPaint paint = new SKPaint();
            paint.Color = paint.Color.WithAlpha(130);
            canvas.DrawBitmap(icoIcon, new SKRect(watermarkLeft, watermarkTop, watermarkLeft + icoWidth, watermarkTop + icoHeight), paint);
        }
        return scaledImage;
    }

    static void Shortcut(string pathLink, string targetPath, string? arguments, string? description, string? hotkey, string? iconLocation, string? workingDirectory)
    {
        WshShell shell = new();
        var shortcut = (IWshShortcut)shell.CreateShortcut(pathLink);
        shortcut.TargetPath = targetPath;

        if (!string.IsNullOrEmpty(description))
            shortcut.Description = description;

        if (!string.IsNullOrEmpty(arguments))
            shortcut.Arguments = arguments;

        if (!string.IsNullOrEmpty(hotkey))
            shortcut.Hotkey = hotkey;

        if (!string.IsNullOrEmpty(iconLocation))
            shortcut.IconLocation = iconLocation;

        if (!string.IsNullOrEmpty(workingDirectory))
            shortcut.WorkingDirectory = workingDirectory;

        shortcut.Save();
    }
}
#endif