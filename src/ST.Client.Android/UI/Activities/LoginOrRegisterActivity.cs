using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;
using Binding;
using ReactiveUI;
using System.Text;
using System.Net;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Application.UI.Fragments;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(LoginOrRegisterActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges,
        Exported = true)]
    [IntentFilter(actions: new[] { Intent.ActionView },
        Categories = new[] {
            Intent.CategoryBrowsable, // 通过浏览器的连接启动
            Intent.CategoryDefault, // 该页面可以被隐式调用
        },
        DataScheme = Constants.CUSTOM_URL_SCHEME_NAME,
        DataHost = UriHost)]
    internal sealed class LoginOrRegisterActivity : BaseActivity<activity_login_or_register, LoginOrRegisterWindowViewModel>, IHandleUri
    {
        /* URL Route
         * spp://auth/fast/{value?} 打开快速登录页面/回调授权值
         * spp://auth/phonenum 打开手机号登录页面
         */

        const string UriHost = "auth";

        protected override int? LayoutResource => Resource.Layout.activity_login_or_register;

        static readonly Dictionary<int, Func<string>> title_strings = new()
        {
            { Resource.Id.navigation_login_or_register_fast, () => AppResources.LoginAndRegister },
            { Resource.Id.navigation_login_or_register_phone_number, () => AppResources.User_PhoneLogin },
        };

        protected override LoginOrRegisterWindowViewModel? OnCreateViewModel()
        {
            return new() { Close = Finish };
        }

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            this.IsNotAuthenticated();

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            var appBarConfiguration = new AppBarConfiguration.Builder(Resource.Id.navigation_login_or_register_fast).Build();
            navController = ((NavHostFragment)SupportFragmentManager.FindFragmentById(Resource.Id.nav_host_fragment)!).NavController;
            NavigationUI.SetupActionBarWithNavController(this, navController, appBarConfiguration);

            R.Subscribe(() =>
            {
                var currentDestination = navController?.CurrentDestination;
                if (currentDestination != null)
                {
                    var currentDestinationId = currentDestination.Id;
                    foreach (var item in title_strings)
                    {
                        var title = item.Value();
                        this.SetNavigationGraphTitle(item.Key, title);
                        if (currentDestinationId == item.Key)
                        {
                            Title = title;
                        }
                    }
                }
            }).AddTo(this);

            this.HandleUri(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            this.HandleUri(intent);
        }

        async void IHandleUri.HandleUri(Uri uri)
        {
            if (uri.Scheme != Constants.CUSTOM_URL_SCHEME_NAME || uri.Host != UriHost || uri.Segments.Length < 2) return;
            switch (uri.Segments[1].TrimEnd('/'))
            {
                case Constants.Urls.Segment_LoginOrRegister_Fast:
                    if (uri.Segments.Length == 3)
                    {
                        var value = uri.Segments[2];
                        value = WebUtility.UrlDecode(value);
                        await ThirdPartyLoginHelper.OnMessage(value);
                    }
                    break;
                case Constants.Urls.Segment_LoginOrRegister_PhoneNum:
                    GoToUsePhoneNumberPage();
                    break;
            }
        }

        void GoToUsePhoneNumberPage()
        {
            var currentDestination = navController?.CurrentDestination;
            if (currentDestination != null)
            {
                if (currentDestination.Id != Resource.Id.navigation_login_or_register_phone_number)
                {
                    FastLoginOrRegisterFragment.GoToUsePhoneNumberPage(navController);
                }
            }
        }

        public static SpannableString CreateAgreementAndPrivacy(LoginOrRegisterWindowViewModel viewModel) => RichTextHelper.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new(AppResources.User_RegisterAgreed);
            str.Append(" ");
            length = str.Length;
            str.Append(AppResources.User_Agreement);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                viewModel.OpenHyperlink.Invoke(LoginOrRegisterWindowViewModel.Agreement);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            str.Append(" ");
            str.Append(AppResources.And);
            str.Append(" ");
            length = str.Length;
            str.Append(AppResources.User_Privacy);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                viewModel.OpenHyperlink.Invoke(LoginOrRegisterWindowViewModel.Privacy);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });

        protected override void OnDestroy()
        {
            ThirdPartyLoginHelper.DisposeServer();

            base.OnDestroy();
        }
    }
}