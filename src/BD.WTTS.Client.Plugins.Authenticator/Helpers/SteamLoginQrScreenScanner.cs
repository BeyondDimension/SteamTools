using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using BD.SteamClient.Models;
using ZXing;
using ZXing.Common;

namespace BD.WTTS.Helpers;

/// <summary>
/// Reads Steam login QR challenges from the visible Windows desktop without saving images.
/// This only discovers challenges; it never approves a login.
/// </summary>
public static partial class SteamLoginQrScreenScanner
{
    /// <summary>
    /// Screen capture requires Windows 10 version 1607 or later for per-monitor physical coordinates.
    /// </summary>
    public static bool IsSupported => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 14393);

    /// <summary>
    /// Captures and scans each monitor separately on a worker thread. Images remain in memory only
    /// and are released before moving to the next monitor.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Screen capture is not supported on this system.</exception>
    public static Task<IReadOnlyList<SteamLoginQrChallenge>> CaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 14393))
            throw new PlatformNotSupportedException("Steam login QR screen scanning requires Windows 10 version 1607 or later.");

        return Task.Run(() => CaptureWindows(cancellationToken), cancellationToken);
    }

    [SupportedOSPlatform("windows10.0.14393")]
    static IReadOnlyList<SteamLoginQrChallenge> CaptureWindows(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Keep enumeration and capture on this thread, with no awaits before restoring its context.
        var previousDpiContext = SetThreadDpiAwarenessContext(new nint(-3)); // PER_MONITOR_AWARE
        if (previousDpiContext == 0)
            throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not enable physical-pixel screen capture.");

        try
        {
            var monitors = new List<MonitorRectangle>();
            bool EnumerateMonitor(nint monitor, nint dc, ref MonitorRectangle bounds, nint data)
            {
                // Do not throw across the native callback boundary.
                if (cancellationToken.IsCancellationRequested)
                    return false;
                if (bounds.Right > bounds.Left && bounds.Bottom > bounds.Top)
                    monitors.Add(bounds);
                return true;
            }

            MonitorEnumProc callback = EnumerateMonitor;
            var enumerated = EnumDisplayMonitors(0, 0, callback, 0);
            cancellationToken.ThrowIfCancellationRequested();
            if (!enumerated)
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not enumerate desktop monitors.");

            var challenges = new List<SteamLoginQrChallenge>();
            var seen = new HashSet<SteamLoginQrChallenge>();
            foreach (var bounds in monitors)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var challenge in CaptureMonitor(bounds, cancellationToken))
                {
                    if (seen.Add(challenge))
                        challenges.Add(challenge);
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            return challenges;
        }
        finally
        {
            SetThreadDpiAwarenessContext(previousDpiContext);
        }
    }

    [SupportedOSPlatform("windows10.0.14393")]
    static unsafe IReadOnlyList<SteamLoginQrChallenge> CaptureMonitor(MonitorRectangle bounds, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var width = checked(bounds.Right - bounds.Left);
        var height = checked(bounds.Bottom - bounds.Top);
        var bytes = checked(width * height * 4);
        var screenDc = GetDC(0);
        if (screenDc == 0)
            throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not access the desktop.");
        nint memoryDc = 0, bitmap = 0, previousBitmap = 0, bits = 0;
        try
        {
            memoryDc = CreateCompatibleDC(screenDc);
            if (memoryDc == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not create the capture device context.");

            var info = new BitmapInfo
            {
                Size = (uint)Marshal.SizeOf<BitmapInfo>(),
                Width = width,
                Height = -height, // Top-down BGRX pixels, independent of the display's color depth.
                Planes = 1,
                BitCount = 32,
            };
            bitmap = CreateDIBSection(screenDc, in info, 0, out bits, 0, 0);
            if (bitmap == 0 || bits == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not allocate the capture bitmap.");
            previousBitmap = SelectObject(memoryDc, bitmap);
            if (previousBitmap == 0 || previousBitmap == -1)
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not select the capture bitmap.");
            cancellationToken.ThrowIfCancellationRequested();
            const uint sourceCopyWithLayeredWindows = 0x00CC0020 | 0x40000000;
            if (!BitBlt(memoryDc, 0, 0, width, height, screenDc, bounds.Left, bounds.Top, sourceCopyWithLayeredWindows) || !GdiFlush())
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not capture the desktop pixels.");
            cancellationToken.ThrowIfCancellationRequested();

            // Convert directly from the DIB; no System.Drawing dependency or full-color copy.
            var pixels = new ReadOnlySpan<byte>((void*)bits, bytes);
            var luminance = GC.AllocateUninitializedArray<byte>(checked(width * height));
            try
            {
                for (int i = 0, offset = 0; i < luminance.Length; i++, offset += 4)
                    luminance[i] = (byte)((pixels[offset + 2] + pixels[offset + 1] * 2 + pixels[offset]) / 4);
                return DecodeLuminance(new RGBLuminanceSource(luminance, width, height, RGBLuminanceSource.BitmapFormat.Gray8), cancellationToken);
            }
            finally
            {
                Array.Clear(luminance);
            }
        }
        finally
        {
            if (bits != 0)
                new Span<byte>((void*)bits, bytes).Clear();
            if (previousBitmap != 0 && previousBitmap != -1)
                SelectObject(memoryDc, previousBitmap);
            if (bitmap != 0)
                DeleteObject(bitmap);
            if (memoryDc != 0)
                DeleteDC(memoryDc);
            ReleaseDC(0, screenDc);
        }
    }

    /// <summary>
    /// Decodes tightly packed, top-down BGRX pixels (four bytes per pixel; the fourth byte is ignored).
    /// Kept separate from capture so synthetic QR images can exercise the same decoding path.
    /// </summary>
    internal static IReadOnlyList<SteamLoginQrChallenge> DecodeBgr32(byte[] pixels, int width, int height, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(pixels);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (pixels.Length != checked(width * height * 4))
            throw new ArgumentException("Expected tightly packed BGRX pixels.", nameof(pixels));

        var source = new RGBLuminanceSource(pixels, width, height, RGBLuminanceSource.BitmapFormat.BGR32);
        return DecodeLuminance(source, cancellationToken);
    }

    static IReadOnlyList<SteamLoginQrChallenge> DecodeLuminance(RGBLuminanceSource source, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reader = new BarcodeReaderGeneric
            {
                Options = new DecodingOptions
                {
                    PossibleFormats = [BarcodeFormat.QR_CODE],
                    TryHarder = true,
                    TryInverted = false,
                },
            };
            var challenges = new List<SteamLoginQrChallenge>();
            var seen = new HashSet<SteamLoginQrChallenge>();
            // ZXing's multiple-QR path does not honor TryInverted. Scan both polarities
            // explicitly, including mixed black-on-white and white-on-dark codes.
            for (var polarity = 0; polarity < 2; polarity++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (polarity != 0)
                {
                    var matrix = source.Matrix;
                    for (var i = 0; i < matrix.Length; i++)
                        matrix[i] = (byte)(255 - matrix[i]);
                }
                var results = reader.DecodeMultiple(source);
                if (results == null)
                    continue;
                foreach (var result in results)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (SteamLoginQrChallenge.TryParse(result.Text, out var challenge) && challenge != null && seen.Add(challenge))
                        challenges.Add(challenge);
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            return challenges;
        }
        finally
        {
            Array.Clear(source.Matrix);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MonitorRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct BitmapInfo
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint SizeImage;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public uint ColorsUsed;
        public uint ColorsImportant;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial nint GetDC(nint window);

    [LibraryImport("user32.dll")]
    private static partial int ReleaseDC(nint window, nint dc);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    private static partial nint CreateCompatibleDC(nint dc);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    private static partial nint CreateDIBSection(nint dc, in BitmapInfo info, uint usage, out nint bits, nint section, uint offset);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    private static partial nint SelectObject(nint dc, nint obj);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool BitBlt(nint dest, int x, int y, int width, int height, nint source, int sourceX, int sourceY, uint operation);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GdiFlush();

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(nint obj);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteDC(nint dc);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    delegate bool MonitorEnumProc(nint monitor, nint dc, ref MonitorRectangle bounds, nint data);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumDisplayMonitors(nint dc, nint clip, MonitorEnumProc callback, nint data);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial nint SetThreadDpiAwarenessContext(nint context);
}
