using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Properties;
using System.Reactive;
using static System.Application.MainThread2;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class TaskBarWindowViewModel : WindowViewModel
    {
        public string Version => ThisAssembly.Version;

        public IEnumerable<TabItemViewModel>? Tabs { get; }

        public async void Show(int x, int y)
        {
            SetPosition(x, y);
            await IWindowManager.Instance.Show(CustomWindow.TaskBar, this, resizeMode: ResizeMode.NoResize, isParent: false);
        }

        public void SetPosition(int x, int y)
        {
            SizePosition.X = x;
            SizePosition.Y = y;
        }

        public ReactiveCommand<object, Unit> MenuClickCommand { get; }

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
                if (main.AllTabLazyItems.ContainsKey(tabItemType))
                {
                    var tab = main.AllTabLazyItems[tabItemType].Value;
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
                case CommandExit:
                    IApplication.Instance.Shutdown();
                    return true;
                default:
                    if (tabItemId.IsDefined())
                    {
                        BeginInvokeOnMainThread(() =>
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
}