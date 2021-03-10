using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Application.Models;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI.Views;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Properties;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.UI
{
    internal static class Startup
    {
        static bool isInitialized;

        public static void Init()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                FileSystemDesktop.InitFileSystem();
                ModelValidatorProvider.Init();
                DI.Init(ConfigureServices);
            }
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDesktopAppService>(s => App.Instance);
            services.AddSingleton<IDesktopAvaloniaAppService>(s => App.Instance);

            // 桌面平台服务 此项放在其他通用业务实现服务之前
            services.AddDesktopPlatformService();

            services.TryAddToast();

            // 添加日志实现
            services.AddDesktopLogging();

            // 模型验证框架
            services.TryAddModelValidator();

            // app 配置项
            services.TryAddOptions(AppSettings);

            // 添加安全服务
            services.AddSecurityService<EmbeddedAesDataProtectionProvider, LocalDataProtectionProvider>();

            services.AddRepositories();

            // 键值对存储
            services.TryAddStorage();

            // 业务平台用户管理
            services.TryAddUserManager();

            // 服务端API调用
            services.TryAddCloudServiceClient<CloudServiceClient>();

            // 主线程助手类(MainThreadDesktop)
            services.AddMainThreadPlatformService();

            // 业务用户配置文件服务
            services.AddConfigFileService();

            // hosts 文件助手服务
            services.AddHostsFileService();

            // 通用 Http 服务
            services.AddHttpService();

            // Steam 相关助手、工具类服务
            services.AddSteamService();

            // Steamworks LocalApi Service
            services.TryAddSteamworksLocalApiService();

            // SteamDb WebApi Service
            services.AddSteamDbWebApiService();

            // Steamworks WebApi Service
            services.AddSteamworksWebApiService();

            // 应用程序更新服务
            services.AddAppUpdateService();

            // 托盘图标
#if WINDOWS
            services.AddTransient<INotifyIconWindow<ContextMenu>, Win32NotifyIconWindow>();
#endif
            services.AddNotifyIcon<NotifyIconImpl>();

            #region MessageBox

            /* System.Windows.MessageBox 在 WPF 库中，仅支持 Win 平台
             * 改为 System.Windows.MessageBoxCompat 可跨平台兼容
             * 在其他平台上使用 MessageBox.Avalonia 库实现
             * API变更说明：
             * - 如果需要获取返回值，即点击那个按钮，则使用异步版本 ShowAsync
             * - 如果不需要获取返回值，则可直接使用 同步版本 Show
             * 注意事项：
             * - 图标(Icon)与按钮(Button)不要使用标记为 Obsolete 的
             * - WPF 中 显示窗口(Show)会锁死父窗口等，类似 ShowDialog
             * - MessageBox.Avalonia 中则不会锁死窗口
             * 已知问题：
             * - 在 MessageBox.Avalonia 中
             *  - 如果内容文本(messageBoxText)过短 UI 上的文字显示不全
             *  - 点击窗口按 Ctrl+C 无法复制弹窗中的文本内容
             *  - 按钮文本(ButtonText)缺少本地化翻译(Translate)
             *  - 某些图标图片与枚举值不太匹配，例如 Information
             */
            services.AddShowWindowService();

#if WINDOWS
            // 可选项，在 Win 平台使用 WPF 实现的 MessageBox
            //services.AddSingleton<IMessageBoxCompatService, WPFMessageBoxCompatService>();
#endif

            #endregion
        }

        static AppSettings AppSettings
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var options = new AppSettings
                {
                    AppVersion = GetResValue("app-id", false).TryParseGuidN() ?? default,
                    AppSecretVisualStudioAppCenter = GetResValue("appcenter-secret", false),
                    AesSecret = GetResValue("aes-key", true),
                    RSASecret = GetResValue("rsa-public-key", false),
                    ApiBaseUrl = GetResValue("api-base-url", false),
                };
                return options;
                string? GetResValue(string name, bool isSingle)
                {
                    const string thisProjectRootNamespace = "System.Application.UI";
                    var resName = isSingle ? $"{thisProjectRootNamespace}.{name}.pfx" : $"{thisProjectRootNamespace}.{name}-{(ThisAssembly.Debuggable ? "debug" : "release")}.pfx";
                    using var stream = assembly.GetManifestResourceStream(resName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        return reader.ReadToEnd();
                    }
                    return null;
                }
            }
        }

        sealed class NotifyIconImpl : NotifyIcon<ContextMenu>, INotifyIcon { }

