using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public class SteamOtherImportViewModel : ViewModelBase
{
    string? _importAuthNewName;
    string? _importAuthFilePath;
    bool _importFromFile;
    string? _phoneImportUuid;
    string? _phoneImportSteamGuard;
    AuthImportType _authImportType;

    public string? ImportAuthNewName
    {
        get => _importAuthNewName;
        set
        {
            if (value == _importAuthNewName) return;
            _importAuthNewName = value;
            this.RaisePropertyChanged();
        }
    }

    public string? ImportAuthFilePath
    {
        get => _importAuthFilePath;
        set
        {
            if (value == _importAuthFilePath) return;
            _importAuthFilePath = value;
            this.RaisePropertyChanged();
        }
    }

    public bool ImportFromFile
    {
        get => _importFromFile;
        set
        {
            if (value == _importFromFile) return;
            _importFromFile = value;
            this.RaisePropertyChanged();
        }
    }

    public string? PhoneImportUUID
    {
        get => _phoneImportUuid;
        set
        {
            if (value == _phoneImportUuid) return;
            _phoneImportUuid = value;
            this.RaisePropertyChanged();
        }
    }

    public string? PhoneImportSteamGuard
    {
        get => _phoneImportSteamGuard;
        set
        {
            if (value == _phoneImportSteamGuard) return;
            _phoneImportSteamGuard = value;
            this.RaisePropertyChanged();
        }
    }

    public AuthImportType[] AuthImportTypes => Enum.GetValues<AuthImportType>();

    public AuthImportType CurrentAuthImportType
    {
        get => _authImportType;
        set
        {
            if (value == _authImportType) return;
            switch (value)
            {
                case AuthImportType.None:
                    break;
                case AuthImportType.SteamPhoneImport:
                    ImportFromFile = false;
                    break;
                default:
                    ImportFromFile = true;
                    break;
            }
            _authImportType = value;
            this.RaisePropertyChanged();
        }
    }

    public SteamOtherImportViewModel()
    {
        ImportFromFile = true;
    }

    public async void OpenFolder()
    {
        switch (CurrentAuthImportType)
        {
            case AuthImportType.WattToolkitV1:
                break;
            case AuthImportType.WattToolkitV2:
                break;
            case AuthImportType.WinAuth:
                break;
            case AuthImportType.SteamDesktopAuth:
                PickOptions pickOptions = new();
                if (IApplication.IsDesktop())
                {
                    FilePickerFileType filetypes = new[] { ("maFile 文件", new[] { FileEx.maFile }), ("Json 文件", new[] { FileEx.JSON }) };
                    pickOptions.FileTypes = filetypes;
                }
                var tempfilepath = await FilePicker2.PickAsync(pickOptions);
                if (tempfilepath == null) return;
                ImportAuthFilePath = tempfilepath.FullPath;
                break;
        }
    }

    public void Import()
    {
        switch (CurrentAuthImportType)
        {
            case AuthImportType.None:
                Toast.Show("请选择导入方式");
                break;
            case AuthImportType.SteamPhoneImport:
                break;
            case AuthImportType.WattToolkitV1:
                break;
            case AuthImportType.WattToolkitV2:
                break;
            case AuthImportType.WinAuth:
                ImportWithWinAuthFile();
                break;
            case AuthImportType.SteamDesktopAuth:
                ImportWithSDAFile();
                break;
        }
    }

    public void ImportWithWinAuthFile()
    {
        
    }

    public void ImportWithSDAFile()
    {
        if (string.IsNullOrEmpty(ImportAuthFilePath) || !File.Exists(ImportAuthFilePath))
        {
            Toast.Show("mafile文件路径不存在");
            return;
        }

        try
        {
            string text = File.ReadAllText(ImportAuthFilePath);
            var sdafilemodel = JsonSerializer.Deserialize(text, SdaFileModelJsonContext.Default.SdaFileModel);
            if (sdafilemodel == null) return;
            SteamAuthenticator steamAuthenticator = new()
            {
                DeviceId = sdafilemodel.DeviceId,
                Serial = sdafilemodel.SerialNumber,
                SecretKey = Convert.FromBase64String(sdafilemodel.SharedSecret),
                SteamData = JsonSerializer.Serialize(sdafilemodel, SdaFileModelJsonContext.Default.SdaFileModel),
            };
            var authDto =
                new AuthenticatorDTO() { Name = $"(Steam){ImportAuthNewName}", Value = steamAuthenticator };
            AuthenticatorService.AddOrUpdateSaveAuthenticatorsAsync(authDto, false, null);
        }
        catch (Exception e)
        {
            Toast.Show("文件读取不成功，导入失败");
        }
    }
}