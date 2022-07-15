using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using System.Application.Services;

namespace System.Application.UI.Views.Controls
{
    public class TitleBar : UserControl
    {
        public static readonly StyledProperty<bool> IsVisibleBackgroundProperty =
            AvaloniaProperty.Register<TitleBar, bool>(nameof(IsVisibleBackground), true);

        public bool IsVisibleBackground
        {
            get { return GetValue(IsVisibleBackgroundProperty); }
            set { SetValue(IsVisibleBackgroundProperty, value); }
        }

        public const int DefaultHeight = 32;

        /// <summary>
        /// 是否需要显示 TitleBar
        /// </summary>
        /// <returns></returns>
        public static bool GetIsVisible()
        {
#pragma warning disable CA1416 // 验证平台兼容性
            if (OperatingSystem2.IsWindows())
            {
                if (OperatingSystem2.IsWindows7())
                {
                    if (!IPlatformService.Instance.DwmIsCompositionEnabled)
                    {
                        return false;
                    }
                }
            }
#pragma warning restore CA1416 // 验证平台兼容性
            return true;
        }

        public TitleBar()
        {
            InitializeComponent();

            if (!GetIsVisible())
            {
                IsVisible = false;
                return;
            }

#pragma warning disable CA1416 // 验证平台兼容性
            if (OperatingSystem2.IsMacOS())
            {
                var title = this.FindControl<StackPanel>("title");
                title.HorizontalAlignment = HorizontalAlignment.Center;
            }
#pragma warning restore CA1416 // 验证平台兼容性

            var back = this.FindControl<ExperimentalAcrylicBorder>("Back");

            this.GetObservable(IsVisibleBackgroundProperty)
                  .Subscribe(x => back.IsVisible = x);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
