using Avalonia.Controls;
using BD.WTTS.Helpers;
using BD.WTTS.UI.Views.Pages;
using WinAuth;
using static SteamKit2.DepotManifest;
using SJsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.UI.ViewModels;

public record AuthenticatorImportMethod(string Name, string Description, ICommand Command);

public partial class AuthenticatorImportPageViewModel : ViewModelBase
{
    public static string Name => Strings.AuthImport;

    public IReadOnlyCollection<AuthenticatorImportMethod> AuthenticatorImportMethods { get; }

    public ICommand BackCommand { get; }

    [Reactive]
    public partial Control? ImportContent { get; private set; }

    [Reactive]
    public partial string? ImportTitle { get; private set; }

    public bool IsSelecting => ImportContent is null;

    public AuthenticatorImportPageViewModel()
    {
        var backCommand = ReactiveCommand.Create(CloseImportPage);
        CompositeDisposable.Add(backCommand);
        BackCommand = backCommand;

        AuthenticatorImportMethods = new List<AuthenticatorImportMethod>
        {
            PageMethod(Strings.LocalAuth_Import.Format(Strings.LocalAuth_JoinSteamAuthenticator), Strings.LocalAuth_JoinSteamAuthenticatorDesc,
                static () => new JoinSteamAuthenticatorPage()),
            PageMethod(Strings.Auth_SteamLoginImport, Strings.Steam_UserLoginTip, static () => new SteamLoginImportPage()),
            PageMethod(Strings.LocalAuth_Import.Format(Strings.SteamGuard), Strings.LocalAuth_SteamGuardImport,
                static () => new SteamGuardImportPage()),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.Mafile), Strings.LocalAuth_SDAImport,
                                            Command: CreateImportCommand(async () =>
                                            {
                                                if (await AuthenticatorHelper.VerifyMaxValue())
                                                    await ImportFromSdaFile();
                                            })),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.WattToolKitV2), Strings.LocalAuth_WattToolKitV2Import,
                                            Command: CreateImportCommand(async () =>
                                            {
                                                if (await AuthenticatorHelper.VerifyMaxValue())
                                                {
                                                    var r = await AuthenticatorHelper.GetCurrentPassword();
                                                    if (!r.authorized) return;
                                                    await ImportFromWattToolKitV2(r.password, r.hasLocalPcEncrypt, r.verifiedWithHello);
                                                }
                                            })),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.WattToolKitV1), Strings.LocalAuth_WattToolKitV1Import,
                                            Command: CreateImportCommand(async () =>
                                            {
                                                if (await AuthenticatorHelper.VerifyMaxValue())
                                                {
                                                    var r = await AuthenticatorHelper.GetCurrentPassword();
                                                    if (!r.authorized) return;
                                                    await ImportFromWattToolKitV1(r.password, r.hasLocalPcEncrypt, r.verifiedWithHello);
                                                }
                                            })),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.WinAuth), Strings.LocalAuth_WinAuthImport,
                                            Command: CreateImportCommand(async () =>
                                            {
                                                if (await AuthenticatorHelper.VerifyMaxValue())
                                                {
                                                    var r = await AuthenticatorHelper.GetCurrentPassword();
                                                    if (!r.authorized) return;
                                                    await ImportFromWinAuthFile(r.password, r.hasLocalPcEncrypt, r.verifiedWithHello);
                                                }
                                            })),
            PageMethod(Strings.LocalAuth_Import.Format(Strings.Google), Strings.LocalAuth_GoogleImport,
                static () => new AuthenticatorGeneralImportPage(AuthenticatorPlatform.Google)),
            PageMethod(Strings.LocalAuth_Import.Format(Strings.Microsoft), Strings.LocalAuth_MicrosoftImport,
                static () => new AuthenticatorGeneralImportPage(AuthenticatorPlatform.Microsoft)),
            PageMethod(Strings.LocalAuth_Import.Format(Strings.HOTP), Strings.LocalAuth_HOTPImport,
                static () => new AuthenticatorGeneralImportPage(AuthenticatorPlatform.HOTP)),
        };
    }

    AuthenticatorImportMethod PageMethod(string name, string description, Func<Control> createPage)
    {
        var command = ReactiveCommand.Create(() =>
        {
            if (!IsSelecting) return;

            ImportContent = createPage();
            ImportTitle = name;
            this.RaisePropertyChanged(nameof(IsSelecting));
            TracepointHelper.TrackEvent("AuthenticatorImport", new Dictionary<string, string> { ["ImportMethod"] = name });
        });
        CompositeDisposable.Add(command);
        CompositeDisposable.Add(command.ThrownExceptions.Subscribe(static ex => ex.LogAndShowT()));
        return new(name, description, command);
    }

    void CloseImportPage()
    {
        if (ImportContent is not { } page) return;
        ImportContent = null;
        ImportTitle = null;
        this.RaisePropertyChanged(nameof(IsSelecting));
        (page.DataContext as ViewModelBase)?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
            CloseImportPage();
        base.Dispose(disposing);
    }

    ICommand CreateImportCommand(Func<Task> import)
    {
        var command = ReactiveCommand.CreateFromTask(import);
        CompositeDisposable.Add(command);
        CompositeDisposable.Add(command.ThrownExceptions.Subscribe(static ex => ex.LogAndShowT()));
        return command;
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

    static async Task ImportFromWattToolKitV2(string? password, bool isLocalProtect, bool verifiedWithHello)
    {
        var filePath = await AuthenticatorHelper.SelectFolderPath(FileEx.MPO);

        if (string.IsNullOrEmpty(filePath)) return;

        var metadata = await IOPath.TryReadAllBytesAsync(filePath);
        if (!metadata.success || metadata.byteArray == null) return;
        string? exportPassword = null;
        while (true)
        {
            var result = await AuthenticatorHelper.ImportAsync(exportPassword, metadata.byteArray);
            if (result.resultCode == IAccountPlatformAuthenticatorRepository.ImportResultCode.SecondaryPasswordFail)
            {
                if (exportPassword != null)
                    Toast.Show(ToastIcon.Warning, Strings.Auth_VaultPasswordFailed);
                exportPassword = await AuthenticatorHelper.RequestPasswordAsync(Strings.ModelContent_ExportPassword);
                if (string.IsNullOrWhiteSpace(exportPassword)) return;
                continue;
            }

            if (result.resultCode != IAccountPlatformAuthenticatorRepository.ImportResultCode.Success)
            {
                Toast.Show(ToastIcon.Error, AuthenticatorHelper.ReadErrorText(result.resultCode));
                return;
            }

            try
            {
                await AuthenticatorHelper.SaveImportedAuthenticatorsAsync(result.result, isLocalProtect, password, verifiedWithHello);
                Toast.Show(ToastIcon.Success, Strings.LocalAuth_AddAuthSuccess);
            }
            catch (AuthenticatorReadException ex)
            {
                Toast.Show(ToastIcon.Warning, AuthenticatorHelper.ReadErrorText(ex.ResultCode));
            }
            catch (Exception ex)
            {
                Log.Error(nameof(AuthenticatorImportPageViewModel), ex, "Could not import the token backup.");
                Toast.Show(ToastIcon.Error, Strings.Auth_VaultChangeFailed);
            }
            return;
        }
    }

    static async Task ImportFromWattToolKitV1(string? password, bool isLocalProtect, bool verifiedWithHello)
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
                        if (!await AuthenticatorHelper.SaveAuthenticator(authDto, password, isLocalProtect, verifiedWithHello)) return;
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

    static async Task ImportFromWinAuthFile(string? password, bool isLocalProtect, bool verifiedWithHello)
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

                if (!await AuthenticatorHelper.SaveAuthenticator(authenticatorDto, password, isLocalProtect, verifiedWithHello)) return;
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