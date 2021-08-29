using ReactiveUI;
using System.Application.Services;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Windows;
using System.Windows.Threading;

namespace System.Application.UI.ViewModels
{
    public class TaskBarWindowViewModel : WindowViewModel
    {
#pragma warning disable CA1822 // 将成员标记为 static
        public new string Title => TitleString;
#pragma warning restore CA1822 // 将成员标记为 static

        public static string TitleString => ThisAssembly.AssemblyTrademark;

        public string Version => ThisAssembly.Version;

        //private ObservableCollection<TabItemViewModel> _tabs;
        //public ObservableCollection<TabItemViewModel> Tabs
        //{
        //    get => _tabs;
        //    set => this.RaiseAndSetIfChanged(ref _tabs, value);
        //}

        public async void Show(int x, int y)
        {
            SetPosition(x, y);
            await IShowWindowService.Instance.Show(CustomWindow.TaskBar, this, string.Empty, ResizeModeCompat.NoResize, isParent: false);
        }

        public void SetPosition(int x, int y)
        {
            SizePosition.X = x;
            SizePosition.Y = y;
        }

        public ReactiveCommand<string, Unit> MenuClickCommand { get; }

        public TaskBarWindowViewModel()
        {
            MenuClickCommand = ReactiveCommand.Create<string>(MenuClick);
        }

        public const string CommandExit = "Exit";

        public void MenuClick(string tag) => OnMenuClickCore(tag, Hide);

        static void OnMenuClick(Func<TabItemViewModel, bool> predicate, Action? hideTaskBarWindow = null)
        {
            if (IWindowService.Instance.MainWindow is MainWindowViewModel main)
            {
                var tab = main.TabItems.FirstOrDefault(predicate);
                tab ??= main.FooterTabItems.FirstOrDefault(predicate);
                if (tab != null)
                {
                    DI.Get<IDesktopAppService>().RestoreMainWindow();
                    main.SelectedItem = tab;
                    hideTaskBarWindow?.Invoke();
                }
            }
        }

        static bool OnMenuClickCore(string tag, Action? hideTaskBarWindow = null, MainWindowViewModel.TabItemId tabItemId = default)
        {
            switch (tag)
            {
                case CommandExit:
                    DI.Get<IDesktopAppService>().Shutdown();
                    return true;
                default:
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        if (tabItemId != default)
                            OnMenuClick(s =>
                                s is MainWindowViewModel.ITabItemViewModel item &&
                                    item.Id == tabItemId, hideTaskBarWindow);
                        else
                            OnMenuClick(s => s.Name == tag, hideTaskBarWindow);
                    }, DispatcherPriorityCompat.MaxValue);
                    break;
            }
            return false;
        }

        public static bool OnMenuClick(string command)
        {
            if (Enum.TryParse<MainWindowViewModel.TabItemId>(command, out var tabItemId))
                return OnMenuClick(tabItemId);
            else
                return OnMenuClickCore(command);
        }

        public static bool OnMenuClick(MainWindowViewModel.TabItemId tabItemId) => OnMenuClickCore(string.Empty, tabItemId: tabItemId);
    }
}