using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Windows
{
    public partial class TaskBarWindow : ReactiveWindow<TaskBarWindowViewModel>
    {
        private bool IsPointerOverSubMenu = false;

        public TaskBarWindow()
        {
            InitializeComponent();

            Topmost = true;
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            ShowInTaskbar = false;
            SizeToContent = Avalonia.Controls.SizeToContent.Height;
            TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;
            CanResize = false;

            this.Opened += Window_Opened;
            this.LostFocus += Window_LostFocus;

            //var localbtn = this.FindControl<Button>("LocalMenu");

            //if (localbtn != null)
            //{
            //    localbtn.PointerEnter += MenuButton_PointerEnter;
            //    //localbtn.PointerLeave += MenuButton_PointerLeave;
            //}
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void MenuButton_PointerLeave(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (sender is Control c)
            {
                var flyout = FlyoutBase.GetAttachedFlyout(c);
                flyout?.Hide();
                IsPointerOverSubMenu = false;
            }
        }

        private void MenuButton_PointerEnter(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (sender is Control c && !IsPointerOverSubMenu)
            {
                var flyout = FlyoutBase.GetAttachedFlyout(c);
                if (flyout is not null)
                {
                    flyout.ShowAt(c);
                    IsPointerOverSubMenu = flyout.IsOpen;
                    flyout.Closed += (sender, e) => IsPointerOverSubMenu = flyout.IsOpen;
                    c.Focus();
                }
            }
        }

        private void Window_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!this.IsPointerOver && !IsPointerOverSubMenu)
            {
                if (this.DataContext is TaskBarWindowViewModel vm)
                {
                    this.Hide();
                }
            }
        }

        private void Window_Opened(object? sender, EventArgs e)
        {
            if (this.DataContext is TaskBarWindowViewModel vm)
            {
                //if (vm.SizePosition.X > 0 && vm.SizePosition.Y > 0)
                //{
                //    this.Position = new PixelPoint(vm.SizePosition.X, vm.SizePosition.Y - (int)this.Height);
                //}

                vm.WhenAnyValue(x => x.SizePosition.X, x => x.SizePosition.Y)
                    .Subscribe(x =>
                    {
                        var screen = this.Screens.ScreenFromPoint(new PixelPoint(x.Item1, x.Item2));
                        var heightPoint = x.Item2 - (int)(this.Height * screen.PixelDensity);

                        if (heightPoint < 0)
                        {
                            this.Position = new PixelPoint(x.Item1, x.Item2);
                        }
                        else if ((x.Item1 + (int)this.Width + 30) > screen.WorkingArea.Width)
                        {
                            this.Position = new PixelPoint(x.Item1 - (int)(this.Width * screen.PixelDensity), heightPoint);
                        }
                        else
                        {
                            this.Position = new PixelPoint(x.Item1, heightPoint);
                        }
                    });
            }

            DI.Get<ISystemWindowApiService>().SetActiveWindow(new Models.HandleWindow { Handle = this.PlatformImpl.Handle.Handle });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
