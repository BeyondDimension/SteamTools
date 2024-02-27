using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Controls;

/// <summary>
/// 游戏加速控件
/// </summary>
public partial class GameAccelerator : ReactiveUserControl<GameAcceleratorViewModel>
{
    /// <summary>
    /// Defines the <see cref="ItemsPerPage"/> property.
    /// </summary>
    public static readonly StyledProperty<int> ItemsPerPageProperty =
        AvaloniaProperty.Register<CarouselItems, int>(nameof(ItemsPerPage), 4);

    /// <summary>
    /// ItemsPerPage
    /// </summary>
    public int ItemsPerPage
    {
        get => GetValue(ItemsPerPageProperty);
        set => SetValue(ItemsPerPageProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameAccelerator"/> class.
    /// </summary>
    public GameAccelerator()
    {
        InitializeComponent();
        DataContext = new GameAcceleratorViewModel();

        SearchGameBox.SelectionChanged += SearchGameBox_SelectionChanged;
    }

    private void SearchGameBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SearchGameBox.SelectedItem is XunYouGame xunYouGame && xunYouGame is not null)
        {
            GameAcceleratorService.AddMyGame(xunYouGame);
            SearchGameBox.Text = null;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsPerPageProperty)
        {
            GameCarousel.ItemsPerPage = ItemsPerPage;
        }
    }
}
