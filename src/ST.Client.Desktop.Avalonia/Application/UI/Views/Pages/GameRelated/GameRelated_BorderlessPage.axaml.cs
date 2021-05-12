using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using CefNet;
using System.Application.UI.Resx;
using System.Application.UI.Views.Controls;

namespace System.Application.UI.Views.Pages
{
    public class GameRelated_BorderlessPage : UserControl
    {
        public GameRelated_BorderlessPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}