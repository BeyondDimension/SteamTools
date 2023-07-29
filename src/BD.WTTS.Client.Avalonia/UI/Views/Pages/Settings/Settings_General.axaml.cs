using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class Settings_General : UserControl
{
    public Settings_General()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        SettingsPageViewModel.StartSizeCalcByCacheSize(x =>
        {
            if (CacheSize is not null) CacheSize.Text = x;
        });
        SettingsPageViewModel.StartSizeCalcByLogSize(x =>
        {
            if (LogSize is not null) LogSize.Text = x;
        });
    }
}
