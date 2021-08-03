using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Threading.Tasks;

namespace System.Application.UI.Views.Pages
{
    public class Settings_General : UserControl
    {
        TextBlock? cacheSize;

        public Settings_General()
        {
            InitializeComponent();
            cacheSize = this.FindControl<TextBlock>("CacheSize");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            CacheSizeCalcToString();
        }

        public async void CacheSizeCalcToString()
        {
            if (cacheSize is not null)
            {
                MainThread2.BeginInvokeOnMainThread(() =>
                {
                    cacheSize.Text = AppResources.Settings_General_Calcing;
                });
                await Task.Run(() =>
                {
                    var length = IOPath.GetDirectoryLength(IOPath.CacheDirectory);
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        cacheSize.Text = string.Format(AppResources.Settings_General_CacheSize, (length / 1024 / 1024).ToString("0.00") + "MB");
                    });
                });
            }
        }
    }
}