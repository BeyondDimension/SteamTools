using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using System.Application.Services;

namespace System.Application.UI.Views.Pages;

public partial class UserItem : UserControl
{
    public UserItem()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (UserService.Current.User != null)
        {
            var lastSignInTime = UserService.Current.LastSignInTime;
            if (lastSignInTime.HasValue)
            {
                if (DateTimeOffset.Now - lastSignInTime.Value >= TimeSpan.FromDays(1))
                {
                    UserService.Current.User.IsSignIn = true;
                }
            }
        }

        base.OnAttachedToLogicalTree(e);
    }
}
