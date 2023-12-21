namespace BD.WTTS.UI.Views.Controls;

public sealed class FlexPanel : Panel
{
    private static readonly Func<Layoutable, int> s_getOrder = x => x is { } y ? Flex.GetOrder(y) : 0;
    private static readonly Func<Layoutable, bool> s_isVisible = x => x.IsVisible;

    /// <summary>
    /// Defines the <see cref="Direction"/> property.
    /// </summary>
    public static readonly StyledProperty<FlexDirection> DirectionProperty =
        AvaloniaProperty.Register<FlexPanel, FlexDirection>(nameof(Direction));

    /// <summary>
    /// Defines the <see cref="JustifyContent"/> property.
    /// </summary>
    public static readonly StyledProperty<JustifyContent> JustifyContentProperty =
        AvaloniaProperty.Register<FlexPanel, JustifyContent>(nameof(JustifyContent));

    /// <summary>
    /// Defines the <see cref="AlignItems"/> property.
    /// </summary>
    public static readonly StyledProperty<AlignItems> AlignItemsProperty =
        AvaloniaProperty.Register<FlexPanel, AlignItems>(nameof(AlignItems));

    /// <summary>
    /// Defines the <see cref="AlignContent"/> property.
    /// </summary>
    public static readonly StyledProperty<AlignContent> AlignContentProperty =
        AvaloniaProperty.Register<FlexPanel, AlignContent>(nameof(AlignContent));

    /// <summary>
    /// Defines the <see cref="Wrap"/> property.
    /// </summary>
    public static readonly StyledProperty<FlexWrap> WrapProperty =
        AvaloniaProperty.Register<FlexPanel, FlexWrap>(nameof(Wrap), FlexWrap.Wrap);

    /// <summary>
    /// Defines the <see cref="ColumnSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<FlexPanel, double>(nameof(ColumnSpacing));

    /// <summary>
    /// Defines the <see cref="RowSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<FlexPanel, double>(nameof(RowSpacing));

    private FlexLayoutState? _state;

    static FlexPanel()
    {
        AffectsMeasure<FlexPanel>(
            DirectionProperty,
            JustifyContentProperty,
            WrapProperty,
            ColumnSpacingProperty,
            RowSpacingProperty);

        AffectsArrange<FlexPanel>(
            AlignItemsProperty,
            AlignContentProperty);

        AffectsParentMeasure<FlexPanel>(
            HorizontalAlignmentProperty,
            VerticalAlignmentProperty,
            Flex.OrderProperty,
            Flex.BasisProperty,
            Flex.ShrinkProperty,
            Flex.GrowProperty);

        AffectsParentArrange<FlexPanel>(
            Flex.AlignSelfProperty);
    }

