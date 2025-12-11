namespace BD.WTTS.Client.Tools.Publish.Enums;

/// <summary>
/// 静态资源文件类型
/// </summary>
public enum CloudFileType
{
    // 不使用值为 0 的

    #region ImageFormat 1~255 值用于图片类型

    BMP = ImageFormat.BMP,
    GIF = ImageFormat.GIF,
    ICO = ImageFormat.ICO,
    JPEG = ImageFormat.JPEG,
    PNG = ImageFormat.PNG,
    WebP = ImageFormat.WebP,
    HEIF = ImageFormat.HEIF,
    HEIFSequence = ImageFormat.HEIFSequence,
    HEIC = ImageFormat.HEIC,
    HEICSequence = ImageFormat.HEICSequence,

    #endregion

    #region 256~xxx 待定

    WinExe = 256,
    TarGzip,
    SevenZip,
    TarBrotli,
    TarXz,
    TarZstd,
    DMG,
    DEB,
    RPM,
    APK,

    #endregion

    Json = 300,
    Dll,
    Xml,
    So,
    Dylib,
    None,
    Js,
    Xaml,
    AXaml,
    CSharp,

    Msix = 901,
    MsixUpload,
    MsixBundle,
}