namespace BD.WTTS.UI.Views.Windows;

internal static class WindowPlacement
{
    internal static (PixelPoint Position, Size ClientSize)? Restore(PixelPoint position, Size clientSize,
        PixelRect workArea, double scaling, Size frame, Size minimum, Size maximum, IEnumerable<PixelRect> otherWorkAreas)
    {
        if (!IsPositive(clientSize.Width) || !IsPositive(clientSize.Height) || !IsPositive(scaling) ||
            workArea.Width <= 0 || workArea.Height <= 0)
            return null;

        var maxWidth = Math.Max(minimum.Width, maximum.Width);
        var maxHeight = Math.Max(minimum.Height, maximum.Height);
        var desired = new Size(Math.Clamp(clientSize.Width, minimum.Width, maxWidth),
            Math.Clamp(clientSize.Height, minimum.Height, maxHeight));
        var frameWidth = Math.Max(0, frame.Width);
        var frameHeight = Math.Max(0, frame.Height);
        var desiredFrameWidth = ToPixelExtent(desired.Width + frameWidth, scaling);
        var desiredFrameHeight = ToPixelExtent(desired.Height + frameHeight, scaling);
        var desiredFrame = new Rect(position.X, position.Y, desiredFrameWidth, desiredFrameHeight);
        if (workArea.Contains(position) && desiredFrameWidth <= int.MaxValue && desiredFrameHeight <= int.MaxValue &&
            (AsRect(workArea).Contains(desiredFrame) || otherWorkAreas.Any(area => AsRect(area).Intersects(desiredFrame))))
            return (position, PreserveClientPixels(desired));

        var fitted = new Size(
            Math.Max(minimum.Width, Math.Min(desired.Width, workArea.Width / scaling - frameWidth)),
            Math.Max(minimum.Height, Math.Min(desired.Height, workArea.Height / scaling - frameHeight)));
        if (!IsPositive(fitted.Width) || !IsPositive(fitted.Height))
            return null;

        var maxX = Math.Max(workArea.X, workArea.Right - ToPixelExtent(fitted.Width + frameWidth, scaling));
        var maxY = Math.Max(workArea.Y, workArea.Bottom - ToPixelExtent(fitted.Height + frameHeight, scaling));
        var fittedPosition = new PixelPoint((int)Math.Clamp(position.X, workArea.X, maxX),
            (int)Math.Clamp(position.Y, workArea.Y, maxY));
        return (fittedPosition, PreserveClientPixels(fitted));

        Size PreserveClientPixels(Size size) => OperatingSystem.IsWindows()
            ? new Size(PreservePixelDimension(size.Width, scaling, maxWidth),
                PreservePixelDimension(size.Height, scaling, maxHeight))
            : size;
    }

    // A native pixel extent can round-trip through DIPs to just above the same integer.
    private static double ToPixelExtent(double extent, double scaling) => Math.Ceiling(Math.BitDecrement(extent * scaling));

    private static double PreservePixelDimension(double extent, double scaling, double maximum)
    {
        // Win32 truncates the scaled client size. Correct only a one-step departure from a whole pixel.
        var pixels = extent * scaling;
        return Math.BitIncrement(pixels) == Math.Ceiling(pixels) ? Math.Min(Math.BitIncrement(extent), maximum) : extent;
    }

    private static bool IsPositive(double value) => value > 0 && double.IsFinite(value);

    private static Rect AsRect(PixelRect area) => new(area.X, area.Y, area.Width, area.Height);
}
