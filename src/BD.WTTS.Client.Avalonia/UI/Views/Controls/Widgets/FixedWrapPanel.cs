using static System.Math;

namespace BD.WTTS.UI.Views.Controls;

/// <summary>
/// 固定的 WrapPanel
/// </summary>
public class FixedWrapPanel : Panel, INavigableContainer
{
    /// <summary>
    /// Defines the <see cref="ItemsPerLine"/> property.
    /// </summary>
    public static readonly StyledProperty<int> ItemsPerLineProperty =
        AvaloniaProperty.Register<FixedWrapPanel, int>(nameof(ItemsPerLine), 3);

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
    AvaloniaProperty.Register<FixedWrapPanel, double>(nameof(Spacing), 0);

    static FixedWrapPanel()
    {
        AffectsMeasure<FixedWrapPanel>(ItemsPerLineProperty);
    }

    /// <summary>
    /// ItemsPerLine
    /// </summary>
    public int ItemsPerLine
    {
        get => GetValue(ItemsPerLineProperty);
        set => SetValue(ItemsPerLineProperty, value);
    }

    /// <summary>
    /// Spacing
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <inheritdoc/>
    IInputElement INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        int index = from is Control control ? Children.IndexOf(control) : -1;
        switch (direction)
        {
            case NavigationDirection.First:
                index = 0;
                break;
            case NavigationDirection.Last:
                index = Children.Count - 1;
                break;
            case NavigationDirection.Next:
                ++index;
                break;
            case NavigationDirection.Previous:
                --index;
                break;
            case NavigationDirection.Left:
                index -= 1;
                break;
            case NavigationDirection.Right:
                index += 1;
                break;
            case NavigationDirection.Up:
                index = -1;
                break;
            case NavigationDirection.Down:
                index = -1;
                break;
        }

        if (index >= 0 && index < Children.Count)
        {
            return Children[index];
        }

        return this;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint)
    {
        var panelSize = default(MutableSize);
        var lineSize = default(MutableSize);
        int itemsInCurrentLine = 0;
        double itemWidth = (constraint.Width - (Spacing * (ItemsPerLine - 1))) / ItemsPerLine;

        foreach (var child in Children.OfType<Control>())
        {
            child.Measure(new Size(itemWidth, constraint.Height));
            var childSize = child.DesiredSize;

            if (itemsInCurrentLine == ItemsPerLine)
            {
                // Start a new line
                panelSize.Width = Math.Max(panelSize.Width, lineSize.Width);
                panelSize.Height += lineSize.Height + Spacing;
                lineSize = new MutableSize(childSize.Width, childSize.Height);
                itemsInCurrentLine = 1;
            }
            else
            {
                // Add to the current line
                lineSize.Width += childSize.Width + (itemsInCurrentLine > 0 ? Spacing : 0);
                lineSize.Height = Math.Max(lineSize.Height, childSize.Height);
                itemsInCurrentLine++;
            }
        }

        // Add the last line size
        panelSize.Width = Math.Max(panelSize.Width, lineSize.Width);
        panelSize.Height += lineSize.Height;

        return panelSize.ToSize();
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var itemWidth = (finalSize.Width - (Spacing * (ItemsPerLine - 1))) / ItemsPerLine;
        var position = default(MutablePoint);
        var lineHeight = 0.0;
        int itemsInCurrentLine = 0;
        try
        {
            foreach (var child in Children.OfType<Control>())
            {
                if (itemsInCurrentLine == ItemsPerLine)
                {
                    // Move to the next line
                    position.X = 0;
                    position.Y += lineHeight + Spacing;
                    lineHeight = 0;
                    itemsInCurrentLine = 0;
                }

                var childSize = new Size(itemWidth, child.DesiredSize.Height);
                child.Arrange(new Rect(position.ToPoint(), childSize));

                position.X += itemWidth + Spacing;
                lineHeight = Math.Max(lineHeight, childSize.Height);
                itemsInCurrentLine++;
            }
        }
        catch
        { }

        return finalSize;
    }

    void ArrangeLine(double y, double height, int start, int end, double width)
    {
        double x = 0;
        for (int i = start; i < end; i++)
        {
            Control child = Children[i];
            if (child == null)
            {
                continue;
            }

            try
            {
                if (i < end - 1 || i % ItemsPerLine == 0)
                {

                    child.Arrange(new Rect(x, y, width - Spacing, height));
                    x += width + Spacing;
                }
                else
                {
                    child.Arrange(new Rect(x, y, width, height));
                    x += width;
                }
            }
            catch { }
        }
    }

    record struct MutablePoint
    {
        internal double X;
        internal double Y;

        internal readonly Point ToPoint() => new Point(X, Y);
    }

    record struct MutableSize
    {
        internal MutableSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        internal MutableSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }

        internal double Width;
        internal double Height;

        internal readonly Size ToSize()
        {
            return new Size(Width, Height);
        }
    }
}
