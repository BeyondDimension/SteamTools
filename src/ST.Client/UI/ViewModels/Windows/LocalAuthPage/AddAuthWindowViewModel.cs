using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using static System.Application.FilePicker2;
using static System.Application.Models.GAPAuthenticatorDTO;
using static System.Application.Repositories.IGameAccountPlatformAuthenticatorRepository;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class AddAuthWindowViewModel : WindowViewModel
    {
        readonly IHttpService httpService = IHttpService.Instance;
        readonly IGameAccountPlatformAuthenticatorRepository repository = DI.Get<IGameAccountPlatformAuthenticatorRepository>();

        public static string DisplayName => AppResources.LocalAuth_AddAuth;

        public AddAuthWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);

            SppV2Btn_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                var fileTypes = !IsSupportedFileExtensionFilter ? (FilePickerFileType?)null : new FilePickerFilter(new (string, IEnumerable<string>)[] {
                    ("MsgPack Files", new[] { "mpo" }),
                    ("Data Files", new[] { "dat" }),
                    ("All Files", new[] { "*" }),
                });
                await PickAsync(ImportSteamPlusPlusV2, fileTypes);
            });
            SppBtn_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                var fileTypes = !IsSupportedFileExtensionFilter ? (FilePickerFileType?)null : new FilePickerFilter(new (string, IEnumerable<string>)[] {
                    ("Data Files", new[] { "dat" }),
                    ("All Files", new[] { "*" }),
                });
                await PickAsync(ImportSteamPlusPlusV1, fileTypes);
            });
            SdaBtn_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                var fileTypes = !IsSupportedFileExtensionFilter ? (FilePickerFileType?)null : new FilePickerFilter(new (string, IEnumerable<string>)[] {
                    ("MaFile Files", new[] { "maFile" }),
                    ("JSON Files", new[] { "json" }),
                    ("All Files", new[] { "*" }),
                });
                await PickAsync(ImportSDA, fileTypes);
            });
            WinAuthBtn_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                var fileTypes = OperatingSystem2.IsAndroid ?
                        new GeneralFilePickerFileType(new[] { MediaTypeNames.TXT }) :
                        (!IsSupportedFileExtensionFilter ? (FilePickerFileType?)null : new FilePickerFilter(new (string, IEnumerable<string>)[] {
                            ("Text Files", new[] { "txt" }),
                            ("All Files", new[] { "*" }),
                        }));
                await PickAsync(ImportWinAuth, fileTypes);
            });

            Initialize();
        }

        GAPAuthenticatorValueDTO.SteamAuthenticator? _SteamAuthenticator;
        readonly GAPAuthenticatorValueDTO.SteamAuthenticator.EnrollState _Enroll = new() { RequiresLogin = true };
        string? AuthPassword;
        bool AuthIsLocal;

        new async void Initialize()
        {
            var auths = await repository.GetAllSourceAsync();
            var hasPassword = repository.HasSecondaryPassword(auths);
            if (hasPassword)
            {
                var (success, password) = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
                if (success)
                {
                    AuthPassword = password;
                }
                else
                {
                    Close();
                }
            }
            AuthIsLocal = repository.HasLocal(auths);
        }

        string? _AuthName;
        public string? AuthName
        {
            get => _AuthName;
            set
            {
                if (value != null && value.Length > IGAPAuthenticatorDTO.MaxLength_Name)
                {
                    value = value.Substring(0, IGAPAuthenticatorDTO.MaxLength_Name);
                }
                _AuthName = value;
            }
        }

        string? _UUID;
        public string? UUID
        {
            get => _UUID;
            set
            {
                if (value != null && !value.StartsWith("android:", StringComparison.Ordinal))
                {
                    value = $"android:{value}";
                }
                _UUID = value;
            }
        }

        public string? SteamGuard { get; set; }

        public bool RequiresLogin
        {
            get
            {
                return _Enroll.RequiresLogin;
            }
            set
            {
                _Enroll.RequiresLogin = value;
                this.RaisePropertyChanged();
            }
        }

        public bool RequiresActivation
        {
            get
            {
                return _Enroll.RequiresActivation;
            }
            set
            {
                this.RaisePropertyChanged();
            }
        }

        public string? UserName
        {
            get
            {
                return _Enroll.Username;
            }
            set
            {
                _Enroll.Username = value;
            }
        }

        public string? Password
        {
            get
            {
                return _Enroll.Password;
            }
            set
            {
                _Enroll.Password = value;
            }
        }

        public string? CaptchaText
        {
            get
            {
                return _Enroll.CaptchaText;
            }
            set
            {
                _Enroll.CaptchaText = value;
                this.RaisePropertyChanged();
            }
        }

        public string? EmailAuthText
        {
            get
            {
                return _Enroll.EmailAuthText;
            }
            set
            {
                _Enroll.EmailAuthText = value;
            }
        }

        public string? ActivationCode
        {
            get
            {
                return _Enroll.ActivationCode;
            }
            set
            {
                _Enroll.ActivationCode = value;
            }
        }

        public string? RevocationCode
        {
            get
            {
                return _Enroll.RevocationCode;
            }
            set
            {
                this.RaisePropertyChanged();
            }
        }

        private string? _CaptchaImage;
        public string? CaptchaImage
        {
            get => _CaptchaImage;
            set => this.RaiseAndSetIfChanged(ref _CaptchaImage, value);
        }

        public void CaptchaUrlButton_Click()
        {
            AuthService.ShowCaptchaUrl(_CaptchaImage);
        }

        private string? _EmailDomain;
        public string? EmailDomain
        {
            get => _EmailDomain;
            set => this.RaiseAndSetIfChanged(ref _EmailDomain, value);
        }

        private bool _RequiresAdd;
        public bool RequiresAdd
        {
            get => _RequiresAdd;
            set => this.RaiseAndSetIfChanged(ref _RequiresAdd, value);
        }

        public void ImportSteamGuard() => ImportSteamGuard2();

        public bool ImportSteamGuard2()
        {
            if (AuthService.Current.ImportSteamGuard(AuthName!, UUID!, SteamGuard!, AuthIsLocal, AuthPassword))
            {
                Toast.Show(AppResources.LocalAuth_AddAuthSuccess);
                return true;
            }
            else
            {
                Toast.Show(AppResources.LocalAuth_ImportFaild);
                return false;
            }
        }

        private bool _IsLogining = false;
        public void LoginSteamImport()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                return;
            }

            if (_IsLogining)
            {
                return;
            }

            _IsLogining = true;

            if (_SteamAuthenticator == null)
                _SteamAuthenticator = new GAPAuthenticatorValueDTO.SteamAuthenticator();

            _Enroll.Language = R.GetCurrentCultureSteamLanguageName();

            bool result = false;
            Task.Run(() =>
            {
                var isSupportedToastService = ToastService.IsSupported;
                if (isSupportedToastService)
                {
                    ToastService.Current.Set(AppResources.Logining);
                }
                LoginSteamLoadingText = AppResources.Logining;
                result = _SteamAuthenticator.Enroll(_Enroll);
                if (isSupportedToastService)
                {
                    ToastService.Current.Set();
                }
                LoginSteamLoadingText = null;

                if (!result)
                {
                    if (string.IsNullOrEmpty(_Enroll.Error) == false)
                    {
                        MessageBox.Show(_Enroll.Error);
                        //ToastService.Current.Notify(_Enroll.Error);
                    }

                    //已有令牌无法导入
                    if (_Enroll.Requires2FA == true)
                    {
                        MessageBox.Show(AppResources.LocalAuth_SteamUser_Requires2FA);
                        //ToastService.Current.Notify(AppResources.LocalAuth_SteamUser_Requires2FA);
                        return;
                    }

                    //频繁登录会需要验证码
                    if (_Enroll.RequiresCaptcha == true)
                    {
                        CaptchaText = null;
                        CaptchaImage = null;
                        //using var web = new WebClient();
                        //var bt = web.DownloadData(_Enroll.CaptchaUrl);
                        //using var stream = new MemoryStream(bt);
                        CaptchaImage = _Enroll.CaptchaUrl;
                        return;
                    }

                    //需要邮箱验证
                    if (_Enroll.RequiresEmailAuth == true)
                    {
                        RequiresLogin = false;
                        CaptchaText = null;
                        CaptchaImage = null;
                        EmailDomain = string.IsNullOrEmpty(_Enroll.EmailDomain) == false ? "***@" + _Enroll.EmailDomain : string.Empty;
                        return;
                    }

                    //需要短信验证 此步骤导入还没有结束
                    if (_Enroll.RequiresActivation == true)
                    {
                        EmailDomain = null;
                        _Enroll.Error = null;
                        RequiresLogin = false;

                        RequiresActivation = true;
                        RevocationCode = _Enroll.RevocationCode;
                        return;
                    }

                    if (_Enroll.RequiresLogin == true)
                    {
                        return;
                    }

                    string error = _Enroll.Error!;
                    if (string.IsNullOrEmpty(error) == true)
                    {
                        error = AppResources.LocalAuth_SteamUser_Error;
                    }
                    MessageBox.Show(_Enroll.Error!);
                    return;
                }
                else
                {
                    RequiresActivation = false;
                    RequiresAdd = true;

                    //导入成功，添加令牌
                    AuthService.Current.AddOrUpdateSaveAuthenticators(new GAPAuthenticatorDTO
                    {
                        Name = nameof(GamePlatform.Steam) + "(" + UserName + ")",
                        Value = _SteamAuthenticator
                    }, AuthIsLocal, AuthPassword);

                    Toast.Show(AppResources.LocalAuth_SteamUserImportSuccess);
                }
            }).ContinueWith(s =>
            {
                Log.Error(nameof(AddAuthWindowViewModel), s.Exception, nameof(LoginSteamImport));
                MessageBox.Show(s.Exception, "Error " + nameof(LoginSteamImport));
            }, TaskContinuationOptions.OnlyOnFaulted).ContinueWith(s =>
            {
                _IsLogining = false;
                if (ToastService.IsSupported)
                {
                    ToastService.Current.Set();
                }
                LoginSteamLoadingText = null;
                s.Dispose();
            });
        }

        public void ImportWinAuth(string filePath)
        {
            AuthService.Current.ImportWinAuthenticators(filePath, AuthIsLocal, AuthPassword);
        }

        public void ImportSDA(string filePath)
        {
            AuthService.Current.ImportSDAFile(filePath, AuthIsLocal, AuthPassword);
        }

        public void ImportSteamPlusPlusV1(string filePath)
        {
            AuthService.Current.ImportSteamToolsV1Authenticator(filePath, AuthIsLocal, AuthPassword);
        }

        public async void ImportSteamPlusPlusV2(string filePath)
        {
            await AuthService.Current.ImportAuthenticatorFile(filePath, AuthIsLocal, AuthPassword);
        }

        [Obsolete("use bool ImportAutoAsnyc(..", true)]
        public void ImportAuto(string filePath, Func<string, bool>? func = null)
        {
            var extension = Path.GetExtension(filePath);
            if (string.Equals(extension, FileEx.TXT, StringComparison.OrdinalIgnoreCase))
            {
                ImportWinAuth(filePath);
            }
            else if (string.Equals(extension, FileEx.MPO, StringComparison.OrdinalIgnoreCase))
            {
                ImportSteamPlusPlusV2(filePath);
            }
            else if (string.Equals(extension, ".dat", StringComparison.OrdinalIgnoreCase))
            {
                ImportSteamPlusPlusV1(filePath);
            }
            else if (string.Equals(extension, ".maFile", StringComparison.OrdinalIgnoreCase))
            {
                ImportSDA(filePath);
            }
            else if (func == null || !func(extension))
            {
                Toast.Show(AppResources.LocalAuth_ExportAuth_Error.Format(ImportResultCode.IncorrectFormat));
            }
        }

        public async Task<bool> ImportAutoAsnyc(string filePath, string? extension = null)
        {
            extension ??= Path.GetExtension(filePath);
            if (string.Equals(extension, FileEx.TXT, StringComparison.OrdinalIgnoreCase))
            {
                return AuthService.Current.ImportWinAuthenticators(filePath, AuthIsLocal, AuthPassword);
            }
            else if (string.Equals(extension, FileEx.MPO, StringComparison.OrdinalIgnoreCase))
            {
                return await AuthService.Current.ImportAuthenticatorFile(filePath, AuthIsLocal, AuthPassword);
            }
            else if (string.Equals(extension, ".dat", StringComparison.OrdinalIgnoreCase))
            {
                return AuthService.Current.ImportSteamToolsV1Authenticator(filePath, AuthIsLocal, AuthPassword);
            }
            else if (string.Equals(extension, ".maFile", StringComparison.OrdinalIgnoreCase))
            {
                return AuthService.Current.ImportSDAFile(filePath, AuthIsLocal, AuthPassword);
            }
            return false;
        }

        IEnumerable<string>? ToUrls(byte[] bytes)
        {
            var bytes_decompress_br = bytes.DecompressByteArrayByBrotli();
            var dtos = Serializable.DMP<LightweightExportDTO[]>(bytes_decompress_br);
            if (dtos.Any_Nullable())
            {
                var urls = dtos!.Select(x => x.ToString());
                return urls;
            }
            return null;
        }

        [Obsolete("use bool ImportSteamPlusPlusV2B(..", true)]
        public void ImportSteamPlusPlusV2(byte[] bytes) => ImportSteamPlusPlusV2B(bytes);

        public bool ImportSteamPlusPlusV2B(byte[] bytes)
        {
            var isOK = false;
            try
            {
                var urls = ToUrls(bytes);
                if (urls != null)
                {
                    AuthService.Current.ImportWinAuthenticators(urls, AuthIsLocal, AuthPassword);
                    isOK = true;
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(ImportSteamPlusPlusV2B), e, string.Empty);
            }
            if (!isOK)
            {
                Toast.Show(string.Format(AppResources.LocalAuth_ExportAuth_Error, ImportResultCode.IncorrectFormat));
            }
            return isOK;
        }

        [Obsolete("use bool ImportSteamPlusPlusV2B(..", true)]
        public void ImportSteamPlusPlusV2(IEnumerable<byte[]> items) => ImportSteamPlusPlusV2B(items);

        public bool ImportSteamPlusPlusV2B(IEnumerable<byte[]> items)
        {
            var isOK = false;
            List<string> list = new();
            foreach (var bytes in items)
            {
                try
                {
                    var urls = ToUrls(bytes);
                    if (urls != null)
                    {
                        list.AddRange(urls);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(nameof(ImportSteamPlusPlusV2B), e, string.Empty);
                }
            }
            if (list.Any())
            {
                AuthService.Current.ImportWinAuthenticators(list, AuthIsLocal, AuthPassword);
                isOK = true;
            }
            if (!isOK)
            {
                Toast.Show(string.Format(AppResources.LocalAuth_ExportAuth_Error, ImportResultCode.IncorrectFormat));
            }
            return isOK;
        }

        public ICommand SppV2Btn_Click { get; }

        public ICommand SppBtn_Click { get; }

        public ICommand SdaBtn_Click { get; }

        public ICommand WinAuthBtn_Click { get; }
    }
}
