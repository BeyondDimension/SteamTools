using SkiaSharp;

namespace BD.WTTS.UnitTest;

/// <summary>
/// Windows ICO 格式测试
/// </summary>
public sealed class IcoTest
{
    static readonly string assemblyDir;

    static IcoTest()
    {
        var assemblyLocation = typeof(IcoTest).Assembly.Location;
        assemblyLocation.ThrowIsNull();
        assemblyDir = Path.GetDirectoryName(assemblyLocation)!;
        assemblyDir.ThrowIsNull();
    }

    /// <summary>
    /// Windows ICO 格式编码测试
    /// </summary>
    //[Test]
    public void EncodeTest()
    {
        var logo512Path = Path.Combine(ProjectUtils.ProjPath, "res", "icons", "app", "v3", "Logo_512.png");
        SKBitmap[]? logoBitmaps = null;
        try
        {
            using var logo512Bitmap = SKBitmap.Decode(logo512Path);

            logoBitmaps = new[] { logo512Bitmap }.Concat(new[] { 256, 128, 96, 64, 48, 32, 24, 16 }
                .Select(x => logo512Bitmap.Resize(new SKSizeI { Height = x, Width = x }, SKFilterQuality.High)))
                .ToArray(); // 根据 512 大小生成所有挡位的位图数组

            var savePath = Path.Combine(assemblyDir, "EncodeTest.ico");
            // 创建 ico 文件内存流
            using var fs = new FileStream(
                savePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.ReadWrite | FileShare.Delete);
            IcoEncoder.Encode(fs, logoBitmaps);
            TestContext.WriteLine(savePath);
        }
        finally
        {
            if (logoBitmaps != null)
            {
                foreach (var logoBitmap in logoBitmaps)
                {
                    logoBitmap.Dispose();
                }
            }
        }
    }

    //static IEnumerable<int> G(int size, int[] dw)
    //{
    //    foreach (var item in dw)
    //    {
    //        if (item <= size) yield return item;
    //    }
    //}

    [Test]
    public void CreateShortcutIcon()
    {
        var logo512Path = Path.Combine(ProjectUtils.ProjPath, "res", "icons", "app", "v3", "Logo_512.png");
        var imgDirInfo = new DirectoryInfo(Path.Combine(ProjectUtils.ProjPath, "res", "avatars"));
        using var logo512Bitmap = SKBitmap.Decode(logo512Path);
        var saveDirPath = Path.Combine(assemblyDir, "avatars");
        IOPath.DirCreateByNotExists(saveDirPath);
        foreach (var img in imgDirInfo.GetFiles())
        {
            SKBitmap[]? bitmaps = null;
            using var imgBitmap = SKBitmap.Decode(img.FullName);

            using var fBitmap = ResizeImage(imgBitmap, logo512Bitmap);

            bitmaps = new[] { fBitmap }.Concat(new[] { 256, 128, 96, 64, 48, 32, 24, 16 }
                .Select(x => fBitmap.Resize(new SKSizeI { Height = x, Width = x }, SKFilterQuality.High)))
                .ToArray();

            var savePath = Path.Combine(saveDirPath, $"{img.Name}.ico");
            // 创建 ico 文件内存流
            using var fs = new FileStream(
                savePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.ReadWrite | FileShare.Delete);
            IcoEncoder.Encode(fs, bitmaps);
            TestContext.WriteLine(savePath);
        }
    }

    static SKBitmap ResizeImage(SKBitmap image, SKBitmap icoIcon)
    {
        int icoWidth = icoIcon.Width / 4;
        int icoHeight = icoIcon.Height / 4;
        int watermarkLeft = icoIcon.Width - icoWidth;
        int watermarkTop = icoIcon.Height - icoHeight;

        SKBitmap scaledImage = new(icoIcon.Width, icoIcon.Height);
        image.ScalePixels(scaledImage, SKFilterQuality.High);

        using SKCanvas canvas = new(scaledImage);
        using SKPaint paint = new();
        paint.Color = paint.Color.WithAlpha(10);
        canvas.DrawBitmap(icoIcon, new SKRect(watermarkLeft, watermarkTop, watermarkLeft + icoWidth, watermarkTop + icoHeight), paint);
        return scaledImage;
    }
}
