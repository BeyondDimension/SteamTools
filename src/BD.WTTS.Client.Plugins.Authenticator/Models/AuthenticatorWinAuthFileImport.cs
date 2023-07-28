using WinAuth;

namespace BD.WTTS.Models;

public class AuthenticatorWinAuthFileImport : AuthenticatorFileImportBase
{
    public override string Name => Strings.LocalAuth_Import.Format(Strings.WinAuth);

    public override string Description => Strings.LocalAuth_WinAuthImport;

    public override ResIcon IconName => ResIcon.OpenFile;

    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    protected override string FileExtension => FileEx.Txt;

    public AuthenticatorWinAuthFileImport(string? password = null)
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await VerifyMaxValue())
                await ImportFromWinAuthFile(password);
        });
    }

    async Task ImportFromWinAuthFile(string? password = null)
    {
        var filePath = await SelectFolderPath();

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

                if (Enum.TryParse(query["algorithm"], true, out IAuthenticatorValueDTO.HMACTypes hmactype))
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

                await SaveAuthenticator(authenticatorDto);
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