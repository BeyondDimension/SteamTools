using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Properties;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xamarin.Essentials;
using static System.Application.FilePicker2;
using static System.Application.Models.GAPAuthenticatorDTO;
using static System.Application.Repositories.IGameAccountPlatformAuthenticatorRepository;

namespace System.Application.UI.ViewModels
{
    public partial class AddAuthWindowViewModel
    {
        readonly IHttpService httpService = DI.Get<IHttpService>();
        readonly IGameAccountPlatformAuthenticatorRepository repository = DI.Get<IGameAccountPlatformAuthenticatorRepository>();

        public static string TitleName => AppResources.LocalAuth_AddAuth;

        public AddAuthWindowViewModel() : base()
        {
            Title =
#if !__MOBILE__
                ThisAssembly.AssemblyTrademark + " | " +
#endif
                TitleName;

            SppV2Btn_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                FilePickerFileType? fileTypes = DI.DeviceIdiom switch
                {
                    DeviceIdiom.Desktop => new FilePickerFilter(new (string, IEnumerable<string>)[] {
                        ("MsgPack Files", new[] { "mpo" }),
                        ("Data Files", new[] { "dat" }),
                        ("All Files", new[] { "*" }),
                    }),
                    _ => null,
                };
                await PickAsync(ImportSteamPlusPlusV2, fileTypes);
            });
            SppBtn_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                FilePickerFileType? fileTypes = DI.DeviceIdiom switch
                {
                    DeviceIdiom.Desktop => new FilePickerFilter(new (string, IEnumerable<string>)[] {
                        ("Data Files", new[] { "dat" }),
                        ("All Files", new[] { "*" }),
                    }),
                    _ => null,
                };
                await PickAsync(ImportSteamPlusPlusV1, fileTypes);
            });
            SdaBtn_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                FilePickerFileType? fileTypes = DI.DeviceIdiom switch
                {
                    DeviceIdiom.Desktop => new FilePickerFilter(new (string, IEnumerable<string>)[] {
                        ("MaFile Files", new[] { "maFile" }),
                        ("JSON Files", new[] { "json" }),
                        ("All Files", new[] { "*" }),
                    }),
                    _ => null,
                };
                await PickAsync(ImportSDA, fileTypes);
            });
            WinAuthBtn_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                FilePickerFileType? fileTypes = DI.DeviceIdiom switch
                {
                    DeviceIdiom.Desktop => new FilePickerFilter(new (string, IEnumerable<string>)[] {
                        ("Text Files", new[] { "txt" }),
                        ("All Files", new[] { "*" }),
                    }),
                    _ => DI.Platform switch
                    {
                        Platform.Android => new GeneralFilePickerFileType(new[] { MediaTypeNames.TXT }),
                        _ => null,
                    },
                };
                await PickAsync(ImportWinAuth, fileTypes);
            });

            Initialize();
        }

        private GAPAuthenticatorValueDTO.SteamAuthenticator? _SteamAuthenticator;
        private readonly GAPAuthenticatorValueDTO.SteamAuthenticator.EnrollState _Enroll = new() { RequiresLogin = true };
        private string? AuthPassword;
        private bool AuthIsLocal;

        private async void Initialize()
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
                    this.Close();
                }
            }
            AuthIsLocal = repository.HasLocal(auths);
        }

        public string? AuthName { get; set; }

        public string? UUID { get; set; }

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

        private Stream? _CaptchaImage;
        public Stream? CaptchaImage
        {
            get => _CaptchaImage;
            set => this.RaiseAndSetIfChanged(ref _CaptchaImage, value);
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

        public void ImportSteamGuard()
        {
            if (AuthService.Current.ImportSteamGuard(AuthName!, UUID!, SteamGuard!, AuthIsLocal, AuthPassword))
            {
                Toast.Show(AppResources.LocalAuth_AddAuthSuccess);
            }
            else
            {
                Toast.Show(AppResources.LocalAuth_ImportFaild);
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
#if !__MOBILE__
                ToastService.Current.Set(AppResources.Logining);
#else
                LoginSteamLoadingText = AppResources.Logining;
#endif
                result = _SteamAuthenticator.Enroll(_Enroll);
#if !__MOBILE__
                ToastService.Current.Set();
#else
                LoginSteamLoadingText = null;
#endif

                if (!result)
                {
                    if (string.IsNullOrEmpty(_Enroll.Error) == false)
                    {
                        MessageBoxCompat.Show(_Enroll.Error);
                        //ToastService.Current.Notify(_Enroll.Error);
                    }

                    if (_Enroll.Requires2FA == true)
                    {
                        MessageBoxCompat.Show(AppResources.LocalAuth_SteamUser_Requires2FA);
                        //ToastService.Current.Notify(AppResources.LocalAuth_SteamUser_Requires2FA);
                        return;
                    }

                    if (_Enroll.RequiresCaptcha == true)
                    {
                        CaptchaText = null;
                        CaptchaImage = null;
                        using var web = new WebClient();
                        var bt = web.DownloadData(_Enroll.CaptchaUrl);
                        using var stream = new MemoryStream(bt);
                        CaptchaImage = stream;
                        return;
                    }

                    if (_Enroll.RequiresEmailAuth == true)
                    {
                        RequiresLogin = false;
                        CaptchaText = null;
                        CaptchaImage = null;
                        EmailDomain = string.IsNullOrEmpty(_Enroll.EmailDomain) == false ? "***@" + _Enroll.EmailDomain : string.Empty;
                        return;
                    }

                    if (_Enroll.RequiresActivation == true)
                    {
                        EmailDomain = null;
                        _Enroll.Error = null;
                        RequiresLogin = false;

                        AuthService.AddOrUpdateSaveAuthenticators(new GAPAuthenticatorDTO
                        {
                            Name = nameof(GamePlatform.Steam) + "(" + UserName + ")",
                            Value = _SteamAuthenticator
                        }, AuthIsLocal, AuthPassword);

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
                    MessageBoxCompat.Show(_Enroll.Error!);
                    return;
                }
                RequiresActivation = false;
                RequiresAdd = true;
            }).ContinueWith(s =>
            {
                Log.Error(nameof(AddAuthWindowViewModel), s.Exception, nameof(LoginSteamImport));
                MessageBoxCompat.Show(s.Exception, "Error " + nameof(LoginSteamImport));
            }, TaskContinuationOptions.OnlyOnFaulted).ContinueWith(s =>
            {
                _IsLogining = false;
#if !__MOBILE__
                ToastService.Current.Set();
#else
                LoginSteamLoadingText = null;
#endif
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

        public void ImportSteamPlusPlusV2(string filePath)
        {
            AuthService.Current.ImportAuthenticatorFile(filePath, AuthIsLocal, AuthPassword);
        }

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

        public void ImportSteamPlusPlusV2(byte[] bytes)
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
                Log.Error(nameof(ImportSteamPlusPlusV2), e, string.Empty);
            }
            if (!isOK)
            {
                Toast.Show(string.Format(AppResources.LocalAuth_ExportAuth_Error, ImportResultCode.IncorrectFormat));
            }
        }

        public void ImportSteamPlusPlusV2(IEnumerable<byte[]> items)
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
                    Log.Error(nameof(ImportSteamPlusPlusV2), e, string.Empty);
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
        }

        public ICommand SppV2Btn_Click { get; }
        public ICommand SppBtn_Click { get; }
        public ICommand SdaBtn_Click { get; }
        public ICommand WinAuthBtn_Click { get; }
    }
}