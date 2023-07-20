namespace BD.WTTS.Models;

public abstract class AuthenticatorImportBase : IAuthenticatorImport
{
    public abstract string Name { get; }
    
    public abstract string Description { get; }
    
    public abstract ICommand AuthenticatorImportCommand { get; set; }
    
    string? _currentPassword { get; set; }
    
    public async Task<bool> VerifyMaxValue()
    {
        var auths = await AuthenticatorService.GetAllSourceAuthenticatorAsync();
        if (auths.Length < IAccountPlatformAuthenticatorRepository.MaxValue) return true;
        Toast.Show(ToastIcon.Info, "已达到本地令牌数量上限");
        return false;
    }

    public async Task SaveAuthenticator(IAuthenticatorDTO authenticatorDto)
    {
        var sourceList = await AuthenticatorService.GetAllSourceAuthenticatorAsync(); 
        
        var (hasLocalPcEncrypt, hasPasswordEncrypt) = AuthenticatorService.HasEncrypt(sourceList);

        if (hasPasswordEncrypt && string.IsNullOrEmpty(_currentPassword))
        {
            var textViewmodel = new TextBoxWindowViewModel()
            {
                InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
            };
            if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, "请输入令牌保护密码", isDialog: false,
                    isCancelButton: true) &&
                textViewmodel.Value != null)
            {
                if (await AuthenticatorService.ValidatePassword(sourceList[0], textViewmodel.Value))
                {
                    _currentPassword = textViewmodel.Value;
                }
                else
                {
                    Toast.Show(ToastIcon.Warning, "密码错误，请重试");
                    return;
                }
            }
            else return;
        }

        await AuthenticatorService.AddOrUpdateSaveAuthenticatorsAsync(authenticatorDto, _currentPassword,
            hasLocalPcEncrypt);
    }
}