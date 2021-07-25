using QRCoder.Exceptions;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.IO;
using System.Properties;
using System.Threading.Tasks;
using static System.Application.FilePicker2;

namespace System.Application.UI.ViewModels
{
    public partial class ExportAuthWindowViewModel
    {
        public static string TitleName => AppResources.LocalAuth_ExportAuth;

        public ExportAuthWindowViewModel() : base()
        {
            Title =
#if !__MOBILE__
                ThisAssembly.AssemblyTrademark + " | " +
#endif
                TitleName;

#if !__MOBILE__
            SelectPathButton_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await SaveAsync(new SaveOptions
                {
                    FileTypes = new FilePickerFilter(new (string, IEnumerable<string>)[] {
                        ("MsgPack Files", new[] { "mpo" }),
                        ("Data Files", new[] { "dat" }),
                        ("All Files", new[] { "*" }),
                    }),
                    InitialFileName = DefaultExportAuthFileName,
                    PickerTitle = ThisAssembly.AssemblyTrademark,
                });
                Path = result?.FullPath;
            });
#endif
        }

        private bool _IsPasswordEncrypt;
        public bool IsPasswordEncrypt
        {
            get => _IsPasswordEncrypt;
            set => this.RaiseAndSetIfChanged(ref _IsPasswordEncrypt, value);
        }

        private string? _Password;
        public string? Password
        {
            get => _Password;
            set => this.RaiseAndSetIfChanged(ref _Password, value);
        }

        private string? _VerifyPassword;
        public string? VerifyPassword
        {
            get => _VerifyPassword;
            set => this.RaiseAndSetIfChanged(ref _VerifyPassword, value);
        }

        private bool _IsOnlyCurrentComputerEncrypt;
        public bool IsOnlyCurrentComputerEncrypt
        {
            get => _IsOnlyCurrentComputerEncrypt;
            set => this.RaiseAndSetIfChanged(ref _IsOnlyCurrentComputerEncrypt, value);
        }

        private string? _Path;
        public string? Path
        {
            get => _Path;
            set => this.RaiseAndSetIfChanged(ref _Path, value);
        }

        private Stream? _QRCode;
        /// <summary>
        /// 当前导出的二维码图像数据流
        /// </summary>
        public Stream? QRCode
        {
            get => _QRCode;
            set
            {
                var oldValue = _QRCode;
                if (this.RaiseAndSetIfChanged2(ref _QRCode, value)) return;
                oldValue?.Dispose();
            }
        }

        private bool _IsExportQRCode;
        /// <summary>
        /// 是否启用导出为二维码
        /// </summary>
        public bool IsExportQRCode
        {
            get => _IsExportQRCode;
            set => this.RaiseAndSetIfChanged(ref _IsExportQRCode, value);
        }

        private bool _IsExporting;
        /// <summary>
        /// 是否正在导出中
        /// </summary>
        public bool IsExporting
        {
            get => _IsExporting;
            set => this.RaiseAndSetIfChanged(ref _IsExporting, value);
        }

        async Task ExportAuthCore(Func<bool, string?, Task> func)
        {
            var (success, _) = await AuthService.Current.HasPasswordEncryptionShowPassWordWindow();
            if (!success)
            {
                this.Close();
            }

            if (string.IsNullOrEmpty(Path))
            {
                Toast.Show(AppResources.LocalAuth_ProtectionAuth_PathError);
                return;
            }

            if (IsPasswordEncrypt)
            {
                if (string.IsNullOrWhiteSpace(VerifyPassword) && VerifyPassword != Password)
                {
                    Toast.Show(AppResources.LocalAuth_ProtectionAuth_PasswordErrorTip);
                    return;
                }
            }
            else
            {
                VerifyPassword = null;
            }

            await func(IsOnlyCurrentComputerEncrypt, VerifyPassword);
        }

        public async void ExportAuth()
        {
            IsExporting = true;

            if (IsExportQRCode)
            {
                await ExportAuthToQRCodeAsync();
            }
            else
            {
                await ExportAuthToFileAsync();
            }

            IsExporting = false;
        }

        async Task ExportAuthToFileAsync() => await ExportAuthCore((isLocal, password) =>
        {
            AuthService.Current.ExportAuthenticators(Path, isLocal, password);

            this.Close();

            Toast.Show(string.Format(AppResources.LocalAuth_ExportAuth_ExportSuccess, Path));

            return Task.CompletedTask;
        });

        async Task ExportAuthToQRCodeAsync() => await ExportAuthCore(async (isLocal, password) =>
        {
            var bytes = await AuthService.Current.GetExportAuthenticatorsAsync(isLocal, password);
            QRCode = await Task.Run(() => CreateQRCode(bytes));
        });

        static Stream? CreateQRCode(byte[] bytes)
        {
            try
            {
                return QRCodeHelper.Create(bytes);
            }
            catch (DataTooLongException)
            {
                Toast.Show(AppResources.AuthLocal_ExportToQRCodeTooLongErrorTip);
            }
            catch (Exception e)
            {
                Toast.Show(e.ToString());
            }
            return null;
        }

        public static string DefaultExportAuthFileName => "Steam++  Authenticator " + DateTime.Now.ToString(DateTimeFormat.Date) + FileEx.MPO;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _QRCode?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}