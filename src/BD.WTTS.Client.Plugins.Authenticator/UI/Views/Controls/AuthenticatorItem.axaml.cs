using Avalonia.Controls;
using Avalonia.Input;

namespace BD.WTTS.UI.Views.Controls;
public partial class AuthenticatorItem : UserControl
{
    public AuthenticatorItem()
    {
        InitializeComponent();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (DataContext is AuthenticatorItemModel authenticatorItemModel)
        {
            authenticatorItemModel.OnPointerPressed();
        }
    }
}
