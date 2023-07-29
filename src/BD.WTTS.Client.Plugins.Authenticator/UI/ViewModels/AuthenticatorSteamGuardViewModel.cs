using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public class AuthenticatorSteamGuardViewModel : ViewModelBase
{
    string _phoneImportUuid = string.Empty;
    string _phoneImportSteamGuard = string.Empty;
    string? _importAuthNewName;

    public string PhoneImportUuid
    {
        get => _phoneImportUuid;
        set
        {
            if (value == _phoneImportUuid) return;
            if (!value.StartsWith("android:", StringComparison.Ordinal))
            {
                value = $"android:{value}";
            }
            _phoneImportUuid = value;
            this.RaisePropertyChanged();
        }
    }

    public string PhoneImportSteamGuard
    {
        get => _phoneImportSteamGuard;
        set
        {
            if (value == _phoneImportSteamGuard) return;
            _phoneImportSteamGuard = value;
            this.RaisePropertyChanged();
        }
    }

    public string? ImportAuthNewName
    {
        get => _importAuthNewName;
        set
        {
            if (value == _importAuthNewName) return;
            if (value != null && value.Length > IAuthenticatorDTO.MaxLength_Name)
            {
                value = value.Substring(0, IAuthenticatorDTO.MaxLength_Name);
            }
            _importAuthNewName = value;
            this.RaisePropertyChanged();
        }
    }

    readonly Func<IAuthenticatorDTO, Task> _saveAuth;

    public AuthenticatorSteamGuardViewModel()
    {
        _saveAuth = (authenticatorDto) => Task.CompletedTask;
    }

    public AuthenticatorSteamGuardViewModel(Func<IAuthenticatorDTO, Task> saveAuthFunc)
    {
        _saveAuth = saveAuthFunc;
    }

    public async Task Import()
    {
        /* AuthService.ImportSteamGuard (System.String name, System.String uuid, System.String steamGuard, System.Boolean isLocal, System.String password)
             * System.NullReferenceException: Object reference not set to an instance of an object
             * Crash Version 2.6.5(20220206) 12 users 14 reports
             * Android 9 ~ 12
             */

        // check the deviceid
        string deviceId;
        if (PhoneImportUuid.IndexOf("?xml", StringComparison.Ordinal) != -1)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(PhoneImportUuid);
                var node = doc.SelectSingleNode("//string[@name='uuidKey']");
                if (node == null)
                {
                    //WinAuthForm.ErrorDialog(this, "Cannot find uuidKey in xml");
                    return;
                }

                deviceId = node.InnerText;
            }
            catch (Exception ex)
            {
                //WinAuthForm.ErrorDialog(this, "Invalid uuid xml: " + ex.Message);
                //ToastService.Current.Notify("Invalid uuid xml");
                ex.LogAndShowT();
                return;
            }
        }
        else
        {
            deviceId = PhoneImportUuid;
        }

        if (string.IsNullOrEmpty(deviceId) || Regex.IsMatch(deviceId, @"android:[0-9abcdef-]+",
                RegexOptions.Singleline | RegexOptions.IgnoreCase) == false)
        {
            //WinAuthForm.ErrorDialog(this, "Invalid deviceid, expecting \"android:NNNN...\"");
            return;
        }

        // check the steamguard
        byte[] secret;
        string serial;
        try
        {
            var steamGuardModel = JsonSerializer.Deserialize(PhoneImportSteamGuard,
                ImportFileModelJsonContext.Default.SteamGuardModel);

            if (steamGuardModel == null) return;

            if (string.IsNullOrEmpty(steamGuardModel.SharedSecret))
            {
                throw new ApplicationException("no shared_secret");
            }

            secret = Convert.FromBase64String(steamGuardModel.SharedSecret);

            if (string.IsNullOrEmpty(steamGuardModel.SerialNumber))
            {
                throw new ApplicationException("no serial_number");
            }

            serial = steamGuardModel.SerialNumber;
        }
        catch (Exception ex)
        {
            //WinAuthForm.ErrorDialog(this, "Invalid SteamGuard JSON contents: " + ex.Message);
            //ToastService.Current.Notify("Invalid SteamGuard JSON");
            ex.LogAndShowT();
            return;
        }

        var auth = new SteamAuthenticator
        {
            SecretKey = secret,
            Serial = serial,
            SteamData = PhoneImportSteamGuard,
            DeviceId = deviceId
        };

        await _saveAuth.Invoke(new AuthenticatorDTO()
        {
            Name = $"(Steam){ImportAuthNewName}",
            Value = auth,
            Created = DateTimeOffset.Now,
        });
    }
}