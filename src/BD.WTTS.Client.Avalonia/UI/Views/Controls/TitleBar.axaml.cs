using Avalonia.Controls;
using BD.Common.Models.Abstractions;

namespace BD.WTTS.UI.Views.Controls;

public partial class TitleBar : UserControl
{
    public Image WindowIcon => BarIcon;

    internal Control WindowDragRegion => DragRegion;

    /// <summary>
    /// Defines the <see cref="Icon"/> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> IconProperty =
        Image.SourceProperty.AddOwner<TitleBar>();

    public IImage? Icon
    {
        get { return GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    [GeneratedStyledProperty]
    public partial object? ActionContent { get; set; }

    [GeneratedStyledProperty]
    public partial bool IsShowSearchBox { get; set; }

    [GeneratedStyledProperty]
    public partial string SearchText { get; set; }

    public TitleBar()
    {
        InitializeComponent();
    }
}
