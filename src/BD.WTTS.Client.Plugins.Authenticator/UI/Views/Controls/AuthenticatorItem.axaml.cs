using Avalonia;
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

        if (e.GetCurrentPoint(e.Source as Visual).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
        {
            if (DataContext is AuthenticatorItemModel authenticatorItemModel)
            {
                authenticatorItemModel.OnPointerLeftPressed();
            }
        }
        // else if (e.GetCurrentPoint(e.Source as Visual).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        // {
        //     if (DataContext is AuthenticatorItemModel authenticatorItemModel)
        //     {
        //         authenticatorItemModel.OnPointerRightPressed();
        //     }
        // }
    }

    async void InputElement_OnTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is AuthenticatorItemModel authenticatorItemModel)
        {
            await authenticatorItemModel.OnTextPanelOnTapped();
        }
    }
}
