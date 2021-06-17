using Android.Runtime;
using Binding;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(PhoneNumberLoginOrRegisterFragment))]
    internal sealed class PhoneNumberLoginOrRegisterFragment : BaseFragment<fragment_login_and_register_by_phone_number>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_login_and_register_by_phone_number;
    }
}