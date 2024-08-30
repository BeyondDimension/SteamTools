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

            this.Bind(ViewModel, vm => vm.SelectedSTUNAddress, v => v.NetworkCheckControl.STUNServer.SelectedItem).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.NATCheckCommand, v => v.NetworkCheckControl.NATCheckButton).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.LocalEndPoint, v => v.NetworkCheckControl.LocalIPAddress.Text).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.DNSCheckCommand, v => v.NetworkCheckControl.DNSCheckButton).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.DomainPendingTest, v => v.NetworkCheckControl.DomainTextBox.Text).DisposeWith(disposables);

            NetworkCheckControl.PingOK.IsVisible = false;
            NetworkCheckControl.PingError.IsVisible = false;
            NetworkCheckControl.NATCheckButton.Click += (_, _) => NetworkCheckControl.PingError.IsVisible = NetworkCheckControl.PingOK.IsVisible = false;
            ViewModel!.NATCheckCommand
                .Subscribe(result =>
                {
                    (NetworkCheckControl.NATTextBlock.Text, NetworkCheckControl.NATTypeTip.Text) = result.Nat switch
                    {
                        AcceleratorPageViewModel.NatTypeSimple.Open => ("开放 NAT", "您可与在其网络上具有任意 NAT 类型的用户玩多人游戏和发起多人游戏。"),
                        AcceleratorPageViewModel.NatTypeSimple.Moderate => ("中等 NAT", "您可与一些用户玩多人游戏；但是，并且通常你将不会被选为比赛的主持人。"),
                        AcceleratorPageViewModel.NatTypeSimple.Strict => ("严格 NAT", "您只能与具有开放 NAT 类型的用户玩多人游戏。您不能被选为比赛的主持人。"),
                        _ => ("不可用 NAT", "如果 NAT 不可用，您将无法使用群聊天或连接到某些 Xbox 游戏的多人游戏。"),
                    };

                    if (result.PingSuccess)
                    {
                        NetworkCheckControl.PingOK.IsVisible = true;
                    }
                    else
                    {
                        NetworkCheckControl.PingError.IsVisible = true;
                    }
                }).DisposeWith(disposables);
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