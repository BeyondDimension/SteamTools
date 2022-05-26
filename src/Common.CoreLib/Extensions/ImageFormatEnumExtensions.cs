using System.IO.FileFormats;
using static System.IO.FileFormats.Internals.ImageFileFormat;

namespace System;

/// <summary>
/// Enum 扩展 <see cref="ImageFormat"/>
/// </summary>
public static partial class ImageFormatEnumExtensions
{
    /// <summary>
    /// 获取扩展名
    /// </summary>
    /// <param name="imageFormat"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string GetExtension(this ImageFormat imageFormat)
    {
        return imageFormat switch
        {
            ImageFormat.BMP => BMP.DefaultFileExtension,
            ImageFormat.GIF => GIF.DefaultFileExtension,
            ImageFormat.ICO => ICO.DefaultFileExtension,
            ImageFormat.JPEG => JPG.DefaultFileExtension,
            ImageFormat.PNG => PNG.DefaultFileExtension,
            ImageFormat.WebP => WebP.DefaultFileExtension,
            ImageFormat.HEIF => HEIF_HEIC.HEIF.DefaultFileExtension,
            ImageFormat.HEIFSequence => HEIF_HEIC.HEIF.DefaultFileExtension,
            ImageFormat.HEIC => HEIF_HEIC.HEIC.DefaultFileExtension,
            ImageFormat.HEICSequence => HEIF_HEIC.HEIC.DefaultFileExtension,
            //ImageFormat.APNG => APNG.DefaultFileExtension,
            _ => throw new ArgumentOutOfRangeException(nameof(imageFormat), imageFormat, null),
        };
    }

    /// <summary>
    /// 获取MIME
    /// </summary>
    /// <param name="imageFormat"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string GetMIME(this ImageFormat imageFormat)
    {
        return imageFormat switch
        {
            ImageFormat.BMP => BMP.DefaultMIME,
            ImageFormat.GIF => GIF.DefaultMIME,
            ImageFormat.ICO => ICO.DefaultMIME,
            ImageFormat.JPEG => JPG.DefaultMIME,
            ImageFormat.PNG => PNG.DefaultMIME,
            ImageFormat.WebP => WebP.DefaultMIME,
            ImageFormat.HEIF => HEIF_HEIC.HEIF.DefaultMIME,
            ImageFormat.HEIFSequence => HEIF_HEIC.HEIFSequence.DefaultMIME,
            ImageFormat.HEIC => HEIF_HEIC.HEIC.DefaultMIME,
            ImageFormat.HEICSequence => HEIF_HEIC.HEICSequence.DefaultMIME,
            //ImageFormat.APNG => APNG.DefaultMIME,
            _ => throw new ArgumentOutOfRangeException(nameof(imageFormat), imageFormat, null),
        };
    }

    /// <summary>
    /// 获取 MagicNumber
    /// <para>注意！仅允许以下返回类型：byte[] / byte?[] byte[][] / byte?[][]</para>
    /// </summary>
    /// <param name="imageFormat"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static object GetMagicNumber(this ImageFormat imageFormat)
    {
        return imageFormat switch
        {
            ImageFormat.BMP => BMP.MagicNumber,
            ImageFormat.GIF => GIF.MagicNumber,
            ImageFormat.ICO => ICO.MagicNumber,
            ImageFormat.JPEG => JPG.MagicNumber,
            ImageFormat.PNG => PNG.MagicNumber,
            ImageFormat.WebP => WebP.MagicNumber,
            ImageFormat.HEIF => HEIF_HEIC.HEIF.MagicNumber,
            ImageFormat.HEIFSequence => HEIF_HEIC.HEIFSequence.MagicNumber,
            ImageFormat.HEIC => HEIF_HEIC.HEIC.MagicNumber,
            ImageFormat.HEICSequence => HEIF_HEIC.HEICSequence.MagicNumber,
            //ImageFormat.APNG => APNG.MagicNumber,
            _ => throw new ArgumentOutOfRangeException(nameof(imageFormat), imageFormat, null),
        };
    }

    /// <summary>
    /// 检查图片格式是否允许使用
    /// </summary>
    /// <param name="imageFormat"></param>
    /// <returns></returns>
    public static bool IsAllow(this ImageFormat imageFormat) => FileFormat.AllowImageFormats.Contains(imageFormat);

    /// <summary>
    /// 是否为 HEIF/HEIC 高效率图像文件格式
    /// </summary>
    /// <param name="imageFormat"></param>
    /// <returns></returns>
    public static bool IsHEIF_HEIC(this ImageFormat imageFormat) => FileFormat.HEIF_HEIC_Formats.Contains(imageFormat);

    /// <summary>
    /// 检查 二进制数据 是否为指定的图片格式
    /// </summary>
    /// <param name="imageFormat"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static bool IsImage(this ImageFormat imageFormat, IReadOnlyList<byte> buffer)
    {
        var magicNumber = imageFormat.GetMagicNumber();
        return FileFormat.MagicNumber.Match(magicNumber, buffer, null);
    }

    /// <summary>
    /// 检查 流中的数据 是否为指定的图片格式
    /// </summary>
    /// <param name="imageFormat"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static bool IsImage(this ImageFormat imageFormat, Stream stream)
    {
        var magicNumber = imageFormat.GetMagicNumber();
        return FileFormat.MagicNumber.Match(magicNumber, null, stream);
    }
}
