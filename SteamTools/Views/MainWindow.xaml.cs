using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MetroRadiance.Interop.Win32;
using MetroRadiance.UI.Controls;
using SteamTools.Services;
using SteamTools.ViewModels;

namespace SteamTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow: MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (this.WindowSettings != null)
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                var placement = User32.GetWindowPlacement(hwnd);

                this.WindowSettings.Placement = this.IsRestoringWindowPlacement ? (WINDOWPLACEMENT?)placement : null;
                this.WindowSettings.Save();
            }
            (WindowService.Current.MainWindow as MainWindowViewModel).IsVisible = false;
        }
    }
}
