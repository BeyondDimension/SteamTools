using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Pages;

/// <summary>
/// 网络加速页面
/// </summary>
public partial class AcceleratorPage2 : PageBase<AcceleratorPageViewModel>
{
    readonly Dictionary<string, string[]> dictPinYinArray = new();

    List<int> acceleratorTabsSelectedIndexs = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AcceleratorPage2"/> class.
    /// </summary>
    public AcceleratorPage2()
    {
        //Tabstrip有BUG会导致界面初始化后值被重置，所以这里先保存一下
        var mode = ProxySettings.ProxyMode.Value;

        InitializeComponent();
        this.SetViewModel<AcceleratorPageViewModel>(true);

        for (int i = 0; i < AcceleratorTabs.Items.Count; i++)
        {
            var item = AcceleratorTabs.Items[i];
            if (item is Visual visual && visual.IsVisible)
            {
                acceleratorTabsSelectedIndexs.Add(i);
            }
        }
        var itemCount = AcceleratorTabs.ItemCount;
        var acceleratorTabsSelectedIndex = ProxySettings.AcceleratorTabsSelectedIndex.Value;
        if (acceleratorTabsSelectedIndex >= 0 && acceleratorTabsSelectedIndex < itemCount)
        {
            if (acceleratorTabsSelectedIndexs.Contains(acceleratorTabsSelectedIndex))
            {
                AcceleratorTabs.SelectedIndex = acceleratorTabsSelectedIndex;
            }
        }

        AcceleratorTabs.SelectionChanged += AcceleratorTabs_SelectionChanged;

        //重置回保存的值
        ProxySettings.ProxyMode.Value = mode;
        this.WhenActivated(disposables =>
        {
            disposables.Add(
                ProxyService.Current.WhenValueChanged(x => x.ProxyStatus, false)
                    .Subscribe(x =>
                    {
                        AcceleratorTabs.SelectedIndex = x ? 1 : 0;
                    }));

            if (XunYouSDK.IsSupported)
            {
                disposables.Add(
                    GameAcceleratorService.Current.WhenValueChanged(x => x.CurrentAcceleratorGame, false)
                        .Subscribe(x =>
                        {
                            if (x != null && GameAcceleratorService.Current.Games != null)
                                GameAcceleratorService.Current.Games.AddOrUpdate(x);
                            GameScrollViewer.ScrollToHome();
                        }));
            }
            else
            {
                GameAccTab.IsVisible = false;
                AcceleratorTabs.SelectedIndex = 0;
            }
        });

        SearchGameBox.DropDownClosed += SearchGameBox_DropDownClosed;
        SearchGameBox.TextSelector = (_, _) => null!;
        SearchGameBox.TextFilter = GameContains;
    }

    void AcceleratorTabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var value = AcceleratorTabs.SelectedIndex;
        if (acceleratorTabsSelectedIndexs.Contains(value))
        {
            ProxySettings.AcceleratorTabsSelectedIndex.Value = value;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (AcceleratorTabs.SelectedIndex == default && ProxyService.Current.ProxyStatus)
            AcceleratorTabs.SelectedIndex = 1;
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
    //protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    //{
    //    base.OnDetachedFromVisualTree(e);

    //    //try
    //    //{
    //    //    ISettingsLoadService.Current.ForceSave<GameAcceleratorSettingsModel>();
    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    Log.Error(nameof(AcceleratorPage), ex, "ForceSave fail.");
    //    //}
    //}
}