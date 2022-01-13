using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Activities;
using System.Application.UI.Adapters;
using System.Application.UI.ViewModels;
using static System.Application.UI.ViewModels.MyPageViewModel;
#if __XAMARIN_FORMS__
using XFShell = Xamarin.Forms.Shell;
#endif

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(MyFragment))]
    internal sealed class MyFragment : BaseFragment<fragment_my, MyPageViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_my;

        protected override MyPageViewModel? OnCreateViewModel() => Instance;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            ViewModel!.WhenAnyValue(x => x.NickName).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.tvNickName.Text = value;
            }).AddTo(this);

            SetOnClickListener(binding!.layoutUser);

            var adapter = new LargePreferenceButtonAdapter<PreferenceButtonViewModel, PreferenceButton>(ViewModel!.PreferenceButtons);
            adapter.ItemClick += (_, e) =>
            {
                if (e.Current.Authentication && !UserService.Current.IsAuthenticated)
                {
                    OnClick(binding!.layoutUser);
                    return;
                }

#if !DEBUG
                var isUnderConstruction = e.Current.Id switch
                {
                    PreferenceButton.BindPhoneNumber or
                    PreferenceButton.ChangePhoneNumber or PreferenceButton.UserProfile => true,
                    _ => false,
                };
                if (isUnderConstruction)
                {
                    MainApplication.ShowUnderConstructionTips();
                    return;
                }
#endif

#if __XAMARIN_FORMS__
                var route = e.Current.Id switch
                {
                    PreferenceButton.Settings => AppShell.IsUseBottomNav ?
                        "//MyPage/SettingsPage" : "//SettingsPage",
                    PreferenceButton.About => AppShell.IsUseBottomNav ?
                        "//MyPage/AboutPage" : "//AboutPage",
                    _ => null,
                };

                if (route != null)
                {
                    XFShell.Current.GoToAsync(route);
                    return;
                }
#endif

                var activityType = e.Current.Id switch
                {
                    PreferenceButton.UserProfile => typeof(UserProfileActivity),
                    PreferenceButton.BindPhoneNumber => typeof(BindPhoneNumberActivity),
                    PreferenceButton.ChangePhoneNumber => typeof(ChangePhoneNumberActivity),
                    PreferenceButton.Settings => typeof(SettingsActivity),
                    PreferenceButton.About => typeof(AboutActivity),
                    _ => (Type?)null,
                };
                if (activityType != null) this.StartActivity(activityType);
            };
            binding.rvPreferenceButtons.SetLinearLayoutManager();
            binding.rvPreferenceButtons.AddVerticalGroupItemDecoration(binding.rvPreferenceButtons.PaddingTop);
            binding.rvPreferenceButtons.SetAdapter(adapter);
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.layoutUser)
            {
                MainApplication.ShowUnderConstructionTips();
                //this.StartActivity<LoginOrRegisterActivity>();
                return true;
            }
            //else if (view.Id == Resource.Id.???)
            //{
            //}

            return base.OnClick(view);
        }
    }
}