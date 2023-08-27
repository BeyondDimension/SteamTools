namespace BD.WTTS.UI;

public static class ControlExtensions
{
    public static void SetViewModel<T>(this Control c) where T : ViewModelBase
    {
        if (c.DataContext == null || c.DataContext.GetType() != typeof(T))
        {
            Task2.InBackground(() =>
            {
                var cx = IViewModelManager.Instance.Get<T>();
                Dispatcher.UIThread.Post(() => c.DataContext = cx);
            });
        }
    }
}
