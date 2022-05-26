namespace System.IO.FileFormats;

/// <summary>
/// 图片格式
/// <para>添加新格式操作说明：</para>
/// <para>在 <see cref="ImageFileFormat"/> 添加新的格式</para>
/// <para>在 <see cref="ImageFormatEnumExtensions.GetExtension(ImageFormat)"/> 添加新的 case</para>
/// <para>在 <see cref="ImageFormatEnumExtensions.GetMIME(ImageFormat)"/> 添加新的 case</para>
/// <para>在 <see cref="ImageFormatEnumExtensions.GetMagicNumber(ImageFormat)"/> 添加新的 case</para>
/// </summary>
public enum ImageFormat : byte
{
    // ReSharper disable once InconsistentNaming
    // 此枚举中禁止出现值为0的

    /// <summary>
    /// BMP取自位图Bitmap的缩写，也称为DIB（与设备无关的位图），是一种与显示器无关的位图数字图像文件格式。常见于 Microsoft Windows 和 OS/2 操作系统，Windows GDI API内部使用的DIB数据结构与 BMP 文件格式几乎相同。
    /// <para>https://en.wikipedia.org/wiki/BMP_file_format</para>
    /// </summary>
    BMP = 2,

    /// <summary>
    /// 图像互换格式（GIF，Graphics Interchange Format）是一种位图图形文件格式，以8位色（即256种颜色）重现真彩色的图像。它实际上是一种压缩文档，采用LZW压缩算法进行编码，有效地减少了图像文件在网络上传输的时间。它是目前万维网广泛应用的网络传输图像格式之一。
    /// <para>https://en.wikipedia.org/wiki/GIF</para>
    /// <para>https://baike.sogou.com/v2844.htm</para>
    /// </summary>
    GIF,

    /// <summary>
    /// ICO文件格式是Microsoft Windows中计算机图标的图像文件格式。 ICO文件包含多个尺寸和颜色深度的一个或多个小图像，以便可以适当地缩放它们。 在Windows中，向用户，桌面，“开始”菜单或Windows资源管理器中显示图标的所有可执行文件都必须带有ICO格式的图标。
    /// <para>https://en.wikipedia.org/wiki/ICO_(file_format)</para>
    /// <para>https://baike.sogou.com/v64848120.htm</para>
    /// </summary>
    ICO,

    /// <summary>
    /// JPG/JPEG
    /// 在计算机中，JPEG（发音为jay-peg, IPA：[ˈdʒeɪpɛg]）是一种针对照片视频而广泛使用的有损压缩标准方法。这个名称代表Joint Photographic Experts Group（联合图像专家小组）。此团队创立于1986年，1992年发布了JPEG的标准而在1994年获得了ISO 10918-1的认定。JPEG与视频音频压缩标准的MPEG（Moving Picture Experts Group）很容易混淆，但两者是不同的组织及标准。
    /// <para>https://en.wikipedia.org/wiki/JPEG</para>
    /// <para>https://baike.sogou.com/v241266.htm</para>
    /// </summary>
    JPEG,

    /// <summary>
    /// 便携式网络图形（Portable Network Graphics，PNG）是一种无损压缩的位图图形格式，支持索引、灰度、RGB三种颜色方案以及Alpha通道等特性。PNG的开发目标是改善并取代GIF作为适合网络传输的格式而不需专利许可，所以被广泛应用于互联网及其他方面上。
    /// <para>https://en.wikipedia.org/wiki/Portable_Network_Graphics</para>
    /// <para>https://baike.sogou.com/v44717.htm</para>
    /// </summary>
    PNG,

    /// <summary>
    /// WebP（发音weppy），是一种同时提供了有损压缩与无损压缩（可逆压缩）的图片文件格式，派生自视频编码格式VP8，被认为是WebM多媒体格式的姊妹项目，是由Google在购买On2 Technologies后发展出来，以BSD授权条款发布。
    /// <para>https://zh.wikipedia.org/wiki/WebP</para>
    /// <para>https://baike.sogou.com/v10183483.htm</para>
    /// <para>https://developers.google.com/speed/webp/</para>
    /// <para>https://developers.google.cn/speed/webp/</para>
    /// </summary>
    WebP,

    #region HEIF/HEIC

    /// <summary>
    /// HEIF/HEIC
    /// 高效率图像文件格式（英语：High Efficiency Image File Format, HEIF；也称高效图像文件格式）是一个用于单张图像或图像序列的文件格式。它由运动图像专家组（MPEG）开发，并在MPEG-H Part 12（ISO/IEC 23008-12）中定义。
    /// <para>https://en.wikipedia.org/wiki/High_Efficiency_Image_File_Format</para>
    /// <para>https://baike.baidu.com/item/HEIC/10444257</para>
    /// </summary>
    HEIF,

    /// <inheritdoc cref="HEIF"/>
    HEIFSequence,

    /// <inheritdoc cref="HEIF"/>
    HEIC,

    /// <inheritdoc cref="HEIF"/>
    HEICSequence,

    #endregion

    ///// <summary>
    ///// 动态可移植网络图形（Animated Portable Network Graphics，APNG）是一种继承自便携式网络图形（PNG）的文件格式，他允许像GIF格式一样播放动态图片，并且拥有GIF不支持的24位图像和8位透明性。 它还保留了与非动画PNG文件的向后兼容性。
    ///// <para>https://zh.wikipedia.org/wiki/APNG</para>
    ///// <para>https://wiki.mozilla.org/APNG_Specification</para>
    ///// </summary>
    //APNG,
}