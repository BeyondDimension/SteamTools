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

                var isUnderConstruction = e.Current.Id switch
                {
                    PreferenceButton.UserProfile or
                    PreferenceButton.BindPhoneNumber or
                    PreferenceButton.ChangePhoneNumber => true,
                    _ => false,
                };
                if (isUnderConstruction)
                {
                    MainApplication.ShowUnderConstructionTips();
                    return;
                }

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
            var layout = new LinearLayoutManager2(Context, LinearLayoutManager.Vertical, false);
            binding.rvPreferenceButtons.SetLayoutManager(layout);
            binding.rvPreferenceButtons.AddItemDecoration(new VerticalGroupItemDecoration(binding.rvPreferenceButtons.PaddingTop));
            binding.rvPreferenceButtons.SetAdapter(adapter);
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.layoutUser)
            {
#if !DEBUG
                MainApplication.ShowUnderConstructionTips();
                return true;
#endif

                this.StartActivity<LoginOrRegisterActivity>();
                return true;
            }
            //else if (view.Id == Resource.Id.???)
            //{
            //}

            return base.OnClick(view);
        }
    }
}