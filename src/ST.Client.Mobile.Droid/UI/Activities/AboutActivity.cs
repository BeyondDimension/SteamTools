using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Webkit;
using AndroidX.RecyclerView.Widget;
using Binding;
using Google.Android.Material.Dialog;
using ReactiveUI;
using System.Application.Security;
using System.Application.Services;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Reflection;
using System.Text;
using System.Windows;
using Xamarin.Essentials;
using static System.Application.Services.CloudService.Constants;
using static System.Application.UI.ViewModels.AboutPageViewModel;
using _ThisAssembly = System.Properties.ThisAssembly;
using AndroidApplication = Android.App.Application;
using Process = System.Diagnostics.Process;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(AboutActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class AboutActivity : BaseActivity<activity_about, AboutPageViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.activity_about;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            ViewModel = new();
            ViewModel.AddTo(this);

            binding!.tvDevelopers.SetLinkMovementMethod();
            binding!.tvBusinessCooperationContact.SetLinkMovementMethod();
            binding!.tvOpenSourceLicensed.SetLinkMovementMethod();
            binding!.tvAgreementAndPrivacy.SetLinkMovementMethod();

            binding!.tvTitle.TextFormatted = CreateTitle();
            binding!.tvVersion.Text = $"{ViewModel.LabelVersionDisplay} {ViewModel.VersionDisplay}";
            binding!.tvDevelopers.TextFormatted = CreateDevelopers();
            binding!.tvBusinessCooperationContact.TextFormatted = CreateBusinessCooperationContact();
            binding!.tvOpenSourceLicensed.TextFormatted = CreateOpenSourceLicensed();
            binding!.tvCopyright.Text = ViewModel.Copyright;

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                Title = ViewModel.Title;
                binding!.tvAgreementAndPrivacy.TextFormatted = CreateAgreementAndPrivacy();
            }).AddTo(this);

            static void BrowserOpenByDialogClick(DialogClickEventArgs e, Func<string, string?> func)
            {
                if (e.Which >= 0 && e.Which < SourceRepositories.Length)
                {
                    var value = func(SourceRepositories[e.Which]);
                    if (value != default) BrowserOpen(value);
                }
            }

            var adapter = new SmallPreferenceButtonAdapter<PreferenceButtonViewModel, PreferenceButton>(ViewModel!.PreferenceButtons);
            adapter.ItemClick += (_, e) =>
            {
                switch (e.Current.Id)
                {
                    case PreferenceButton.检查更新:
                        ViewModel!.CheckUpdateCommand.Invoke();
                        break;
                    case PreferenceButton.更新日志:
                        BrowserOpen(string.Format(
                            UrlConstants.OfficialWebsite_Box_Changelog_,
                            MainApplication.GetTheme(),
                            R.Language));
                        break;
                    case PreferenceButton.常见问题疑难解答:
                        BrowserOpen(string.Format(
                            UrlConstants.OfficialWebsite_Box_Faq_,
                            MainApplication.GetTheme(),
                            R.Language));
                        break;
                    case PreferenceButton.开放源代码许可:
                        TextBlockActivity.StartActivity(this, new TextBlockViewModel
                        {
                            Title = AppResources.About_OpenSource,
                            ContentSource = TextBlockViewModel.ContentSourceEnum.OpenSourceLibrary,
                        });
                        break;
                    case PreferenceButton.源码仓库:
                        new MaterialAlertDialogBuilder(this).SetItems(SourceRepositories, (_, e) => BrowserOpenByDialogClick(e, value => value switch
                        {
                            GitHub => UrlConstants.GitHub_Repository,
                            Gitee => UrlConstants.Gitee_Repository,
                            _ => default,
                        })).Show();
                        break;
                    case PreferenceButton.产品官网:
                        BrowserOpen(UrlConstants.OfficialWebsite);
                        break;
                    case PreferenceButton.联系我们:
                        BrowserOpen(UrlConstants.OfficialWebsite_Contact);
                        break;
                    case PreferenceButton.Bug反馈:
                        new MaterialAlertDialogBuilder(this).SetItems(SourceRepositories, (_, e) => BrowserOpenByDialogClick(e, value => value switch
                        {
                            GitHub => UrlConstants.GitHub_Issues,
                            Gitee => UrlConstants.Gitee_Issues,
                            _ => default,
                        })).Show();
                        break;
                    case PreferenceButton.账号注销:
                        ViewModel!.DelAccountCommand.Invoke();
                        break;
                }
            };
            var layout = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            binding.rvPreferenceButtons.SetLayoutManager(layout);
            binding.rvPreferenceButtons.SetAdapter(adapter);

            SetOnClickListener(binding.ivLogo);
        }

        int show_runtime_info_counter;
        DateTime show_runtime_info_last_click_time;
        const int show_runtime_info_counter_max = 5;
        const double show_runtime_info_click_effective_interval = 1.5;
        protected override void OnClick(View view)
        {
            if (view.Id == Resource.Id.ivLogo)
            {
                var now = DateTime.Now;
                if (show_runtime_info_last_click_time == default || (now - show_runtime_info_last_click_time).TotalSeconds <= show_runtime_info_click_effective_interval)
                {
                    show_runtime_info_counter++;
                }
                else
                {
                    show_runtime_info_counter = 1;
                }
                show_runtime_info_last_click_time = now;
                if (show_runtime_info_counter >= show_runtime_info_counter_max)
                {
                    show_runtime_info_counter = 0;
                    show_runtime_info_last_click_time = default;
                    StringBuilder b = new("[os.ver] Android ");
                    var sdkInt = Build.VERSION.SdkInt;
                    b.AppendFormat("{0}(API {1})", sdkInt, (int)sdkInt);
                    b.AppendLine();
                    b.Append("[app.ver] ");
                    GetAppDisplayVersion(this, b);
                    static void GetAppDisplayVersion(Context context, StringBuilder b)
                    {
                        var info = context.PackageManager!.GetPackageInfo(context.PackageName!, default);
                        if (info == default) return;
#pragma warning disable CS0618 // 类型或成员已过时
                        b.AppendFormat("{0}({1})", info.VersionName, Build.VERSION.SdkInt >= BuildVersionCodes.P ? info.LongVersionCode : info.VersionCode);
#pragma warning restore CS0618 // 类型或成员已过时
                    }
                    b.AppendLine();
                    if (_ThisAssembly.Debuggable)
                    {
                        b.Append("[app.multi] ");
                        VirtualApkCheckUtil.GetCheckResult(AndroidApplication.Context, b);
                        b.AppendLine();
                    }
                    b.Append("[rom.ver] ");
                    AndroidROM.Current.ToString(b);
                    b.AppendLine();
                    b.Append("[webview.ver] ");
                    GetWebViewImplementationVersionDisplayString(b);
                    b.AppendLine();
                    static void GetWebViewImplementationVersionDisplayString(StringBuilder b)
                    {
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var webViewPackage = WebView.CurrentWebViewPackage;
                            if (webViewPackage != default)
                            {
                                var packageName = webViewPackage.PackageName;
                                var packageVersion = webViewPackage.VersionName;
                                if (string.Equals(packageName, "com.android.webview", StringComparison.OrdinalIgnoreCase) || string.Equals(packageName, "com.google.android.webview", StringComparison.OrdinalIgnoreCase))
                                {
                                    packageName = "asw"; // Android System Webview
                                }
                                else if (string.Equals(packageName, "com.android.chrome", StringComparison.OrdinalIgnoreCase))
                                {
                                    packageName = "chrome"; // Chrome
                                }
                                b.AppendFormat("{0}({1})", packageVersion, packageName);
                                return;
                            }
                        }
                    }
                    b.Append("[time] ");
                    GetTime(b);
                    static void GetTime(StringBuilder b)
                    {
                        string timeString;
                        const string f = "yy-MM-dd HH:mm:ss";
                        const string f2 = "HH:mm:ss";
                        const string f3 = "dd HH:mm:ss";
                        var time = Process.GetCurrentProcess().StartTime;
                        time = time.ToLocalTime();
                        var utc_time = time.ToUniversalTime();
                        var local = TimeZoneInfo.Local;
                        if (utc_time.Hour == time.Hour)
                            timeString = time.ToString(time.Year >= 2100 ? DateTimeFormat.Standard : f);
                        else if (utc_time.Day == time.Day)
                            timeString = $"{utc_time.ToString(f)}({time.ToString(f2)} {local.StandardName})";
                        else
                            timeString = $"{utc_time.ToString(f)}({time.ToString(f3)} {local.StandardName})";
                        b.Append(timeString);
                    }
                    b.AppendLine();
                    b.Append("[screen] ");
                    var metrics = new DisplayMetrics();
                    WindowManager?.DefaultDisplay?.GetRealMetrics(metrics);
                    GetScreen(this, metrics, b);
                    static void GetScreen(Context context, DisplayMetrics metrics, StringBuilder b)
                    {
                        var screen_w = metrics.WidthPixels;
                        var screen_h = metrics.HeightPixels;
                        var screen_max = Math.Max(screen_w, screen_h);
                        var screen_min = screen_max == screen_w ? screen_h : screen_w;
                        var configuration = context.Resources?.Configuration;
                        var screen_dp_w = configuration?.ScreenWidthDp ?? 0;
                        var screen_dp_h = configuration?.ScreenHeightDp ?? 0;
                        var screen_dp_max = Math.Max(screen_dp_w, screen_dp_h);
                        var screen_dp_min = screen_max == screen_dp_w ? screen_dp_h : screen_dp_w;
                        b.AppendFormat("{0}x{1}({2}x{3})", screen_max, screen_min, screen_dp_max, screen_dp_min);
                        var dpi = (int)metrics.DensityDpi;
                        b.AppendFormat(" {0}dpi", dpi);
                        if (dpi < (int)DisplayMetricsDensity.Low)
                        {
                            b.Append("(<ldpi)");
                        }
                        else if (dpi == (int)DisplayMetricsDensity.Low)
                        {
                            b.Append("(ldpi)");
                        }
                        else if (dpi < (int)DisplayMetricsDensity.Medium)
                        {
                            b.Append("(ldpi~mdpi)");
                        }
                        else if (dpi == (int)DisplayMetricsDensity.Medium)
                        {
                            b.Append("(mdpi)");
                        }
                        else if (dpi == (int)DisplayMetricsDensity.Tv)
                        {
                            b.Append("(tv)");
                        }
                        else if (dpi < (int)DisplayMetricsDensity.High)
                        {
                            b.Append("(mdpi~hdpi)");
                        }
                        else if (dpi == (int)DisplayMetricsDensity.High)
                        {
                            b.Append("(hdpi)");
                        }
                        else if (dpi < (int)DisplayMetricsDensity.Xhigh)
                        {
                            b.Append("(hdpi~xhdpi)");
                        }
                        else if (dpi == (int)DisplayMetricsDensity.Xhigh)
                        {
                            b.Append("(xhdpi)");
                        }
                        else if (dpi < (int)DisplayMetricsDensity.Xxhigh)
                        {
                            b.Append("(xhdpi~xxhdpi)");
                        }
                        else if (dpi == (int)DisplayMetricsDensity.Xxhigh)
                        {
                            b.Append("(xxhdpi)");
                        }
                        else if (dpi < (int)DisplayMetricsDensity.Xxxhigh)
                        {
                            b.Append("(xxhdpi~xxxhdpi)");
                        }
                        else if (dpi == (int)DisplayMetricsDensity.Xxxhigh)
                        {
                            b.Append("(xxxhdpi)");
                        }
                    }
                    b.AppendLine();
                    static string ToLowerString(bool? value)
                    {
                        if (value.HasValue)
                        {
                            return value.Value ? "true" : "false";
                        }
                        return string.Empty;
                    }
                    b.Append("[screen.notch] ");
                    b.Append(ToLowerString(ScreenCompatUtil.IsNotch(this)));
                    b.AppendLine();
                    b.Append("[screen.notch.hide] ");
                    b.Append(ToLowerString(ScreenCompatUtil.IsHideNotch(this)));
                    b.AppendLine();
                    b.Append("[screen.full.gestures] ");
                    b.Append(ToLowerString(ScreenCompatUtil.IsFullScreenGesture(this)));
                    b.AppendLine();
                    static string GetJavaSystemGetProperty(string propertyKey)
                    {
                        try
                        {
                            return Java.Lang.JavaSystem.GetProperty(propertyKey) ?? "";
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    }
                    b.Append("[jvm.ver] ");
                    b.Append(GetJavaSystemGetProperty("java.vm.version"));
                    b.AppendLine();
                    b.Append("[mono.ver] ");
                    b.Append(Mono.Runtime.GetDisplayName());
                    b.AppendLine();
                    b.Append("[kernel.ver] ");
                    b.Append(GetJavaSystemGetProperty("os.version"));
                    b.AppendLine();
                    b.Append("[device] ");
                    b.Append(Build.Device ?? "");
                    b.AppendLine();
                    b.Append("[device.model] ");
                    b.Append(Build.Model ?? "");
                    b.AppendLine();
                    b.Append("[device.product] ");
                    b.Append(Build.Product ?? "");
                    b.AppendLine();
                    b.Append("[device.brand] ");
                    b.Append(Build.Brand ?? "");
                    b.AppendLine();
                    b.Append("[device.manufacturer] ");
                    b.Append(Build.Manufacturer ?? "");
                    b.AppendLine();
                    b.Append("[device.fingerprint] ");
                    b.Append(Build.Fingerprint ?? "");
                    b.AppendLine();
                    b.Append("[device.hardware] ");
                    b.Append(Build.Hardware ?? "");
                    b.AppendLine();
                    b.Append("[device.tags] ");
                    b.Append(Build.Tags ?? "");
                    b.AppendLine();
                    if (_ThisAssembly.Debuggable)
                    {
                        b.Append("[device.arc] ");
                        b.Append(ToLowerString(DeviceSecurityCheckUtil.IsCompatiblePC(this)));
                        b.AppendLine();
                        b.Append("[device.emulator] ");
                        b.Append(ToLowerString(DeviceSecurityCheckUtil.IsEmulator));
                        b.AppendLine();
                    }
                    b.Append("[device.gl.renderer] ");
                    b.Append(GLES20.GlGetString(GLES20.GlRenderer) ?? "");
                    b.AppendLine();
                    b.Append("[device.gl.vendor] ");
                    b.Append(GLES20.GlGetString(GLES20.GlVendor) ?? "");
                    b.AppendLine();
                    b.Append("[device.gl.version] ");
                    b.Append(GLES20.GlGetString(GLES20.GlVersion) ?? "");
                    b.AppendLine();
                    b.Append("[device.gl.extensions] ");
                    b.Append(GLES20.GlGetString(GLES20.GlExtensions) ?? "");
                    b.AppendLine();
                    b.Append("[device.biometric] ");
                    b.Append(ToLowerString(IBiometricService.Instance.IsSupportedAsync().Result));
                    b.AppendLine();
                    b.Append("[xamarin.essentials.supported] ");
                    static bool? GetXamarinEssentialsIsSupported()
                    {
                        try
                        {
                            if (typeof(MainThread2).Assembly.GetType("System.Application.XamarinEssentials").GetProperty("IsSupported", BindingFlags.Public | BindingFlags.Static).GetValue(null) is bool b)
                                return b;
                        }
                        catch
                        {
                        }
                        return null;
                    }
                    b.Append(ToLowerString(GetXamarinEssentialsIsSupported()));
                    b.AppendLine();
                    var b_str = b.ToString();
                    MessageBoxCompat.Show(b_str, "");
                }
                return;
            }
            base.OnClick(view);
        }

        static SpannableString CreateTitle()
        {
            SpannableString spannable = new(Title_0 + Title_1);
            ForegroundColorSpan fcs = new(ThemeAccentBrushKey.ToPlatformColor());
            int start = Title_0.Length, end = Title_0.Length + Title_1.Length;
            spannable.SetSpan(fcs, start, end, SpanTypes.ExclusiveExclusive);
            StyleSpan ss = new(TypefaceStyle.Bold);
            spannable.SetSpan(ss, start, end, SpanTypes.ExclusiveExclusive);
            return spannable;
        }

        static SpannableString CreateDevelopers() => MainApplication.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new(Developers_0);
            length = str.Length;
            str.Append(At_Rmbadmin);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                BrowserOpen(UrlConstants.GitHub_User_Rmbadmin);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            str.Append(Separator);
            length = str.Length;
            str.Append(At_AigioL);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                BrowserOpen(UrlConstants.GitHub_User_AigioL);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            str.Append(Separator);
            length = str.Length;
            str.Append(At_Mossimos);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                BrowserOpen(UrlConstants.GitHub_User_Mossimos);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });

        static SpannableString CreateBusinessCooperationContact() => MainApplication.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new(BusinessCooperationContact_0);
            length = str.Length;
            str.Append(At_Cliencer);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                BrowserOpen(UrlConstants.BILI_User_Cliencer);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });

        static SpannableString CreateAgreementAndPrivacy() => MainApplication.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new();
            length = str.Length;
            str.Append(AppResources.User_Agreement);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                BrowserOpen(UrlConstants.OfficialWebsite_Box_Agreement);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            str.Append(Separator2);
            length = str.Length;
            str.Append(AppResources.User_Privacy);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                BrowserOpen(UrlConstants.OfficialWebsite_Box_Privacy);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });

        static SpannableString CreateOpenSourceLicensed() => MainApplication.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new(OpenSourceLicensed_0);
            length = str.Length;
            str.Append(OpenSourceLicensed_1);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                BrowserOpen(UrlConstants.License_GPLv3);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });
    }
}