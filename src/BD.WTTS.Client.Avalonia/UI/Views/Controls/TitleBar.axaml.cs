using Avalonia.Controls;
using BD.Common.Models.Abstractions;

namespace BD.WTTS.UI.Views.Controls;

public partial class TitleBar : UserControl
{
    public Image WindowIcon => BarIcon;

    /// <summary>
    /// Defines the <see cref="Icon"/> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> IconProperty =
        Image.SourceProperty.AddOwner<TitleBar>();

    /// <summary>
    /// Defines the <see cref="ActionContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<TitleBar, object?>(nameof(ActionContent));

    /// <summary>
    /// Defines the <see cref="IsShowSearchBox"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowSearchBoxProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsShowSearchBox));

    /// <summary>
    /// Defines the <see cref="SearchText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> SearchTextProperty =
        AvaloniaProperty.Register<TitleBar, string>(nameof(SearchText));

    public IImage? Icon
    {
        get { return GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    public object? ActionContent
    {
        get { return GetValue(ActionContentProperty); }
        set { SetValue(ActionContentProperty, value); }
    }

    public bool IsShowSearchBox
    {
        get { return GetValue(IsShowSearchBoxProperty); }
        set { SetValue(IsShowSearchBoxProperty, value); }
    }

    public string SearchText
    {
        get { return GetValue(SearchTextProperty); }
        set { SetValue(SearchTextProperty, value); }
    }

    public TitleBar()
    {
        InitializeComponent();
    }
}
