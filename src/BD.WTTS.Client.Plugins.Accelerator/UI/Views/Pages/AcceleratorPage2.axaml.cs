using Avalonia;
using Avalonia.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace BD.WTTS.UI.Views.Pages;

/// <summary>
/// 网络加速页面
/// </summary>
public partial class AcceleratorPage2 : PageBase<AcceleratorPageViewModel>
{
    readonly Dictionary<string, string[]> dictPinYinArray = new();

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
            disposables.Add(
                GameAcceleratorService.Current.WhenPropertyChanged(x => x.CurrentAcceleratorGame, false)
                    .Subscribe(x =>
                    {
                        GameScrollViewer.ScrollToHome();
                    }));
        });

        SearchGameBox.DropDownClosed += SearchGameBox_DropDownClosed;
        SearchGameBox.TextSelector = (_, _) => null;
        SearchGameBox.TextFilter = GameContains;
    }

    private void SearchGameBox_DropDownClosed(object? sender, EventArgs e)
    {
        if (SearchGameBox.SelectedItem is XunYouGame xunYouGame && xunYouGame is not null)
        {
            GameAcceleratorService.AddMyGame(xunYouGame);
            SearchGameBox.Text = null;
            SearchGameBox.SelectedItem = null;
            GameScrollViewer.ScrollToHome();
        }
    }

    //private void SearchGameBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    //{
    //    if (SearchGameBox.SelectedItem is XunYouGame xunYouGame && xunYouGame is not null)
    //    {
    //        GameAcceleratorService.AdddMyGame(xunYouGame);
    //        SearchGameBox.Text = null;
    //        SearchGameBox.SelectedItem = null;
    //        GameScrollViewer.ScrollToHome();
    //    }
    //}

    private bool GameContains(string? text, string? s)
    {
        if (string.IsNullOrEmpty(s))
            return false;
        if (string.IsNullOrEmpty(text))
            return true;
        if (s.Contains(text, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        var pinyinArray = Pinyin.GetPinyin(s, dictPinYinArray);
        if (Pinyin.SearchCompare(text, s, pinyinArray))
        {
            return true;
        }
        return false;
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
