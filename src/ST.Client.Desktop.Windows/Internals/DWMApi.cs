using System;
using System.Runtime.InteropServices;

static class DWMApi
{
    public const int DWM_TNP_VISIBLE = 0x8,
        DWM_TNP_OPACITY = 0x4,
        DWM_TNP_RECTDESTINATION = 0x1;

    const string DllName = "dwmapi";
    [DllImport(DllName, CharSet = CharSet.Auto)]
    public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

    [DllImport(DllName, CharSet = CharSet.Auto)]
    public static extern int DwmUnregisterThumbnail(IntPtr thumb);

    [DllImport(DllName, CharSet = CharSet.Auto)]
    public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

    [DllImport(DllName, CharSet = CharSet.Auto)]
    public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);
}

[StructLayout(LayoutKind.Sequential)]
struct DWM_THUMBNAIL_PROPERTIES
{
    public int dwFlags;
    public RECT rcDestination;
    public RECT rcSource;
    public byte opacity;
    public bool fVisible;
    public bool fSourceClientAreaOnly;
}

[StructLayout(LayoutKind.Sequential)]
struct PSIZE
{
    public int x;
    public int y;
}
