using BD.WTTS.Client.Resources;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public class SteamOtherImportViewModel : DialogWindowViewModel
{
    readonly string? _currentPassword;
    
    string? _importAuthNewName;
    string _importAuthFilePath = string.Empty;
    bool _importFromFile;
    string _phoneImportUuid = string.Empty;
    string _phoneImportSteamGuard = string.Empty;
    AuthImportType _authImportType;

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

    public string ImportAuthFilePath
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

    public string PhoneImportUuid
    {
        get => _phoneImportUuid;
        set
        {
            if (value == _phoneImportUuid) return;
            if (value != null && !value.StartsWith("android:", StringComparison.Ordinal))
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
    
    public SteamOtherImportViewModel(string? password)
    {
        ImportFromFile = true;
        _currentPassword = password;
    }

    public async void OpenFolder()
    {
        // switch (CurrentAuthImportType)
        // {
        //     case AuthImportType.WattToolkitV1:
        //         break;
        //     case AuthImportType.WattToolkitV2:
        //         break;
        //     case AuthImportType.WinAuth:
        //         break;
        //     case AuthImportType.SteamDesktopAuth:
        //         PickOptions pickOptions = new();
        //         if (IApplication.IsDesktop())
        //         {
        //             FilePickerFileType filetypes = new[] { ("maFile 文件", new[] { FileEx.maFile }), ("Json 文件", new[] { FileEx.JSON }) };
        //             pickOptions.FileTypes = filetypes;
        //         }
        //         var tempfilepath = await FilePicker2.PickAsync(pickOptions);
        //         if (tempfilepath == null) return;
        //         ImportAuthFilePath = tempfilepath.FullPath;
        //         break;
        // }
        var tempfilepath = await FilePicker2.PickAsync(new PickOptions());
        if (tempfilepath == null) return;
        
        var extension = Path.GetExtension(tempfilepath.FullPath);
        if (string.Equals(extension, FileEx.Txt, StringComparison.OrdinalIgnoreCase))
        {
            CurrentAuthImportType = AuthImportType.WinAuth;
        }
        else if (string.Equals(extension, FileEx.MPO, StringComparison.OrdinalIgnoreCase))
        {
            CurrentAuthImportType = AuthImportType.WattToolkitV2;
        }
        else if (string.Equals(extension, FileEx.Dat, StringComparison.OrdinalIgnoreCase))
        {
            CurrentAuthImportType = AuthImportType.WattToolkitV1;
        }
        else if (string.Equals(extension, FileEx.maFile, StringComparison.OrdinalIgnoreCase))
        {
            CurrentAuthImportType = AuthImportType.SteamDesktopAuth;
        }
        else
        {
            Toast.Show("不支持该文件类型的导入方式。");
            return;
        }

        ImportAuthFilePath = tempfilepath.FullPath.TrimStart("file:///");
    }

    public async void Import()
    {
        if (CurrentAuthImportType == AuthImportType.SteamPhoneImport)
        {
            if (string.IsNullOrEmpty(PhoneImportUuid) || string.IsNullOrEmpty(PhoneImportSteamGuard))
            {
                Toast.Show("UUID及SteamGuard不可为空");
                return;
            }
            ImportFromPhoneSteamGuard();
            return;
        }
        if (string.IsNullOrEmpty(ImportAuthFilePath) || !File.Exists(ImportAuthFilePath))
        {
            Toast.Show("文件路径不存在");
            return;
        }
        switch (CurrentAuthImportType)
        {
            case AuthImportType.None:
                Toast.Show("请选择导入方式");
                break;
            case AuthImportType.WattToolkitV1:
                if (ImportFromWattToolKitV1())
                    Toast.Show("WattToolKitV1令牌导入成功");
                else 
                    Toast.Show("WattToolKitV1令牌导入失败");
                break;
            case AuthImportType.WattToolkitV2:
                if (await ImportFromWattToolKitV2())
                    Toast.Show("WattToolKitV2令牌导入成功");
                else 
                    Toast.Show("WattToolKitV2令牌导入失败");
                break;
            case AuthImportType.WinAuth:
                if (ImportFromWinAuthFile())
                    Toast.Show("WinAuth令牌导入成功");
                else
                    Toast.Show("WinAuth令牌导入失败");
                break;
            case AuthImportType.SteamDesktopAuth:
                if (ImportFromSDAFile())
                    Toast.Show("SteamDesktopAuth令牌导入成功");
                else
                    Toast.Show("SteamDesktopAuth令牌导入失败");
                break;
        }
    }

    public bool ImportFromSDAFile()
    {
        try
        {
            string text = File.ReadAllText(ImportAuthFilePath);
            var sdaFileModel = JsonSerializer.Deserialize(text, ImportFileModelJsonContext.Default.SdaFileModel);
            if (sdaFileModel == null) return false;
            var steamDataModel = new SdaFileConvertToSteamDataModel(sdaFileModel);
            SteamAuthenticator steamAuthenticator = new()
            {
                DeviceId = sdaFileModel.DeviceId,
                Serial = sdaFileModel.SerialNumber,
                SecretKey = Convert.FromBase64String(sdaFileModel.SharedSecret),
                SteamData = JsonSerializer.Serialize(steamDataModel, ImportFileModelJsonContext.Default.SdaFileConvertToSteamDataModel),
            };
            var authDto =
                new AuthenticatorDTO() { Name = $"(Steam){ImportAuthNewName}", Value = steamAuthenticator };
            SaveImport(authDto);
        }
        catch (Exception e)
        {
            Log.Error(nameof(SteamOtherImportViewModel), e, nameof(ImportFromSDAFile));
            return false;
        }

        return true;
    }
    
    public async Task<bool> ImportFromWattToolKitV2(string? exportPassword = null)
    {
        var metadata = await IOPath.TryReadAllBytesAsync(ImportAuthFilePath);
        if (!metadata.success || metadata.byteArray == null) return false;
        var result = await AuthenticatorService.ImportAsync(exportPassword, metadata.byteArray);
        switch (result.resultCode)
        {
            case IAccountPlatformAuthenticatorRepository.ImportResultCode.Success
                or IAccountPlatformAuthenticatorRepository.ImportResultCode.PartSuccess:
                foreach (var item in result.result)
                {
                    SaveImport(item);
                }

                Toast.Show(result.resultCode == IAccountPlatformAuthenticatorRepository.ImportResultCode.Success
                    ? Strings.LocalAuth_AddAuthSuccess
                    : Strings.LocalAuth_AddAuth_PartSuccess);
                break;
            case IAccountPlatformAuthenticatorRepository.ImportResultCode.SecondaryPasswordFail:
                Toast.Show(Strings.LocalAuth_ProtectionAuth_PasswordErrorTip);
                var textviewmodel =
                    new TextBoxWindowViewModel { InputType = TextBoxWindowViewModel.TextBoxInputType.Password };
                if (await IWindowManager.Instance.ShowTaskDialogAsync(textviewmodel, "请输入此令牌导出时设置的密码", isDialog: false,
                        isCancelButton: true))
                {
                    exportPassword = textviewmodel.Value;
                }

                ImportFromWattToolKitV2(exportPassword);
                break;
            default:
                Toast.Show(Strings.LocalAuth_ExportAuth_Error.Format(result.resultCode));
                break;
        }

        return true;
    }

    public bool ImportFromWattToolKitV1()
    {
        if (IOPath.TryReadAllText(ImportAuthFilePath, out var content, out var _))
        {
            string authString;
            try
            {
                authString = content.DecompressString();
            }
            catch
            {
                return false;
            }

            if (!string.IsNullOrEmpty(authString))
            {
                XmlReader reader = XmlReader.Create(new StringReader(authString));
                reader.Read();
                while (reader.EOF == false && reader.IsEmptyElement == true)
                {
                    reader.Read();
                }

                reader.MoveToContent();
                while (reader.EOF == false)
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.Name == "Auth")
                        {
                            reader.Read();
                        }

                        if (reader.Name == "WinAuthAuthenticator")
                        {
                            var authDto = new AuthenticatorDTO();
                            ReadXml(ref authDto, reader, null);
                            SaveImport(authDto);
                        }
                    }
                    else
                    {
                        reader.Read();
                        break;
                    }
                }
            }
        }

        return true;
    }

    public bool ImportFromPhoneSteamGuard()
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
                        return false;
                    }

                    deviceId = node.InnerText;
                }
                catch (Exception ex)
                {
                    //WinAuthForm.ErrorDialog(this, "Invalid uuid xml: " + ex.Message);
                    //ToastService.Current.Notify("Invalid uuid xml");
                    Log.Error(nameof(SteamOtherImportViewModel), ex, nameof(ImportFromPhoneSteamGuard));
                    return false;
                }
            }
            else
            {
                deviceId = PhoneImportUuid;
            }
            if (string.IsNullOrEmpty(deviceId) || Regex.IsMatch(deviceId, @"android:[0-9abcdef-]+", RegexOptions.Singleline | RegexOptions.IgnoreCase) == false)
            {
                //WinAuthForm.ErrorDialog(this, "Invalid deviceid, expecting \"android:NNNN...\"");
                return false;
            }

            // check the steamguard
            byte[] secret;
            string serial;
            try
            {
                var steamGuardModel = JsonSerializer.Deserialize(PhoneImportSteamGuard,
                    ImportFileModelJsonContext.Default.SteamGuardModel);

                if (steamGuardModel == null) return false;

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
                Log.Error(nameof(SteamOtherImportViewModel), ex, nameof(ImportFromPhoneSteamGuard));
                return false;
            }

            var auth = new SteamAuthenticator
            {
                SecretKey = secret,
                Serial = serial,
                SteamData = PhoneImportSteamGuard,
                DeviceId = deviceId
            };

            SaveImport(new AuthenticatorDTO() { Name = $"(Steam){ImportAuthNewName}", Value = auth });
            
            return true;
    }

    public bool ImportFromWinAuthFile()
    {
        var urls = ReadUrlsByFilePath(ImportAuthFilePath);
        var isOK = false;

        int linenumber = 0;
        try
        {
            string? line;
            foreach (var url in urls)
            {
                linenumber++;
                line = url;

                // ignore blank lines or comments
                line = line.Trim();
                if (line.Length == 0 || line.IndexOf("#", StringComparison.Ordinal) == 0)
                {
                    continue;
                }

                // bug if there is a hash before ?
                var hash = line.IndexOf("#", StringComparison.Ordinal);
                var qm = line.IndexOf("?", StringComparison.Ordinal);
                if (hash != -1 && hash < qm)
                {
                    line = $"{line.Substring(0, hash)}%23{line[(hash + 1)..]}";
                }

                // parse and validate URI
                var uri = new Uri(line);

                // we only support "otpauth"
                if (uri.Scheme != "otpauth")
                {
                    throw new ApplicationException("Import only supports otpauth://");
                }

                // we only support totp (not hotp)
                if (uri.Host != "totp" && uri.Host != "hotp")
                {
                    throw new ApplicationException("Import only supports otpauth://totp/ or otpauth://hotp/");
                }

                // get the label and optional issuer
                string issuer = string.Empty;
                string label = string.IsNullOrEmpty(uri.LocalPath) == false
                    ? uri.LocalPath[1..]
                    : string.Empty; // skip past initial /
                int p = label.IndexOf(":");
                if (p != -1)
                {
                    issuer = label.Substring(0, p);
                    label = label[(p + 1)..];
                }

                // + aren't decoded
                label = label.Replace("+", " ");

                var query = HttpUtility.ParseQueryString(uri.Query);
                string secret = query["secret"];
                if (string.IsNullOrEmpty(secret))
                {
                    throw new ApplicationException("Authenticator does not contain secret");
                }

                string counter = query["counter"];
                if (uri.Host == "hotp" && string.IsNullOrEmpty(counter))
                {
                    throw new ApplicationException("HOTP authenticator should have a counter");
                }

                AuthenticatorDTO authenticatorDto = new();
                //
                AuthenticatorValueDTO auth;
                if (String.Compare(issuer, "BattleNet", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    string serial = query["serial"];
                    if (string.IsNullOrEmpty(serial))
                    {
                        throw new ApplicationException("Battle.net Authenticator does not have a serial");
                    }

                    serial = serial.ToUpper();
                    if (Regex.IsMatch(serial, @"^[A-Z]{2}-?[\d]{4}-?[\d]{4}-?[\d]{4}$") == false)
                    {
                        throw new ApplicationException("Invalid serial for Battle.net Authenticator");
                    }

                    auth = new BattleNetAuthenticator();
                    //char[] decoded = Base32.getInstance().Decode(secret).Select(c => Convert.ToChar(c)).ToArray(); // this is hex string values
                    //string hex = new string(decoded);
                    //((BattleNetAuthenticator)auth).SecretKey = Authenticator.StringToByteArray(hex);

                    ((BattleNetAuthenticator)auth).SecretKey = Base32.GetInstance().Decode(secret);

                    ((BattleNetAuthenticator)auth).Serial = serial;

                    issuer = string.Empty;
                }
                else if (string.Compare(issuer, "Steam", true) == 0)
                {
                    auth = new SteamAuthenticator();
                    ((SteamAuthenticator)auth).SecretKey = Base32.GetInstance().Decode(secret);
                    ((SteamAuthenticator)auth).Serial = string.Empty;
                    ((SteamAuthenticator)auth).DeviceId = query["deviceid"] ?? string.Empty;
                    ((SteamAuthenticator)auth).SteamData = query["data"] ?? string.Empty;
                    issuer = string.Empty;
                }
                else if (uri.Host == "hotp")
                {
                    auth = new HOTPAuthenticator();
                    ((HOTPAuthenticator)auth).SecretKey = Base32.GetInstance().Decode(secret);
                    ((HOTPAuthenticator)auth).Counter = int.Parse(counter!);

                    if (!string.IsNullOrEmpty(issuer))
                    {
                        auth.Issuer = issuer;
                    }
                }
                else // if (string.Compare(issuer, "Google", true) == 0)
                {
                    auth = new GoogleAuthenticator();
                    ((GoogleAuthenticator)auth).Enroll(secret);

                    if (string.Compare(issuer, "Google", true) == 0)
                    {
                        issuer = string.Empty;
                    }
                    else if (!string.IsNullOrEmpty(issuer))
                    {
                        auth.Issuer = issuer;
                    }
                }

                int.TryParse(query["period"], out int period);
                if (period != 0)
                {
                    auth.Period = period;
                }

                int.TryParse(query["digits"], out int digits);
                if (digits != 0)
                {
                    auth.CodeDigits = digits;
                }

                if (Enum.TryParse(query["algorithm"], true, out IAuthenticatorValueDTO.HMACTypes hmactype) == true)
                {
                    auth.HMACType = hmactype;
                }

                //
                if (label.Length != 0)
                {
                    authenticatorDto.Name = (issuer.Length != 0 ? issuer + " (" + label + ")" : label);
                }
                else if (issuer.Length != 0)
                {
                    authenticatorDto.Name = issuer;
                }
                else
                {
                    authenticatorDto.Name = "Imported";
                }

                //
                authenticatorDto.Value = auth;

                // sync
                Toast.Show(Strings.LocalAuth_AddAuthSyncTip, ToastLength.Short);
                authenticatorDto.Value.SyncAsync();

                SaveImport(authenticatorDto);
            }

            Toast.Show(Strings.LocalAuth_AddAuthSuccess);
            isOK = true;
        }
        catch (UriFormatException)
        {
            Toast.Show(string.Format("UriFormatException Invalid authenticator at line {0}", linenumber));
        }
        catch (Exception ex)
        {
            Toast.Show(string.Format("Error importing at line {0}", ex.Message));
        }

        return isOK;
    }

    static IEnumerable<string> ReadUrlsByFilePath(string filePath)
    {
        StringBuilder lines = new();
        bool retry;
        do
        {
            retry = false;
            lines.Length = 0;
            // read a plain text file
            lines.Append(File.ReadAllText(filePath));
        } while (retry);

        using var sr = new StringReader(lines.ToString());
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            yield return line;
        }
    }
    
    private void SaveImport(IAuthenticatorDTO authenticatorDto)
    {
        AuthenticatorService.AddOrUpdateSaveAuthenticatorsAsync(authenticatorDto, _currentPassword);
    }

    public bool ReadXml(ref AuthenticatorDTO authenticatorDto, XmlReader reader, string? password)
    {
        bool changed = false;
        
        var authenticatorType = reader.GetAttribute("type");
        switch (authenticatorType)
        {
            case "WinAuth.SteamAuthenticator":
                authenticatorDto.Value = new SteamAuthenticator();
                break;
            case "WinAuth.BattleNetAuthenticator":
                authenticatorDto.Value = new BattleNetAuthenticator();
                break;
            case "WinAuth.GoogleAuthenticator":
                authenticatorDto.Value = new GoogleAuthenticator();
                break;
            case "WinAuth.HOTPAuthenticator":
                authenticatorDto.Value = new HOTPAuthenticator();
                break;
            case "WinAuth.MicrosoftAuthenticator":
                authenticatorDto.Value = new MicrosoftAuthenticator();
                break;
            default:
                return false;
        }

        reader.MoveToContent();

        if (reader.IsEmptyElement)
        {
            reader.Read();
            return changed;
        }

        reader.Read();
        while (reader.EOF == false)
        {
            if (reader.IsStartElement())
            {
                switch (reader.Name)
                {
                    case "name":
                        authenticatorDto.Name = reader.ReadElementContentAsString();
                        break;

                    case "created":
                        long t = reader.ReadElementContentAsLong();
                        t += Convert.ToInt64(new TimeSpan(new DateTime(1970, 1, 1).Ticks).TotalMilliseconds);
                        t *= TimeSpan.TicksPerMillisecond;
                        authenticatorDto.Created = new DateTimeOffset(new DateTime(t).ToLocalTime());
                        break;

                    case "authenticatordata":
                        try
                        {
                            // we don't pass the password as they are locked till clicked
                            changed = authenticatorDto.Value.ReadXml(reader) || changed;
                        }
                        catch (WinAuthEncryptedSecretDataException)
                        {
                            // no action needed
                        }
                        catch (WinAuthBadPasswordException)
                        {
                            // no action needed
                        }

                        break;

                    // v2
                    case "authenticator":
                        authenticatorDto.Value = AuthenticatorValueDTO.ReadXmlv2(reader, password);
                        break;
                    // v2
                    case "servertimediff":
                        authenticatorDto.Value.ServerTimeDiff = reader.ReadElementContentAsLong();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }
            else
            {
                reader.Read();
                break;
            }
        }

        return changed;
    }
}