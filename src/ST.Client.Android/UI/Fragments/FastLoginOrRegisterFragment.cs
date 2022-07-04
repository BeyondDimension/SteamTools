using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Navigation;
using AndroidX.RecyclerView.Widget;
using Binding;
using ReactiveUI;
using System.Application.UI.Activities;
using System.Application.UI.Adapters;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.ViewModels.LoginOrRegisterWindowViewModel;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(FastLoginOrRegisterFragment))]
    internal sealed class FastLoginOrRegisterFragment : BaseFragment<fragment_login_and_register_by_fast, LoginOrRegisterWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_login_and_register_by_fast;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            binding!.tvAgreementAndPrivacy.SetLinkMovementMethod();

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.tvTip.Text = AppResources.User_FastLoginTip;
                binding.tvAgreementAndPrivacy.TextFormatted = LoginOrRegisterActivity.CreateAgreementAndPrivacy(ViewModel!);
            }).AddTo(this);

            var adapter = new FastLoginChannelAdapter(ViewModel!);
            adapter.ItemClick += (_, e) =>
            {
                switch (e.Current.Id)
                {
                    case FastLoginChannelViewModel.PhoneNumber:
                        GoToUsePhoneNumberPage(this.GetNavController());
                        break;
                    default:
                        ViewModel!.ChooseChannel.Invoke(e.Current.Id);
                        break;
                }
            };
            binding.rvFastLoginChannels.SetLinearLayoutManager();
            binding.rvFastLoginChannels.AddVerticalItemDecorationRes(Resource.Dimension.fast_login_or_register_margin_subtract_compat_padding);
            binding.rvFastLoginChannels.SetAdapter(adapter);

            if (Activity is AppCompatActivity activity)
            {
                activity.SupportActionBar?.SetDisplayHomeAsUpEnabled(true);
            }
        }

        public static void GoToUsePhoneNumberPage(NavController? navController)
        {
#if !DEBUG
            MainApplication.ShowUnderConstructionTips();
#else
            navController?.Navigate(Resource.Id.action_navigation_login_or_register_fast_to_navigation_login_or_register_phone_number);
#endif
        }
    }
}