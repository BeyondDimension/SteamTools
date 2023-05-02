// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

/// <summary>
/// Windows 任务栏托盘的右键窗口
/// </summary>
public sealed partial class TaskBarWindowViewModel : WindowViewModel
{
    public static TaskBarWindowViewModel Instance => IViewModelManager.Instance.TaskBarWindow ?? new TaskBarWindowViewModel();

    public string Version => AssemblyInfo.Version;

    public static string TitleString => AssemblyInfo.Trademark;

    public const string CommandExit = "Exit";

    public const string CommandRestore = "Restore";

    public IEnumerable<TabItemViewModel>? Tabs { get; }

    public async void Show(int x, int y)
    {
        SetPosition(x, y);
        await IWindowManager.Instance.ShowAsync(AppEndPoint.TaskBar, this, resizeMode: ResizeMode.NoResize, isParent: false);
    }

    public void SetPosition(int x, int y)
    {
        SizePosition.X = x;
        SizePosition.Y = y;
    }

    public ICommand MenuClickCommand { get; }

    public TaskBarWindowViewModel() : this(IViewModelManager.Instance.MainWindow)
    {

    }

    public TaskBarWindowViewModel(MainWindowViewModel? mainwindow)
    {
        MenuClickCommand = ReactiveCommand.Create<object>(MenuClick);
        if (mainwindow != null)
        {
            Tabs = mainwindow.TabItems;
        }
    }

    public TaskBarWindowViewModel(WindowViewModel? mainwindow) : this(mainwindow is MainWindowViewModel mainwindow2 ? mainwindow2 : null)
    {

    }

    public const TabItemViewModel.TabItemId SettingsId = TabItemViewModel.TabItemId.Settings;

    public void MenuClick(object tag)
    {
        if (tag is string str)
        {
            OnMenuClickCore(str, Close);
        }
        else if (tag is TabItemViewModel.TabItemId id)
        {
            OnMenuClickCore(string.Empty, Close, id);
        }
    }

    static void OnMenuClick(TabItemViewModel.TabItemId tabItemId, Action? hideTaskBarWindow = null)
    {
        if (IViewModelManager.Instance.MainWindow is MainWindowViewModel main)
        {
            var tabItemType = TabItemViewModel.GetType(tabItemId);
            if (main.AllTabLazyItems.TryGetValue(tabItemType, out var value))
            {
                var tab = value.Value;
                IApplication.Instance.RestoreMainWindow();
                main.SelectedItem = tab;
                hideTaskBarWindow?.Invoke();
            }
        }
    }

    static bool OnMenuClickCore(string tag, Action? hideTaskBarWindow = null, TabItemViewModel.TabItemId tabItemId = default)
    {
        switch (tag)
        {
            case CommandRestore:
                IApplication.Instance.RestoreMainWindow();
                return true;
            case CommandExit:
                IApplication.Instance.Shutdown();
                return true;
            default:
                if (tabItemId.IsDefined())
                {
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        OnMenuClick(tabItemId, hideTaskBarWindow);
                    }, ThreadingDispatcherPriority.MaxValue);
                }
                break;
        }
        return false;
    }

    public static bool OnMenuClick(string command)
    {
        if (Enum.TryParse<TabItemViewModel.TabItemId>(command, out var tabItemId))
            return OnMenuClick(tabItemId);
        else
            return OnMenuClickCore(command);
    }

    public static bool OnMenuClick(TabItemViewModel.TabItemId tabItemId) => OnMenuClickCore(string.Empty, tabItemId: tabItemId);
}