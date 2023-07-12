using BD.WTTS.Client.Resources;
using WinAuth;
using System;

namespace BD.WTTS.UI.ViewModels;

public class AuthenticatorExportViewModel : ViewModelBase
{
    bool _hasPasswordProtection;
    bool _hasLocalProtection;
    string? _password;
    string? _verifyPassword;
    SaveFileResult? _exportFile;

    public bool HasPasswordProtection
    {
        get => _hasPasswordProtection;
        set
        {
            if (value == _hasPasswordProtection) return;
            _hasPasswordProtection = value;
            this.RaisePropertyChanged();
        }
    }

    public bool HasLocalProtection
    {
        get => _hasLocalProtection;
        set
        {
            if (value == _hasLocalProtection) return;
            _hasLocalProtection = value;
            this.RaisePropertyChanged();
        }
    }

    public string? Password
    {
        get => _password;
        set
        {
            if (value == _password) return;
            _password = value;
            this.RaisePropertyChanged();
        }
    }

    public string? VerifyPassword
    {
        get => _verifyPassword;
        set
        {
            if (value == _verifyPassword) return;
            _verifyPassword = value;
            this.RaisePropertyChanged();
        }
    }
    
    /// <summary>
    /// 默认导出文件名
    /// </summary>
    public string DefaultExportAuthFileName =>
        string.Format($"Watt Toolkit Authenticators {{0}}{FileEx.MPO}",
            DateTime.Now.ToString(DateTimeFormat.File));

    public SaveFileResult? ExportFile
    {
        get => _exportFile;
        set
        {
            if (_exportFile != null)
            {
                _exportFile.Dispose();
            }
            _exportFile = value;
        }
    }

    public async Task Export()
    {
        if (Essentials.IsSupportedSaveFileDialog)
        {
            FilePickerFileType? fileTypes;
            if (IApplication.IsDesktop())
            {
                fileTypes = new ValueTuple<string, string[]>[]
                {
                    ("MsgPack Files", new[] { FileEx.MPO, }),
                    ("Data Files", new[] { FileEx.DAT, }),
                    //("All Files", new[] { "*", }),
                };
            }
            else
            {
                fileTypes = null;
            }
            ExportFile = await FilePicker2.SaveAsync(new PickOptions
            {
                FileTypes = fileTypes,
                InitialFileName = DefaultExportAuthFileName,
                PickerTitle = "Watt Toolkit",
            });
            if (ExportFile == null) return;
        }
        
        if (HasPasswordProtection)
        {
            if (string.IsNullOrWhiteSpace(VerifyPassword) && VerifyPassword != Password)
            {
                Toast.Show(ToastIcon.Warning, Strings.LocalAuth_ProtectionAuth_PasswordErrorTip);
                return;
            }
        }
        
        var filestream = ExportFile?.OpenWrite();
        if (filestream == null)
        {
            Toast.Show(ToastIcon.Error, Strings.LocalAuth_ProtectionAuth_PathError);
            return;
        }

        if (filestream.CanSeek && filestream.Position != 0) filestream.Position = 0;

        var sourceData = await AuthenticatorService.GetAllSourceAuthenticatorAsync();
        var protection = AuthenticatorService.HasEncrypt(sourceData);
        string? password = null;
        if (protection.haspassword)
        {
            password = await GetAuthenticators(sourceData);
            if (string.IsNullOrEmpty(password)) return;
        }

        var auths = await AuthenticatorService.GetAllAuthenticatorsAsync(sourceData, password);

        await AuthenticatorService.ExportAsync(filestream, HasLocalProtection, auths, VerifyPassword);
        
        await filestream.FlushAsync();
        await filestream.DisposeAsync();

        Toast.Show(ToastIcon.Success, Strings.ExportedToPath_.Format(ExportFile?.ToString()));
        
        ExportFile = null;
    }

    public async Task<string?> GetAuthenticators(AccountPlatformAuthenticator[] sourceData)
    {
        var textViewmodel = new TextBoxWindowViewModel()
        {
            InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
        };
        if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewmodel, "请输入原令牌保护密码", isDialog: false,
                isCancelButton: true) &&
            textViewmodel.Value != null)
        {
            if (!(await AuthenticatorService.ValidatePassword(sourceData[0], textViewmodel.Value)))
            {
                Toast.Show(ToastIcon.Warning, "密码错误，请重试");
                await GetAuthenticators(sourceData);
            }
            return textViewmodel.Value;
        }

        return null;
    }
}