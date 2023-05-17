namespace BD.WTTS.UI.Views.Pages;

public partial class TextInputDialogPage : ReactiveUserControl<TextBoxWindowViewModel>
{
    public TextInputDialogPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Called when the control is added to a rooted visual tree.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        switch (ViewModel?.InputType)
        {
            case TextBoxWindowViewModel.TextBoxInputType.Password:
                PasswordBox.IsVisible = true;
                PasswordBox.PasswordChar = '*';
                PasswordBox.Classes.Set("revealPasswordButton", true);
                break;
            case TextBoxWindowViewModel.TextBoxInputType.TextBox:
                PasswordBox.IsVisible = true;
                PasswordBox.PasswordChar = default;
                PasswordBox.Classes.Set("revealPasswordButton", false);
                break;
            case TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText:
                PasswordBox.IsVisible = false;
                break;
        }

        PasswordBox.Focus();
    }
}
