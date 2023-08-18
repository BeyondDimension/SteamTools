using System.Drawing;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

public static partial class IconFactory
{
    #region constants

    public const int MaxIconWidth = 256;
    public const int MaxIconHeight = 256;

    private const ushort HeaderReserved = 0;
    private const ushort HeaderIconType = 1;
    private const byte HeaderLength = 6;

    private const byte EntryReserved = 0;
    private const byte EntryLength = 16;

    private const byte PngColorsInPalette = 0;
    private const ushort PngColorPlanes = 1;

    #endregion

    #region --------- 合成 ico 图标

    /// <summary>
    /// 合成包含不同分辨率ico图标
    /// </summary>
    /// <param name="images">png类型图片集</param>
    /// <param name="stream"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SavePngsAsIcon(IEnumerable<Bitmap> images, Stream stream)
    {
        Bitmap[] orderedImages = images.OrderBy(i => i.Width).ThenBy(i => i.Height).ToArray();

        using var writer = new BinaryWriter(stream);
        writer.Write(HeaderReserved);
        writer.Write(HeaderIconType);
        writer.Write((ushort)orderedImages.Length);

        Dictionary<uint, byte[]> buffers = new();

        uint lengthSum = 0;
        uint baseOffset = (uint)(HeaderLength + (EntryLength * orderedImages.Length));

        for (int i = 0; i < orderedImages.Length; i++)
        {
            Bitmap image = orderedImages[i];

            byte[] buffer = CreateImageBuffer(image);
            uint offset = baseOffset + lengthSum;

            writer.Write(GetIconWidth(image));
            writer.Write(GetIconHeight(image));
            writer.Write(PngColorsInPalette);
            writer.Write(EntryReserved);
            writer.Write(PngColorPlanes);
            writer.Write((ushort)Image.GetPixelFormatSize(image.PixelFormat));
            writer.Write((uint)buffer.Length);
            writer.Write(offset);

            lengthSum += (uint)buffer.Length;

            buffers.Add(offset, buffer);
        }

        foreach (var kvp in buffers)
        {

            writer.BaseStream.Seek(kvp.Key, SeekOrigin.Begin);

            writer.Write(kvp.Value);
        }
    }

    private static byte GetIconHeight(Bitmap image)
    {
        if (image.Height == IconFactory.MaxIconHeight)
            return 0;

        return (byte)image.Height;
    }

    private static byte GetIconWidth(Bitmap image)
    {
        if (image.Width == IconFactory.MaxIconWidth)
            return 0;

        return (byte)image.Width;
    }

    private static byte[] CreateImageBuffer(Bitmap image)
    {
        using (var stream = new MemoryStream())
        {
            image.Save(stream, ImageFormat.Png);

            return stream.ToArray();
        }
    }
    #endregion

    #region --------- 获取 ico 分辨率

    [DllImport("shell32.dll", EntryPoint = "#727")]
    public static extern int SHGetImageList(IMAGELIST_SIZE_FLAG iImageList, ref Guid riid, ref IImageList ppv);

    [DllImport("Shell32.dll")]
    public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

    /// <summary>
    /// 系统图标大小标识
    /// </summary>
    public enum IMAGELIST_SIZE_FLAG
    {
        /// <summary>
        /// Size(32,32)
        /// </summary>
        SHIL_LARGE = 0x0,

        /// <summary>
        /// Size(16,16)
        /// </summary>
        SHIL_SMALL = 0x1,

        /// <summary>
        /// Size(48,48)
        /// </summary>
        SHIL_EXTRALARGE = 0x2,

        /// <summary>
        /// Size(256,256)
        /// </summary>
        SHIL_JUMBO = 0x4,

    }

    public const int ILD_TRANSPARENT = 0x00000001;
    public const int ILD_IMAGE = 0x00000020;

    public const string IID_IImageList2 = "192B9D83-50FC-457B-90A0-2B82A8B5DAE1";
    public const string IID_IImageList = "46EB5926-582E-4017-9FDF-E8998DAA0950";

    [Flags]
    public enum SHGFI : uint
    {
        Icon = 0x000000100,

        DisplayName = 0x000000200,

        TypeName = 0x000000400,

        Attributes = 0x000000800,

        IconLocation = 0x000001000,

        ExeType = 0x000002000,

        SysIconIndex = 0x000004000,

        LinkOverlay = 0x000008000,

        Selected = 0x000010000,

        Attr_Specified = 0x000020000,

        LargeIcon = 0x000000000,

        SmallIcon = 0x000000001,

        OpenIcon = 0x000000002,

        ShellIconSize = 0x000000004,

