namespace BD.WTTS.UI;

public static class ControlExtensions
{
    public static void SetViewModel<T>(this Control c, bool useCache = false) where T : ViewModelBase, new()
    {
        T cx;

        if (useCache)
            cx = IViewModelManager.Instance.Get<T>();
        else
            cx = new T();

        c.DataContext = cx;
    }
}
