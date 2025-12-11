using WinAuth;
using SJsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamGuardImportPageViewModel : ViewModelBase
{
    public static string Name => Strings.LocalAuth_Import.Format(Strings.SteamGuard);

    string? _phoneImportUuid;

    public string? PhoneImportUuid
    {
        get => _phoneImportUuid;
        set
        {
            if (value == _phoneImportUuid)
                return;
            if (value?.StartsWith("android:", StringComparison.Ordinal) == false)
                value = $"android:{value.TrimStart("android:", StringComparison.OrdinalIgnoreCase)}";
            _phoneImportUuid = value;
            this.RaisePropertyChanged();
        }
    }

    string? _phoneImportSteamGuard;

    public string? PhoneImportSteamGuard
    {
        get => _phoneImportSteamGuard;
        set => this.RaiseAndSetIfChanged(ref _phoneImportSteamGuard, value);
    }

    string? _importAuthNewName;

    public string? ImportAuthNewName
    {
        get => _importAuthNewName;
        set
        {
            if (value == _importAuthNewName) return;
            if (value != null && value.Length > IAuthenticatorDTO.MaxLength_Name)
                value = value.Substring(0, IAuthenticatorDTO.MaxLength_Name);
            _importAuthNewName = value;
            this.RaisePropertyChanged();
        }
    }

    public async Task Import()
    {
        try
        {
            /* AuthService.ImportSteamGuard (System.String name, System.String uuid, System.String steamGuard, System.Boolean isLocal, System.String password)
                 * System.NullReferenceException: Object reference not set to an instance of an object
                 * Crash Version 2.6.5(20220206) 12 users 14 reports
                 * Android 9 ~ 12
                 */
            PhoneImportUuid.ThrowIsNull();
            PhoneImportSteamGuard.ThrowIsNull();
            ImportAuthNewName.ThrowIsNull();

            // check the deviceid
            string deviceId;
            if (PhoneImportUuid.IndexOf("?xml", StringComparison.Ordinal) != -1)
                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(PhoneImportUuid);
                    var node = doc.SelectSingleNode("//string[@name='uuidKey']");
                    if (node == null)
                        //WinAuthForm.ErrorDialog(this, "Cannot find uuidKey in xml");
                        return;

                    deviceId = node.InnerText;
                }
                catch (Exception ex)
                {
                    //WinAuthForm.ErrorDialog(this, "Invalid uuid xml: " + ex.Message);
                    //ToastService.Current.Notify("Invalid uuid xml");
                    ex.LogAndShowT();
                    return;
                }
            else
                deviceId = PhoneImportUuid;

            if (string.IsNullOrEmpty(deviceId) || DeviceIdRegex().IsMatch(deviceId) == false)
                //WinAuthForm.ErrorDialog(this, "Invalid deviceid, expecting \"android:NNNN...\"");
                return;

            // check the steamguard
            byte[] secret;
            string? serial;
            var steamGuardModel = SJsonSerializer.Deserialize(PhoneImportSteamGuard,
                ImportFileModelJsonContext.Default.SteamGuardModel);

            if (steamGuardModel == null) return;

            if (string.IsNullOrEmpty(steamGuardModel.SharedSecret))
                throw new ApplicationException("no shared_secret");

            secret = Convert.FromBase64String(steamGuardModel.SharedSecret);

            if (string.IsNullOrEmpty(steamGuardModel.SerialNumber))
                throw new ApplicationException("no serial_number");

            serial = steamGuardModel.SerialNumber;

            var auth = new SteamAuthenticator
            {
                SecretKey = secret,
                Serial = serial,
                SteamData = PhoneImportSteamGuard,
                DeviceId = deviceId,
            };

            await AuthenticatorHelper.SaveAuthenticator(new AuthenticatorDTO
            {
                Name = $"(Steam){ImportAuthNewName}",
                Value = auth,
                Created = DateTimeOffset.Now,
            });
        }
        catch (Exception ex)
        {
            //WinAuthForm.ErrorDialog(this, "Invalid SteamGuard JSON contents: " + ex.Message);
            //ToastService.Current.Notify("Invalid SteamGuard JSON");
            ex.LogAndShowT();
        }
    }

    [GeneratedRegex("android:[0-9abcdef-]+", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex DeviceIdRegex();
}