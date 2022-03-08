using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using AndroidX.Annotations;
using AndroidX.RecyclerView.Widget;
using System.Application.UI.Activities;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Text;
using Xamarin.Essentials;
using static System.Application.UI.ViewModels.AboutPageViewModel;
using M = System.Application.UI.ViewModels.AboutPageViewModel;
using V = Binding.fragment_about;
using C = System.Application.UI.Controllers.AboutController;

namespace System.Application.UI.Controllers
{
    internal sealed class AboutController : ControllerBase<V, M>
    {
        public AboutController(IHost host, V binding) : base(host, binding)
        {

        }

        public override M? OnCreateViewModel() => Instance;

        public override void OnCreate()
        {
            if (IsActivity)
            {
                SetSupportActionBarWithNavigationClick(true);
            }

            binding!.tvDevelopers.SetLinkMovementMethod();
            binding!.tvBusinessCooperationContact.SetLinkMovementMethod();
            binding!.tvOpenSourceLicensed.SetLinkMovementMethod();
            binding!.tvAgreementAndPrivacy.SetLinkMovementMethod();
            //binding!.tvContributors.SetLinkMovementMethod();

            binding!.tvTitle.TextFormatted = CreateTitle();
            binding!.tvVersion.Text = $"{ViewModel!.LabelVersionDisplay} {ViewModel.VersionDisplay}";
            binding!.tvDevelopers.TextFormatted = CreateDevelopers();
            binding!.tvBusinessCooperationContact.TextFormatted = CreateBusinessCooperationContact();
            binding!.tvOpenSourceLicensed.TextFormatted = CreateOpenSourceLicensed();
            binding!.tvCopyright.Text = Copyright;
            //binding!.tvContributors.TextFormatted = CreateContributors();

            R.Subscribe(() =>
            {
                if (IsActivity)
                {
                    Activity.Title = ViewModel.Name;
                }
                if (binding == null) return;
                binding.tvAgreementAndPrivacy.TextFormatted = CreateAgreementAndPrivacy();
            }).AddTo(this);

            var adapter = new SmallPreferenceButtonAdapter<PreferenceButtonViewModel, PreferenceButton>(ViewModel!.PreferenceButtons);
            adapter.ItemClick += async (_, e) =>
            {
                switch (e.Current.Id)
                {
                    case PreferenceButton.捐助:
                        Activity.StartActivity<DonateActivity>();
                        break;
                    case PreferenceButton.检查更新:
                        ViewModel!.CheckUpdateCommand.Invoke();
                        break;
                    case PreferenceButton.更新日志:
                        await Browser2.OpenAsync(string.Format(
                            UrlConstants.OfficialWebsite_Box_Changelog_,
                            IApplication.Instance.Theme.ToString2(),
                            R.Language));
                        break;
                    case PreferenceButton.常见问题疑难解答:
                        await Browser2.OpenAsync(string.Format(
                            UrlConstants.OfficialWebsite_Box_Faq_,
                            IApplication.Instance.Theme.ToString2(),
                            R.Language));
                        break;
                    case PreferenceButton.开放源代码许可:
                        TextBlockActivity.StartActivity(Activity, new TextBlockViewModel
                        {
                            Title = AppResources.About_OpenSource,
                            ContentSource = TextBlockViewModel.ContentSourceEnum.OpenSourceLibrary,
                        });
                        break;
                    case PreferenceButton.源码仓库:
                        ComboBoxHelper.Dialog(Activity, SourceRepositories, async x => await Browser2.OpenAsync(x switch
                        {
                            GitHub => UrlConstants.GitHub_Repository,
                            Gitee => UrlConstants.Gitee_Repository,
                            _ => default,
                        }));
                        break;
                    case PreferenceButton.产品官网:
                        await Browser2.OpenAsync(UrlConstants.OfficialWebsite);
                        break;
                    case PreferenceButton.联系我们:
                        await Browser2.OpenAsync(UrlConstants.OfficialWebsite_Contact);
                        break;
                    case PreferenceButton.Bug反馈:
                        ComboBoxHelper.Dialog(Activity, SourceRepositories, async x => await Browser2.OpenAsync(x switch
                        {
                            GitHub => UrlConstants.GitHub_Issues,
                            Gitee => UrlConstants.Gitee_Issues,
                            _ => default,
                        }));
                        break;
                    case PreferenceButton.账号注销:
                        ViewModel!.DelAccountCommand.Invoke();
                        break;
                    case PreferenceButton.社区翻译:
                        TextBlockActivity.StartActivity(Activity, new TextBlockViewModel
                        {
                            Title = nameof(PreferenceButton.社区翻译),
                            ContentSource = TextBlockViewModel.ContentSourceEnum.Translators,
                            FontSizeResId = Resource.Dimension.translators_font_size,
                        });
                        break;
                }
            };
            binding.rvPreferenceButtons.SetLinearLayoutManager();
            binding.rvPreferenceButtons.AddItemDecoration(new PreferenceButtonItemDecoration(Context, Resource.Dimension.preference_buttons_space_min));
            binding.rvPreferenceButtons.SetAdapter(adapter);

            SetOnClickListener(binding.ivLogo, binding.tvTitle);
        }

