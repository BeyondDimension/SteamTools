using SkiaSharp;

namespace BD.WTTS.UnitTest;

/// <summary>
/// Windows ICO 格式测试
/// </summary>
public sealed class IcoTest
{
    /// <summary>
    /// Windows ICO 格式编码测试
    /// </summary>
    [Test]
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

            var savePath = $"{logo512Path}.ico";
            try
            {
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
                // 删除生成的文件，清理
                IOPath.FileTryDelete(savePath);
            }
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

}
