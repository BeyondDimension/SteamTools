using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using Binding;
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.UI.Activities;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(FastLoginOrRegisterFragment))]
    internal sealed class FastLoginOrRegisterFragment : BaseFragment<fragment_login_and_register_by_fast, LoginOrRegisterPageViewModel>, IDisposableHolder
    {
        readonly CompositeDisposable disposables = new();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => disposables;

        protected override int? LayoutResource => Resource.Layout.fragment_login_and_register_by_fast;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            if (Activity is AppCompatActivity activity)
            {
                activity.SetSupportActionBar(binding!.toolbar);
            }

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding != null)
                {
                    binding.toolbar.Title = AppResources.LoginAndRegister;
                    binding.tvTip.Text = AppResources.User_FastLoginTip;
                    binding.tvAgreementAndPrivacy.TextFormatted = LoginOrRegisterActivity.CreateAgreementAndPrivacy(ViewModel!);
                }
            }).AddTo(this);
        }

        void GoToUsePhoneNumberPage(View? view)
        {
            var navController = this.GetNavController(view);
            navController?.Navigate(Resource.Id.action_navigation_login_or_register_fast_to_navigation_login_or_register_phone_number);
        }
    }
}