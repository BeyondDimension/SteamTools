using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class HomePage : ReactiveUserControl<HomePageViewModel>
{
    public HomePage()
    {
        InitializeComponent();
        this.SetViewModel<HomePageViewModel>();

        ShopsScrollViewer.PointerWheelChanged += ShopsScrollViewer_PointerWheelChanged;
    }

    private void ShopsScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (ShopsScrollViewer.IsPointerOver)
        {
            // 将滚动事件传递给最内层的 ScrollViewer
            ShopsScrollViewer.RaiseEvent(e);
            e.Handled = true;
        }
    }
}
