using Avalonia.Controls.Primitives;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class AcceleratorPage : PageBase<AcceleratorPageViewModel>
{
    public AcceleratorPage()
    {
        InitializeComponent();
        DataContext = new AcceleratorPageViewModel();

        Title = Strings.CommunityFix;
        Subtitle = "插件作者: Steam++ 官方";
        Description = "提供一些游戏相关网站服务的加速功能。";

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        Tabs.Items.Add(new TabStripItem { Content = "网络加速", Tag = typeof(ProxyPage) });
        Tabs.Items.Add(new TabStripItem { Content = "脚本配置", Tag = typeof(ScriptPage) });
    }

    private void Tabs_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        //foreach (var item in e.AddedItems)
        //{
        //    if (item is TabStripItem tab && tab.Tag is Type t)
        //    {
        //        NavigationService.Instance.Navigate(t);
        //    }
        //}
    }
}
