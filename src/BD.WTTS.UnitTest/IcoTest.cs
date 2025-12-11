using DynamicData;
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

    [Test]
    public void CreateShortcutIcon()
    {
        var logo512Path = Path.Combine(ProjectUtils.ProjPath, "res", "icons", "app", "v3", "Logo_512.png");
        var gameicoPath = Path.Combine(ProjectUtils.ProjPath, "res", "avatars", "gameicos", "origin.png");
        var imgDirInfo = new DirectoryInfo(Path.Combine(ProjectUtils.ProjPath, "res", "avatars"));

        using var logo512Bitmap = SKBitmap.Decode(logo512Path);
        using var gameBitmap = SKBitmap.Decode(gameicoPath);

        var gear = new[] { 512, 256, 128, 96, 64, 48, 32, 24, 16 };
        var saveDirPath = Path.Combine(assemblyDir, "avatars");
        IOPath.DirCreateByNotExists(saveDirPath);

        foreach (var img in imgDirInfo.GetFiles())
        {
            SKBitmap[]? bitmaps = null;
            using var imgBitmap = SKBitmap.Decode(img.FullName);

            var allSize = GetImgResolutionPower(imgBitmap.Width, gear);
            var maxSize = allSize.Max();
            using var fBitmap = ResizeImage(imgBitmap, gameBitmap, maxSize);
            bitmaps = new[] { fBitmap }.Concat(allSize.Where(x => x != maxSize)
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

            if (bitmaps != null)
                bitmaps.ForEach(x => x.Dispose());
        }
    }

    static SKBitmap ResizeImage(SKBitmap image, SKBitmap icoIcon, int size)
    {
        int icoWidth = size / 4;
        int icoHeight = size / 4;

        int watermarkLeft = size - icoWidth;
        int watermarkTop = size - icoHeight;

        SKBitmap scaledImage = new(size, size);
        image.ScalePixels(scaledImage, SKFilterQuality.High);

        using SKCanvas canvas = new(scaledImage);
        canvas.DrawBitmap(icoIcon, new SKRect(watermarkLeft, watermarkTop, watermarkLeft + icoWidth, watermarkTop + icoHeight));
        return scaledImage;
    }

    static IEnumerable<int> GetImgResolutionPower(int size, int[] rp)
    {
        foreach (var item in rp)
        {
            if (item <= size) yield return item;
        }
    }

}
