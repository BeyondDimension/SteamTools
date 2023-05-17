using Avalonia.Controls;
using BD.Common.Models.Abstractions;

namespace BD.WTTS.UI.Views.Controls;

public partial class TitleBar : UserControl
{
    /// <summary>
    /// Defines the <see cref="Source"/> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<TitleBar, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get { return GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    public TitleBar()
    {
        InitializeComponent();
    }
}