    /// <summary>
    /// Gets or sets the flex direction
    /// </summary>
    public FlexDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the flex justify content mode
    /// </summary>
    public JustifyContent JustifyContent
    {
        get => GetValue(JustifyContentProperty);
        set => SetValue(JustifyContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the flex align items mode
    /// </summary>
    public AlignItems AlignItems
    {
        get => GetValue(AlignItemsProperty);
        set => SetValue(AlignItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the flex align content mode
    /// </summary>
    public AlignContent AlignContent
    {
        get => GetValue(AlignContentProperty);
        set => SetValue(AlignContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the flex wrap mode
    /// </summary>
    public FlexWrap Wrap
    {
        get => GetValue(WrapProperty);
        set => SetValue(WrapProperty, value);
    }

    /// <summary>
    /// Gets or sets the column spacing
    /// </summary>
    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the row spacing
    /// </summary>
    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var children = (IReadOnlyList<Layoutable>)Children;

        var isColumn = Direction is FlexDirection.Column or FlexDirection.ColumnReverse;

        var max = Uv.FromSize(availableSize, isColumn);
        var spacing = Uv.FromSize(ColumnSpacing, RowSpacing, isColumn);

        var (lineU, lineV, lineShrink, lineGrow, lineAutoMargins) = (0.0, 0.0, 0.0, 0.0, 0);
        var (childIndex, firstChildIndex, itemIndex, lineIndex) = (0, 0, 0, 0);

        var lines = new List<FlexLine>();
        children = children.Where(s_isVisible).OrderBy(s_getOrder).ToArray();

        foreach (var element in children)
        {
            var basis = Flex.GetBasis(element);
            var flexConstraint = basis.Kind switch
            {
                FlexBasisKind.Auto => max.U,
                FlexBasisKind.Absolute => basis.Value,
                FlexBasisKind.Relative => max.U * basis.Value / 100,
                _ => throw new InvalidOperationException()
            };
            element.Measure(Uv.ToSize(max.WithU(flexConstraint), isColumn));

            var size = Uv.FromSize(element.DesiredSize, isColumn);
            var flexLength = basis.Kind switch
            {
                FlexBasisKind.Auto => size.U,
                FlexBasisKind.Absolute or FlexBasisKind.Relative => Math.Max(size.U, flexConstraint),
                _ => throw new InvalidOperationException()
            };
            size = size.WithU(flexLength);
            Flex.SetBaseLength(element, flexLength);
            Flex.SetCurrentLength(element, flexLength);

            if (Wrap != FlexWrap.NoWrap && lineU + size.U + itemIndex * spacing.U > max.U)
            {
                lines.Add(new FlexLine(firstChildIndex, childIndex - 1,
                    lineU, lineV, lineShrink, lineGrow, lineAutoMargins));
                (lineU, lineV, lineShrink, lineGrow, lineAutoMargins) = (0.0, 0.0, 0.0, 0.0, 0);
                firstChildIndex = childIndex;
                itemIndex = 0;
                lineIndex++;
            }

            lineU += size.U;
            lineV = Math.Max(lineV, size.V);
            lineShrink += Flex.GetShrink(element);
            lineGrow += Flex.GetGrow(element);
            lineAutoMargins += GetItemAutoMargins(element, isColumn);
            itemIndex++;
            childIndex++;
        }

        if (itemIndex != 0)
        {
            lines.Add(new FlexLine(firstChildIndex, firstChildIndex + itemIndex - 1,
                lineU, lineV, lineShrink, lineGrow, lineAutoMargins));
        }

        var state = new FlexLayoutState(children, lines, Wrap);

        var totalSpacingV = (lines.Count - 1) * spacing.V;
        var panelSizeU = lines.Count > 0 ? lines.Max(line => line.U + (line.Count - 1) * spacing.U) : 0.0;

        // Reizing along main axis using grow and shrink factors can affect cross axis, so remeasure affected items and lines.
        foreach (var line in lines)
        {
            var (itemsCount, totalSpacingU, totalU, freeU) = GetLineMeasureU(line, max.U, spacing.U);
            var (lineMult, autoMargins, remainingFreeU) = GetLineMultInfo(line, freeU);
            if (lineMult != 0.0 && remainingFreeU != 0.0)
            {
                foreach (var element in state.GetLineItems(line))
                {
                    var baseLength = Flex.GetBaseLength(element);
                    var mult = GetItemMult(element, freeU);
                    if (mult != 0.0)
                    {
                        var length = Math.Max(0.0, baseLength + remainingFreeU * mult / lineMult);
                        element.Measure(Uv.ToSize(max.WithU(length), isColumn));
                    }
                }

                line.V = state.GetLineItems(line).Max(i => Uv.FromSize(i.DesiredSize, isColumn).V);
            }
        }

        _state = state;
        var totalLineV = lines.Sum(l => l.V);
        var panelSize = lines.Count == 0 ? default : new Uv(panelSizeU, totalLineV + totalSpacingV);
        return Uv.ToSize(panelSize, isColumn);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var state = _state ?? throw new InvalidOperationException();

        var isColumn = Direction is FlexDirection.Column or FlexDirection.ColumnReverse;
        var isReverse = Direction is FlexDirection.RowReverse or FlexDirection.ColumnReverse;

        var panelSize = Uv.FromSize(finalSize, isColumn);
        var spacing = Uv.FromSize(ColumnSpacing, RowSpacing, isColumn);

        var linesCount = state.Lines.Count;
        var totalLineV = state.Lines.Sum(s => s.V);
        var totalSpacingV = (linesCount - 1) * spacing.V;
        var totalV = totalLineV + totalSpacingV;
        var freeV = panelSize.V - totalV;

        var alignContent = freeV >= 0.0 ? AlignContent : AlignContent switch
        {
            AlignContent.FlexStart or AlignContent.Stretch or AlignContent.SpaceBetween => AlignContent.FlexStart,
            AlignContent.Center or AlignContent.SpaceAround or AlignContent.SpaceEvenly => AlignContent.Center,
            AlignContent.FlexEnd => AlignContent.FlexEnd,
            _ => throw new InvalidOperationException()
        };

        var (spacingV, v) = alignContent switch
        {
            AlignContent.FlexStart => (spacing.V, 0.0),
            AlignContent.FlexEnd => (spacing.V, freeV),
            AlignContent.Center => (spacing.V, freeV / 2),
            AlignContent.Stretch => (spacing.V, 0.0),
            AlignContent.SpaceBetween => (spacing.V + freeV / (linesCount - 1), 0.0),
            AlignContent.SpaceAround => (spacing.V + freeV / linesCount, freeV / linesCount / 2),
            AlignContent.SpaceEvenly => (spacing.V + freeV / (linesCount + 1), freeV / (linesCount + 1)),
            _ => throw new InvalidOperationException()
        };

        var scaleV = alignContent == AlignContent.Stretch ? (panelSize.V - totalSpacingV) / totalLineV : 1.0;

        foreach (var line in state.Lines)
        {
            var lineV = scaleV * line.V;
            var (itemsCount, totalSpacingU, totalU, freeU) = GetLineMeasureU(line, panelSize.U, spacing.U);
            var (lineMult, lineAutoMargins, remainingFreeU) = GetLineMultInfo(line, freeU);

            var currentFreeU = remainingFreeU;
            if (lineMult != 0.0 && remainingFreeU != 0.0)
            {
                foreach (var element in state.GetLineItems(line))
                {
                    var baseLength = Flex.GetBaseLength(element);
                    var mult = GetItemMult(element, freeU);
                    if (mult != 0.0)
                    {
                        var length = Math.Max(0.0, baseLength + remainingFreeU * mult / lineMult);
                        Flex.SetCurrentLength(element, length);
                        currentFreeU -= length - baseLength;
                    }
                }
            }
            remainingFreeU = currentFreeU;

            if (lineAutoMargins != 0 && remainingFreeU != 0.0)
            {
                foreach (var element in state.GetLineItems(line))
                {
                    var baseLength = Flex.GetCurrentLength(element);
                    var autoMargins = GetItemAutoMargins(element, isColumn);
                    if (autoMargins != 0)
                    {
                        var length = Math.Max(0.0, baseLength + remainingFreeU * autoMargins / lineAutoMargins);
                        Flex.SetCurrentLength(element, length);
                        currentFreeU -= length - baseLength;
                    }
                }
            }
            remainingFreeU = currentFreeU;

            var (spacingU, u) = line.Grow > 0 ? (spacing.U, 0.0) : JustifyContent switch
            {
                JustifyContent.FlexStart => (spacing.U, 0.0),
                JustifyContent.FlexEnd => (spacing.U, remainingFreeU),
                JustifyContent.Center => (spacing.U, remainingFreeU / 2),
                JustifyContent.SpaceBetween => (spacing.U + remainingFreeU / (itemsCount - 1), 0.0),
                JustifyContent.SpaceAround => (spacing.U + remainingFreeU / itemsCount, remainingFreeU / itemsCount / 2),
                JustifyContent.SpaceEvenly => (spacing.U + remainingFreeU / (itemsCount + 1), remainingFreeU / (itemsCount + 1)),
                _ => throw new InvalidOperationException()
            };

            foreach (var element in state.GetLineItems(line))
            {
                var size = Uv.FromSize(element.DesiredSize, isColumn).WithU(Flex.GetCurrentLength(element));
                var align = Flex.GetAlignSelf(element) ?? AlignItems;

                var positionV = align switch
                {
                    AlignItems.FlexStart => v,
                    AlignItems.FlexEnd => v + lineV - size.V,
                    AlignItems.Center => v + (lineV - size.V) / 2,
                    AlignItems.Stretch => v,
                    _ => throw new InvalidOperationException()
                };

                size = size.WithV(align == AlignItems.Stretch ? lineV : size.V);
                var position = new Uv(isReverse ? panelSize.U - size.U - u : u, positionV);
                element.Arrange(new Rect(Uv.ToPoint(position, isColumn), Uv.ToSize(size, isColumn)));

                u += size.U + spacingU;
            }

            v += lineV + spacingV;
        }

        return finalSize;
    }

    private static (int ItemsCount, double TotalSpacingU, double TotalU, double FreeU) GetLineMeasureU(
        FlexLine line, double panelSizeU, double spacingU)
    {
        var itemsCount = line.Count;
        var totalSpacingU = (itemsCount - 1) * spacingU;
        var totalU = line.U + totalSpacingU;
        var freeU = panelSizeU - totalU;
        return (itemsCount, totalSpacingU, totalU, freeU);
    }

    private static (double LineMult, double LineAutoMargins, double RemainingFreeU) GetLineMultInfo(FlexLine line, double freeU)
    {
        var lineMult = freeU switch
        {
            < 0 => line.Shrink,
            > 0 => line.Grow,
            _ => 0.0,
        };
        // https://www.w3.org/TR/css-flexbox-1/#remaining-free-space
        // Sum of flex factors less than 1 reduces remaining free space to be distributed.
        return lineMult is > 0 and < 1
            ? (lineMult, line.AutoMargins, freeU * lineMult)
            : (lineMult, line.AutoMargins, freeU);
    }

    private static double GetItemMult(Layoutable element, double freeU)
    {
        var mult = freeU switch
        {
            < 0 => Flex.GetShrink(element),
            > 0 => Flex.GetGrow(element),
            _ => 0.0,
        };
        return mult;
    }

    private static int GetItemAutoMargins(Layoutable element, bool isColumn)
    {
        return isColumn
            ? element.VerticalAlignment switch
            {
                VerticalAlignment.Stretch => 0,
                VerticalAlignment.Top or VerticalAlignment.Bottom => 1,
                VerticalAlignment.Center => 2,
                _ => throw new InvalidOperationException()
            }
            : element.HorizontalAlignment switch
            {
                HorizontalAlignment.Stretch => 0,
                HorizontalAlignment.Left or HorizontalAlignment.Right => 1,
                HorizontalAlignment.Center => 2,
                _ => throw new InvalidOperationException()
            };
    }

    private readonly struct FlexLayoutState
    {
        private readonly IReadOnlyList<Layoutable> _children;

        public IReadOnlyList<FlexLine> Lines { get; }

        public FlexLayoutState(IReadOnlyList<Layoutable> children, List<FlexLine> lines, FlexWrap wrap)
        {
            if (wrap == FlexWrap.WrapReverse)
            {
                lines.Reverse();
            }
            _children = children;
            Lines = lines;
        }

        public IEnumerable<Layoutable> GetLineItems(FlexLine line)
        {
            for (var i = line.First; i <= line.Last; i++)
                yield return _children[i];
        }
    }

    private class FlexLine
    {
        public FlexLine(int first, int last, double u, double v, double shrink, double grow, int autoMargins)
        {
            First = first;
            Last = last;
            U = u;
            V = v;
            Shrink = shrink;
            Grow = grow;
            AutoMargins = autoMargins;
        }

        /// <summary>First item index.</summary>
        public int First { get; }

        /// <summary>Last item index.</summary>
        public int Last { get; }

        /// <summary>Sum of main sizes of items.</summary>
        public double U { get; }

        /// <summary>Max of cross sizes of items.</summary>
        public double V { get; set; }

        /// <summary>Sum of shrink factors of flexible items.</summary>
        public double Shrink { get; }

        /// <summary>Sum of grow factors of flexible items.</summary>
        public double Grow { get; }

        /// <summary>Number of "auto margins" along main axis.</summary>
        public int AutoMargins { get; }

        /// <summary>Number of items.</summary>
        public int Count => Last - First + 1;
    }
}