        PIDL = 0x000000008,

        UseFileAttributes = 0x000000010,

        AddOverlays = 0x000000020,

        OverlayIndex = 0x000000040,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public const int NAMESIZE = 80;
        public IntPtr HIcon;
        public int Iicon;
        public uint DwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string SzDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string SzTypeName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        int x;
        int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGELISTDRAWPARAMS
    {
        public int CbSize;
        public IntPtr Himl;
        public int I;
        public IntPtr HdcDst;
        public int X;
        public int Y;
        public int CX;
        public int CY;
        public int XBitmap;
        public int YBitmap;
        public int RgbBk;
        public int RgbFg;
        public int FStyle;
        public int DwRop;
        public int FState;
        public int Frame;
        public int CrEffect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGEINFO
    {
        public IntPtr HbmImage;
        public IntPtr HbmMask;
        public int Unused1;
        public int Unused2;
        public RECT RcImage;
    }

    [ComImport]
    [Guid("192B9D83-50FC-457B-90A0-2B82A8B5DAE1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IImageList
    {
        [PreserveSig]
        int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

        [PreserveSig]
        int ReplaceIcon(int i, IntPtr hicon, ref int pi);

        [PreserveSig]
        int SetOverlayImage(int iImage, int iOverlay);

        [PreserveSig]
        int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

        [PreserveSig]
        int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

        [PreserveSig]
        int Draw(ref IMAGELISTDRAWPARAMS pimldp);

        [PreserveSig]
        int Remove(int i);

        [PreserveSig]
        int GetIcon(int i, int flags, ref IntPtr picon);

        [PreserveSig]
        int GetImageInfo(int i, ref IMAGEINFO pImageInfo);

        [PreserveSig]
        int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);

        [PreserveSig]
        int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int Clone(ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int GetImageRect(int i, ref RECT prc);

        [PreserveSig]
        int GetIconSize(ref int cx, ref int cy);

        [PreserveSig]
        int SetIconSize(int cx, int cy);

        [PreserveSig]
        int GetImageCount(ref int pi);

        [PreserveSig]
        int SetImageCount(int uNewCount);

        [PreserveSig]
        int SetBkColor(int clrBk, ref int pclr);

        [PreserveSig]
        int GetBkColor(ref int pclr);

        [PreserveSig]
        int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);

        [PreserveSig]
        int EndDrag();

        [PreserveSig]
        int DragEnter(IntPtr hwndLock, int x, int y);

        [PreserveSig]
        int DragLeave(IntPtr hwndLock);

        [PreserveSig]
        int DragMove(int x, int y);

        [PreserveSig]
        int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);

        [PreserveSig]
        int DragShowNolock(int fShow);

        [PreserveSig]
        int GetDragImage(ref POINT ppt, ref POINT pptHotspot, ref Guid riid, ref IntPtr ppv);

        [PreserveSig]
        int GetItemFlags(int i, ref int dwFlags);

        [PreserveSig]
        int GetOverlayImage(int iOverlay, ref int piIndex);
    }

    /// <summary>
    /// 获取文件的图标索引号
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>图标索引号</returns>
    public static int GetIconIndex(string fileName)
    {
        SHFILEINFO info = default;
        IntPtr iconIntPtr = SHGetFileInfo(fileName, 0, ref info, (uint)Marshal.SizeOf(info), SHGFI.SysIconIndex | SHGFI.OpenIcon);
        if (iconIntPtr == IntPtr.Zero)
            return -1;
        return info.Iicon;
    }

    /// <summary>
    /// 根据图标索引号获取图标
    /// </summary>
    /// <param name="iIcon">图标索引号</param>
    /// <param name="flag">图标尺寸标识</param>
    /// <returns></returns>
    public static Icon GetIcon(int iIcon, IMAGELIST_SIZE_FLAG flag)
    {
        IImageList? list = null;
        Guid theGuid = new Guid(IID_IImageList);
        _ = SHGetImageList(flag, ref theGuid, ref list);
        IntPtr hIcon = IntPtr.Zero;
        list.GetIcon(iIcon, ILD_TRANSPARENT | ILD_IMAGE, ref hIcon);
        return Icon.FromHandle(hIcon);
    }

    /// <summary>
    ///  从文件获取Icon图标
    /// </summary>
    /// <param name="fileName">文件名称</param>
    /// <param name="flag">图标尺寸标识</param>
    /// <returns></returns>
    public static Icon GetIconFromFile(string fileName, IMAGELIST_SIZE_FLAG flag)
    {
        return GetIcon(GetIconIndex(fileName), flag);
    }
    #endregion
}