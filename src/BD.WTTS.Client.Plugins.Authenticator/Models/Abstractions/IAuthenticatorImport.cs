namespace BD.WTTS.Models;

public interface IAuthenticatorImport
{
    string Name { get; }

    string Description { get; }

    ICommand AuthenticatorImportCommand { get; set; }

    public static async Task<bool> VerifyMaxValue()
    {
        var auths = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
        if (auths.Length < IAccountPlatformAuthenticatorRepository.MaxValue) return true;
        Toast.Show(ToastIcon.Info, Strings.Info_AuthMaximumQuantity);
        return false;
    }

    public static async Task SaveAuthenticator(IAuthenticatorDTO authenticatorDto, string? password = null)
    {
        var sourceList = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();

        var (hasLocalPcEncrypt, hasPasswordEncrypt) = AuthenticatorHelper.HasEncrypt(sourceList);

        if (hasPasswordEncrypt && string.IsNullOrEmpty(password))
        {
            var textViewmodel = new TextBoxWindowViewModel()
            {
                InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
            };
            if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, Strings.Title_InputAuthPassword, isDialog: false,
                    isCancelButton: true) &&
                textViewmodel.Value != null)
            {
                if (await AuthenticatorHelper.ValidatePassword(sourceList[0], textViewmodel.Value))
                {
                    password = textViewmodel.Value;
                }
                else
                {
                    Toast.Show(ToastIcon.Warning, Strings.Warning_PasswordError);
                    return;
                }
            }
            else
                return;
        }

        await AuthenticatorHelper.AddOrUpdateSaveAuthenticatorsAsync(authenticatorDto, password,
            hasLocalPcEncrypt);
    }
}