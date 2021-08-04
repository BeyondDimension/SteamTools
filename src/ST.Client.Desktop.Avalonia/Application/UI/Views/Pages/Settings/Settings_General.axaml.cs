using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Pages
{
    public class Settings_General : UserControl
    {
        readonly TextBlock? cacheSize;
        public Settings_General()
        {
            InitializeComponent();
            cacheSize = this.FindControl<TextBlock>("CacheSize");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        bool isStartCacheSizeCalc;
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            SettingsPageViewModel.StartCacheSizeCalc(ref isStartCacheSizeCalc, x =>
            {
                if (cacheSize is not null) cacheSize.Text = x;
            });
        }
    }
}