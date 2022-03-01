using DynamicData;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Entities;
using System.Application.Models;
using System.Application.UI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using WinAuth;
using static System.Application.Models.GAPAuthenticatorValueDTO;
using GAPRepository = System.Application.Repositories.IGameAccountPlatformAuthenticatorRepository;

// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    public sealed class AuthService : ReactiveObject
    {
        const string TAG = "AuthS";

        static AuthService? mCurrent;
        public static AuthService Current => mCurrent ?? new();

        static Stream? GetQrCodeStream(IEnumerable<IGAPAuthenticatorDTO> datas)
        {
            var dtos = datas.Select(x => x.ToLightweightExportDTO()).ToArray();
            var bytes = Serializable.SMP(dtos);
#if DEBUG
            var bytes_compress_gzip = bytes.CompressByteArray();
#endif
            var bytes_compress_br = bytes.CompressByteArrayByBrotli();
#if DEBUG
            Toast.Show($"bytesLength, source: {bytes.Length}, gzip: {bytes_compress_gzip.Length}, br: {bytes_compress_br.Length}");
#endif
            (var result, var stream, var e) = QRCodeHelper.Create(bytes_compress_br);
            switch (result)
            {
                case QRCodeHelper.QRCodeCreateResult.DataTooLong:
                    Toast.Show(AppResources.AuthLocal_ExportToQRCodeTooLongErrorTip);
                    break;
                case QRCodeHelper.QRCodeCreateResult.Exception:
                    e?.LogAndShowT(TAG);
                    break;
            }
            return stream;
        }

        public static async Task<Stream?> GetQrCodeStreamAsync(IEnumerable<IGAPAuthenticatorDTO> datas)
        {
            var qrCode = await Task.Run(() =>
            {
                return GetQrCodeStream(datas);
            });
            return qrCode;
        }

        public static Task<Stream?> GetQrCodeStreamAsync(params IGAPAuthenticatorDTO[] datas)
        {
            IEnumerable<IGAPAuthenticatorDTO> datas_ = datas;
            return GetQrCodeStreamAsync(datas_);
        }

        private readonly GAPRepository repository = DI.Get<GAPRepository>();

        public SourceCache<MyAuthenticator, ushort> Authenticators { get; }

        public GAPRepository Repository => repository;

        private AuthService()
        {
            mCurrent = this;

            Authenticators = new(t => t.Id);
        }

        public async Task InitializeAsync(bool isSync = false)
        {
            var auths = await repository.GetAllSourceAsync();
            await InitializeAsync(auths, isSync);
        }

        public async Task InitializeAsync(GameAccountPlatformAuthenticator[] auths, bool isSync = false)
        {
            var hasPassword = repository.HasSecondaryPassword(auths);
            List<IGAPAuthenticatorDTO> list;
            if (hasPassword)
            {
                var password = await TextBoxWindowViewModel.ShowDialogByPresetAsync(TextBoxWindowViewModel.PresetType.LocalAuth_PasswordRequired);
                if (string.IsNullOrEmpty(password))
                {
                    return;
                }
                list = await repository.ConvertToList(auths, password);
            }
            else
            {
                list = await repository.ConvertToList(auths);
            }
            if (list.Any_Nullable())
            {
                var authenticators = list.Select(s => new MyAuthenticator(s));

                //MainThread2.BeginInvokeOnMainThread(() =>
                //{
                if (isSync)
                {
                    Task.Run(() =>
                    {
                        foreach (var item in authenticators)
                            item.Sync();
                        //ToastService.Current.Notify(AppResources.LocalAuth_RefreshAuthSuccess);
                    }).ForgetAndDispose();
                }
                //else
                //    ToastService.Current.Notify(AppResources.LocalAuth_RefreshAuthSuccess);
                //});
                Authenticators.Clear();
                Authenticators.AddOrUpdate(authenticators);
            }
            else
            {
                Toast.Show(AppResources.LocalAuth_ProtectionAuth_PasswordError);
            }
        }

        /// <summary>
        /// 是否设置了密码加密
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HasPasswordEncryption()
        {
            var auths = await repository.GetAllSourceAsync();
            if (!auths.Any_Nullable())
            {
                return false;
            }
            return repository.HasSecondaryPassword(auths);
        }

        /// <summary>
        /// 是否设置了密码加密且弹出解密框
        /// </summary>
        /// <returns></returns>
        public async Task<(bool success, string? password)> HasPasswordEncryptionShowPassWordWindow()
        {
            var auth = await repository.GetFirstOrDefaultSourceAsync();
            if (auth == null)
            {
                return (false, null);
            }
            var hasPassword = repository.HasSecondaryPassword(auth);
            if (hasPassword)
            {
                var password = await TextBoxWindowViewModel.ShowDialogByPresetAsync(TextBoxWindowViewModel.PresetType.LocalAuth_PasswordRequired);
                var list = await repository.ConvertToList(new[] { auth }, password);
                if (list.Any_Nullable())
                {
                    return (true, password);
                }
                Toast.Show(AppResources.LocalAuth_ProtectionAuth_PasswordError);
                return (false, null);
            }
            else
            {
                //没有设置密码直接成功
                return (true, null);
            }
        }

        [Obsolete]
        public async Task<int> GetRealAuthenticatorCount()
        {
            var auths = await repository.GetAllSourceAsync();
            return auths.Count();
        }

        /// <inheritdoc cref="ImportAuthenticatorFile(string, bool, string?, string?)"/>
        public bool ImportWinAuthenticators(IEnumerable<string> urls, bool isLocal, string? password)
        {
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
                    if (line.Length == 0 || line.IndexOf("#") == 0)
                    {
                        continue;
                    }

                    // bug if there is a hash before ?
                    var hash = line.IndexOf("#");
                    var qm = line.IndexOf("?");
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
                    string label = string.IsNullOrEmpty(uri.LocalPath) == false ? uri.LocalPath[1..] : string.Empty; // skip past initial /
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

                    GAPAuthenticatorDTO importedAuthenticator = new();
                    //
                    GAPAuthenticatorValueDTO auth;
                    if (string.Compare(issuer, "BattleNet", true) == 0)
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

                        ((BattleNetAuthenticator)auth).SecretKey = WinAuthBase32.GetInstance().Decode(secret);

                        ((BattleNetAuthenticator)auth).Serial = serial;

                        issuer = string.Empty;
                    }
                    else if (string.Compare(issuer, "Steam", true) == 0)
                    {
                        auth = new SteamAuthenticator();
                        ((SteamAuthenticator)auth).SecretKey = WinAuthBase32.GetInstance().Decode(secret);
                        ((SteamAuthenticator)auth).Serial = string.Empty;
                        ((SteamAuthenticator)auth).DeviceId = query["deviceid"] ?? string.Empty;
                        ((SteamAuthenticator)auth).SteamData = query["data"] ?? string.Empty;
                        issuer = string.Empty;
                    }
                    else if (uri.Host == "hotp")
                    {
                        auth = new HOTPAuthenticator();
                        ((HOTPAuthenticator)auth).SecretKey = WinAuthBase32.GetInstance().Decode(secret);
                        ((HOTPAuthenticator)auth).Counter = int.Parse(counter);

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

                    if (Enum.TryParse(query["algorithm"], true, out HMACTypes hmactype) == true)
                    {
                        auth.HMACType = hmactype;
                    }

                    //
                    if (label.Length != 0)
                    {
                        importedAuthenticator.Name = (issuer.Length != 0 ? issuer + " (" + label + ")" : label);
                    }
                    else if (issuer.Length != 0)
                    {
                        importedAuthenticator.Name = issuer;
                    }
                    else
                    {
                        importedAuthenticator.Name = "Imported";
                    }
                    //
                    importedAuthenticator.Value = auth;

                    // sync
                    Toast.Show(AppResources.LocalAuth_AddAuthSyncTip, ToastLength.Short);
                    importedAuthenticator.Value.Sync();

                    AddOrUpdateSaveAuthenticators(importedAuthenticator, isLocal, password);
                }
                Toast.Show(AppResources.LocalAuth_AddAuthSuccess);
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

        /// <summary>
        /// WinAuth令牌导入
        /// </summary>
        public bool ImportWinAuthenticators(string filePath, bool isLocal, string? password)
        {
            var urls = ReadUrlsByFilePath(filePath);
            return ImportWinAuthenticators(urls, isLocal, password);
        }

        /// <summary>
        /// Steam APP令牌导入
        /// </summary>
        /// <returns>true if successful</returns>
        public bool ImportSteamGuard(string name, string uuid, string steamGuard, bool isLocal, string? password)
        {
            if (string.IsNullOrEmpty(uuid))
            {
                //WinAuthForm.ErrorDialog(this, "Please enter the contents of the steam.uuid.xml file or your DeviceId");
                return false;
            }
            if (steamGuard.Length == 0)
            {
                //WinAuthForm.ErrorDialog(this, "Please enter the contents of your SteamGuard file");
                return false;
            }

            // check the deviceid
            string deviceId;
            if (uuid.IndexOf("?xml") != -1)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(uuid);
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
                    Log.Error(nameof(AuthService), ex, nameof(ImportSteamGuard));
                    return false;
                }
            }
            else
            {
                deviceId = uuid;
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
                var json = JObject.Parse(steamGuard);

                var node = json.SelectToken("shared_secret");
                if (node == null)
                {
                    throw new ApplicationException("no shared_secret");
                }
                secret = Convert.FromBase64String(node.Value<string>());

                node = json.SelectToken("serial_number");
                if (node == null)
                {
                    throw new ApplicationException("no serial_number");
                }
                serial = node.Value<string>()!;
            }
            catch (Exception ex)
            {
                //WinAuthForm.ErrorDialog(this, "Invalid SteamGuard JSON contents: " + ex.Message);
                //ToastService.Current.Notify("Invalid SteamGuard JSON");
                Log.Error(nameof(AuthService), ex, nameof(ImportSteamGuard));
                return false;
            }

            var auth = new SteamAuthenticator
            {
                SecretKey = secret,
                Serial = serial,
                SteamData = steamGuard,
                DeviceId = deviceId
            };

            AddOrUpdateSaveAuthenticators(new GAPAuthenticatorDTO
            {
                Name = string.IsNullOrEmpty(name) ? "Steam (" + auth.AccountName + ")" : name,
                Value = auth,
            }, isLocal, password);
            return true;
        }

        public bool ImportSDAFile(string mafile, bool isLocal, string? password)
        {
            string data;
            if (File.Exists(mafile) == false || (data = File.ReadAllText(mafile)) == null)
            {
                throw new ApplicationException("Cannot read file " + mafile);
            }

            var token = JObject.Parse(data);
            var sdaentry = new ImportedSDAEntry
            {
                Username = token.SelectToken("account_name")?.Value<string>(),
            };
            if (string.IsNullOrEmpty(sdaentry.SteamId) == true)
            {
                sdaentry.SteamId = token.SelectToken("Session.SteamID")?.Value<string>();
            }
            if (string.IsNullOrEmpty(sdaentry.SteamId) == true)
            {
                sdaentry.SteamId = mafile.Split('.')[0];
            }
            sdaentry.json = data;

            //importSDAList.Items.Add(sdaentry);
            SteamAuthenticator auth = new();
            GAPAuthenticatorDTO winAuth = new();
            foreach (var prop in token.Root.Children().ToList())
            {
                var child = token.SelectToken(prop.Path);

                string lkey = prop.Path.ToLower();
                if (lkey == "fully_enrolled" || lkey == "session")
                {
                    prop.Remove();
                }
                else if (lkey == "device_id")
                {
                    auth.DeviceId = child!.Value<string>();
                    prop.Remove();
                }
                else if (lkey == "serial_number")
                {
                    auth.Serial = child!.Value<string>();
                }
                else if (lkey == "account_name")
                {
                    //if (this.nameField.Text.Length == 0)
                    //{
                    //    this.nameField.Text = "Steam (" + child.Value<string>() + ")";
                    //}
                    winAuth.Name = "Steam (" + child!.Value<string>() + ")";
                }
                else if (lkey == "shared_secret")
                {
                    auth.SecretKey = Convert.FromBase64String(child!.Value<string>());
                }
            }
            auth.SteamData = token.ToString(Newtonsoft.Json.Formatting.None);
            winAuth.Value = auth;

            AddOrUpdateSaveAuthenticators(winAuth, isLocal, password);
            return true;
        }

        /// <inheritdoc cref="ImportAuthenticatorFile(string, bool, string?, string?)"/>
        public async Task<bool> ImportAuthenticatorFile(byte[] bt, bool isLocal, string? password, string? exportPassword = null)
        {
            var isOK = false;
        Run:
            var result = await repository.ImportAsync(exportPassword, bt!);

            if (result.resultCode == GAPRepository.ImportResultCode.Success)
            {
                foreach (var item in result.result)
                {
                    AddOrUpdateSaveAuthenticators(new MyAuthenticator(item), isLocal, password);
                }

                Toast.Show(AppResources.LocalAuth_AddAuthSuccess);
                isOK = true;
            }
            else if (result.resultCode == GAPRepository.ImportResultCode.PartSuccess)
            {
                foreach (var item in result.result)
                {
                    AddOrUpdateSaveAuthenticators(new MyAuthenticator(item), isLocal, password);
                }
                Toast.Show(AppResources.LocalAuth_AddAuth_PartSuccess);
                isOK = true;
            }
            else if (result.resultCode == GAPRepository.ImportResultCode.SecondaryPasswordFail)
            {
                Toast.Show(AppResources.LocalAuth_ProtectionAuth_PasswordErrorTip);
                exportPassword = await TextBoxWindowViewModel.ShowDialogByPresetAsync(TextBoxWindowViewModel.PresetType.LocalAuth_PasswordRequired);
                if (exportPassword == null) return false; // 当值为 null 时代表取消
                else goto Run;
            }
            else
            {
                Toast.Show(AppResources.LocalAuth_ExportAuth_Error.Format(result.resultCode));
            }
            return isOK;
        }

        /// <summary>
        /// 导入Steam++导出的令牌数据文件 V2
        /// </summary>
        public async Task<bool> ImportAuthenticatorFile(string file, bool isLocal, string? password, string? exportPassword = null)
        {
            if (File.Exists(file))
            {
                (bool success, byte[]? bt, Exception? _) = await IOPath.TryReadAllBytesAsync(file);
                if (!success)
                {
                    //Toast.Show($"Import authenticator file fail, exception: {ex}");
                    return false;
                }
                return await ImportAuthenticatorFile(bt!, isLocal, password, exportPassword);
            }
            return false;
        }

        public bool ImportSteamToolsV1Authenticator(string file, bool isLocal, string? password)
        {
            var isOK = false;
            if (IOPath.TryReadAllText(file, out var content, out var _))
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
                                var wa = new MyAuthenticator();
                                wa.ReadXml(reader, null);
                                AddOrUpdateSaveAuthenticators(wa, isLocal, password);
                                isOK = true;
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
            return isOK;
        }

        public void AddOrUpdateSaveAuthenticators(GAPAuthenticatorDTO auth, bool isLocal, string? password)
        {
            AddOrUpdateSaveAuthenticators(new MyAuthenticator(auth), isLocal, password);
        }

        public async void AddOrUpdateSaveAuthenticators(MyAuthenticator auth, bool isLocal, string? password)
        {
            var repository = DI.Get<GAPRepository>();
            await repository.InsertOrUpdateAsync(auth.AuthenticatorData, isLocal, password);
            if (Authenticators.Items.Any(s => s.Id == auth.Id))
            {
                return;
            }
            Authenticators.AddOrUpdate(auth);
        }

        public async void AddOrUpdateSaveAuthenticators(IEnumerable<MyAuthenticator> auths)
        {
            if (auths.Any_Nullable())
            {
                var repository = DI.Get<GAPRepository>();
                var sources = await repository.GetAllSourceAsync();
                if (!sources.Any_Nullable())
                {
                    foreach (var item in auths)
                    {
                        AddOrUpdateSaveAuthenticators(item, false, null);
                    }
                }
                else
                {
                    var hasLocal = repository.HasLocal(sources);
                    var result = await Current.HasPasswordEncryptionShowPassWordWindow();
                    var password = result.password;
                    foreach (var item in auths)
                    {
                        AddOrUpdateSaveAuthenticators(item, hasLocal, password);
                    }
                }
            }
        }

        public static async void DeleteSaveAuthenticators(MyAuthenticator auth)
        {
            var repository = DI.Get<GAPRepository>();
            if (auth.AuthenticatorData.ServerId.HasValue)
            {
                await repository.DeleteAsync(auth.AuthenticatorData.ServerId.Value);
            }
            await repository.DeleteAsync(auth.AuthenticatorData.Id);
            Current.Authenticators.Remove(auth);
        }

        public async Task SaveEditNameByAuthenticatorAsync(MyAuthenticator auth)
        {
            var isLocal = await DI.Get<GAPRepository>().HasLocalAsync();
            await repository.RenameAsync(auth.Id, auth.Name, isLocal);
            auth.OriginName = auth.Name;
        }

        public async void SwitchEncryptionAuthenticators(bool isLocal, string? password)
        {
            try
            {
                await repository.SwitchEncryptionModeAsync(isLocal, password, Authenticators.Items.Select(s => s.AuthenticatorData));
            }
            catch (Exception ex)
            {
                Toast.Show(AppResources.LocalAuth_ProtectionAuth_Error + ex.Message);
                return;
            }
            Toast.Show(AppResources.LocalAuth_ProtectionAuth_Success);
        }

        public IEnumerable<IGAPAuthenticatorDTO> GetExportSourceAuthenticators(Func<MyAuthenticator, bool>? predicateWhere = null)
        {
            var items = Authenticators.Items;
            if (predicateWhere != null) items = items.Where(predicateWhere);
            return items.Select(s => s.AuthenticatorData);
        }

        //public async Task<byte[]> GetExportAuthenticatorsAsync(bool isLocal, string? password = null, Func<MyAuthenticator, bool>? predicateWhere = null)
        //{
        //    var items = GetExportSourceAuthenticators(predicateWhere);
        //    var bt = await repository.ExportAsync(isLocal, password, items);
        //    return bt;
        //}

        public async Task GetExportAuthenticatorsAsync(Stream stream, bool isLocal, string? password = null, Func<MyAuthenticator, bool>? predicateWhere = null)
        {
            var items = GetExportSourceAuthenticators(predicateWhere);
            await repository.ExportAsync(stream, isLocal, password, items);
        }

        public async void ExportAuthenticators(Stream? fileWriteStream, bool isLocal, string? password = null, Func<MyAuthenticator, bool>? predicateWhere = null)
        {
            try
            {
                if (fileWriteStream == null)
                {
                    Toast.Show(AppResources.LocalAuth_ProtectionAuth_PathError);
                    return;
                }

                if (fileWriteStream.CanSeek && fileWriteStream.Position != 0)
                    fileWriteStream.Position = 0;

                await GetExportAuthenticatorsAsync(fileWriteStream, isLocal, password, predicateWhere);
                await fileWriteStream.FlushAsync();
                await fileWriteStream.DisposeAsync();
            }
            catch (Exception e)
            {
                e.LogAndShowT(TAG);
            }
        }

        /// <summary>
        /// 查看当前验证码URL
        /// </summary>
        public static async void ShowCaptchaUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Toast.Show("CaptchaUrl is null or white space.");
            }
            else
            {
                if (!await Browser2.OpenAsync(value))
                {
                    await Clipboard2.SetTextAsync(value);
                    Toast.Show(AppResources.CopyToClipboard);
                }
            }
        }
    }
}