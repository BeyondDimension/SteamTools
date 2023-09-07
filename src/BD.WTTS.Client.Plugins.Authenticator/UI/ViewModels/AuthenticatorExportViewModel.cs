using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.ViewModels;

public class AuthenticatorExportViewModel : ViewModelBase
{
    //SaveFileResult? _exportFile;

    [Reactive]
    public bool HasPasswordProtection { get; set; }

    [Reactive]
    public bool HasLocalProtection { get; set; }

    [Reactive]
    public string? Password { get; set; }

    [Reactive]
    public string? VerifyPassword { get; set; }

    //public SaveFileResult? ExportFile
    //{
    //    get => _exportFile;
    //    set
    //    {
    //        if (_exportFile != null)
    //        {
    //            _exportFile.Dispose();
    //        }
    //        _exportFile = value;
    //    }
    //}

    public AuthenticatorExportViewModel()
    {

    }

    public async Task Export()
    {
        if (string.IsNullOrWhiteSpace(VerifyPassword) && VerifyPassword != Password)
        {
            Toast.Show(ToastIcon.Warning, Strings.LocalAuth_ProtectionAuth_PasswordErrorTip);
            return;
        }

        var sourceData = await AuthenticatorHelper.GetAllSourceAuthenticatorAsync();
        var r = AuthenticatorHelper.HasEncrypt(sourceData);

        string? password = null;
        if (r.haspassword)
        {
            var textViewmodel = new TextBoxWindowViewModel()
            {
                InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
            };
            if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, Strings.Title_InputAuthPassword, isDialog: false,
                  isCancelButton: true) &&
              textViewmodel.Value != null)
            {
                password = textViewmodel.Value;
            }
        }

        var auths = await AuthenticatorHelper.GetAllAuthenticatorsAsync(sourceData, password);

        if (!auths.Any_Nullable())
        {
            Toast.Show(ToastIcon.Warning, Strings.Warning_PasswordError);
            return;
        }

        var exportFile = await AuthenticatorHelper.ExportAsync($"WattToolkit Authenticators {DateTime.Now.ToString(DateTimeFormat.File)}", HasLocalProtection, auths, VerifyPassword);
        if (exportFile == null) return;
        Toast.Show(ToastIcon.Success, Strings.ExportedToPath_.Format(exportFile?.ToString()));

        //ExportFile = null;
    }
}