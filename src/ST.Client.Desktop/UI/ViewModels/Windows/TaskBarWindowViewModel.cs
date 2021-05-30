using ReactiveUI;
using System.Application.Services;
using System.Collections.ObjectModel;
using System.Properties;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using System.Reactive;

namespace System.Application.UI.ViewModels
{
    public class TaskBarWindowViewModel : WindowViewModel
    {
        private bool _IsVisible;
        public bool IsVisible
        {
            get => _IsVisible;
            set => this.RaiseAndSetIfChanged(ref _IsVisible, value);
        }

        public new string Title => ThisAssembly.AssemblyTrademark;

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
            IsVisible = true;
            await IShowWindowService.Instance.Show(CustomWindow.TaskBar, this, string.Empty, ResizeModeCompat.NoResize, isParent: false);
        }

        public void SetPosition(int x, int y)
        {
            SizePosition.X = x;
            SizePosition.Y = y;
        }

        public override void Hide()
        {
            IsVisible = false;
            base.Hide();
        }

        public ReactiveCommand<string, Unit> MenuClickCommand { get; }

        public TaskBarWindowViewModel()
        {
            MenuClickCommand = ReactiveCommand.Create<string>(MenuClick);
        }

        public void MenuClick(string tag)
        {
            switch (tag)
            {
                case "Exit":
                    DI.Get<IDesktopAppService>().Shutdown();
                    break;
                default:
                    if (IWindowService.Instance.MainWindow is MainWindowViewModel main)
                    {
                        var tab = main.TabItems.FirstOrDefault(s => s.Name == tag);
                        if (tab != null)
                        {
                            DI.Get<IDesktopAppService>().RestoreMainWindow();
                            main.SelectedItem = tab;
                            Hide();
                        }
                    }
                    break;
            }
        }
    }
}