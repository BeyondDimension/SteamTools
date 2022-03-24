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
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Application.UI.Fragments;
using System.Text;
using Android.Views;
using Android.Net;
using static AndroidX.Activity.Result.ActivityResultTask;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 引导开启 VPN 页面
    /// </summary>
    [Register(JavaPackageConstants.Activities + nameof(GuideVPNActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
      LaunchMode = LaunchMode.SingleTask,
      ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class GuideVPNActivity : BaseActivity<activity_guide_vpn>
    {
        protected override int? LayoutResource => Resource.Layout.activity_guide_vpn;

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            SetOnClickListener(binding.btnDone);
        }

        async void Prepare()
        {
            var intent = VpnService.Prepare(this);
            if (intent != null)
            {
                void OnResult(Intent intent)
                {
                    intent = IntermediateActivity.SetResult(intent, Result.Ok);
                    SetResult(Result.Ok, intent);
                    Finish();
                }
                await IntermediateActivity.StartAsync(intent,
                    requestCodeVpnService, onResult: OnResult);
            }
            else
            {
                Finish();
            }
        }

        protected override void OnClick(View view)
        {
            if (view.Id == Resource.Id.btnDone)
            {
                Prepare();
            }
        }
    }
}
