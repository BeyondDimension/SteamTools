using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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

        public TitleBar()
        {
            InitializeComponent();

            if (OperatingSystem2.IsMacOS)
            {
                var title = this.FindControl<StackPanel>("title");
                title.HorizontalAlignment = HorizontalAlignment.Center;
            }

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
