using System.IO.FileFormats.Internals;

namespace System.IO.FileFormats;

/// <summary>
/// 文件格式工具类
/// </summary>
public static partial class FileFormat
{
    static readonly IReadOnlyDictionary<ImageFormat, object> imgFileMagicNums;

    static FileFormat()
    {
        ImageFormats = Enum2.GetAll<ImageFormat>().OrderByDescending(x => (byte)x).ToArray();
        imgFileMagicNums = ImageFormats.ToDictionary(x => x, x => x.GetMagicNumber());
    }

    /// <summary>
    /// 允许使用的图片格式
    /// </summary>
    public static ImageFormat[] AllowImageFormats { get; } = { ImageFormat.JPEG, ImageFormat.PNG };

    /// <summary>
    /// 允许使用的图片格式的媒体类型
    /// </summary>
    public static string[] AllowImageMediaTypeNames { get; } = AllowImageFormats.Select(x => x.GetMIME()).ToArray();

    /// <summary>
    /// 支持的图片格式
    /// </summary>
    public static ImageFormat[] ImageFormats { get; }

    /// <summary>
    /// HEIF/HEIC 高效率图像文件格式
    /// </summary>
    public static ImageFormat[] HEIF_HEIC_Formats { get; } = { ImageFormat.HEIF, ImageFormat.HEIFSequence, ImageFormat.HEIC, ImageFormat.HEICSequence };

    /// <summary>
    /// 检查 二进制数据 是否为图片格式
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static bool IsImage(IReadOnlyList<byte> buffer, out ImageFormat format)
    {
        foreach (var item in ImageFormats)
        {
            if (item.IsImage(buffer))
            {
                format = item;
                return true;
            }
        }
        format = 0;
        return false;
    }

    /// <summary>
    /// 检查 二进制数据 是否为图片格式
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static (bool isImage, ImageFormat format) IsImage(IReadOnlyList<byte> buffer)
    {
        var isImage = IsImage(buffer, out var format);
        return (isImage, format);
    }

    /// <summary>
    /// 检查 流中的数据 是否为图片格式
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="format"></param>
    /// <param name="formats"></param>
    /// <returns></returns>
    public static bool IsImage(Stream stream, out ImageFormat format)
    {
        var length = imgFileMagicNums.Values.Max(MagicNumber.GetLength);
        var buffer = MagicNumber.ReadHeaderBuffer(stream, length);
        foreach (var item in imgFileMagicNums)
        {
            if (MagicNumber.Match(item.Value, buffer, null))
            {
                format = item.Key;
                return true;
            }
        }
        format = 0;
        return false;
    }

    /// <summary>
    /// 检查 流中的数据 是否为图片格式
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="formats"></param>
    /// <returns></returns>
    public static (bool isImage, ImageFormat format) IsImage(Stream stream)
    {
        var isImage = IsImage(stream, out var format);
        return (isImage, format);
    }

    /// <summary>
    /// 检查 二进制数据 是否为 SQLite3 数据库文件
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static bool IsSQLite3(IReadOnlyList<byte> buffer)
        => MagicNumber.SequenceEqual(buffer, DataBaseFileFormat.SQLite3.MagicNumber);

    /// <summary>
    /// 检查 流中的数据 是否为 SQLite3 数据库文件
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static bool IsSQLite3(Stream stream)
        => IsSQLite3(MagicNumber.ReadHeaderBuffer(stream, DataBaseFileFormat.SQLite3.MagicNumber.Length));
}