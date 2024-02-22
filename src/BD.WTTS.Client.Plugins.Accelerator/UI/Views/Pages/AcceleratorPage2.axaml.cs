using Avalonia;
using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

/// <summary>
/// 网络加速页面
/// </summary>
public partial class AcceleratorPage2 : PageBase<AcceleratorPageViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AcceleratorPage2"/> class.
    /// </summary>
    public AcceleratorPage2()
    {
        InitializeComponent();
        this.SetViewModel<AcceleratorPageViewModel>();

        this.WhenActivated(disposables =>
        {
            disposables.Add(
                ProxyService.Current.WhenPropertyChanged(x => x.ProxyStatus, false)
                    .Subscribe(x =>
                    {
                        AcceleratorTabs.SelectedIndex = x.Value ? 2 : 1;
                    }));
        });

        SearchGameBox.SelectionChanged += SearchGameBox_SelectionChanged;
    }

    private void SearchGameBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SearchGameBox.SelectedItem is XunYouGame xunYouGame && xunYouGame is not null)
        {
            GameAcceleratorService.AdddMyGame(xunYouGame);
            SearchGameBox.Text = null;
            SearchGameBox.SelectedItem = null;
            GameScrollViewer.ScrollToHome();
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        //try
        //{
        //    ISettingsLoadService.Current.ForceSave<GameAcceleratorSettingsModel>();
        //}
        //catch (Exception ex)
        //{
        //    Log.Error(nameof(AcceleratorPage), ex, "ForceSave fail.");
        //}
    }
}
