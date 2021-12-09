using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Pages
{
    public class Settings_General : UserControl
    {
        readonly TextBlock? cacheSize;
        readonly TextBlock? logSize;
        public Settings_General()
        {
            InitializeComponent();
            cacheSize = this.FindControl<TextBlock>("CacheSize");
            logSize = this.FindControl<TextBlock>("LogSize");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        //bool isStartCacheSizeCalc;
        //bool isStartLogSizeCalc;
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            //SettingsPageViewModel.StartCacheSizeCalc(ref isStartCacheSizeCalc, AppResources.Settings_General_CacheSize, x =>
            //{
            //    if (cacheSize is not null) cacheSize.Text = x;
            //});
            //SettingsPageViewModel.StartCacheSizeCalc(ref isStartLogSizeCalc, AppResources.Settings_General_LogSize, x =>
            //{
            //    if (logSize is not null) logSize.Text = x;
            //});
            SettingsPageViewModel.StartSizeCalcByCacheSize(x =>
            {
                if (cacheSize is not null) cacheSize.Text = x;
            });
            SettingsPageViewModel.StartSizeCalcByLogSize(x =>
            {
                if (logSize is not null) logSize.Text = x;
            });
        }
    }
}