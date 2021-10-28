using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Properties;

namespace System.Application.UI.Views.Windows
{
    public class ExportAuthWindow : FluentWindow<ExportAuthWindowViewModel>
    {
        public ExportAuthWindow() : base()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