#if WINDOWS
        sealed class Win32NotifyIconWindow : MainWindow, INotifyIconWindow<ContextMenu>
        {
            sealed class Win32WindowImpl : Avalonia.Win32.WindowImpl
            {
                public Win32NotifyIconWindow? Window { get; set; }

                protected override IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
                {
                    var _notifyIcon = Window?.NotifyIcon;
                    var value = _notifyIcon == null ? null : NotifyIcon<ContextMenu>.WndProc(_notifyIcon, msg, wParam, lParam);
                    return value ?? base.WndProc(hWnd, msg, wParam, lParam);
                }
            }

            public Win32NotifyIconWindow() : base(new Win32WindowImpl())
            {
                if (PlatformImpl is Win32WindowImpl impl)
                {
                    impl.Window = this;
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            }

            public IntPtr Handle => PlatformImpl.Handle.Handle;

            [NotNull, DisallowNull] // C# 8 not null
            public NotifyIcon<ContextMenu>? NotifyIcon { get; private set; }

            public void Initialize(INotifyIcon<ContextMenu> notifyIcon)
            {
                if (notifyIcon is NotifyIcon<ContextMenu> _notifyIcon)
                {
                    NotifyIcon = _notifyIcon;
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
                //Content = NotifyIcon;
            }
        }

        sealed class WPFMessageBoxCompatService : IMessageBoxCompatService
        {
            static MessageBoxButton GetButtonEnum(MessageBoxButtonCompat button) => button switch
            {
                MessageBoxButtonCompat.OK => MessageBoxButton.OK,
                MessageBoxButtonCompat.OKCancel => MessageBoxButton.OKCancel,
#pragma warning disable CS0618 // 类型或成员已过时
                MessageBoxButtonCompat.YesNo => MessageBoxButton.YesNo,
                MessageBoxButtonCompat.YesNoCancel => MessageBoxButton.YesNoCancel,
#pragma warning restore CS0618 // 类型或成员已过时
                _ => throw new ArgumentOutOfRangeException(nameof(button), $"value: {button}"),
            };

            static MessageBoxImage GetIcon(MessageBoxImageCompat icon) => icon switch
            {
                MessageBoxImageCompat.Asterisk => MessageBoxImage.Asterisk,
                MessageBoxImageCompat.Error => MessageBoxImage.Error,
                MessageBoxImageCompat.Exclamation => MessageBoxImage.Exclamation,
                MessageBoxImageCompat.None => MessageBoxImage.None,
#pragma warning disable CS0618 // 类型或成员已过时
                MessageBoxImageCompat.Question => MessageBoxImage.Question,
#pragma warning restore CS0618 // 类型或成员已过时
                _ => throw new ArgumentOutOfRangeException(nameof(icon), $"value: {icon}"),
            };

            static MessageBoxResultCompat GetResult(MessageBoxResult result) => result switch
            {
                MessageBoxResult.OK => MessageBoxResultCompat.OK,
#pragma warning disable CS0618 // 类型或成员已过时
                MessageBoxResult.Yes => MessageBoxResultCompat.Yes,
                MessageBoxResult.No => MessageBoxResultCompat.No,
#pragma warning restore CS0618 // 类型或成员已过时
                MessageBoxResult.Cancel => MessageBoxResultCompat.Cancel,
                MessageBoxResult.None => MessageBoxResultCompat.None,
                _ => throw new ArgumentOutOfRangeException(nameof(result), $"value: {result}"),
            };

            public Task<MessageBoxResultCompat> ShowAsync(string messageBoxText, string caption, MessageBoxButtonCompat button, MessageBoxImageCompat? icon)
            {
                var button_ = GetButtonEnum(button);
                if (icon.HasValue)
                {
                    var icon_ = GetIcon(icon.Value);
                    var result = MessageBox.Show(messageBoxText, caption, button_, icon_);
                    return Task.FromResult(GetResult(result));
                }
                else
                {
                    var result = MessageBox.Show(messageBoxText, caption, button_);
                    return Task.FromResult(GetResult(result));
                }
            }
        }
#endif
    }
}