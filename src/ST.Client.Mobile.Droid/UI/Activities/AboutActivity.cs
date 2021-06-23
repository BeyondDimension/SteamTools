using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using AndroidX.RecyclerView.Widget;
using Binding;
using Google.Android.Material.Dialog;
using ReactiveUI;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Text;
using Xamarin.Essentials;
using static System.Application.Services.CloudService.Constants;
using static System.Application.UI.ViewModels.AboutPageViewModel;

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