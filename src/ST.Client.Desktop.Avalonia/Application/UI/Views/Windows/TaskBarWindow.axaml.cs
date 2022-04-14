using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.Styling;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.ViewModels;
using System.Threading.Tasks;

namespace System.Application.UI.Views.Windows
{
    public partial class TaskBarWindow : ReactiveWindow<TaskBarWindowViewModel>
    {
        private bool IsPointerOverSubMenu = false;

        public TaskBarWindow()
        {
            TransparencyLevelHint = (WindowTransparencyLevel)UISettings.WindowBackgroundMateria.Value;

            if (TransparencyLevelHint == WindowTransparencyLevel.Transparent ||
                TransparencyLevelHint == WindowTransparencyLevel.None ||
                TransparencyLevelHint == WindowTransparencyLevel.Blur)
            {
                TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;
            }

            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            SystemDecorations = SystemDecorations.Full;
            SizeToContent = Avalonia.Controls.SizeToContent.Height;
            IsVisible = false;
            //Initialized += Window_Opened;
            Opened += Window_Opened;
            LostFocus += Window_LostFocus;

            //var localAuthbtn = this.FindControl<Button>("LocalAuthMenu");
            //var userChangebtn = this.FindControl<Button>("UserChangeMenu");
            //if (userChangebtn != null)
            //{
            //    userChangebtn.PointerEnter += MenuButton_PointerEnter;
            //    //localbtn.PointerLeave += MenuButton_PointerLeave;
            //}
            //if (localAuthbtn != null)
            //{
            //    localAuthbtn.PointerEnter += MenuButton_PointerEnter;
            //    //localbtn.PointerLeave += MenuButton_PointerLeave;
            //}

            //if (OperatingSystem2.IsWindows11AtLeast)
            //{
            //    AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ForceNativeTitleBarToTheme(this);
            //}
            
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }

        public void MenuButton_PointerLeave(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            //if (sender is Control c)
            //{
            //    //var flyout = FlyoutBase.GetAttachedFlyout(c);
            //    //flyout?.Hide();
            //    //IsPointerOverSubMenu = false;
            //}
        }

        public async void MenuButton_PointerEnter(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (sender is Control c && !IsPointerOverSubMenu)
            {
                if (c.Tag is TabItemViewModel tab && !tab.IsTaskBarSubMenu) return;

                var flyout = FlyoutBase.GetAttachedFlyout(c);
                await Task.Delay(500);
                if (flyout is not null && c.IsPointerOver && !IsPointerOverSubMenu)
                {
                    flyout.ShowAt(c);
                    IsPointerOverSubMenu = flyout.IsOpen;
                    flyout.Closed += (sender, e) => IsPointerOverSubMenu = flyout.IsOpen;
                    //c.Focus();
                }
            }
        }

        private void Window_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!IsPointerOver && !IsPointerOverSubMenu)
            {
                Close();
            }
        }

        private void Window_Opened(object? sender, EventArgs e)
        {
            if (DataContext is TaskBarWindowViewModel vm)
            {
                //if (vm.SizePosition.X > 0 && vm.SizePosition.Y > 0)
                //{
                //    this.Position = new PixelPoint(vm.SizePosition.X, vm.SizePosition.Y - (int)this.Height);
                //}

                vm.WhenAnyValue(x => x.SizePosition.X, x => x.SizePosition.Y)
                    .Subscribe(x =>
                    {
                        var screen = Screens.ScreenFromPoint(new PixelPoint(x.Item1, x.Item2));
                        var heightPoint = x.Item2 - (int)(Height * screen.PixelDensity);

                        if (heightPoint < 0)
                        {
                            Position = new PixelPoint(x.Item1, x.Item2);
                        }
                        else if ((x.Item1 + (int)Width + 30) > screen.WorkingArea.Width)
                        {
                            Position = new PixelPoint(x.Item1 - (int)(Width * screen.PixelDensity), heightPoint);
                        }
                        else
                        {
                            Position = new PixelPoint(x.Item1, heightPoint);
                        }
                    });
            }

            if (OperatingSystem2.IsWindows)
            {
                //INativeWindowApiService.Instance!.SetActiveWindow(new() { Handle = PlatformImpl.Handle.Handle });
                Topmost = false;
                Topmost = true;
                IAvaloniaApplication.Instance.SetTopmostOneTime();
            }

            IsVisible = true;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
