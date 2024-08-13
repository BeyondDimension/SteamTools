using static System.Math;

namespace BD.WTTS.UI.Views.Controls;

/// <summary>
/// 固定的 WrapPanel
/// </summary>
[Mobius(
"""
Mobius.UI.Views.Controls
""")]
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
        double itemWidth = (constraint.Width - (Spacing * (ItemsPerLine - 1))) / ItemsPerLine;
        MutableSize currentLineSize = default;
        MutableSize panelSize = default;
        Size lineConstraint = new(constraint.Width, constraint.Height);
        Size childConstraint = new(itemWidth - Spacing, constraint.Height);

        for (int i = 0, count = Children.Count; i < count; i++)
        {
            Control child = Children[i];
            if (child is null)
            {
                continue;
            }

            child.Measure(childConstraint);
            Size childSize = new(itemWidth, child.DesiredSize.Height);

            if (MathUtilities.GreaterThan(currentLineSize.Width + childSize.Width + Spacing, lineConstraint.Width))
            {
                // Need to switch to another line
                panelSize.Width = Max(currentLineSize.Width, panelSize.Width);
                panelSize.Height += currentLineSize.Height;
                currentLineSize = new(childSize);
            }
            else
            {
                // Continue to accumulate a line
                currentLineSize.Width += childSize.Width + Spacing;
                currentLineSize.Height = Max(childSize.Height, currentLineSize.Height);
            }
        }

        // The last line size, if any should be added
        panelSize.Width = Max(currentLineSize.Width, panelSize.Width);
        panelSize.Height = currentLineSize.Height;

        if (double.IsInfinity(panelSize.Width))
            panelSize.Width = 0;

        return panelSize.ToSize();
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        double itemWidth = (finalSize.Width - (Spacing * (ItemsPerLine - 1))) / ItemsPerLine;
        int firstInLine = 0;
        double accumulatedHeight = 0;
        var currentLineSize = default(MutableSize);

        for (int i = 0; i < Children.Count; i++)
        {
            Control child = Children[i];
            if (child == null)
            {
                continue;
            }

            MutableSize itemSize = new(itemWidth, child.DesiredSize.Height);
            if (MathUtilities.GreaterThan(currentLineSize.Width + itemSize.Width, finalSize.Width))
            {
                // Need to switch to another line
                ArrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, i, itemWidth);
                accumulatedHeight += currentLineSize.Height;
                currentLineSize = itemSize;
                firstInLine = i;
            }
            else
            {
                // Continue to accumulate a line
                currentLineSize.Width += itemSize.Width;
                currentLineSize.Height = Max(itemSize.Height, currentLineSize.Height);
            }
        }

        if (firstInLine < Children.Count)
        {
            // Arrange the last line, if any
            ArrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, Children.Count, itemWidth);
        }

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

            if (i < end - 1)
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
