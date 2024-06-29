using BD.WTTS.UI.Views.Pages;
using WinAuth;
using static SteamKit2.DepotManifest;
using SJsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.UI.ViewModels;

public record AuthenticatorImportMethod(string Name, string Description, string? Image = null, Type? PageType = null, ICommand? Command = null, AuthenticatorPlatform? Platform = null);

public class AuthenticatorImportPageViewModel : ViewModelBase
{
    public static string Name => Strings.AuthImport;

    public IReadOnlyCollection<AuthenticatorImportMethod> AuthenticatorImportMethods { get; }

    public AuthenticatorImportPageViewModel()
    {
        AuthenticatorImportMethods = new List<AuthenticatorImportMethod>
        {
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.LocalAuth_JoinSteamAuthenticator), Strings.LocalAuth_JoinSteamAuthenticatorDesc,
                                            PageType: typeof(JoinSteamAuthenticatorPage)),
            new AuthenticatorImportMethod(Strings.Auth_SteamLoginImport, Strings.Steam_UserLoginTip,
                                            PageType: typeof(SteamLoginImportPage)),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.SteamGuard), Strings.LocalAuth_SteamGuardImport,
                                            PageType: typeof(SteamGuardImportPage)),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.Mafile), Strings.LocalAuth_SDAImport,
                                            Command: ReactiveCommand.Create(async () =>
                                            {
                                                if (await AuthenticatorHelper.VerifyMaxValue())
                                                    await ImportFromSdaFile();
                                            })),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.WattToolKitV2), Strings.LocalAuth_WattToolKitV2Import,
                                            Command: ReactiveCommand.Create(async () =>
                                            {
                                                if (await AuthenticatorHelper.VerifyMaxValue())
                                                {
                                                    var r = await AuthenticatorHelper.GetCurrentPassword();
                                                    await ImportFromWattToolKitV2(password: r.password, isLocalProtect: r.hasLocalPcEncrypt);
                                                }
                                            })),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.WattToolKitV1), Strings.LocalAuth_WattToolKitV1Import,
                                            Command: ReactiveCommand.Create(async () =>
                                            {
                                                if (await AuthenticatorHelper.VerifyMaxValue())
                                                {
                                                    var r = await AuthenticatorHelper.GetCurrentPassword();
                                                    await ImportFromWattToolKitV1(password: r.password, isLocalProtect: r.hasLocalPcEncrypt);
                                                }
                                            })),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.WinAuth), Strings.LocalAuth_WinAuthImport,
                                            Command: ReactiveCommand.Create(async () =>
                                            {
                                                if (await AuthenticatorHelper.VerifyMaxValue())
                                                {
                                                    var r = await AuthenticatorHelper.GetCurrentPassword();
                                                    await ImportFromWinAuthFile(password: r.password, isLocalProtect: r.hasLocalPcEncrypt);
                                                }
                                            })),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.Google), Strings.LocalAuth_GoogleImport,
                                            PageType: typeof(AuthenticatorGeneralImportPage), Platform: AuthenticatorPlatform.Google),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.Microsoft), Strings.LocalAuth_MicrosoftImport,
                                            PageType: typeof(AuthenticatorGeneralImportPage), Platform: AuthenticatorPlatform.Microsoft),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.HOTP), Strings.LocalAuth_HOTPImport,
                                            PageType: typeof(AuthenticatorGeneralImportPage), Platform: AuthenticatorPlatform.HOTP),
        };
    }

    static async Task ImportFromSdaFile()
    {
        try
        {
            var filePaths = await AuthenticatorHelper.MultipleSelectFolderPath(FileEx.maFile);

            if (!filePaths.Any_Nullable())
                return;

            foreach (var file in filePaths)
            {
                if (!File.Exists(file.FullPath)) return;
                var text = await File.ReadAllTextAsync(file.FullPath);

                var sdaFileModel = SJsonSerializer.Deserialize(text, ImportFileModelJsonContext.Default.SdaFileModel);
                sdaFileModel.ThrowIsNull();
                var steamDataModel = new SdaFileConvertToSteamDataModel(sdaFileModel);

                SteamAuthenticator steamAuthenticator = new()
                {
                    DeviceId = sdaFileModel.DeviceId,
                    Serial = sdaFileModel.SerialNumber,
                    SecretKey = Convert.FromBase64String(sdaFileModel.SharedSecret),
                    SteamData = SJsonSerializer.Serialize(steamDataModel, ImportFileModelJsonContext.Default.SdaFileConvertToSteamDataModel),
                };

                var authDto =
                    new AuthenticatorDTO()
                    {
                        Name = $"(Steam){steamAuthenticator.AccountName}",
                        Value = steamAuthenticator,
                        Created = DateTimeOffset.Now,
                    };

                if (await AuthenticatorHelper.SaveAuthenticator(authDto))
                {
                    Toast.Show(ToastIcon.Success, Strings.ModelContent_ImportSuccessful_.Format(authDto.Name));
                }
            }
        }
        catch (Exception e)
        {
            e.LogAndShowT();
        }
    }

    static async Task ImportFromWattToolKitV2(string? exportPassword = null, string? password = null, bool? isLocalProtect = null, string? filePath = null)
    {
        filePath ??= await AuthenticatorHelper.SelectFolderPath(FileEx.MPO);

        if (string.IsNullOrEmpty(filePath)) return;

        var metadata = await IOPath.TryReadAllBytesAsync(filePath);
        if (!metadata.success || metadata.byteArray == null) return;
        var result = await AuthenticatorHelper.ImportAsync(exportPassword, metadata.byteArray);
        switch (result.resultCode)
        {
            case IAccountPlatformAuthenticatorRepository.ImportResultCode.Success
                or IAccountPlatformAuthenticatorRepository.ImportResultCode.PartSuccess:
                foreach (var item in result.result)
                {
                    await AuthenticatorHelper.SaveAuthenticator(item, password, isLocalProtect);
                }
                Toast.Show(ToastIcon.Success, result.resultCode == IAccountPlatformAuthenticatorRepository.ImportResultCode.Success
                    ? Strings.LocalAuth_AddAuthSuccess
                    : Strings.LocalAuth_AddAuth_PartSuccess);
                break;

            case IAccountPlatformAuthenticatorRepository.ImportResultCode.SecondaryPasswordFail:
                Toast.Show(ToastIcon.Warning, Strings.LocalAuth_ProtectionAuth_PasswordErrorTip);
                var textViewModel =
                    new TextBoxWindowViewModel { InputType = TextBoxWindowViewModel.TextBoxInputType.Password };
                if (await IWindowManager.Instance.ShowTaskDialogAsync(textViewModel, Strings.ModelContent_ExportPassword, isDialog: false,
                        isCancelButton: true))
                {
                    if (textViewModel.Value == null)
                        return; // 弹窗点击了取消跳出
                    exportPassword = textViewModel.Value;
                }
                await ImportFromWattToolKitV2(exportPassword, password, isLocalProtect, filePath);
                break;

            default:
                Toast.Show(ToastIcon.Error, Strings.LocalAuth_ExportAuth_Error.Format(result.resultCode));
                break;
        }
    }

    static async Task ImportFromWattToolKitV1(string? password = null, bool? isLocalProtect = null)
    {
        var filePath = await AuthenticatorHelper.SelectFolderPath(FileEx.DAT);

        if (string.IsNullOrEmpty(filePath)) return;

        if (IOPath.TryReadAllText(filePath, out var content, out var _))
        {
            string authString;
            try
            {
                authString = content.DecompressString();
            }
            catch
            {
                return;
            }

            if (!string.IsNullOrEmpty(authString))
            {
                var reader = XmlReader.Create(new StringReader(authString));
                reader.Read();
                while (reader is { EOF: false, IsEmptyElement: true })
                {
                    await reader.ReadAsync();
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

                        if (reader.Name != "WinAuthAuthenticator") continue;
                        var authDto = new AuthenticatorDTO();
                        AuthenticatorHelper.ReadXml(authDto, reader, null);
                        await AuthenticatorHelper.SaveAuthenticator(authDto, password, isLocalProtect);
                    }
                    else
                    {
                        reader.Read();
                        break;
                    }
                }
            }
        }
    }

    static async Task ImportFromWinAuthFile(string? password = null, bool? isLocalProtect = null)
    {
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

        var filePath = await AuthenticatorHelper.SelectFolderPath(FileEx.TXT);

        if (string.IsNullOrEmpty(filePath)) return;

        var urls = ReadUrlsByFilePath(filePath);

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
                int p = label.IndexOf(":", StringComparison.Ordinal);
                if (p != -1)
                {
                    issuer = label.Substring(0, p);
                    label = label[(p + 1)..];
                }

                // + aren't decoded
                label = label.Replace("+", " ");

                var query = HttpUtility.ParseQueryString(uri.Query);
                string? secret = query["secret"];
                if (string.IsNullOrEmpty(secret))
                {
                    throw new ApplicationException("Authenticator does not contain secret");
                }

                string? counter = query["counter"];
                if (uri.Host == "hotp" && string.IsNullOrEmpty(counter))
                {
                    throw new ApplicationException("HOTP authenticator should have a counter");
                }

                AuthenticatorDTO authenticatorDto = new();

                AuthenticatorValueDTO auth;
                if (string.Compare(issuer, "BattleNet", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    string? serial = query["serial"];
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
                else if (string.Compare(issuer, "Steam", StringComparison.OrdinalIgnoreCase) == 0)
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

                    if (string.Compare(issuer, "Google", StringComparison.OrdinalIgnoreCase) == 0)
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

                if (Enum.TryParse(query["algorithm"], true, out HMACTypes hmactype))
                {
                    auth.HMACType = hmactype;
                }

                if (label.Length != 0)
                {
                    authenticatorDto.Name = issuer.Length != 0 ? issuer + " (" + label + ")" : label;
                }
                else if (issuer.Length != 0)
                {
                    authenticatorDto.Name = issuer;
                }
                else
                {
                    authenticatorDto.Name = "Imported";
                }

                authenticatorDto.Value = auth;

                // sync
                Toast.Show(ToastIcon.Info, Strings.LocalAuth_AddAuthSyncTip, ToastLength.Short);
                authenticatorDto.Value.Sync();

                await AuthenticatorHelper.SaveAuthenticator(authenticatorDto, password, isLocalProtect);
            }
            Toast.Show(ToastIcon.Success, Strings.LocalAuth_AddAuthSuccess);
        }
        catch (UriFormatException)
        {
            Toast.Show(ToastIcon.Error, $"UriFormatException Invalid authenticator at line {linenumber}");
        }
        catch (Exception ex)
        {
            Toast.Show(ToastIcon.Error, $"Error importing at line {ex.Message}");
        }
    }
}