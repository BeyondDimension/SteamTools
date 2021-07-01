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
using Com.Tencent.Tauth;
using Microsoft.Identity.Client;
using Org.Json;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Text;
using JObject = Java.Lang.Object;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(LoginOrRegisterActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
          LaunchMode = LaunchMode.SingleTask,
          ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class LoginOrRegisterActivity : BaseActivity<activity_login_or_register, LoginOrRegisterPageViewModel>, LoginOrRegisterPageViewModel.IPlatformUIHost, IUiListener
    {
        protected override int? LayoutResource => Resource.Layout.activity_login_or_register;

        static readonly Dictionary<int, Func<string>> title_strings = new()
        {
            { Resource.Id.navigation_login_or_register_fast, () => AppResources.LoginAndRegister },
            { Resource.Id.navigation_login_or_register_phone_number, () => AppResources.User_PhoneLogin },
        };

        Tencent? tencent;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            tencent = TencentOpenApiSDK.GetTencent(this);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar);

            ViewModel = new() { Close = Finish };
            ViewModel.AddTo(this);

            var appBarConfiguration = new AppBarConfiguration.Builder(Resource.Id.navigation_login_or_register_fast).Build();
            navController = ((NavHostFragment)SupportFragmentManager.FindFragmentById(Resource.Id.nav_host_fragment)).NavController;
            NavigationUI.SetupActionBarWithNavController(this, navController, appBarConfiguration);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                var currentDestinationId = navController.CurrentDestination.Id;
                foreach (var item in title_strings)
                {
                    var title = item.Value();
                    this.SetNavigationGraphTitle(item.Key, title);
                    if (currentDestinationId == item.Key)
                    {
                        Title = title;
                    }
                }
            }).AddTo(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            tencent = null;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            var resultCode_ = (int)resultCode;
            if (requestCode == Com.Tencent.Connect.Common.Constants.RequestLogin)
            {
                Tencent.OnActivityResultData(requestCode, resultCode_, data, this);
                return;
            }

            // Return control to MSAL
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);

            base.OnActivityResult(requestCode, resultCode, data);
        }

        public static SpannableString CreateAgreementAndPrivacy(LoginOrRegisterPageViewModel viewModel) => MainApplication.CreateSpannableString(list =>
        {
            int length;
            StringBuilder str = new(AppResources.User_RegisterAgreed);
            str.Append(" ");
            length = str.Length;
            str.Append(AppResources.User_Agreement);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                viewModel.OpenHyperlink.Invoke(LoginOrRegisterPageViewModel.Agreement);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            str.Append(" ");
            str.Append(AppResources.And);
            str.Append(" ");
            length = str.Length;
            str.Append(AppResources.User_Privacy);
            list.Add((new HyperlinkClickableSpan(_ =>
            {
                viewModel.OpenHyperlink.Invoke(LoginOrRegisterPageViewModel.Privacy);
            }), length, str.Length, SpanTypes.ExclusiveExclusive));
            return str;
        });

        void LoginOrRegisterPageViewModel.IPlatformUIHost.QQLogin() => tencent!.LoginServerSide(this);

        void IUiListener.OnCancel()
        {
        }

        void IUiListener.OnComplete(JObject? p0)
        {
            if (p0 != null && p0 is JSONObject jsonObj)
            {
                var jsonStr = jsonObj.ToString();
                Toast.Show(jsonStr);
            }
        }

        void IUiListener.OnError(UiError? p0) => p0.ShowError();

        void IUiListener.OnWarning(int p0)
        {
        }
    }
}