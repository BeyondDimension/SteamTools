using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.Services;

namespace System.Application.UI.Views.Controls
{
    public class ToastMessage : UserControl
    {
        public ToastMessage()
        {
            InitializeComponent();
            var show = this.GetObservable(IsVisibleProperty);
            show.Subscribe(value =>
            {
                if (value == true)
                {
                    var window = this.VisualRoot as Window;
                    if (window.IsActiveWindow())
                    {
                        return;
                    }
                    IsVisible = false;
                }
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
