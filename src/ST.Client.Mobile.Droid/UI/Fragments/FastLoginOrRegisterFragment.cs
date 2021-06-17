using Android.Runtime;
using Binding;
using System.Application.UI.Activities;
using AndroidX.Navigation;
using Android.Views;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(FastLoginOrRegisterFragment))]
    internal sealed class FastLoginOrRegisterFragment : BaseFragment<fragment_login_and_register_by_fast>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_login_and_register_by_fast;

        void GoToUsePhoneNumberPage(View? view)
        {
            var navController = this.GetNavController(view);
            navController?.Navigate(Resource.Id.action_navigation_login_or_register_fast_to_navigation_login_or_register_phone_number);
        }
    }
}