        public override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.ivLogo || view.Id == Resource.Id.tvTitle)
            {
                AboutAppInfoPopup.OnClick();
                return true;
            }
            return base.OnClick(view);
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

        static SpannableString CreateDevelopers() => RichTextHelper.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new(Developers_0);
            length = str.Length;
            str.Append(At_Rmbadmin);
            list.Add((new HyperlinkClickableSpan(async _ =>
            {
                await Browser2.OpenAsync(UrlConstants.GitHub_User_Rmbadmin);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            str.Append(Separator);
            length = str.Length;
            str.Append(At_AigioL);
            list.Add((new HyperlinkClickableSpan(async _ =>
            {
                await Browser2.OpenAsync(UrlConstants.GitHub_User_AigioL);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            str.Append(Separator);
            length = str.Length;
            str.Append(At_Mossimos);
            list.Add((new HyperlinkClickableSpan(async _ =>
            {
                await Browser2.OpenAsync(UrlConstants.GitHub_User_Mossimos);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });

        static SpannableString CreateBusinessCooperationContact() => RichTextHelper.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new(BusinessCooperationContact_0);
            length = str.Length;
            str.Append(At_Cliencer);
            list.Add((new HyperlinkClickableSpan(async _ =>
            {
                await Browser2.OpenAsync(UrlConstants.BILI_User_Cliencer);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });

        static SpannableString CreateAgreementAndPrivacy() => RichTextHelper.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new();
            length = str.Length;
            str.Append(AppResources.User_Agreement);
            list.Add((new HyperlinkClickableSpan(async _ =>
            {
                await Browser2.OpenAsync(UrlConstants.OfficialWebsite_Box_Agreement);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            str.Append(Separator2);
            length = str.Length;
            str.Append(AppResources.User_Privacy);
            list.Add((new HyperlinkClickableSpan(async _ =>
            {
                await Browser2.OpenAsync(UrlConstants.OfficialWebsite_Box_Privacy);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });

        static SpannableString CreateOpenSourceLicensed() => RichTextHelper.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new(OpenSourceLicensed_0);
            length = str.Length;
            str.Append(OpenSourceLicensed_1);
            list.Add((new HyperlinkClickableSpan(async _ =>
            {
                await Browser2.OpenAsync(UrlConstants.License_GPLv3);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });

        //SpannableString CreateContributors() => RichTextHelper.CreateSpannableString(list =>
        //{
        //    int length;
        //    StringBuilder str = new(Contributors_0);
        //    str.AppendLine();
        //    var i = 1;
        //    foreach (var item in contributors_translations)
        //    {
        //        length = str.Length;
        //        str.Append(item.Key);
        //        list.Add((new HyperlinkClickableSpan(_ =>
        //        {
        //            ViewModel!.ContributorsCommand.Invoke(item.Key);
        //        }), length, str.Length, SpanTypes.ExclusiveExclusive));
        //        str.Append(Separator);
        //        str.Append(item.Value);
        //        str.Append(" ");
        //        if (i == contributors_translations.Count)
        //        {
        //            str.Append(Translation);
        //        }
        //        else
        //        {
        //            str.AppendLine(Translation);
        //        }
        //        i++;
        //    }
        //    return str;
        //});

        sealed class PreferenceButtonItemDecoration : TopItemDecoration<PreferenceButtonViewModel>
        {
            public PreferenceButtonItemDecoration(int top) : base(top)
            {
            }

            public PreferenceButtonItemDecoration(Context context, [IdRes] int topResId) : base(context, topResId)
            {
            }

            protected override bool IsHeader(PreferenceButtonViewModel viewModel)
            {
                return IsHeaderPreferenceButton(viewModel.Id);
            }
        }
    }
}

namespace System.Application.UI.Fragments
{
#if __XAMARIN_FORMS__
    internal sealed class AboutFragment : BaseMvcFragment<V, M, C>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_about;
    }
#endif
}

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(AboutActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class AboutActivity : BaseMvcActivity<V, M, C>
    {
        protected override int? LayoutResource => Resource.Layout.activity_about_not_binding;
    }
}